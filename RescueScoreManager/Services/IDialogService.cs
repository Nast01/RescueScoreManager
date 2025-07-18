using Microsoft.Extensions.Logging;

using RescueScoreManager.Modules.Login;
using RescueScoreManager.Modules.SelectNewCompetition;

namespace RescueScoreManager.Services;

public interface IDialogService
{
    bool? ShowLoginView(LoginViewModel viewModel);
    bool? ShowSelectNewCompetition(SelectNewCompetitionViewModel viewModel);
    bool? ShowDialog<TView, TViewModel>(TViewModel viewModel, bool isModal = true)
        where TView : class, new()
        where TViewModel : class;
    void ShowMessage(string title, string message);
    bool ShowConfirmation(string title, string message);
}
