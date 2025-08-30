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
            try
            {
                // Always set the correct ViewModel (in case DataContext was inherited from parent)
                if (DataContext == null || DataContext.GetType().Name != nameof(PlanningStructureCompetitionViewModel))
                {
                    // Get the ViewModel from DI container
                    var viewModel = App.ServiceProvider?.GetService<PlanningStructureCompetitionViewModel>();
                    DataContext = viewModel;
                }
                
                // Refresh data when view is loaded
                if (DataContext is PlanningStructureCompetitionViewModel vm)
                {
                    vm.RefreshData();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the UI
                System.Diagnostics.Debug.WriteLine($"Error loading PlanningStructureCompetitionView: {ex.Message}");
            }
        }
    }
}
