using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using RescueScoreManager.Data;
using RescueScoreManager.Modules.Home;
using RescueScoreManager.Messages;
using RescueScoreManager.Services;
using RescueScoreManager.Modules.Login;
using CommunityToolkit.Mvvm.Input;
using RescueScoreManager.Modules.SelectNewCompetition;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RescueScoreManager.Modules.Forfeit;

namespace RescueScoreManager;

public partial class MainWindowViewModel : ObservableObject,
                                            IRecipient<LoginMessage>,
                                            IRecipient<IsBusyMessage>,
                                            IRecipient<SnackMessage>
{
    private readonly HomeViewModel _homeViewModel;
    private readonly IApiService _apiService;
    private readonly IAuthenticationService _authService;
    private readonly IXMLService _xmlService;
    private readonly ILocalizationService _localizationService;
    private readonly IDialogService _dialogService;
    private readonly IMessenger _messenger;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;


    // ViewModels cache
    private ForfeitViewModel? _forfeitViewModel;

    [ObservableProperty]
    private ObservableObject? _currentViewModel;
    [ObservableProperty]
    private bool _isBusy = false;
    [ObservableProperty]
    private string _busyMessage = "";
    [ObservableProperty]
    private SnackbarMessageQueue _snackbarMessageQueue;
    [ObservableProperty]
    private string _title = "Rescue Score Manager";
    [ObservableProperty]
    private LanguageModel _currentLanguage;
    [ObservableProperty]
    private bool _isLoggedIn;
    [ObservableProperty] 
    private string _userLabel = "";
    [ObservableProperty]
    private string _userRole = "";
    [ObservableProperty]
    private string _userType = "";

    public ObservableCollection<LanguageModel> AvailableLanguages => _localizationService.AvailableLanguages;

    public MainWindowViewModel(
        HomeViewModel homeViewModel,
        IApiService apiService,
        IAuthenticationService authService,
        IXMLService xmlService,
        ILocalizationService localizationService,
        IDialogService dialogService,
        IMessenger messenger,
        ILogger<MainWindowViewModel> logger,
        IServiceProvider serviceProvider)
    {

        _homeViewModel = homeViewModel ?? throw new ArgumentNullException(nameof(homeViewModel));
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));


        _currentViewModel = homeViewModel;
        _snackbarMessageQueue = new SnackbarMessageQueue();
        _currentLanguage = AvailableLanguages.FirstOrDefault() ?? new LanguageModel { DisplayName = "English", CultureCode = "en-US" };

        UpdateLogInfo();
        messenger.Register<LoginMessage>(this);
        messenger.Register<IsBusyMessage>(this);
        messenger.Register<SnackMessage>(this);

        // Check existing authentication
        CheckExistingAuthAsync();
    }

    #region Commands

    #region Logout Command
    [RelayCommand]
    private void Logout()
    {
        try
        {
            _authService.Logout();
            UpdateLogInfo();
            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    #endregion Logout Command
    #region Login Command
    [RelayCommand]
    private void Login()
    {
        try
        {
            // Create LoginViewModel through DI to get proper logger injection
            var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();

            // Use the specific ShowLoginView method instead of generic ShowDialog
            bool? result = _dialogService.ShowLoginView(loginViewModel);

            if (result == true)
            {
                UpdateLogInfo();
                _logger.LogInformation("User logged in successfully");
            }
            else
            {
                _logger.LogInformation("Login was cancelled or failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login process");
            SnackbarMessageQueue.Enqueue("Login error occurred. Please try again.");
        }

    }

    #endregion Login Command

    #region Navigation Commands

    [RelayCommand]
    private void NavigateToHome()
    {
        try
        {
            CurrentViewModel = _homeViewModel;
            _logger.LogInformation("Navigated to Home view");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Home view");
            _messenger.Send(new SnackMessage("Error navigating to Home"));
        }
    }

    [RelayCommand]
    private async Task NavigateToForfeit()
    {
        try
        {
            // Check if data is loaded
            //if (_xmlService.IsLoaded())
            //{
            //    _messenger.Send(new SnackMessage("Please load a competition first"));
            //    return;
            //}

            // Create or get cached ViewModel
            _forfeitViewModel = null;
            if (_forfeitViewModel == null)
            {
                _forfeitViewModel = _serviceProvider.GetRequiredService<ForfeitViewModel>();
                await _forfeitViewModel.InitializeAsync();
            }

            CurrentViewModel = _forfeitViewModel;
            _logger.LogInformation("Navigated to Forfeit view");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Forfeit view");
            _messenger.Send(new SnackMessage("Error navigating to Forfeit management"));
        }
    }
    #endregion Navigation Commands


    #endregion Commands

    #region Private functions
    private async void CheckExistingAuthAsync()
    {
        try
        {
            bool isValid = await _authService.ValidateAndRefreshTokenAsync();
            if (isValid)
            {
                OnLoginSucceeded();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existing authentication");
        }
    }
    private void OnLoginSucceeded()
    {
        UpdateLogInfo();
        _logger.LogInformation("Login succeeded, UI updated");
    }

    private void UpdateLogInfo()
    {
        IsLoggedIn = _authService.IsAuthenticated;
        UserLabel = _authService.CurrentUser?.Label ?? GetLocalizedString("NotConnected");
        UserRole = _authService.CurrentUser?.Role ?? "";
        UserType = _authService.CurrentUser?.Type ?? "";
    }

    private string GetLocalizedString(string key)
    {
        return ResourceManagerLocalizationService.Instance.GetString(key) ?? key;
    }
    #endregion Private functions

    #region Message
    /// <summary>
    /// Message received when the a login has been validated
    /// </summary>
    /// <param name="message"></param>
    public void Receive(LoginMessage message)
    {
        UserLabel = _authService.IsAuthenticated ? _authService.CurrentUser.Label : $"{ResourceManagerLocalizationService.Instance.GetString("NotConnected")}";
        UserRole = _authService.IsAuthenticated ? _authService.CurrentUser.Role : "";
        UserType = _authService.IsAuthenticated ? _authService.CurrentUser.Type : "";
        IsLoggedIn = _authService.IsAuthenticated;

        UpdateLogInfo();
        if (message.IsConnected)
        {
            SnackbarMessageQueue.Enqueue(GetLocalizedString("LoginSuccessful"));
        }
    }

    public void Receive(IsBusyMessage message)
    {
        IsBusy = message.IsBusy;
        BusyMessage = message.Text ?? "";
    }
    public void Receive(SnackMessage message)
    {
        SetSnackBarMessage(message.Text);
    }

    //public void Receive(OpenCompetitionMessage message)
    //{
    //    Title = Title + " " + _xmlService.GetCompetition().Name;
    //    SetSnackBarMessage("Compétition chargée!");
    //}

    private void SetSnackBarMessage(string message, int duration = 3)
    {
        SnackbarMessageQueue.Enqueue(content: message,
                                            actionContent: null,
                                            actionHandler: null,
                                            actionArgument: null,
                                            promote: false,
                                            neverConsiderToBeDuplicate: true,
                                            durationOverride: TimeSpan.FromSeconds(3));
    } 
    #endregion Message
}
