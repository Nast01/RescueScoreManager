using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class ManualTimeSlotDialogViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private int _duration = 15;

        [ObservableProperty]
        private DateOption? _selectedDate;

        [ObservableProperty]
        private TimeSlotOption? _selectedBeginTimeSlot;

        public ObservableCollection<SiteSelectionViewModel> AvailableSites { get; }
        public ObservableCollection<DateOption> AvailableDates { get; }
        public ObservableCollection<TimeSlotOption> AvailableTimeSlots { get; }

        public DateTime CurrentDate { get; private set; }
        public PlannedEventViewModel? CreatedEvent { get; private set; }

        public ManualTimeSlotDialogViewModel(IXMLService xmlService, DateTime currentDate)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            CurrentDate = currentDate;

            AvailableSites = new ObservableCollection<SiteSelectionViewModel>();
            AvailableDates = new ObservableCollection<DateOption>();
            AvailableTimeSlots = new ObservableCollection<TimeSlotOption>();

            Initialize();
        }

        private void Initialize()
        {
            LoadAvailableSites();
            LoadAvailableDates();
            LoadAvailableTimeSlots();

            // Set default selections
            SelectedDate = AvailableDates.FirstOrDefault(d => d.Date.Date == CurrentDate.Date);
        }

        private void LoadAvailableSites()
        {
            AvailableSites.Clear();
            var sites = _xmlService.GetSites();

            foreach (var site in sites)
            {
                AvailableSites.Add(new SiteSelectionViewModel
                {
                    Id = site.Id,
                    Name = site.Name,
                    IsSelected = false
                });
            }
        }

        private void LoadAvailableDates()
        {
            AvailableDates.Clear();
            var competition = _xmlService.GetCompetition();

            if (competition != null)
            {
                for (var date = competition.BeginDate.Date; date <= competition.EndDate.Date; date = date.AddDays(1))
                {
                    AvailableDates.Add(new DateOption
                    {
                        Date = date,
                        DisplayText = GetDateDisplayText(date)
                    });
                }
            }
        }

        private void LoadAvailableTimeSlots()
        {
            AvailableTimeSlots.Clear();
            var startTime = new TimeSpan(7, 0, 0);

            for (int i = 0; i < 51; i++)
            {
                var currentTime = startTime.Add(TimeSpan.FromMinutes(i * 15));
                AvailableTimeSlots.Add(new TimeSlotOption
                {
                    Time = currentTime.ToString(@"hh\:mm"),
                    TimeSpan = currentTime
                });
            }

            // Set default to current time or nearest available slot
            var currentTimeSpan = DateTime.Now.TimeOfDay;
            SelectedBeginTimeSlot = AvailableTimeSlots.FirstOrDefault(ts => ts.TimeSpan >= currentTimeSpan) 
                                   ?? AvailableTimeSlots.FirstOrDefault();
        }

        private string GetDateDisplayText(DateTime date)
        {
            return date switch
            {
                var d when d.Date == DateTime.Today => $"Aujourd'hui - {GetDayName(d.DayOfWeek)} {d.Day} {GetMonthName(d.Month)}",
                var d when d.Date == DateTime.Today.AddDays(1) => $"Demain - {GetDayName(d.DayOfWeek)} {d.Day} {GetMonthName(d.Month)}",
                var d => $"{GetDayName(d.DayOfWeek)} {d.Day} {GetMonthName(d.Month)} {d.Year}"
            };
        }

        private string GetDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Lundi",
                DayOfWeek.Tuesday => "Mardi",
                DayOfWeek.Wednesday => "Mercredi",
                DayOfWeek.Thursday => "Jeudi",
                DayOfWeek.Friday => "Vendredi",
                DayOfWeek.Saturday => "Samedi",
                DayOfWeek.Sunday => "Dimanche",
                _ => dayOfWeek.ToString()
            };
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Janvier",
                2 => "Février",
                3 => "Mars",
                4 => "Avril",
                5 => "Mai",
                6 => "Juin",
                7 => "Juillet",
                8 => "Août",
                9 => "Septembre",
                10 => "Octobre",
                11 => "Novembre",
                12 => "Décembre",
                _ => month.ToString()
            };
        }

        public bool ValidateAndCreate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }

            if (SelectedDate == null || SelectedBeginTimeSlot == null)
            {
                return false;
            }

            var selectedSites = AvailableSites.Where(s => s.IsSelected).ToList();
            if (!selectedSites.Any())
            {
                return false;
            }

            // Create the planned event
            CreatedEvent = new PlannedEventViewModel
            {
                Id = GetNextEventId(),
                Title = Name,
                Subtitle = "Événement manuel",
                Color = "#10B981", // Green color for manual events
                Duration = Duration,
                ConfigurationLabel = "Manuel",
                StatusColor = "#10B981",
                ParticipantCount = 0
            };

            return true;
        }

        private int GetNextEventId()
        {
            // Generate a unique ID for manual events (using negative numbers to avoid conflicts)
            var existingIds = _xmlService.GetRaceFormatConfigurations()
                .SelectMany(c => c.RaceFormatDetails)
                .Select(d => d.Id)
                .ToList();

            int nextId = -1;
            while (existingIds.Contains(nextId))
            {
                nextId--;
            }
            return nextId;
        }

        public List<SiteSelectionViewModel> GetSelectedSites()
        {
            return AvailableSites.Where(s => s.IsSelected).ToList();
        }

        public DateTime GetSelectedDateTime()
        {
            if (SelectedDate != null && SelectedBeginTimeSlot != null)
            {
                return SelectedDate.Date.Add(SelectedBeginTimeSlot.TimeSpan);
            }
            return CurrentDate;
        }
    }

    public partial class SiteSelectionViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [ObservableProperty]
        private bool _isSelected;
    }

    public class DateOption
    {
        public DateTime Date { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    public class TimeSlotOption
    {
        public string Time { get; set; } = string.Empty;
        public TimeSpan TimeSpan { get; set; }
    }
}
