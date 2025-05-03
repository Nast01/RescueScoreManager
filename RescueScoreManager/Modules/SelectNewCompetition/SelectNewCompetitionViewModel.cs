using System.ComponentModel.DataAnnotations;
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
    [ObservableProperty]
    private string _title;
    [ObservableProperty]
    private DateTime _beginDate;
    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ValidateCommand))]
    public Competition _selectedCompetition;

    [ObservableProperty]
    private List<Competition> _competitions = new();

    #endregion Properties

    #region Attributes
    private IApiService _apiService { get; }
    private IAuthenticationService _authService { get; }
    private IMessenger _messenger { get; }
    #endregion Attributes

    public event EventHandler RequestClose;

    #region Command
    [RelayCommand]
    public async Task Refresh()
    {
        IsLoading = true;
        await UpdateCompetitionList();
        IsLoading = false;
    }

    [RelayCommand(CanExecute = nameof(CanValidate))]
    private Task Validate()
    {
        _messenger.Send(new SelectNewCompetitionMessage(SelectedCompetition));
        return Task.CompletedTask;
    }

    private bool CanValidate() => SelectedCompetition != null;

    [RelayCommand]
    private void Cancel()
    {
        _messenger.Send(new SelectNewCompetitionMessage());
    }

    public void OnRequestClose()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
    #endregion Command

    public SelectNewCompetitionViewModel(
                            IApiService apiService,
                            IAuthenticationService authenticationService,
                            IMessenger messenger)
    {
        _title = $"{ResourceManagerLocalizationService.Instance.GetString("NewCompetition")}";
        _beginDate = DateTime.Now;

        _apiService = apiService;
        _authService = authenticationService;

        _messenger = messenger;
    }



    public async Task UpdateCompetitionList()
    {
        Competitions.Clear();
        Competitions.AddRange(await _apiService.GetCompetitions(BeginDate, _authService.AuthenticationInfo));
        Competitions = Competitions.OrderBy(c => c.BeginDate).ToList();
    }
}
