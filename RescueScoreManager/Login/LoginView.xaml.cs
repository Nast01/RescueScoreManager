using System.Windows;

using RescueScoreManager.Services;

namespace RescueScoreManager.Login
{
    /// <summary>
    /// Logique d'interaction pour LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        public void CloseWindow()
        {
            this.Close();
        }
    }
}
