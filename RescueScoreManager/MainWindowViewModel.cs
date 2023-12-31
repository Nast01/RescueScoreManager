using System.IO;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using RescueScoreManager.Data;
using RescueScoreManager.Home;
using RescueScoreManager.Messages;
using RescueScoreManager.Services;

namespace RescueScoreManager;

public partial class MainWindowViewModel : ObservableObject,
                                            IRecipient<LoginMessage>,
                                            IRecipient<SelectNewCompetitionMessage>,
                                            IRecipient<OpenCompetitionMessage>
{
    //private readonly RescueScoreManagerContext _context;

    //This is using the source generators from CommunityToolkit.Mvvm to generate a RelayCommand
    //See: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/generators/observableproperty
    //and: https://learn.microsoft.com/windows/communitytoolkit/mvvm/observableobject
    //[ObservableProperty]
    //[NotifyCanExecuteChangedFor(nameof(IncrementCountCommand))]
    //private int _count;

    private readonly HomeViewModel _homeViewModel;
    private readonly IWSIRestService _wsiService;
    private readonly IXMLService _xmlService;

    private IMessenger _messenger { get; }

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    [ObservableProperty]
    private string _connexionText = "Non Connecté";
    [ObservableProperty]
    private bool _isConnected = false;
    [ObservableProperty]
    private bool _isBusy = false;
    [ObservableProperty]
    private string _busyMessage = "";
    [ObservableProperty]
    private SnackbarMessageQueue _snackbarMessageQueue;
    [ObservableProperty]
    private string _title = "Rescue Score Manager";

    public MainWindowViewModel(HomeViewModel homeViewModel,
                                IWSIRestService wsiService,
                                IXMLService xmlService,
                                IMessenger messenger)//RescueScoreManagerContext context,
    {
        //_context = context;
        _homeViewModel = homeViewModel;
        _currentViewModel = homeViewModel;

        _wsiService = wsiService;
        _xmlService = xmlService;
        _messenger = messenger;
        _snackbarMessageQueue = new SnackbarMessageQueue();

        messenger.RegisterAll(this);

    }


    /// <summary>
    /// Message received when the a login has been validated
    /// </summary>
    /// <param name="message"></param>
    public async void Receive(LoginMessage message)
    {
        if (message.IsConnected == true)
        {
            ConnexionText = "Connecté";
            IsConnected = true;
        }
        else
        {
            ConnexionText = "Non Connecté";
            IsConnected = false;
        }
    }

    /// <summary>
    /// Message received when a Competition has been selected
    /// </summary>
    /// <param name="message"></param>
    public async void Receive(SelectNewCompetitionMessage message)
    {
        if (message.NewCompetition != null && IsConnected)
        {
            IsBusy = true;
            BusyMessage = "Chargement des données...";
            // load the data coming from the rest service
            await _wsiService.Load(message.NewCompetition);

            BusyMessage = "Création du fichier...";
            // set the path to the folder of the competition
            _xmlService.SetPath(message.NewCompetition.Name);

            // create the xml file and load it
            _xmlService.Initialize(message.NewCompetition, _wsiService.GetCategories(), _wsiService.GetClubs(),
                                    _wsiService.GetLicensees(), _wsiService.GetRaces(), _wsiService.GetTeams());
            _xmlService.Save();
            //_xmlService.Load();
            BusyMessage = "Fin du chargement...";

            IsBusy = false;
            BusyMessage = "";

            _title = _title + " " + message.NewCompetition.Name;
            SetSnackBarMessage( "Compétition récupérée!");
            #region entityframework
            //_context.FileName = message.NewCompetition.Name + ".ffss";
            //_context.DbPath = new FileInfo(Path.Join(dirPath, "RescueScore", dirName, message.NewCompetition.Name + ".ffss")); ;
            //_context.Database.Migrate();
            //Competition competition = message.NewCompetition;
            //if (_context.Competitions.Contains(message.NewCompetition) == false)
            //    _context.Competitions.Add(competition);

            //await _context.SaveChangesAsync();
            //if (_context.Competitions.Contains(message.NewCompetition) == false)
            //    _context.Categories.AddRange(_wsiService.GetCategories());

            //await _context.SaveChangesAsync();
            //foreach (var club in message.NewCompetition.Clubs)
            //{
            //    _context.Clubs.Add(club);
            //    _context.Licensees.AddRange(club.Licensees);
            //    foreach (Licensee licensee in club.Licensees)
            //    {
            //        if(licensee is Referee)
            //        {
            //            _context.RefereeDates.AddRange((licensee as Referee).RefereeAvailabilities);
            //        }
            //    }
            //}
            //await _context.SaveChangesAsync();
            //foreach (var race in message.NewCompetition.Races)
            //{
            //    _context.Races.Add(race);
            //    _context.Teams.AddRange(race.Teams);
            //}
            //await _context.SaveChangesAsync();


            //await _context.SaveChangesAsync();
            #endregion entityframework
        }
    }
    public async void Receive(OpenCompetitionMessage message)
    {
        _title = _title + " " + _xmlService.GetCompetition().Name;
        SetSnackBarMessage("Compétition chargée!");
    }

    private void SetSnackBarMessage(string message,int duration = 3)
    {
        _snackbarMessageQueue.Enqueue(content: message,
                                            actionContent: null,
                                            actionHandler: null,
                                            actionArgument: null,
                                            promote: false,
                                            neverConsiderToBeDuplicate: true,
                                            durationOverride: TimeSpan.FromSeconds(3));
    }
}
