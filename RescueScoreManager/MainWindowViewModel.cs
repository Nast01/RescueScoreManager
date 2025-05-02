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
    private readonly IApiService _apiService; 
    private readonly IAuthenticationService _authService;
    private readonly IXMLService _xmlService; 
    private readonly ILocalizationService _localizationService;
    private readonly IDialogService _dialogService;

    private IMessenger _messenger { get; }

    [ObservableProperty]
    private ObservableObject _currentViewModel;
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
    private string _userLabel;
    [ObservableProperty]
    private string _userRole;
    [ObservableProperty]
    private string _userType;

    public ObservableCollection<LanguageModel> AvailableLanguages => _localizationService.AvailableLanguages;

    public MainWindowViewModel(HomeViewModel homeViewModel,
                                IApiService apiService,
                                IAuthenticationService authService,
                                IXMLService xmlService,
                                ILocalizationService localizationService,
                                IDialogService dialogService,
                                IMessenger messenger)//RescueScoreManagerContext context,
    {
        //_context = context;
        _homeViewModel = homeViewModel;
        _currentViewModel = homeViewModel;

        _apiService = apiService;
        _authService = authService;
        _xmlService = xmlService;
        _localizationService = localizationService;
        _dialogService = dialogService;


        _messenger = messenger;
        _snackbarMessageQueue = new SnackbarMessageQueue();

        _currentLanguage = AvailableLanguages[0];

        UpdateLogInfo();

        messenger.RegisterAll(this);

        // Check user connected
        CheckExistingAuthAsync();
    }

    #region Commands

    #region Logout Command
    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();
        UpdateLogInfo();
    }

    #endregion Logout Command
    #region Login Command
    [RelayCommand]
    private void Login()
    {
        var loginViewModel = new LoginViewModel(_authService,_messenger);
        bool? result = _dialogService.ShowDialog(loginViewModel);

        if (result == true)
        {
            UpdateLogInfo();
        }
        loginViewModel.OnRequestClose();

    }

    #endregion Login Command
    #endregion Commands

    #region Private functions
    private async void CheckExistingAuthAsync()
    {
        bool isValid = await _authService.ValidateAndRefreshTokenAsync();

        if (isValid)
        {
            OnLoginSucceeded();
        }
    }
    private void OnLoginSucceeded()
    {
        // Mettre à jour l'interface après connexion
        UpdateLogInfo();

        // Ici, vous pourriez charger un autre ViewModel pour afficher le contenu après connexion
        // Par exemple : CurrentViewModel = new DashboardViewModel();
    }

    private void UpdateLogInfo()
    {
        IsLoggedIn = _authService.IsAuthenticated;
        UserLabel = _authService.CurrentUser?.Label ?? $"{ResourceManagerLocalizationService.Instance.GetString("NotConnected")}";
        UserRole = _authService.CurrentUser?.Role ?? "";
        UserType = _authService.CurrentUser?.Type ?? "";
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
    }

    public void Receive(IsBusyMessage message)
    {
        IsBusy = message.IsBusy;
        BusyMessage = message.Text;
    }
    public void Receive(SnackMessage message)
    {
        SetSnackBarMessage(message.Text);
    }

    public void Receive(OpenCompetitionMessage message)
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
    #endregion Message
}
