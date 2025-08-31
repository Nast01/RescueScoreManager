using System.Windows;
using RescueScoreManager.Modules.Planning.ViewModels;

namespace RescueScoreManager.Modules.Planning.Views
{
    public partial class ManualTimeSlotDialog : Window
    {
        public ManualTimeSlotDialog()
        {
            InitializeComponent();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnCreate(object sender, RoutedEventArgs e)
        {
            if (DataContext is ManualTimeSlotDialogViewModel viewModel)
            {
                if (viewModel.ValidateAndCreate())
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Veuillez remplir tous les champs requis et sélectionner au moins un site.", 
                                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}