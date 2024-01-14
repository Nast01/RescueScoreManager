using System.Diagnostics;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Office.Interop.Word;
using Microsoft.Win32;

using RescueScoreManager.Data;
using RescueScoreManager.Login;
using RescueScoreManager.Messages;
using RescueScoreManager.SelectNewCompetition;
using RescueScoreManager.Services;

using Application = Microsoft.Office.Interop.Word.Application;

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
    private IExcelService _excelService { get; }
    private IMessenger _messenger { get; }

    [ObservableProperty]
    private Competition? _competition = null;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(NewCompetitionCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenStartListFileCommand))]
    private bool _isLoaded = false;

    public HomeViewModel(LoginViewModel loginViewModel,
                            SelectNewCompetitionViewModel selectNewCompetitionViewModel,
                            HomeInformationsViewModel homeGraphsViewModel,
                            IDialogService dialogService,
                            IWSIRestService wsiService,
                            IXMLService xmlService,
                            IExcelService excelService,
                            IMessenger messenger)//RescueScoreManagerContext context,
    {
        //_context = context ?? throw new ArgumentNullException(nameof(context));
        _loginViewModel = loginViewModel ?? throw new ArgumentNullException(nameof(_loginViewModel));
        _selectNewCompetitionViewModel = selectNewCompetitionViewModel;
        _homeGraphsViewModel = homeGraphsViewModel;
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(_dialogService));
        _wsiService = wsiService;
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
    #endregion OpenFile Command
    #region NewCompetition Command
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
    #endregion NewCompetition Command

    #region OpenDisqualicationFile Command
    [RelayCommand]
    private void OpenDisqualicationFile()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Assets\\Documents\\Disqualification.docx");
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Exists == false)
        {
            _messenger.Send(new SnackMessage("Aucun fichier de disqualification existant!"));
        }
        else
        {
            try
            {
                Process.Start("explorer.exe", path);

                _messenger.Send(new SnackMessage("Fichier ouvert!"));
            }
            catch (Exception e)
            {
                _messenger.Send(new SnackMessage("Problème lors de l'ouverture du fichier!"));
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
            string path = _excelService.GenerateStartList(_xmlService.GetCompetition(), _xmlService.GetRaces(),_xmlService.GetReferees());
            Process.Start("explorer.exe", path);
        }
        catch (Exception e)
        {
            _messenger.Send(new SnackMessage("Problème lors de l'ouverture du fichier!"));
        }
    }

    private bool CanOpenStartListFile() => IsLoaded == true;
    #endregion OpenStartListFile Command

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


