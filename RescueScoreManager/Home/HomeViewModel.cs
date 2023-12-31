using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Win32;

using RescueScoreManager.Data;
using RescueScoreManager.Login;
using RescueScoreManager.Messages;
using RescueScoreManager.SelectNewCompetition;
using RescueScoreManager.Services;

namespace RescueScoreManager.Home;

public partial class HomeViewModel : ObservableObject, IRecipient<LoginMessage>, IRecipient<SelectNewCompetitionMessage>
{
    //private RescueScoreManagerContext _context { get; }
    private LoginViewModel _loginViewModel { get; }
    private SelectNewCompetitionViewModel _selectNewCompetitionViewModel { get; }
    private HomeInformationsViewModel _homeGraphsViewModel { get; }
    private IDialogService _dialogService { get; }
    private IWSIRestService _wsiService { get; }
    private IXMLService _xmlService { get; }
    private IMessenger _messenger { get; }

    [ObservableProperty]
    private Competition? _competition = null;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(NewCompetitionCommand))]
    private bool _isLoaded = false;

    public HomeViewModel(LoginViewModel loginViewModel,
                            SelectNewCompetitionViewModel selectNewCompetitionViewModel,
                            HomeInformationsViewModel homeGraphsViewModel,
                            IDialogService dialogService,
                            IWSIRestService wsiService,
                            IXMLService xmlService,
                            IMessenger messenger)//RescueScoreManagerContext context,
    {
        //_context = context ?? throw new ArgumentNullException(nameof(context));
        _loginViewModel = loginViewModel ?? throw new ArgumentNullException(nameof(_loginViewModel));
        _selectNewCompetitionViewModel = selectNewCompetitionViewModel;
        _homeGraphsViewModel = homeGraphsViewModel;
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(_dialogService));
        _wsiService = wsiService;
        _xmlService = xmlService;

        _messenger = messenger;
        messenger.RegisterAll(this);
    }


    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    public void OpenFile()
    {
        if (GetFile() is { } file)
        {
            _messenger.Send(new IsBusyMessage(true, "Chargement des données..."));
            _xmlService.SetPath(file);
            _xmlService.Load();
            IsLoaded = true;

            _messenger.Send(new IsBusyMessage());
            _messenger.Send(new SnackMessage("Compétition chargée!"));

            CurrentViewModel = _homeGraphsViewModel;
            _homeGraphsViewModel.Update();
        }
    }

    private bool CanOpenFile() => IsLoaded == false;


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
        if (openFileDialog.ShowDialog() == true)
        {
            return new FileInfo(openFileDialog.FileName);
        }
        return null;
    }

    [RelayCommand(CanExecute = nameof(CanNewCompetition))]
    private void NewCompetition()
    {
        if (_wsiService.HasToken() == false)
            CurrentViewModel = _loginViewModel;
        //_dialogService.ShowLoginView(_loginViewModel);
        else
            CurrentViewModel = _selectNewCompetitionViewModel;
        //_dialogService.ShowSelectNewCompetition(_selectNewCompetitionViewModel);
    }

    private bool CanNewCompetition() => IsLoaded == false;

    public async void Receive(LoginMessage message)
    {
        if (message.IsConnected == true)
        {
            CurrentViewModel = _selectNewCompetitionViewModel;
            //_dialogService.ShowSelectNewCompetition(_selectNewCompetitionViewModel);
            //List<Competition> comps = await _wsiService.GetCompetitions(DateTime.Now);
            //comps = comps.OrderBy(c => c.BeginDate).ToList();
        }
        else
        {
            CurrentViewModel = null;
            IsLoaded = false;
        }
    }
    public async void Receive(SelectNewCompetitionMessage message)
    {
        if (message.NewCompetition != null)
        {
            _messenger.Send(new IsBusyMessage(true, "Chargement des données..."));

            // load the data coming from the rest service
            await _wsiService.Load(message.NewCompetition);

            _messenger.Send(new IsBusyMessage(true, "Création du fichier..."));
            // set the path to the folder of the competition
            _xmlService.SetPath(message.NewCompetition.Name);

            // create the xml file and load it
            _xmlService.Initialize(message.NewCompetition, _wsiService.GetCategories(), _wsiService.GetClubs(),
                                    _wsiService.GetLicensees(), _wsiService.GetRaces(), _wsiService.GetTeams());
            _xmlService.Save();
            _xmlService.Reset();
            _xmlService.Load();

            _messenger.Send(new IsBusyMessage(true, "Fin du chargement..."));

            _messenger.Send(new IsBusyMessage());
            _messenger.Send(new SnackMessage("Compétition récupérée!"));

            CurrentViewModel = _homeGraphsViewModel;
            _homeGraphsViewModel.Update();
            IsLoaded = true;
        }

    }
}


