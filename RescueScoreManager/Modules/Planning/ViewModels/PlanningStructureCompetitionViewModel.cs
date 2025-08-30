using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using RescueScoreManager.Services;
using RescueScoreManager.Modules.Planning.Views;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class PlanningStructureCompetitionViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<PlanningStructureCompetitionViewModel> _logger;

        [ObservableProperty]
        private int _totalRaces = 18;

        [ObservableProperty]
        private int _configuredEvents;

        [ObservableProperty]
        private int _totalParticipants;

        [ObservableProperty]
        private string _progressionPercentage = "67%";

        [ObservableProperty]
        private bool _showAddEventCard = true;

        public ObservableCollection<RaceCardViewModel> RaceCards { get; }

        public ICommand NouvelleEpreuveCommand { get; }
        public ICommand SauvegarderCommand { get; }
        public ICommand OpenRaceConfigurationCommand { get; }

        public PlanningStructureCompetitionViewModel(
            IXMLService xmlService,
            ILocalizationService localizationService,
            IDialogService dialogService,
            ILogger<PlanningStructureCompetitionViewModel> logger)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RaceCards = new ObservableCollection<RaceCardViewModel>();
            
            NouvelleEpreuveCommand = new RelayCommand(OnNouvelleEpreuve);
            SauvegarderCommand = new RelayCommand(OnSauvegarder);
            OpenRaceConfigurationCommand = new RelayCommand<RaceCardViewModel>(OnOpenRaceConfiguration);

            Initialize();
        }

        private void Initialize()
        {
            RaceCards.Clear();

            try
            {
                var races = _xmlService.GetRaces();
                
                // Create dictionary with DisciplineLabel as key and List<Race> as value
                var racesByDiscipline = new Dictionary<string, List<Race>>();
                
                foreach (var race in races)
                {
                    string disciplineLabel = race.DisciplineLabel;
                    
                    if (!racesByDiscipline.ContainsKey(disciplineLabel))
                    {
                        racesByDiscipline[disciplineLabel] = new List<Race>();
                    }
                    
                    racesByDiscipline[disciplineLabel].Add(race);
                }

                // Create RaceCardViewModel for each dictionary entry (one per discipline)
                foreach (var kvp in racesByDiscipline.OrderBy(x => x.Key))
                {
                    string disciplineLabel = kvp.Key;
                    var racesInDiscipline = kvp.Value;
                    
                    // Create a combined RaceCardViewModel representing all races in this discipline
                    var raceCard = CreateDisciplineRaceCardViewModel(disciplineLabel, racesInDiscipline);
                    RaceCards.Add(raceCard);
                }

                // Update statistics
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing race data");
            }
        }

        private RaceCardViewModel CreateDisciplineRaceCardViewModel(string disciplineLabel, List<Race> races)
        {
            // Use the first race to determine common properties for the discipline
            var firstRace = races.First();
            
            // Determine icon and color based on speciality
            string icon = firstRace.Speciality switch
            {
                EnumRSM.Speciality.EauPlate => "🏊",
                EnumRSM.Speciality.Cotier => "🏄",
                _ => "🏆"
            };

            string iconColor = firstRace.Speciality switch
            {
                EnumRSM.Speciality.EauPlate => "#3B82F6",
                EnumRSM.Speciality.Cotier => "#F59E0B",
                EnumRSM.Speciality.Mixte => "#8B5CF6",
                _ => "#6B7280"
            };

            // Create list of distinct categories from all races in this discipline
            var categories = races
                .SelectMany(r => r.Categories)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            // Calculate aggregate statistics for all races in this discipline
            int totalTeams = races.Sum(r => r.Teams.Count);
            int configuredRaces = races.Count(r => r.Teams.Any());
            double progress = races.Count > 0 ? (configuredRaces * 100.0 / races.Count) : 0;

            // Determine status based on configuration level
            string statusText;
            string statusColor;
            
            if (configuredRaces == races.Count && races.Count > 0)
            {
                statusText = _localizationService.GetString("Configure");
                statusColor = "#10B981";
            }
            else if (configuredRaces > 0)
            {
                statusText = _localizationService.GetString("EnCours");
                statusColor = "#F59E0B";
            }
            else
            {
                statusText = _localizationService.GetString("AConfigurer");
                statusColor = "#6B7280";
                progress = 25.0; // Minimum progress for unconfigured
            }



            return new RaceCardViewModel
            {
                Name = disciplineLabel,
                Type = GetDisciplineTypeDescription(races),
                Icon = icon,
                IconColor = iconColor,
                StatusText = statusText,
                StatusColor = statusColor,
                Progress = progress,
                ProgressText = $"{progress:F0}%",
                Schedule = _localizationService.GetString("NonPlanifie"),
                Categories = categories,
                Races = races,
                ParticipantCount = totalTeams
            };
        }

        private string GetDisciplineTypeDescription(List<Race> races)
        {
            var firstRace = races.First();
            string specialityText = firstRace.Speciality switch
            {
                EnumRSM.Speciality.EauPlate => _localizationService.GetString("EauPlate") ?? "Swimming",
                EnumRSM.Speciality.Cotier => _localizationService.GetString("Cotier") ?? "Cotier",
                EnumRSM.Speciality.Mixte => _localizationService.GetString("Mixte") ?? "Mixte",
                _ => ""
            };

            // Check if this discipline has both individual and relay races
            bool hasRelay = races.Any(r => r.IsRelay);
            bool hasIndividual = races.Any(r => !r.IsRelay);

            if (hasRelay)
            {
                return $"{specialityText} - {_localizationService.GetString("EpreuveParEquipes") ?? "Team Event"}";
            }
            else
            {
                return $"{specialityText} - {_localizationService.GetString("EpreuveIndividuelle") ?? "Individual Event"}";
            }
        }

        private string GetDisciplineCategoryDescription(List<Race> races)
        {
            // Get all unique categories from all races in this discipline
            var allCategories = races
                .SelectMany(r => r.Categories)
                .Select(c => c.Name)
                .Distinct()
                .OrderBy(name => name);

            if (allCategories.Any())
            {
                return string.Join(", ", allCategories);
            }
            return _localizationService.GetString("ADefinir") ?? "To be defined";
        }

        private void UpdateStatistics()
        {
            TotalRaces = RaceCards.Count;
            ConfiguredEvents = RaceCards.Count(rc => rc.Progress >= 100);
            TotalParticipants = RaceCards.Sum(rc => rc.ParticipantCount);
            
            double averageProgress = RaceCards.Any() ? RaceCards.Average(rc => rc.Progress) : 0;
            ProgressionPercentage = $"{averageProgress:F0}%";
        }

        public void RefreshData()
        {
            Initialize();
        }

        //private void InitializeSampleData()
        //{
        //    // Sample data based on the image
        //    Events.Clear();
            
        //    Events.Add(new EpreuveCardViewModel
        //    {
        //        Name = "100m Nage Libre H",
        //        Type = _localizationService.GetString("EpreuveIndividuelle"),
        //        Icon = "🏊",
        //        IconColor = "#3B82F6",
        //        StatusText = _localizationService.GetString("Configure"),
        //        StatusColor = "#10B981",
        //        Progress = 100,
        //        ProgressText = "100%",
        //        Schedule = _localizationService.GetString("Samedi") + " 10h00",
        //        Category = _localizationService.GetString("BassinA"),
        //        ParticipantCount = 32
        //    });

        //    Events.Add(new EpreuveCardViewModel
        //    {
        //        Name = "200m SM Dames",
        //        Type = _localizationService.GetString("SauvetageAquatique"),
        //        Icon = "🏊",
        //        IconColor = "#8B5CF6",
        //        StatusText = _localizationService.GetString("Configure"),
        //        StatusColor = "#10B981",
        //        Progress = 85,
        //        ProgressText = "85%",
        //        Schedule = _localizationService.GetString("Samedi") + " 14h30",
        //        Category = _localizationService.GetString("Plage"),
        //        ParticipantCount = 16
        //    });

        //    Events.Add(new EpreuveCardViewModel
        //    {
        //        Name = "90m Sprint Messieurs",
        //        Type = _localizationService.GetString("Sprint") + " " + _localizationService.GetString("Ocean"),
        //        Icon = "🏃",
        //        IconColor = "#F59E0B",
        //        StatusText = _localizationService.GetString("EnCours"),
        //        StatusColor = "#F59E0B",
        //        Progress = 60,
        //        ProgressText = "60%",
        //        Schedule = _localizationService.GetString("Dimanche") + " 09h00",
        //        Category = _localizationService.GetString("Plage"),
        //        ParticipantCount = 18
        //    });

        //    Events.Add(new EpreuveCardViewModel
        //    {
        //        Name = "Paddle Board H",
        //        Type = _localizationService.GetString("EpreuveTechnique"),
        //        Icon = "🏄",
        //        IconColor = "#10B981",
        //        StatusText = _localizationService.GetString("Configure"),
        //        StatusColor = "#10B981",
        //        Progress = 50,
        //        ProgressText = "50%",
        //        Schedule = _localizationService.GetString("Dimanche") + " 11h30",
        //        Category = _localizationService.GetString("Plage") + " - " + _localizationService.GetString("Zone") + " B",
        //        ParticipantCount = 12
        //    });

        //    Events.Add(new EpreuveCardViewModel
        //    {
        //        Name = "4x50m Relais Mixte",
        //        Type = _localizationService.GetString("EpreuveParEquipes"),
        //        Icon = "👥",
        //        IconColor = "#EC4899",
        //        StatusText = _localizationService.GetString("AConfigurer"),
        //        StatusColor = "#6B7280",
        //        Progress = 25,
        //        ProgressText = "25%",
        //        Schedule = _localizationService.GetString("NonPlanifie"),
        //        Category = _localizationService.GetString("ADefinir"),
        //        ParticipantCount = 8
        //    });

        //    // Update statistics
        //    TotalRaces = Events.Count;
        //    ConfiguredEvents = Events.Count(e => e.Progress >= 100);
        //    TotalParticipants = Events.Sum(e => e.ParticipantCount);
            
        //    var totalProgress = Events.Average(e => e.Progress);
        //    ProgressionPercentage = $"{totalProgress:F0}%";
        //}

        private void OnNouvelleEpreuve()
        {
            try
            {
                _logger.LogInformation("Creating new event");
                // TODO: Open dialog to create new event
                _dialogService.ShowMessage(
                    _localizationService.GetString("NouvelleEpreuve"),
                    _localizationService.GetString("FonctionnaliteEnDeveloppement"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new event");
            }
        }

        private void OnSauvegarder()
        {
            try
            {
                _logger.LogInformation("Saving events configuration");
                // TODO: Implement save functionality
                _dialogService.ShowMessage(
                    _localizationService.GetString("Sauvegarder"),
                    _localizationService.GetString("ConfigurationSauvegardee"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration");
            }
        }

        private void OnOpenRaceConfiguration(RaceCardViewModel raceCard)
        {
            try
            {
                if (raceCard != null)
                {
                    _logger.LogInformation($"Opening race configuration for: {raceCard.Name}");
                    
                    string dialogTitle = _localizationService.GetString("ConfigurationPhasesTitle") ?? "Configuration des phases";
                    string fullTitle = $"{dialogTitle} - {raceCard.Name}";
                    
                    var dialog = new RaceConfigurationDialog(fullTitle, raceCard.Races);
                    bool? result = dialog.ShowDialog();
                    dialog.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening race configuration");
            }
        }
    }

    public class RaceCardViewModel : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = "#6B7280";
        public string StatusText { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "#6B7280";
        public double Progress { get; set; }
        public string ProgressText { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public List<Category> Categories { get; set; } = new();
        public List<Race> Races { get; set; } = new();
        public int ParticipantCount { get; set; }

        public string CategoryDescription => Categories?.Any() == true 
            ? string.Join(", ", Categories.Select(c => c.Name).OrderBy(name => name))
            : "À définir";
    }
}
