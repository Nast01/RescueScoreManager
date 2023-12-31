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
                                            IRecipient<IsBusyMessage>,
                                            IRecipient<SnackMessage>
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

    public async void Receive(IsBusyMessage message)
    {
        IsBusy = message.IsBusy;
        BusyMessage = message.Text;
    }
    public async void Receive(SnackMessage message)
    {
        SetSnackBarMessage(message.Text);
    }

    public async void Receive(OpenCompetitionMessage message)
    {
        Title = Title + " " + _xmlService.GetCompetition().Name;
        SetSnackBarMessage("Compétition chargée!");
    }

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
}
