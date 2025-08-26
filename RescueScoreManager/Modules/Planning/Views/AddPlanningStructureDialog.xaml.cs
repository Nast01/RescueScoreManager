using System.Windows;
using RescueScoreManager.Modules.Planning.ViewModels;

namespace RescueScoreManager.Modules.Planning.Views
{
    public partial class AddPlanningStructureDialog : Window
    {
        public AddPlanningStructureDialog()
        {
            InitializeComponent();
        }

        public AddPlanningStructureDialog(AddPlanningStructureDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.CloseRequested += (result) => 
            {
                DialogResult = result;
                Close();
            };
        }
    }
}