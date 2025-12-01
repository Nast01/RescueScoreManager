using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using RescueScoreManager.Modules.Planning.ViewModels;
using RescueScoreManager.Data;

namespace RescueScoreManager.Modules.Planning.Views
{
    public partial class PlanningProgramView : UserControl
    {
        public PlanningProgramView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // Always set the correct ViewModel (in case DataContext was inherited from parent)
                if (DataContext == null || DataContext.GetType().Name != nameof(PlanningProgramViewModel))
                {
                    // Get the ViewModel from DI container
                    var viewModel = App.ServiceProvider?.GetService<PlanningProgramViewModel>();
                    DataContext = viewModel;
                }

                // Refresh data when view is loaded
                if (DataContext is PlanningProgramViewModel vm)
                {
                    vm.RefreshData();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the UI
                System.Diagnostics.Debug.WriteLine($"Error loading PlanningProgramView: {ex.Message}");
            }
        }

        private void EventCanvas_DragOver(object sender, DragEventArgs e)
        {
            // Allow drop if data is available
            if (e.Data.GetDataPresent("RaceFormatDetail"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void EventCanvas_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent("RaceFormatDetail") && DataContext is PlanningProgramViewModel vm)
                {
                    var raceFormatDetail = e.Data.GetData("RaceFormatDetail");
                    var position = e.GetPosition(sender as UIElement);
                    
                    // Convert position to time slot
                    var timeSlot = ConvertPositionToTime(position);
                    
                    // Handle the drop in the view model
                    vm.HandleEventDrop(raceFormatDetail, timeSlot);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling drop: {ex.Message}");
            }
        }

        private DateTime ConvertPositionToTime(Point position)
        {
            // Convert canvas position to time
            // This would need to be implemented based on the timeline scale
            // For now, return a default time
            var baseTime = DateTime.Today.AddHours(8); // 8 AM start
            var hoursFromPosition = position.Y / 60; // Assuming 60 pixels per hour
            return baseTime.AddHours(hoursFromPosition);
        }

        #region Drag and Drop for RaceFormats

        private Point _startPoint;
        private bool _isDragging;

        private void RaceFormat_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void RaceFormat_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point currentPosition = e.GetPosition(null);
                
                if (Math.Abs(currentPosition.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    
                    if (sender is FrameworkElement element && element.DataContext is RaceFormatDetail raceFormatDetail)
                    {
                        var dataObject = new DataObject("RaceFormatDetail", raceFormatDetail);
                        DragDrop.DoDragDrop(element, dataObject, DragDropEffects.Copy);
                    }
                    
                    _isDragging = false;
                }
            }
        }

        private void ProgramMeeting_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("RaceFormatDetail"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ProgramMeeting_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent("RaceFormatDetail") && 
                    DataContext is PlanningProgramViewModel vm &&
                    sender is FrameworkElement element &&
                    element.DataContext is ProgramMeeting programMeeting)
                {
                    var raceFormatDetail = e.Data.GetData("RaceFormatDetail") as RaceFormatDetail;
                    if (raceFormatDetail != null)
                    {
                        // Handle the drop in the view model
                        vm.HandleRaceFormatDrop(raceFormatDetail, programMeeting);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling race format drop: {ex.Message}");
            }
        }

        #endregion
    }
}