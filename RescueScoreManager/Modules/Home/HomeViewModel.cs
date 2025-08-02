using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RescueScoreManager.Data;
using RescueScoreManager.Modules.Login;
using RescueScoreManager.Messages;
using RescueScoreManager.Modules.SelectNewCompetition;
using RescueScoreManager.Services;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;

namespace RescueScoreManager.Modules.Home;

public partial class HomeViewModel : ObservableObject, IRecipient<SelectNewCompetitionMessage>
{
    private readonly LoginViewModel _loginViewModel;
    private readonly SelectNewCompetitionViewModel _selectNewCompetitionViewModel;
    private readonly HomeInformationsViewModel _homeInformationsViewModel;
    private readonly IDialogService _dialogService;
    private readonly IApiService _apiService;
    private readonly IAuthenticationService _authService;
    private readonly IXMLService _xmlService;
    private readonly IExcelService _excelService;
    private readonly IMessenger _messenger;
    private readonly ILogger<HomeViewModel> _logger;

    [ObservableProperty]
    private Competition? _competition;

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(NewCompetitionCommand))]
    private bool _isLoaded = false;

    public HomeViewModel(
        LoginViewModel loginViewModel,
        SelectNewCompetitionViewModel selectNewCompetitionViewModel,
        HomeInformationsViewModel homeInformationsViewModel,
        IDialogService dialogService,
        IApiService apiService,
        IAuthenticationService authService,
        IXMLService xmlService,
        IExcelService excelService,
        IMessenger messenger,
        ILogger<HomeViewModel> logger)
    {
        _loginViewModel = loginViewModel ?? throw new ArgumentNullException(nameof(loginViewModel));
        _selectNewCompetitionViewModel = selectNewCompetitionViewModel ?? throw new ArgumentNullException(nameof(selectNewCompetitionViewModel));
        _homeInformationsViewModel = homeInformationsViewModel ?? throw new ArgumentNullException(nameof(homeInformationsViewModel));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _messenger.Register<SelectNewCompetitionMessage>(this);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteFileCommands))]
    private async Task OpenFile()
    {
        try
        {
            var fileInfo = GetFile();
            if (fileInfo == null) { return; }

            _messenger.Send(new IsBusyMessage(true, GetLocalizedString("FileLoadingInfo")));

            await LoadCompetitionFileAsync(fileInfo);

            _messenger.Send(new IsBusyMessage(false));
            _messenger.Send(new SnackMessage(GetLocalizedString("FileLoadedSuccess")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading competition file");
            _messenger.Send(new IsBusyMessage(false));
            _messenger.Send(new SnackMessage(GetLocalizedString("FileLoadError")));
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteFileCommands))]
    private async Task NewCompetitionAsync()
    {
        try
        {
            if (!_authService.IsAuthenticated)
            {
                bool? loginResult = _dialogService.ShowLoginView(_loginViewModel);
                if (loginResult != true) { return; }
            }

            bool? result = _dialogService.ShowSelectNewCompetition(_selectNewCompetitionViewModel);
            if (result == true)
            {
                await ProcessNewCompetitionAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new competition");
            _messenger.Send(new SnackMessage(GetLocalizedString("NewCompetitionError")));
        }
    }

    private bool CanExecuteFileCommands() => !IsLoaded;

    private FileInfo? GetFile()
    {
        OpenFileDialog openFileDialog = new()
        {
            Title = GetLocalizedString("OpenFileTitle"),
            Filter = "Competition Files (*.ffss)|*.ffss",
            CheckFileExists = true,
            CheckPathExists = true,
            RestoreDirectory = true,
        };

        return openFileDialog.ShowDialog() == true ? new FileInfo(openFileDialog.FileName) : null;
    }

    public async void Receive(SelectNewCompetitionMessage message)
    {
        if (message.NewCompetition != null)
        {
            await ProcessSelectedCompetitionAsync(message.NewCompetition);
        }
        else
        {
            CurrentViewModel = null;
            _logger.LogInformation("Competition selection was cancelled");
        }
    }

    private async Task ProcessSelectedCompetitionAsync(Competition competition)
    {
        try
        {
            _messenger.Send(new IsBusyMessage(true, GetLocalizedString("DataLoadingInfo")));
            // TODO TO BE UPDATED
            await _apiService.LoadAsync(competition, _authService.AuthenticationInfo);

            _messenger.Send(new IsBusyMessage(true, GetLocalizedString("FileCreationInfo")));

            _xmlService.SetPath(competition.Name);
            _xmlService.Initialize(competition, _apiService.GetCategories(), _apiService.GetClubs(),
                                _apiService.GetLicensees(), _apiService.GetRaces(), _apiService.GetTeams());
            _xmlService.Save();
            _xmlService.Reset();
            _xmlService.Load();

            CurrentViewModel = _homeInformationsViewModel;
            _homeInformationsViewModel.Update();
            IsLoaded = true;

            _messenger.Send(new IsBusyMessage(false));
            _messenger.Send(new SnackMessage(GetLocalizedString("DataLoadedSuccess")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing selected competition: {CompetitionName}", competition?.Name);
            _messenger.Send(new IsBusyMessage(false));
            _messenger.Send(new SnackMessage(GetLocalizedString("DataLoadError")));
        }
    }

    private async Task LoadCompetitionFileAsync(FileInfo fileInfo)
    {
        // Implementation for loading competition file
        await Task.Run(() =>
        {
            _xmlService.LoadFromFile(fileInfo.FullName);
            CurrentViewModel = _homeInformationsViewModel;
            _homeInformationsViewModel.Update();
        });

        IsLoaded = true;
    }

    private async Task ProcessNewCompetitionAsync()
    {
        // Implementation for processing new competition
        await Task.Delay(100); // Placeholder
    }

    private string GetLocalizedString(string key)
    {
        return ResourceManagerLocalizationService.Instance.GetString(key) ?? key;
    }
}


