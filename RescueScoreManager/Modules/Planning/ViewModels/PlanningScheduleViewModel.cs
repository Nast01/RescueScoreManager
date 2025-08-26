using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public class PlanningScheduleViewModel : ObservableObject
    {
        private DateTime _startTime = DateTime.Now.AddHours(1);
        private int _sessionDuration = 120;
        private int _breakDuration = 15;

        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public int SessionDuration
        {
            get => _sessionDuration;
            set => SetProperty(ref _sessionDuration, value);
        }

        public int BreakDuration
        {
            get => _breakDuration;
            set => SetProperty(ref _breakDuration, value);
        }

        public ObservableCollection<ScheduleEventViewModel> ScheduledEvents { get; }

        public ICommand AutoScheduleCommand { get; }
        public ICommand ManualScheduleCommand { get; }

        public PlanningScheduleViewModel()
        {
            ScheduledEvents = new ObservableCollection<ScheduleEventViewModel>
            {
                new ScheduleEventViewModel { Name = "100m Nage Libre Hommes", StartTime = DateTime.Now.AddHours(1), Duration = 30 },
                new ScheduleEventViewModel { Name = "50m Nage Libre Femmes", StartTime = DateTime.Now.AddHours(1.5), Duration = 25 }
            };

            AutoScheduleCommand = new RelayCommand(OnAutoSchedule);
            ManualScheduleCommand = new RelayCommand(OnManualSchedule);
        }

        private void OnAutoSchedule()
        {
            // Auto-schedule logic
        }

        private void OnManualSchedule()
        {
            // Manual schedule logic
        }
    }

    public class ScheduleEventViewModel : ObservableObject
    {
        private string _name;
        private DateTime _startTime;
        private int _duration;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public int Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public DateTime EndTime => StartTime.AddMinutes(Duration);
    }
}
