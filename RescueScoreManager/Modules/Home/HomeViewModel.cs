using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;

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

namespace RescueScoreManager.Modules.Home;

public partial class HomeViewModel : ObservableObject, IRecipient<SelectNewCompetitionMessage>
{
    #region Attributes
    private LoginViewModel _loginViewModel { get; }
    private SelectNewCompetitionViewModel _selectNewCompetitionViewModel { get; }
    private HomeInformationsViewModel _homeGraphsViewModel { get; }
    private IDialogService _dialogService { get; }
    private IApiService _apiService { get; }
    private IAuthenticationService _authService { get; }
    private IXMLService _xmlService { get; }
    private IExcelService _excelService { get; }
    private IMessenger _messenger { get; }
    #endregion Attributes

    #region Properties
    [ObservableProperty]
    private Competition? _competition = null;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(NewCompetitionCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenStartListFileCommand))]
    private bool _isLoaded = false;
    #endregion Properties

    public HomeViewModel(LoginViewModel loginViewModel,
                            SelectNewCompetitionViewModel selectNewCompetitionViewModel,
                            HomeInformationsViewModel homeGraphsViewModel,
                            IDialogService dialogService,
                            IApiService apiService,
                            IAuthenticationService authService,
                            IXMLService xmlService,
                            IExcelService excelService,
                            IMessenger messenger)//RescueScoreManagerContext context,
    {
        //_context = context ?? throw new ArgumentNullException(nameof(context));
        _loginViewModel = loginViewModel ?? throw new ArgumentNullException(nameof(_loginViewModel));
        _selectNewCompetitionViewModel = selectNewCompetitionViewModel;
        _homeGraphsViewModel = homeGraphsViewModel;
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(_dialogService));
        _apiService = apiService;
        _authService = authService;
        _xmlService = xmlService;
        _excelService = excelService;
        _messenger = messenger;
        messenger.RegisterAll(this);
    }


    #region OpenFile Command
    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    public void OpenFile()
    {
        if (GetFile() is { } file)
        {
            _messenger.Send(new IsBusyMessage(true, $"{ResourceManagerLocalizationService.Instance.GetString("DataLoadingInfo")}..."));
            _xmlService.SetPath(file);
            _xmlService.Load();
            IsLoaded = true;

            _messenger.Send(new IsBusyMessage());
            _messenger.Send(new SnackMessage($"{ResourceManagerLocalizationService.Instance.GetString("CompetitionLoadedInfo")} !"));

            CurrentViewModel = _homeGraphsViewModel;
            _homeGraphsViewModel.Update();
        }
    }

    private bool CanOpenFile() => IsLoaded == false;
    #endregion OpenFile Command
    #region NewCompetition Command
    [RelayCommand(CanExecute = nameof(CanNewCompetition))]
    private async Task NewCompetition()
    {

        if (CheckInternetConnection())
        {
            CurrentViewModel = _selectNewCompetitionViewModel;
            await _selectNewCompetitionViewModel.Refresh();
        }
    }

    private bool CanNewCompetition() => IsLoaded == false;
    #endregion NewCompetition Command

    #region OpenDisqualicationFile Command
    [RelayCommand]
    private void OpenDisqualicationFile()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Assets\\Documents\\Disqualification.docx");
        FileInfo fileInfo = new(path);
        if (fileInfo.Exists == false)
        {
            _messenger.Send(new SnackMessage($"{ResourceManagerLocalizationService.Instance.GetString("NoDisqualificationFileError")}"));
        }
        else
        {
            try
            {
                Process.Start("explorer.exe", path);

                _messenger.Send(new SnackMessage($"{ResourceManagerLocalizationService.Instance.GetString("FileOpenInfo")}"));
            }
            catch (Exception)
            {
                _messenger.Send(new SnackMessage($"{ResourceManagerLocalizationService.Instance.GetString("OpenFileError")}"));                
            }
        }
    }

    #endregion NewCompetition Command
    #region OpenStartListFile Command
    [RelayCommand(CanExecute = nameof(CanOpenStartListFile))]
    private void OpenStartListFile()
    {
        try
        {
            string path = _excelService.GenerateStartList(_xmlService.GetCompetition(), _xmlService.GetRaces(), _xmlService.GetReferees());
            Process.Start("explorer.exe", path);
        }
        catch (Exception)
        {
            _messenger.Send(new SnackMessage($"{ResourceManagerLocalizationService.Instance.GetString("OpenFileError")}"));
        }
    }

    private bool CanOpenStartListFile() => IsLoaded == true;
    #endregion OpenStartListFile Command
    #region OpenClubListFile Command
    [RelayCommand(CanExecute = nameof(CanOpenClubListFile))]
    private void OpenClubListFile()
    {
        try
        {
            string path = _excelService.GenerateStartList(_xmlService.GetCompetition(), _xmlService.GetRaces(), _xmlService.GetReferees());
            Process.Start("explorer.exe", path);
        }
        catch (Exception)
        {
            _messenger.Send(new SnackMessage($"{ResourceManagerLocalizationService.Instance.GetString("OpenFileError")}"));
        }
    }

    private bool CanOpenClubListFile() => IsLoaded == true;
    #endregion OpenStartListFile Command


    private bool CheckInternetConnection()
    {
        if (NetworkInterface.GetIsNetworkAvailable())
        {
            return true;
        }
        else
        {
            // No internet connection
            _messenger.Send(new SnackMessage($"{ResourceManagerLocalizationService.Instance.GetString("NoInternetConnectionError")}"));
            return false;
        }
    }

    private static FileInfo? GetFile()
    {
        OpenFileDialog openFileDialog = new()
        {
            Title = "Open File",
            Filter = "All Files (*.ffss)|*.*",
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
            _messenger.Send(new IsBusyMessage(true, $"{ResourceManagerLocalizationService.Instance.GetString("DataLoadingInfo")}..."));

            // load the data coming from the api service
            await _apiService.Load(message.NewCompetition,_authService.AuthenticationInfo);

            _messenger.Send(new IsBusyMessage(true, $"{ResourceManagerLocalizationService.Instance.GetString("FileCreationInfo")}..."));
            // set the path to the folder of the competition
            _xmlService.SetPath(message.NewCompetition.Name);

            // create the xml file and load it
            _xmlService.Initialize(message.NewCompetition, _apiService.GetCategories(), _apiService.GetClubs(),
                                    _apiService.GetLicensees(), _apiService.GetRaces(), _apiService.GetTeams());
            _xmlService.Save();
            _xmlService.Reset();
            _xmlService.Load();

            _messenger.Send(new IsBusyMessage(true, $"{ResourceManagerLocalizationService.Instance.GetString("EndLoadingInfo")}..."));

            _messenger.Send(new IsBusyMessage());
            _messenger.Send(new SnackMessage($"{ResourceManagerLocalizationService.Instance.GetString("DataLoadingInfo")} !"));

            CurrentViewModel = _homeGraphsViewModel;
            _homeGraphsViewModel.Update();
            IsLoaded = true;
        }
        else
        {
            //if selectnewcompetition has been canceled we clean the view
            CurrentViewModel = null;
        }
    }
}


