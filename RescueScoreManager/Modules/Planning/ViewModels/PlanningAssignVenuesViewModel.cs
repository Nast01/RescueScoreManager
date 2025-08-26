using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public class PlanningAssignVenuesViewModel : ObservableObject
    {
        public ObservableCollection<VenueViewModel> Venues { get; }
        public ObservableCollection<EventAssignmentViewModel> EventAssignments { get; }

        public ICommand AddVenueCommand { get; }
        public ICommand RemoveVenueCommand { get; }
        public ICommand AssignEventCommand { get; }

        public PlanningAssignVenuesViewModel()
        {
            Venues = new ObservableCollection<VenueViewModel>
            {
                new VenueViewModel { Name = "Bassin Principal", Capacity = 8, Type = "Bassin 50m" },
                new VenueViewModel { Name = "Bassin d'Échauffement", Capacity = 6, Type = "Bassin 25m" },
                new VenueViewModel { Name = "Zone Technique", Capacity = 50, Type = "Espace" }
            };

            EventAssignments = new ObservableCollection<EventAssignmentViewModel>
            {
                new EventAssignmentViewModel { EventName = "100m Nage Libre Hommes", AssignedVenue = "Bassin Principal" },
                new EventAssignmentViewModel { EventName = "50m Nage Libre Femmes", AssignedVenue = "Bassin Principal" }
            };

            AddVenueCommand = new RelayCommand(OnAddVenue);
            RemoveVenueCommand = new RelayCommand<VenueViewModel>(OnRemoveVenue);
            AssignEventCommand = new RelayCommand<EventAssignmentViewModel>(OnAssignEvent);
        }

        private void OnAddVenue()
        {
            Venues.Add(new VenueViewModel { Name = "Nouveau lieu", Capacity = 10, Type = "Espace" });
        }

        private void OnRemoveVenue(VenueViewModel venue)
        {
            Venues.Remove(venue);
        }

        private void OnAssignEvent(EventAssignmentViewModel assignment)
        {
            // Assignment logic
        }
    }

    public class VenueViewModel : ObservableObject
    {
        private string _name;
        private int _capacity;
        private string _type;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Capacity
        {
            get => _capacity;
            set => SetProperty(ref _capacity, value);
        }

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
    }

    public class EventAssignmentViewModel : ObservableObject
    {
        private string _eventName;
        private string _assignedVenue;

        public string EventName
        {
            get => _eventName;
            set => SetProperty(ref _eventName, value);
        }

        public string AssignedVenue
        {
            get => _assignedVenue;
            set => SetProperty(ref _assignedVenue, value);
        }
    }
}
