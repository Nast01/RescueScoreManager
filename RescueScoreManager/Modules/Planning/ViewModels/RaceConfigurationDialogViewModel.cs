using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class RaceConfigurationDialogViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;
        private readonly IXMLService _xmlService;

        [ObservableProperty]
        private int _selectedCategoryIndex;

        public ObservableCollection<CategoryConfigurationViewModel> CategoryConfigurations { get; }

        public ICommand AddPhaseCommand { get; }
        public ICommand SaveCommand { get; }

        public RaceConfigurationDialogViewModel(ILocalizationService localizationService, IXMLService xmlService, List<Race> races)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));

            CategoryConfigurations = new ObservableCollection<CategoryConfigurationViewModel>();
            AddPhaseCommand = new RelayCommand<CategoryConfigurationViewModel>(OnAddPhase);
            SaveCommand = new RelayCommand(OnSave);

            InitializeFromRaces(races);
        }

        private void InitializeFromRaces(List<Race> races)
        {
            var existingConfigs = _xmlService.GetRaceFormatConfigurations();

            foreach (var race in races)
            {
                foreach (var category in race.Categories)
                {
                    CategoryConfigurationViewModel categoryViewModel = null;
                    // Check if a configuration already exists
                    RaceFormatConfiguration ? raceFormatConfiguration = existingConfigs.ToList().Find(cfg => cfg.Categories.Any(cat => cat.Id == category.Id) && cfg.Discipline == race.Discipline && cfg.Gender == race.Gender);
                    if (raceFormatConfiguration != null)
                    {
                        categoryViewModel= new CategoryConfigurationViewModel
                        {
                            Name = category.Name,
                            ParticipantCount = race.GetAvailableTeams().Count,
                            IsEnabled = true,
                            Race = race
                        };

                        // Populate phases from existing configuration
                        foreach (RaceFormatDetail raceFormatDetail in raceFormatConfiguration.RaceFormatDetails)
                        {
                            categoryViewModel.Phases.Add(new PhaseViewModel
                            {
                                Order = raceFormatDetail.Order,
                                Name = raceFormatDetail.LevelLabel,
                                Level = raceFormatDetail.Level,
                                QualificationLogic = raceFormatDetail.QualificationMethod,
                                NumberOfRaces = raceFormatDetail.NumberOfRun,
                                PlacesPerRace = raceFormatDetail.SpotsPerRace,
                                QualifyingPlaces = raceFormatDetail.QualifyingSpots,
                                RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemovePhase)
                            });
                        }
                    }
                    else
                    {
                        categoryViewModel = new CategoryConfigurationViewModel
                        {
                            Name = category.Name,
                            ParticipantCount = race.GetAvailableTeams().Count,
                            IsEnabled = true,
                            Race = race,
                            Phases = new ObservableCollection<PhaseViewModel>()
                        };
                    }


                    CategoryConfigurations.Add(categoryViewModel);
                }
            }

            if (CategoryConfigurations.Any())
            {
                CategoryConfigurations.First().IsEnabled = true;
                SelectedCategoryIndex = 0;
            }
        }

        private void OnAddPhase(CategoryConfigurationViewModel? category)
        {
            if (category == null)
            {
                // Get currently selected category
                if (SelectedCategoryIndex >= 0 && SelectedCategoryIndex < CategoryConfigurations.Count)
                {
                    category = CategoryConfigurations[SelectedCategoryIndex];
                }
            }

            if (category != null)
            {
                var newPhase = new PhaseViewModel
                {
                    Order = category.Phases.Count + 1,
                    Name = _localizationService.GetString("Series") ?? "Series",
                    Level = EnumRSM.HeatLevel.Heat,
                    QualificationLogic = EnumRSM.QualificationType.Course,
                    NumberOfRaces = 1,
                    PlacesPerRace = 6,
                    QualifyingPlaces = 4,
                    RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemovePhase)
                };

                category.Phases.Add(newPhase);
            }
        }

        private void OnRemovePhase(PhaseViewModel? phase)
        {
            if (phase != null)
            {
                var category = CategoryConfigurations.FirstOrDefault(c => c.Phases.Contains(phase));
                if (category != null)
                {
                    category.Phases.Remove(phase);

                    // Update step numbers
                    for (int i = 0; i < category.Phases.Count; i++)
                    {
                        category.Phases[i].Order = i + 1;
                    }
                }
            }
        }

        private void OnSave()
        {
            try
            {
                var raceFormatConfigurations = new List<RaceFormatConfiguration>();

                foreach (var categoryConfig in CategoryConfigurations)
                {
                    var raceFormatConfig = ConvertToRaceFormatConfiguration(categoryConfig);
                    raceFormatConfigurations.Add(raceFormatConfig);
                }

                // Update XML service with new configurations
                _xmlService.UpdateRaceFormatConfigurations(raceFormatConfigurations);
                _xmlService.Save();
            }
            catch (Exception ex)
            {
                // Handle error - could use a dialog service here
                System.Windows.MessageBox.Show($"Error saving race configurations: {ex.Message}", "Save Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private RaceFormatConfiguration ConvertToRaceFormatConfiguration(CategoryConfigurationViewModel categoryConfig)
        {
            var race = categoryConfig.Race;

            // Create the RaceFormatConfiguration manually since it doesn't have a parameterless constructor
            var raceFormatConfig = new RaceFormatConfiguration();
            raceFormatConfig.Id = 0; // Always 0 as specified
            raceFormatConfig.Label = $"{race.Name} - {race.Gender}";
            raceFormatConfig.FullLabel = $"{race.Name} - {race.Gender} - {categoryConfig.Name}";
            raceFormatConfig.Gender = race.Gender;
            raceFormatConfig.Discipline = race.Discipline;
            raceFormatConfig.Categories = new List<Category>(race.Categories);
            raceFormatConfig.RaceFormatDetails = new List<RaceFormatDetail>();

            // Convert phases to RaceFormatDetails
            foreach (var phase in categoryConfig.Phases)
            {
                var raceFormatDetail = ConvertToRaceFormatDetail(phase, race, raceFormatConfig);
                raceFormatConfig.RaceFormatDetails.Add(raceFormatDetail);
            }

            return raceFormatConfig;
        }

        private RaceFormatDetail ConvertToRaceFormatDetail(PhaseViewModel phase, Race race, RaceFormatConfiguration parentConfig)
        {
            // Get the display name for the level
            string levelDisplayName = PhaseViewModel.HeatLevelOptions.FirstOrDefault(h => h.Value == phase.Level)?.DisplayName ?? phase.Level.ToString();

            // Get the display name for qualification method
            string qualificationDisplayName = PhaseViewModel.QualificationTypeOptions.FirstOrDefault(q => q.Value == phase.QualificationLogic)?.DisplayName ?? phase.QualificationLogic.ToString();

            return new RaceFormatDetail
            {
                Id = 0, // Always 0 as specified
                Order = phase.Order,
                Label = $"{levelDisplayName} - {race.Gender}",
                FullLabel = $"{race.Name} - {race.Gender} - {levelDisplayName} - {race.Gender}",
                LevelLabel = levelDisplayName,
                Level = phase.Level,
                NumberOfRun = phase.NumberOfRaces,
                QualificationMethod = phase.QualificationLogic,
                QualificationMethodLabel = qualificationDisplayName,
                SpotsPerRace = phase.PlacesPerRace,
                QualifyingSpots = phase.QualifyingPlaces,
                RaceFormatConfiguration = parentConfig
            };
        }
    }

    public partial class CategoryConfigurationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private int _participantCount;

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private Race _race;

        public ObservableCollection<PhaseViewModel> Phases { get; set; } = new();

        public string DisplayText => $"{Name} ({ParticipantCount} participants)";
    }

    public partial class PhaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _order;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private EnumRSM.HeatLevel _level = EnumRSM.HeatLevel.Heat;

        [ObservableProperty]
        private EnumRSM.QualificationType _qualificationLogic = EnumRSM.QualificationType.Course;

        [ObservableProperty]
        private int _numberOfRaces;

        [ObservableProperty]
        private int _placesPerRace;

        [ObservableProperty]
        private int _qualifyingPlaces;

        public ICommand RemoveCommand { get; set; } = null!;

        partial void OnLevelChanged(EnumRSM.HeatLevel value)
        {
            // Update the Name property to match the selected level's display name
            var selectedOption = HeatLevelOptions.FirstOrDefault(option => option.Value == value);
            if (selectedOption != null)
            {
                Name = selectedOption.DisplayName;
            }
        }

        public static List<HeatLevelOption> HeatLevelOptions => new()
        {
            new HeatLevelOption { Value = EnumRSM.HeatLevel.Heat, DisplayName = "Série" },
            new HeatLevelOption { Value = EnumRSM.HeatLevel.Quarter, DisplayName = "Quart de Finale" },
            new HeatLevelOption { Value = EnumRSM.HeatLevel.Semi, DisplayName = "Demi Finale" },
            new HeatLevelOption { Value = EnumRSM.HeatLevel.Final, DisplayName = "Finale" }
        };

        public static List<QualificationTypeOption> QualificationTypeOptions => new()
        {
            new QualificationTypeOption { Value = EnumRSM.QualificationType.Course, DisplayName = "Par course" },
            new QualificationTypeOption { Value = EnumRSM.QualificationType.Partie, DisplayName = "Toutes courses confondues" }
        };
    }

    public class HeatLevelOption
    {
        public EnumRSM.HeatLevel Value { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class QualificationTypeOption
    {
        public EnumRSM.QualificationType Value { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
