using System.Windows;
using System.Windows.Controls;

using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Login
{
    /// <summary>
    /// Logique d'interaction pour LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            //if (this.DataContext is LoginViewModel viewModel)
            //{
            //    // Get the parent window
            //    Window parentWindow = Window.GetWindow(this);
            //    if (parentWindow != null)
            //    {
            //        // First handle authentication through the view model
            //        viewModel.Validate();

            //        // Then close the window
            //        if (viewModel.DialogResult == true)
            //        {
            //            parentWindow.DialogResult = true;

            //            viewModel.OnRequestClose();
            //            parentWindow.Close();
            //        }
            //    }
            //}
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //if (this.DataContext is LoginViewModel viewModel)
            //{
            //    // Get the parent window
            //    Window parentWindow = Window.GetWindow(this);
            //    if (parentWindow != null)
            //    {
            //        // Set result to false
            //        viewModel.Cancel();

            //        // Close the window
            //        parentWindow.DialogResult = false;
            //        parentWindow.Close();
            //    }
            //}
        }

        //public void CloseWindow()
        //{
        //    this.Close();
        //}
    }
}
