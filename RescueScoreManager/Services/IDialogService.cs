using RescueScoreManager.Login;

namespace RescueScoreManager.Services;

public interface IDialogService
{
    public bool? ShowLoginView(LoginViewModel loginViewModel);
}
