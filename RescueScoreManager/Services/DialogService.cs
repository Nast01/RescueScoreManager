using MaterialDesignThemes.Wpf;

using RescueScoreManager.Login;
using RescueScoreManager.SelectNewCompetition;

namespace RescueScoreManager.Services;

public class DialogService : IDialogService
{
    public bool? ShowLoginView(LoginViewModel viewModel)
    {
        var view = new LoginView();
        view.DataContext = viewModel;
        //viewModel.RequestClose += delegate (object sender, EventArgs args) { view.CloseWindow(); };
        return null;// view.ShowDialog() == true;
    }

    public bool? ShowSelectNewCompetition(SelectNewCompetitionViewModel viewModel)
    {
        var view = new SelectNewCompetitionView();
        //view.DataContext = viewModel;
        //EventHandler value = delegate (object sender, EventArgs args) { view.CloseWindow(); };
        //viewModel.RequestClose += value;
        return null;// view.ShowDialog() == true;
    }
}
