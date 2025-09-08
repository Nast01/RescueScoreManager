using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Extensions.DependencyInjection;

using RescueScoreManager.Modules.Planning.ViewModels;

namespace RescueScoreManager.Modules.Planning.Views
{
    public partial class PlanningProgramView : UserControl
    {
        private bool _isDragging;
        private object _draggedItem;

        public PlanningProgramView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnEventMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var border = sender as Border;
                _draggedItem = border?.DataContext;
                _isDragging = true;
                Mouse.Capture(border);
            }
        }

        private void OnEventMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
            {
                var border = sender as Border;
                if (border != null)
                {
                    Mouse.Capture(null);
                    _isDragging = false;

                    var dragData = new DataObject("PlanningEvent", _draggedItem);
                    DragDrop.DoDragDrop(border, dragData, DragDropEffects.Move);
                }
            }
            else if (e.LeftButton == MouseButtonState.Released)
            {
                _isDragging = false;
                Mouse.Capture(null);
            }
        }

        private void OnTimeSlotDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("PlanningEvent") || e.Data.GetDataPresent("PlannedEvent"))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void OnTimeSlotDrop(object sender, DragEventArgs e)
        {
            if (DataContext is PlanningProgramViewModel viewModel)
            {
                object timeSlot = ((Border)sender).DataContext;

                if (e.Data.GetDataPresent("PlanningEvent"))
                {
                    object droppedEvent = e.Data.GetData("PlanningEvent");
                    viewModel.MoveEventToTimeSlot(droppedEvent, timeSlot);
                    e.Handled = true;
                }
                else if (e.Data.GetDataPresent("PlannedEvent"))
                {
                    object plannedEvent = e.Data.GetData("PlannedEvent");
                    viewModel.MovePlannedEventToTimeSlot(plannedEvent, timeSlot);
                    e.Handled = true;
                }
            }
        }

        private void OnPlannedEventMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var border = sender as Border;
                _draggedItem = border?.DataContext;
                _isDragging = true;
                Mouse.Capture(border);
            }
        }

        private void OnPlannedEventMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
            {
                var border = sender as Border;
                if (border != null)
                {
                    Mouse.Capture(null);
                    _isDragging = false;

                    var dragData = new DataObject("PlannedEvent", _draggedItem);
                    DragDrop.DoDragDrop(border, dragData, DragDropEffects.Move);
                }
            }
            else if (e.LeftButton == MouseButtonState.Released)
            {
                _isDragging = false;
                Mouse.Capture(null);
            }
        }

        private void OnRemoveEventClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PlannedEventViewModel plannedEvent && 
                DataContext is PlanningProgramViewModel viewModel)
            {
                viewModel.RemoveEventFromTimeSlot(plannedEvent);
            }
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
    }
}
