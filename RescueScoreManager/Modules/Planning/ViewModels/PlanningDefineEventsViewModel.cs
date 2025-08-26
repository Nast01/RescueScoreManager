using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public class PlanningDefineEventsViewModel : ObservableObject
    {
        private string _competitionName;
        private DateTime _competitionDate = DateTime.Now;

        public string CompetitionName
        {
            get => _competitionName;
            set => SetProperty(ref _competitionName, value);
        }

        public DateTime CompetitionDate
        {
            get => _competitionDate;
            set => SetProperty(ref _competitionDate, value);
        }

        public ObservableCollection<EventDefinitionViewModel> Events { get; }

        public ICommand AddEventCommand { get; }
        public ICommand RemoveEventCommand { get; }

        public PlanningDefineEventsViewModel()
        {
            Events = new ObservableCollection<EventDefinitionViewModel>
            {
                new EventDefinitionViewModel { Name = "50m Nage Libre", Category = "Individuel", IsSelected = true },
                new EventDefinitionViewModel { Name = "100m Nage Libre", Category = "Individuel", IsSelected = true },
                new EventDefinitionViewModel { Name = "200m Nage Libre", Category = "Individuel", IsSelected = false },
                new EventDefinitionViewModel { Name = "4x50m Nage Libre", Category = "Relais", IsSelected = false }
            };

            AddEventCommand = new RelayCommand(OnAddEvent);
            RemoveEventCommand = new RelayCommand<EventDefinitionViewModel>(OnRemoveEvent);
        }

        private void OnAddEvent()
        {
            Events.Add(new EventDefinitionViewModel { Name = "Nouvelle épreuve", Category = "Individuel" });
        }

        private void OnRemoveEvent(EventDefinitionViewModel eventToRemove)
        {
            Events.Remove(eventToRemove);
        }
    }

    public class EventDefinitionViewModel : ObservableObject
    {
        private string _name;
        private string _category;
        private bool _isSelected;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
