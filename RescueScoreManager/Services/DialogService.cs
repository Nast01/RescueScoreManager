using MaterialDesignThemes.Wpf;

using RescueScoreManager.Login;

namespace RescueScoreManager.Services;

public class DialogService : IDialogService
{
    public bool? ShowLoginView(LoginViewModel loginViewModel)
    {
        var loginView = new LoginView();
        loginView.DataContext = loginViewModel;
        loginViewModel.RequestClose += delegate (object sender, EventArgs args) { loginView.CloseWindow(); };
        return loginView.ShowDialog() == true;
    }
}
