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
        private int _totalRaces = 1;

        [ObservableProperty]
        private int _configuredEvents;

        [ObservableProperty]
        private int _totalParticipants;

        [ObservableProperty]
        private string _progressionPercentage = "0%";

        [ObservableProperty]
        private bool _showAddEventCard = true;

        [ObservableProperty]
        private string _filterText = string.Empty;

        public ObservableCollection<RaceCardViewModel> RaceCards { get; }
        public ObservableCollection<RaceCardViewModel> FilteredRaceCards { get; }

        public ICommand NouvelleEpreuveCommand { get; }
        public ICommand SaveCommand { get; }
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
            FilteredRaceCards = new ObservableCollection<RaceCardViewModel>();
            
            NouvelleEpreuveCommand = new RelayCommand(OnNouvelleEpreuve);
            SaveCommand = new RelayCommand(OnSave);
            OpenRaceConfigurationCommand = new RelayCommand<RaceCardViewModel>(OnOpenRaceConfiguration);

            Initialize();
        }

        private void Initialize()
        {
            RaceCards.Clear();

            try
            {
                IReadOnlyList<Race> races = _xmlService.GetRaces();
                IReadOnlyList<RaceFormatConfiguration> raceFormatConfigurations = _xmlService.GetRaceFormatConfigurations();

                // Create dictionary with DisciplineLabel as key and List<Race> as value
                Dictionary<string, List<Race>> racesByDiscipline = new Dictionary<string, List<Race>>();
                
                foreach (Race race in races)
                {
                    string disciplineLabel = race.DisciplineLabel;
                    
                    if (!racesByDiscipline.TryGetValue(disciplineLabel, out List<Race>? value))
                    {
                        value = new List<Race>();
                        racesByDiscipline[disciplineLabel] = value;
                    }

                    value.Add(race);
                }

                // Create RaceCardViewModel for each dictionary entry (one per discipline)
                foreach (KeyValuePair<string, List<Race>> kvp in racesByDiscipline.OrderBy(x => x.Key))
                {
                    string disciplineLabel = kvp.Key;
                    List<Race> racesInDiscipline = kvp.Value;

                    // Create a combined RaceCardViewModel representing all races in this discipline
                    RaceCardViewModel raceCard = CreateDisciplineRaceCardViewModel(disciplineLabel, racesInDiscipline, raceFormatConfigurations);
                    RaceCards.Add(raceCard);
                }

                // Update statistics
                UpdateStatistics();
                
                // Apply initial filter
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing race data");
            }
        }

        private RaceCardViewModel CreateDisciplineRaceCardViewModel(string disciplineLabel, List<Race> races, IReadOnlyList<RaceFormatConfiguration> raceFormatConfigurations)
        {
            // Use the first race to determine common properties for the discipline
            Race firstRace = races.First();
            
            
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
            List<Category> categories = races
                .SelectMany(r => r.Categories)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            // Calculate aggregate statistics for all races in this discipline
            int totalTeams = races.Sum(r => r.Teams.Count);
            
            // A race is configured if it has a corresponding RaceFormatConfiguration
            int configuredRaces = races.Count(race => 
                raceFormatConfigurations.Any(config => 
                    config.Discipline == race.Discipline && 
                    config.Gender == race.Gender &&
                    config.Categories.Any(cat => race.Categories.Any(raceCat => raceCat.Id == cat.Id))
                )
            );
            
            double progress = races.Count > 0 ? (configuredRaces * 100.0 / races.Count) : 0;


            // Determine status based on configuration level
            string statusText;
            string statusColor;
            
            if (configuredRaces == races.Count && races.Count > 0)
            {
                statusText = _localizationService.GetString("Configure") ?? "Configuré";
                statusColor = "#10B981";
            }
            else if (configuredRaces > 0)
            {
                statusText = _localizationService.GetString("EnCours") ?? "En cours";
                statusColor = "#F59E0B";
            }
            else
            {
                statusText = _localizationService.GetString("AConfigurer") ?? "À configurer";
                statusColor = "#6B7280";
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
            Race firstRace = races.First();
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

            return hasRelay
                ? $"{specialityText} - {_localizationService.GetString("EpreuveParEquipes") ?? "Team Event"}"
                : $"{specialityText} - {_localizationService.GetString("EpreuveIndividuelle") ?? "Individual Event"}";
        }

        private void UpdateStatistics()
        {
            TotalRaces = RaceCards.Count;
            ConfiguredEvents = RaceCards.Count(rc => rc.Progress == 100);
            TotalParticipants = RaceCards.Sum(rc => rc.ParticipantCount);
            
            double averageProgress = RaceCards.Any() ? RaceCards.Average(rc => rc.Progress) : 0;
            ProgressionPercentage = $"{averageProgress:F0}%";
        }

        public void RefreshData()
        {
            Initialize();
        }

        partial void OnFilterTextChanged(string value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredRaceCards.Clear();

            if (string.IsNullOrWhiteSpace(FilterText))
            {
                // Show all race cards when filter is empty
                foreach (var raceCard in RaceCards)
                {
                    FilteredRaceCards.Add(raceCard);
                }
            }
            else
            {
                // Filter race cards by name (case insensitive)
                var filteredCards = RaceCards.Where(rc => 
                    rc.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                    rc.Type.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

                foreach (var raceCard in filteredCards)
                {
                    FilteredRaceCards.Add(raceCard);
                }
            }
        }

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

        private void OnSave()
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
                    string message = $"Opening race configuration for: {raceCard.Name}";
                    _logger.LogInformation(message);
                    
                    string dialogTitle = _localizationService.GetString("ConfigurationPhasesTitle") ?? "Configuration des phases";
                    string fullTitle = $"{dialogTitle} - {raceCard.Name}";

                    RaceConfigurationDialog dialog = new RaceConfigurationDialog(fullTitle, raceCard.Races);
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
