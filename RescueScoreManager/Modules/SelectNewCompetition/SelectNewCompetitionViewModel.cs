using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using RescueScoreManager.Data;
using RescueScoreManager.Messages;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.SelectNewCompetition;

public partial class SelectNewCompetitionViewModel : ObservableObject
{
    #region Properties

    private DateTime _lastDateChange = DateTime.Now;

    [ObservableProperty]
    private DateTime _beginDate;
    // This method is automatically called when BeginDate changes due to the ObservableProperty attribute
    partial void OnBeginDateChanged(DateTime value)
    {
        _lastDateChange = DateTime.Now;
        // Automatically refresh the competition list when date changes
        _ = RefreshOnDateChangeAsync();
    }

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ValidateCommand))]
    public CompetitionDisplayItem? _selectedCompetitionDetail;

    // Use ObservableCollection for better UI performance and automatic change notifications
    public ObservableCollection<CompetitionDisplayItem> Competitions { get; } = new();

    #endregion Properties

    #region Attributes
    private readonly IApiService _apiService;
    private readonly IAuthenticationService _authService;
    private readonly IMessenger _messenger;
    private readonly IImageService? _imageService;
    private CancellationTokenSource? _refreshCancellationTokenSource;
    #endregion Attributes

    public event EventHandler? RequestClose;

    #region Command
    private async Task RefreshOnDateChangeAsync()
    {
        // Add a small delay to avoid rapid API calls if user is scrolling through dates quickly
        await Task.Delay(300);

        // Only refresh if this is still the current date (user hasn't changed it again)
        if (DateTime.Now.Subtract(_lastDateChange).TotalMilliseconds > 250)
        {
            await Refresh();
        }
    }

    [RelayCommand]
    public async Task Refresh()
    {
        // Cancel any existing refresh operation
        _refreshCancellationTokenSource?.Cancel();
        _refreshCancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsLoading = true;
            await UpdateCompetitionListAsync(_refreshCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            // Handle error - could show message to user
            Debug.WriteLine($"Error refreshing competitions: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanValidate))]
    private void Validate()
    {
        _messenger.Send(new SelectNewCompetitionMessage(SelectedCompetitionDetail!.Competition));
        OnRequestClose();
    }

    private bool CanValidate() => SelectedCompetitionDetail != null;

    [RelayCommand]
    private void Cancel()
    {
        _messenger.Send(new SelectNewCompetitionMessage());

        OnRequestClose();
    }
    #endregion Command

    public SelectNewCompetitionViewModel(
       IApiService apiService,
       IAuthenticationService authenticationService,
       IMessenger messenger,
        IImageService? imageService = null)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _authService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _imageService = imageService; // Optional - for club logo loading


        _beginDate = Debugger.IsAttached ? new DateTime(2024, 3, 30) : DateTime.Now;
    }

    public async Task InitializeAsync()
    {
        await Refresh();
    }

    private async Task UpdateCompetitionListAsync(CancellationToken cancellationToken = default)
    {
        if (_authService.AuthenticationInfo == null)
        {
            return;
        }

        try
        {
            var competitions = await _apiService.GetCompetitionsAsync(
                BeginDate,
                _authService.AuthenticationInfo,
                cancellationToken);

            // Update UI on main thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                Competitions.Clear();

                // Sort competitions by begin date for better UX
                var sortedCompetitions = competitions.OrderBy(c => c.BeginDate);

                foreach (var competition in sortedCompetitions)
                {
                    var displayItem = new CompetitionDisplayItem(competition, _imageService);
                    Competitions.Add(displayItem);
                }
            });
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw to be handled by caller
        }
        catch (Exception ex)
        {
            // Log error and potentially show user-friendly message
            System.Diagnostics.Debug.WriteLine($"Failed to load competitions: {ex.Message}");
            throw;
        }
    }

    private void OnRequestClose()
    {
        // Find and close the parent dialog window
        if (Application.Current.MainWindow != null)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this ||
                    (window.Content is FrameworkElement fe && fe.DataContext == this))
                {
                    window.DialogResult = false;
                    window.Close();
                    break;
                }
            }
        }
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    // Cleanup resources when view model is disposed
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshCancellationTokenSource?.Cancel();
            _refreshCancellationTokenSource?.Dispose();
        }
    }

    ~SelectNewCompetitionViewModel()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
