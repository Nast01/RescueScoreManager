using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

using RescueScoreManager.Data;
using RescueScoreManager.Modules.Login;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class SwimConfigurationDialogViewModel : BaseConfigurationDialogViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private int _selectedPhaseTypeIndex;

        [ObservableProperty]
        private bool _isFinalsTabVisible;

        public ObservableCollection<PhaseViewModel> SeriesPhases { get; }
        public ObservableCollection<PhaseViewModel> FinalsPhases { get; }
        public ObservableCollection<string> SeriesCategories { get; }
        public ObservableCollection<string> FinalsCategories { get; }

        public ICommand AddSeriesPhaseCommand { get; }
        public ICommand AddFinalsPhaseCommand { get; }

        private List<Category> _allCategories = new List<Category>();

        public SwimConfigurationDialogViewModel(ILocalizationService localizationService,
                                               IXMLService xmlService,
                                               IApiService apiService,
                                               IAuthenticationService authService,
                                               IMessenger messenger,
                                               IDialogService dialogService,
                                               IServiceProvider serviceProvider,
                                               List<Race> races)
            : base(localizationService, xmlService, apiService, authService, messenger, races)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            SeriesPhases = new ObservableCollection<PhaseViewModel>();
            FinalsPhases = new ObservableCollection<PhaseViewModel>();
            SeriesCategories = new ObservableCollection<string>();
            FinalsCategories = new ObservableCollection<string>();

            AddSeriesPhaseCommand = new RelayCommand(OnAddSeriesPhase);
            AddFinalsPhaseCommand = new RelayCommand(OnAddFinalsPhase);

            InitializeSwimConfiguration(races);
        }

        private void InitializeSwimConfiguration(List<Race> races)
        {
            AppSetting? appSetting = _xmlService.GetSetting();
            
            // Set finals tab visibility based on app settings
            IsFinalsTabVisible = appSetting?.IsRankingByFinal == true;

            // Collect all categories
            _allCategories = races.SelectMany(r => r.Categories.Select(c => c)).Distinct().ToList();
            
            foreach (Category category in _allCategories)
            {
                SeriesCategories.Add(category.Name);
                FinalsCategories.Add(category.Name);
            }

            // Check if discipline is already configured
            IReadOnlyList<RaceFormatConfiguration> existingConfigs = _xmlService.GetRaceFormatConfigurations();
            bool isDisciplineConfigured = false;
            
            if (appSetting?.IsRankingByTime == true)
            {
                // Check if any existing configuration matches the current races
                foreach (Race race in races)
                {
                    RaceFormatConfiguration? existingConfig = existingConfigs.FirstOrDefault(cfg => 
                        cfg.Discipline == race.Discipline && 
                        cfg.Gender == race.Gender &&
                        cfg.Categories.Any(cat => race.Categories.Any(raceCat => raceCat.Id == cat.Id)));
                        
                    if (existingConfig != null)
                    {
                        isDisciplineConfigured = true;
                        LoadExistingConfiguration(existingConfig);
                        break;
                    }
                }
                
                // If no existing configuration found, create default series phase
                if (!isDisciplineConfigured)
                {
                    CreateDefaultSeriesPhase(races, appSetting);
                }
            }
        }

        private void LoadExistingConfiguration(RaceFormatConfiguration existingConfig)
        {
            // Load existing phases into appropriate collections
            foreach (RaceFormatDetail detail in existingConfig.RaceFormatDetails.OrderBy(d => d.Order))
            {
                PhaseViewModel phase = CreatePhaseViewModelFromDetail(detail);
                
                // Determine if this is a series or finals phase based on level
                if (detail.Level == EnumRSM.HeatLevel.Heat)
                {
                    phase.RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemoveSeriesPhase);
                    SeriesPhases.Add(phase);
                }
                else if (detail.Level == EnumRSM.HeatLevel.Final || detail.Level == EnumRSM.HeatLevel.Semi || detail.Level == EnumRSM.HeatLevel.Quarter)
                {
                    phase.RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemoveFinalsPhase);
                    FinalsPhases.Add(phase);
                }
            }
        }

        private PhaseViewModel CreatePhaseViewModelFromDetail(RaceFormatDetail raceFormatDetail)
        {
            return new PhaseViewModel
            {
                Order = raceFormatDetail.Order,
                Name = raceFormatDetail.LevelLabel,
                Level = raceFormatDetail.Level,
                QualificationLogic = raceFormatDetail.QualificationMethod,
                NumberOfRaces = raceFormatDetail.NumberOfRun,
                PlacesPerRace = raceFormatDetail.SpotsPerRace,
                QualifyingPlaces = raceFormatDetail.QualifyingSpots
            };
        }

        private void CreateDefaultSeriesPhase(List<Race> races, AppSetting appSetting)
        {
            // Calculate total teams across all races
            int totalTeams = races.Sum(race => race.GetAvailableTeams().Count);
            int numberOfLanes = appSetting.NumberOfLanes;
            int numberOfRaces = (int)Math.Ceiling((double)totalTeams / numberOfLanes);

            PhaseViewModel defaultSeriesPhase = new PhaseViewModel
            {
                Order = 1,
                Name = _localizationService.GetString("Series") ?? "Série",
                Level = EnumRSM.HeatLevel.Heat,
                QualificationLogic = EnumRSM.QualificationType.Partie, // toutes courses confondues.
                NumberOfRaces = numberOfRaces,
                PlacesPerRace = numberOfLanes,
                QualifyingPlaces = 0, // No qualifying places for series
                RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemoveSeriesPhase)
            };

            SeriesPhases.Add(defaultSeriesPhase);
        }

        private void OnAddSeriesPhase()
        {
            AppSetting? appSetting = _xmlService.GetSetting();
            int numberOfLanes = appSetting?.NumberOfLanes ?? 8;

            PhaseViewModel newPhase = new PhaseViewModel
            {
                Order = SeriesPhases.Count + 1,
                Name = _localizationService.GetString("Series") ?? "Série",
                Level = EnumRSM.HeatLevel.Heat,
                QualificationLogic = EnumRSM.QualificationType.NA, // Empty for series
                NumberOfRaces = 1,
                PlacesPerRace = numberOfLanes,
                QualifyingPlaces = 0,
                RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemoveSeriesPhase)
            };

            SeriesPhases.Add(newPhase);
        }

        private void OnAddFinalsPhase()
        {
            AppSetting? appSetting = _xmlService.GetSetting();
            int numberOfLanes = appSetting?.NumberOfLanes ?? 8;

            PhaseViewModel newPhase = new PhaseViewModel
            {
                Order = FinalsPhases.Count + 1,
                Name = _localizationService.GetString("Final") ?? "Finale",
                Level = EnumRSM.HeatLevel.Final,
                QualificationLogic = EnumRSM.QualificationType.Course,
                NumberOfRaces = 1,
                PlacesPerRace = numberOfLanes,
                QualifyingPlaces = numberOfLanes,
                RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemoveFinalsPhase)
            };

            FinalsPhases.Add(newPhase);
        }

        private void OnRemoveSeriesPhase(PhaseViewModel? phase)
        {
            if (phase != null && SeriesPhases.Contains(phase))
            {
                SeriesPhases.Remove(phase);
                
                // Update order numbers
                for (int i = 0; i < SeriesPhases.Count; i++)
                {
                    SeriesPhases[i].Order = i + 1;
                }
            }
        }

        private void OnRemoveFinalsPhase(PhaseViewModel? phase)
        {
            if (phase != null && FinalsPhases.Contains(phase))
            {
                FinalsPhases.Remove(phase);
                
                // Update order numbers
                for (int i = 0; i < FinalsPhases.Count; i++)
                {
                    FinalsPhases[i].Order = i + 1;
                }
            }
        }

        protected override PhaseViewModel CreateNewPhase(CategoryConfigurationViewModel category)
        {
            AppSetting? appSetting = _xmlService.GetSetting();
            int numberOfLanes = appSetting?.NumberOfLanes ?? 8;

            // Swimming-specific default values
            return new PhaseViewModel
            {
                Order = category.Phases.Count + 1,
                Name = _localizationService.GetString("Series") ?? "Series",
                Level = EnumRSM.HeatLevel.Heat,
                QualificationLogic = EnumRSM.QualificationType.Course,
                NumberOfRaces = 1,
                PlacesPerRace = numberOfLanes,
                QualifyingPlaces = 0,
                RemoveCommand = new RelayCommand<PhaseViewModel>(OnRemovePhase)
            };
        }

        private async Task<bool> EnsureAuthenticatedAsync()
        {
            // Check if already authenticated
            if (_authService.IsAuthenticated)
            {
                // Validate token is still working
                bool isValid = await _authService.ValidateAndRefreshTokenAsync();
                if (isValid)
                {
                    return true;
                }
            }

            // Show login dialog
            LoginViewModel loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            bool? loginResult = _dialogService.ShowLoginView(loginViewModel);

            return loginResult == true && _authService.IsAuthenticated;
        }

        protected override async Task OnSaveAsync()
        {
            try
            {
                List<RaceFormatConfiguration> raceFormatConfigurations = new List<RaceFormatConfiguration>();
                bool hasApiCalls = false;

                // Create RaceFormatConfiguration for each race, combining series and finals phases
                foreach (var raceGroup in CategoryConfigurations.GroupBy(cc => new { cc.Race.Name, cc.Race.Gender, cc.Race.Discipline }))
                {
                    Race race = raceGroup.First().Race;
                    var allPhases = new List<PhaseViewModel>();
                    
                    // Add all series phases
                    allPhases.AddRange(SeriesPhases);
                    // Add all finals phases
                    allPhases.AddRange(FinalsPhases);

                    if (allPhases.Any())
                    {
                        string categoryNames = $"({string.Join(", ", _allCategories.OrderBy(c => c.AgeMin).Select(c => c.Name))})";

                        RaceFormatConfiguration raceFormatConfig = new RaceFormatConfiguration
                        {
                            Id = 0,
                            Label = $"{race.Name} {categoryNames}",
                            FullLabel = $"{race.Name} {categoryNames}",
                            Gender = race.Gender,
                            Discipline = race.Discipline,
                            Categories = _allCategories,
                            RaceFormatDetails = new List<RaceFormatDetail>()
                        };

                        // Convert phases to RaceFormatDetails
                        foreach (PhaseViewModel phase in allPhases.OrderBy(p => p.Order))
                        {
                            RaceFormatDetail raceFormatDetail = ConvertToRaceFormatDetail(phase, race, raceFormatConfig);
                            raceFormatConfig.RaceFormatDetails.Add(raceFormatDetail);
                        }

                        raceFormatConfigurations.Add(raceFormatConfig);

                        // TODO - Check authentication before making API call
                        //if (!hasApiCalls)
                        //{
                        //    bool isAuthenticated = await EnsureAuthenticatedAsync();
                        //    if (!isAuthenticated)
                        //    {
                        //        // User chose not to authenticate, but continue with local save
                        //        _messenger.Send(new Messages.SnackMessage(_localizationService.GetString("ConfigurationSaved") ?? "Configuration saved"));
                        //        break;
                        //    }
                        //    hasApiCalls = true;
                        //}

                        // TODO - Submit to API only if authenticated
                        //await _apiService.SubmitRaceFormatConfigurationAsync(raceFormatConfig, _xmlService.GetCompetition()!, _authService.AuthenticationInfo);
                    }
                }

                // Update XML service with new configurations
                _xmlService.UpdateRaceFormatConfigurations(raceFormatConfigurations);
                _xmlService.Save();

                _messenger.Send(new Messages.SnackMessage(_localizationService.GetString("ConfigurationSaved") ?? "Configuration saved locally"));
            }
            catch (Exception ex)
            {
                // Handle error - could use a dialog service here
                System.Windows.MessageBox.Show($"Error saving race configurations: {ex.Message}", "Save Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
