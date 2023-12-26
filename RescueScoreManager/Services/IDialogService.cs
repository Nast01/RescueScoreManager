using RescueScoreManager.Login;
using RescueScoreManager.SelectNewCompetition;

namespace RescueScoreManager.Services;

public interface IDialogService
{
    public bool? ShowLoginView(LoginViewModel viewModel);
    public bool? ShowSelectNewCompetition(SelectNewCompetitionViewModel viewModel);
}
