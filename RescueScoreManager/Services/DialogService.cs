using System.Windows.Controls;
using System.Windows;

using MaterialDesignThemes.Wpf;

using RescueScoreManager.Modules.Login;
using RescueScoreManager.Modules.SelectNewCompetition;

namespace RescueScoreManager.Services;

public class DialogService : IDialogService
{
    public bool? ShowLoginView(LoginViewModel viewModel)
    {
        LoginView view = new()
        {
            DataContext = viewModel
        };
        //viewModel.RequestClose += delegate (object sender, EventArgs args) { view.CloseWindow(); };
        return null;// view.ShowDialog() == true;
    }

    public bool? ShowSelectNewCompetition(SelectNewCompetitionViewModel viewModel)
    {
        SelectNewCompetitionView view = new();
        //view.DataContext = viewModel;
        //EventHandler value = delegate (object sender, EventArgs args) { view.CloseWindow(); };
        //viewModel.RequestClose += value;
        return null;// view.ShowDialog() == true;
    }

    public bool? ShowDialog<T>(T viewModel,bool isModal = true) where T : class
    {
        var dialog = new Window
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.CanResizeWithGrip,
        };

        var type = viewModel.GetType();
        var viewTypeName = type.Name.Replace("ViewModel", "View");
        var viewType = type.Assembly.GetTypes().FirstOrDefault(t => t.Name == viewTypeName);

        if (viewType != null)
        {
            var view = Activator.CreateInstance(viewType) as UserControl;
            view.DataContext = viewModel;
            dialog.Content = view;

            // Set CommandParameter to dialog window for both modal and non-modal
            if (view is FrameworkElement frameworkElement)
            {
                frameworkElement.Tag = dialog;
            }

            if (isModal)
            {
                return dialog.ShowDialog();
            }
            else
            {
                dialog.Show();
                return null;
            }
        }

        return null;
    }
}
