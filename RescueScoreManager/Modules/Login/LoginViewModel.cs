using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using RescueScoreManager.Messages;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Login;

public partial class LoginViewModel : ObservableObject
{
    private IMessenger Messenger;
    private readonly IAuthenticationService _authService;
    private string _username;
    private string _password;
    private bool _isLoading = false;
    private string _errorMessage;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }
    public bool? DialogResult { get; private set; }

    public event EventHandler RequestClose;
    public event EventHandler LoginSucceeded;


    public LoginViewModel(IAuthenticationService authenticationService, IMessenger messenger)
    {
        _authService = authenticationService;
        Messenger = messenger;
    }

    //[RelayCommand(CanExecute = nameof(CanValidate))]
    [RelayCommand]
    public async Task Validate(object param)
    {
        try
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            bool result = await _authService.LoginAsync(Username, Password);

            if (result)
            {
                // Connexion réussie, notifier la vue
                DialogResult = true;
                // Close the window
                if (param is Window window)
                {
                    window.DialogResult = true;
                    Messenger.Send(new LoginMessage(result));
                    window.Close();
                }
                //LoginSucceeded?.Invoke(this, EventArgs.Empty);
                //OnRequestClose();
            }
            else
            {
                ErrorMessage = $"{ResourceManagerLocalizationService.Instance.GetString("LoginError")}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{ResourceManagerLocalizationService.Instance.GetString("UnexpectedError")} : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            // Actualiser l'état du bouton de connexion
            //((AsyncRelayCommand)ValidateCommand).NotifyCanExecuteChanged();
        }
        //bool success = await WSIRestService.Instance.RequestToken(login, password);
        //TODO TO BE UPDATED
        //bool success = await WSIService.RequestToken(Login, Password);
        //OnRequestClose();
        //Messenger.Send(new LoginMessage(success));
    }

    [RelayCommand]
    public void Cancel(object param)
    {
        DialogResult = false;
        if (param is Window window)
        {
            window.DialogResult = false;
            Messenger.Send(new LoginMessage(false));
            window.Close();
        }
        OnRequestClose();
    }

    public void OnRequestClose()
    {
        if (RequestClose != null)
            RequestClose(this, EventArgs.Empty);
    }
}
