using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using RescueScoreManager.Messages;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Login;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IMessenger _messenger;
    private readonly ILogger<LoginViewModel> _logger;


    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = "";

    public bool? DialogResult { get; private set; }

    public event EventHandler? RequestClose;
    public event EventHandler? LoginSucceeded;

    public LoginViewModel(IAuthenticationService authenticationService, IMessenger messenger, ILogger<LoginViewModel> logger)
    {
        _authService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    //[RelayCommand(CanExecute = nameof(CanValidate))]
    [RelayCommand]
    public async Task ValidateAsync(object param)
    {
        try
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            _logger.LogInformation("Attempting login for user: {Username}", Username);

            bool result = await _authService.LoginAsync(Username, Password);

            if (result)
            {
                DialogResult = true;
                _messenger.Send(new LoginMessage(true));

                _logger.LogInformation("Login successful for user: {Username}", Username);

                // Find and close the parent window
                if (Application.Current.MainWindow != null)
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.DataContext == this || window.Content is FrameworkElement fe && fe.DataContext == this)
                        {
                            window.DialogResult = true;
                            window.Close();
                            break;
                        }
                    }
                }

                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = ResourceManagerLocalizationService.Instance.GetString("LoginError") ?? "Login failed";
                _logger.LogWarning("Login failed for user: {Username}", Username);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{ResourceManagerLocalizationService.Instance.GetString("UnexpectedError")}: {ex.Message}";
            _logger.LogError(ex, "Unexpected error during login for user: {Username}", Username);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void Cancel(object param)
    {
        DialogResult = false;
        _messenger.Send(new LoginMessage(false));

        // Find and close the parent window
        if (Application.Current.MainWindow != null)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this || window.Content is FrameworkElement fe && fe.DataContext == this)
                {
                    window.DialogResult = false;
                    window.Close();
                    break;
                }
            }
        }

        OnRequestClose();
    }

    public void OnRequestClose()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
