using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Modules.Login;
using RescueScoreManager.Modules.SelectNewCompetition;


namespace RescueScoreManager.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DialogService> _logger;

    public DialogService(IServiceProvider serviceProvider, ILogger<DialogService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool? ShowLoginView(LoginViewModel viewModel)
    {
        try
        {
            _logger.LogInformation("Showing login dialog");

            var loginView = _serviceProvider.GetRequiredService<LoginView>();
            return ShowDialogInternal(loginView, viewModel, ResourceManagerLocalizationService.Instance.GetString("Login"), true,WindowStyle.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing login dialog");
            return false;
        }
    }

    public bool? ShowSelectNewCompetition(SelectNewCompetitionViewModel viewModel)
    {
        try
        {
            _logger.LogInformation("Showing select new competition dialog");

            var selectCompetitionView = _serviceProvider.GetRequiredService<SelectNewCompetitionView>();
            return ShowDialogInternal(selectCompetitionView, viewModel, ResourceManagerLocalizationService.Instance.GetString("SelectCompetition"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing select competition dialog");
            return false;
        }
    }

    public bool? ShowDialog<TView, TViewModel>(TViewModel viewModel, bool isModal = true)
        where TView : class, new()
        where TViewModel : class
    {
        try
        {
            _logger.LogInformation("Showing generic dialog for {ViewType}", typeof(TView).Name);

            var view = _serviceProvider.GetService<TView>() ?? new TView();
            return ShowDialogInternal(view, viewModel, typeof(TView).Name, isModal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing generic dialog for {ViewType}", typeof(TView).Name);
            return false;
        }
    }

    public void ShowMessage(string title, string message)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    public bool ShowConfirmation(string title, string message)
    {
        bool result = false;
        Application.Current.Dispatcher.Invoke(() =>
        {
            result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        });
        return result;
    }

    private bool? ShowDialogInternal<TView, TViewModel>(TView view, TViewModel viewModel, string dialogTitle, bool isModal = true, WindowStyle windowStyle = WindowStyle.SingleBorderWindow)
        where TView : class
        where TViewModel : class
    {
        Window dialog;

        if (view is Window windowView)
        {
            // If the view is already a Window, use it directly
            dialog = windowView;
            dialog.DataContext = viewModel;
            dialog.Owner = Application.Current.MainWindow;
            
            // Set the dialog window as a parameter for commands that need it
            dialog.Tag = dialog;
            
            // If the view model has a DialogWindow property, set it
            if (viewModel?.GetType().GetProperty("DialogWindow") != null)
            {
                viewModel.GetType().GetProperty("DialogWindow")?.SetValue(viewModel, dialog);
            }
        }
        else if (view is FrameworkElement frameworkElement)
        {
            // If the view is not a Window, wrap it in a new Window
            dialog = new Window
            {
                Title = dialogTitle,
                Content = frameworkElement,
                DataContext = viewModel,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = windowStyle
            };

            // Set the dialog window as a parameter for commands that need it
            if (frameworkElement is UserControl userControl)
            {
                userControl.Tag = dialog;
            }
        }
        else
        {
            _logger.LogError("View {ViewType} is not a FrameworkElement or Window", typeof(TView).Name);
            return false;
        }

        _logger.LogInformation("Showing {DialogTitle} dialog (Modal: {IsModal})", dialogTitle, isModal);

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
}