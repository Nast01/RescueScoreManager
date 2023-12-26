using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using RescueScoreManager.Messages;
using RescueScoreManager.Services;

namespace RescueScoreManager.Login;

public partial class LoginViewModel : ObservableObject
{
    private IMessenger Messenger;
    public IWSIRestService WSIService { get; }

    public string Login { get; set; } = "skr";
    public string Password { get; set; } = "skr123@";

    public event EventHandler RequestClose;

    public LoginViewModel(IWSIRestService wsiService, IMessenger messenger)
    {
        WSIService = wsiService;
        Messenger = messenger;
    }

    [RelayCommand(CanExecute = nameof(CanValidate))]
    private async Task Validate()
    {
        //bool success = await WSIRestService.Instance.RequestToken(login, password);
        bool success = await WSIService.RequestToken(Login, Password);
        OnRequestClose();
        Messenger.Send(new LoginMessage(success));
    }

    private bool CanValidate() => String.IsNullOrEmpty(Login) == false && String.IsNullOrEmpty(Password) == false;

    [RelayCommand]
    private void Cancel()
    {
        Messenger.Send(new LoginMessage(false));
        OnRequestClose();
    }

    public void OnRequestClose()
    {
        if (RequestClose != null)
            RequestClose(this, EventArgs.Empty);
    }
}
