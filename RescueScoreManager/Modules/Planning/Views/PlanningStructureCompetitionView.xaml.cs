using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RescueScoreManager.Modules.Planning.ViewModels;

namespace RescueScoreManager.Modules.Planning.Views
{
    public partial class PlanningStructureCompetitionView : UserControl
    {
        public PlanningStructureCompetitionView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Always set the correct ViewModel (in case DataContext was inherited from parent)
            if (DataContext == null || DataContext.GetType().Name != nameof(PlanningStructureCompetitionViewModel))
            {
                // Get the ViewModel from DI container
                DataContext = App.ServiceProvider?.GetService<PlanningStructureCompetitionViewModel>();
            }
            
            // Refresh data when view is loaded
            if (DataContext is PlanningStructureCompetitionViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }
    }
}
