using DocumentFormat.OpenXml.Drawing;

using RescueScoreManager.Modules.Login;
using RescueScoreManager.Modules.SelectNewCompetition;

namespace RescueScoreManager.Services;

public interface IDialogService
{
    public bool? ShowLoginView(LoginViewModel viewModel);
    public bool? ShowSelectNewCompetition(SelectNewCompetitionViewModel viewModel);

    public bool? ShowDialog<T>(T viewModel, bool isModal = true) where T : class;
}
