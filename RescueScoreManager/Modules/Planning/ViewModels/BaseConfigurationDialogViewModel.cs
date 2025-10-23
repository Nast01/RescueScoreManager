using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using RescueScoreManager.Data;
using RescueScoreManager.Messages;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public abstract partial class BaseConfigurationDialogViewModel : ObservableObject
    {
        protected readonly ILocalizationService _localizationService;
        protected readonly IXMLService _xmlService;
        protected readonly IApiService _apiService;
        protected readonly IAuthenticationService _authService;
        protected readonly IMessenger _messenger;

        [ObservableProperty]
        private int _selectedCategoryIndex;

        public ObservableCollection<CategoryConfigurationViewModel> CategoryConfigurations { get; }

        public ICommand AddPhaseCommand { get; }
        public ICommand SaveCommand { get; }

        protected BaseConfigurationDialogViewModel(ILocalizationService localizationService,
                                                 IXMLService xmlService,
                                                 IApiService apiService,
                                                 IAuthenticationService authService,
                                                 IMessenger messenger,
                                                 List<Race> races)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

            CategoryConfigurations = new ObservableCollection<CategoryConfigurationViewModel>();
            AddPhaseCommand = new RelayCommand<CategoryConfigurationViewModel>(OnAddPhase);
            SaveCommand = new AsyncRelayCommand(OnSaveAsync);
            
            InitializeFromRaces(races);
        }

        protected virtual void InitializeFromRaces(List<Race> races)
        {
            IReadOnlyList<RaceFormatConfiguration> existingConfigs = _xmlService.GetRaceFormatConfigurations();

            foreach (Race race in races)
            {
                foreach (Category category in race.Categories)
                {
                    CategoryConfigurationViewModel? categoryViewModel = null;
                    // Check if a configuration already exists
                    RaceFormatConfiguration? raceFormatConfiguration = existingConfigs.ToList().Find(cfg => 
                        cfg.Categories.Any(cat => cat.Id == category.Id) && 
                        cfg.Discipline == race.Discipline && 
                        cfg.Gender == race.Gender);
                        
                    if (raceFormatConfiguration != null)
                    {
                        categoryViewModel = new CategoryConfigurationViewModel
                        {
                            Name = category.Name,
                            ParticipantCount = race.GetAvailableTeams().Count,
                            IsEnabled = true,
                            Race = race
                        };

                        // Populate phases from existing configuration
                        foreach (RaceFormatDetail raceFormatDetail in raceFormatConfiguration.RaceFormatDetails)
                        {
                            categoryViewModel.Phases.Add(CreatePhaseFromRaceFormatDetail(raceFormatDetail));
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

        protected virtual PhaseViewModel CreatePhaseFromRaceFormatDetail(RaceFormatDetail raceFormatDetail)
        {
            return new PhaseViewModel
            {
                Order = raceFormatDetail.Order,
                Name = raceFormatDetail.LevelLabel,
                Level = raceFormatDetail.Level,
                QualificationLogic = raceFormatDetail.QualificationMethod,
                NumberOfRaces = raceFormatDetail.NumberOfRun,
                PlacesPerRace = raceFormatDetail.SpotsPerRace,
                QualifyingPlaces = raceFormatDetail.QualifyingSpots,
                RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemovePhase)
            };
        }

        protected virtual void OnAddPhase(CategoryConfigurationViewModel? category)
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
                PhaseViewModel newPhase = CreateNewPhase(category);
                category.Phases.Add(newPhase);
            }
        }

        protected virtual PhaseViewModel CreateNewPhase(CategoryConfigurationViewModel category)
        {
            return new PhaseViewModel
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
        }

        protected virtual void OnRemovePhase(PhaseViewModel? phase)
        {
            if (phase != null)
            {
                CategoryConfigurationViewModel? category = CategoryConfigurations.FirstOrDefault(c => c.Phases.Contains(phase));
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

        protected virtual async Task OnSaveAsync()
        {
            try
            {
                List<RaceFormatConfiguration> raceFormatConfigurations = new List<RaceFormatConfiguration>();

                foreach (CategoryConfigurationViewModel categoryConfig in CategoryConfigurations)
                {
                    RaceFormatConfiguration raceFormatConfig = ConvertToRaceFormatConfiguration(categoryConfig);
                    raceFormatConfigurations.Add(raceFormatConfig);

                    await _apiService.SubmitRaceFormatConfigurationAsync(raceFormatConfig, _xmlService.GetCompetition()!, _authService.AuthenticationInfo);
                }

                // Update XML service with new configurations
                _xmlService.UpdateRaceFormatConfigurations(raceFormatConfigurations);
                _xmlService.Save();

                _messenger.Send(new SnackMessage(_localizationService.GetString("ConfigurationSaved")));
            }
            catch (Exception ex)
            {
                // Handle error - could use a dialog service here
                System.Windows.MessageBox.Show($"Error saving race configurations: {ex.Message}", "Save Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        protected virtual RaceFormatConfiguration ConvertToRaceFormatConfiguration(CategoryConfigurationViewModel categoryConfig)
        {
            Race race = categoryConfig.Race;

            // Create the RaceFormatConfiguration manually since it doesn't have a parameterless constructor
            RaceFormatConfiguration raceFormatConfig = new RaceFormatConfiguration
            {
                Id = 0, // Always 0 as specified
                Label = $"{race.Name} - {race.Gender}",
                FullLabel = $"{race.Name} - {race.Gender} - {categoryConfig.Name}",
                Gender = race.Gender,
                Discipline = race.Discipline,
                Categories = new List<Category>(race.Categories),
                RaceFormatDetails = new List<RaceFormatDetail>()
            };

            // Convert phases to RaceFormatDetails
            foreach (PhaseViewModel phase in categoryConfig.Phases)
            {
                RaceFormatDetail raceFormatDetail = ConvertToRaceFormatDetail(phase, race, raceFormatConfig);
                raceFormatConfig.RaceFormatDetails.Add(raceFormatDetail);
            }

            return raceFormatConfig;
        }

        protected virtual RaceFormatDetail ConvertToRaceFormatDetail(PhaseViewModel phase, Race race, RaceFormatConfiguration parentConfig)
        {
            // Get the display name for the level
            string levelDisplayName = PhaseViewModel.HeatLevelOptions.FirstOrDefault(h => h.Value == phase.Level)?.DisplayName ?? phase.Level.ToString();

            // Get the display name for qualification method
            string qualificationDisplayName = PhaseViewModel.QualificationTypeOptions.FirstOrDefault(q => q.Value == phase.QualificationLogic)?.DisplayName ?? phase.QualificationLogic.ToString();

            return new RaceFormatDetail
            {
                Id = 0, // Always 0 as specified
                Order = phase.Order,
                Label = $"{race.Name} - {race.Gender} - {levelDisplayName} - {string.Join(", ", race.Categories.Select(c => c.Name))}",
                FullLabel = $"{race.Name} - {race.Gender} - {levelDisplayName} - {string.Join(", ", race.Categories.Select(c => c.Name))}",
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
}