using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using RescueScoreManager.Modules.Forfeit;
using RescueScoreManager.Modules.Planning.Views;
using RescueScoreManager.Services;
using RescueScoreManager.Data;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public class PlanningViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<PlanningViewModel> _logger;

        private int _currentStep = 2;
        private int _totalSteps = 5;
        private string _title;
        private int _totalRaces = 0;
        private int _configuredRaces = 0;
        private int _totalAthletes = 0;

        public int CurrentStep
        {
            get => _currentStep;
            set 
            {
                if (SetProperty(ref _currentStep, value))
                {
                    UpdateCurrentStep();
                }
            }
        }

        public int TotalSteps
        {
            get => _totalSteps;
            set => SetProperty(ref _totalSteps, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public int TotalRaces
        {
            get => _totalRaces;
            set => SetProperty(ref _totalRaces, value);
        }

        public int ConfiguredRaces
        {
            get => _configuredRaces;
            set => SetProperty(ref _configuredRaces, value);
        }

        public int TotalAthletes
        {
            get => _totalAthletes;
            set => SetProperty(ref _totalAthletes, value);
        }

        public ObservableCollection<PlanningStepViewModel> Steps { get; }

        public PlanningStepViewModel SelectedStep { get; set; }

        public ICommand StepSelectedCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand PlanificationCommand { get; }

        public PlanningViewModel(
        IXMLService xmlService,
        ILocalizationService localizationService,
        IDialogService dialogService,
        IServiceProvider serviceProvider,
        ILogger<PlanningViewModel> logger)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _title = _localizationService.GetString("PlanningTitle");

            Steps = new ObservableCollection<PlanningStepViewModel>
            {
                new PlanningStepViewModel(1, _localizationService.GetString("StructureCompetition"), _localizationService.GetString("SeriesPhases"), false),
                new PlanningStepViewModel(2, _localizationService.GetString("PlanSchedules"), _localizationService.GetString("DatesTimes"), false),
                new PlanningStepViewModel(3, _localizationService.GetString("FinalValidation"), _localizationService.GetString("ReviewExport"), false)
            };

            SelectedStep = Steps[1];
            UpdateCurrentStep();
            
            StepSelectedCommand = new RelayCommand<PlanningStepViewModel>(OnStepSelected);
            GenerateCommand = new RelayCommand(OnGenerate);
            PreviousCommand = new RelayCommand(OnPrevious);
            PlanificationCommand = new RelayCommand(OnPlanification);
        }

        private void UpdateCurrentStep()
        {
            foreach (var step in Steps)
            {
                step.IsCurrentStep = step.StepNumber == CurrentStep;
            }
        }

        #region initialization
        public async Task InitializeAsync()
        {
            try
            {
                TotalRaces = _xmlService.GetRaces().Count;
                ConfiguredRaces = _xmlService.GetRaceFormatConfigurations().Count;
                TotalAthletes = _xmlService.GetAthletes().Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Planning");
            }
            finally
            {
            }
        }

        #endregion initialization

        private void OnStepSelected(PlanningStepViewModel step)
        {
            if (step != null)
            {
                SelectedStep = step;
                CurrentStep = step.StepNumber;
                OnPropertyChanged(nameof(SelectedStep));
            }
        }

        private async void OnGenerate()
        {
            try
            {
                _logger.LogInformation("Opening Add Planning Structure dialog");

                // Create the dialog ViewModel
                var dialogViewModel = new AddPlanningStructureDialogViewModel(_xmlService, _localizationService, 
                    _serviceProvider.GetRequiredService<ILogger<AddPlanningStructureDialogViewModel>>());

                // Create and show the dialog directly since it has a custom constructor
                var dialog = new AddPlanningStructureDialog(dialogViewModel);
                dialog.Owner = System.Windows.Application.Current.MainWindow;
                
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // User clicked Create - handle the creation logic
                    await CreatePlanningStructure(dialogViewModel.SelectedRace, dialogViewModel.SelectedGender, dialogViewModel.SelectedCategory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing Add Planning Structure dialog");
                _dialogService.ShowMessage(
                    _localizationService.GetString("Error"),
                    _localizationService.GetString("ErrorCreatingPlanningStructure"));
            }
        }

        private async Task CreatePlanningStructure(Data.Race selectedRace, string selectedGender, Data.Category selectedCategory)
        {
            try
            {
                _logger.LogInformation("Creating planning structure for Race: {RaceId}, Gender: {Gender}, Category: {CategoryId}",
                    selectedRace?.Id, selectedGender, selectedCategory?.Id);

                // Create a new EventViewModel based on the selected parameters
                var newEvent = CreateEventFromRaceAndCategory(selectedRace, selectedGender, selectedCategory);
                
                // Add the event to the PlanningStructureCompetition
                AddEventToPlanningStructure(newEvent);

                _dialogService.ShowMessage(
                    _localizationService.GetString("Success"),
                    _localizationService.GetString("PlanningStructureCreatedSuccessfully"));

                // Refresh data if needed
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating planning structure");
                _dialogService.ShowMessage(
                    _localizationService.GetString("Error"),
                    _localizationService.GetString("ErrorCreatingPlanningStructure"));
            }
        }

        private EventViewModel CreateEventFromRaceAndCategory(Data.Race race, string gender, Data.Category category)
        {
            // Calculate participant count based on race and category
            int participantCount = CalculateParticipantCount(race, category);
            
            var eventViewModel = new EventViewModel
            {
                Name = $"{race.Name} {gender} {category.Name}",
                ParticipantCount = participantCount,
                Category = category.Name,
                ConfigurationStatus = _localizationService.GetString("NotConfigured") ?? "Non configuré"
            };

            _logger.LogInformation("Created EventViewModel: {EventName} with {ParticipantCount} participants", 
                eventViewModel.Name, participantCount);

            return eventViewModel;
        }

        private int CalculateParticipantCount(Race race, Category category)
        {
            try
            {
                int count = 0;
                
                foreach (var team in race.Teams.Where(t => !t.IsForfeit))
                {
                    // Check if team belongs to the specified category
                    if (team.Category.Id != category.Id)
                    {
                        continue;
                    }
                        
                    // Since we already filtered by category, just count the team
                    count++;
                }
                
                _logger.LogInformation("Calculated {Count} participants for race {RaceId} and category {CategoryId}", 
                    count, race.Id, category.Id);
                    
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating participant count for race {RaceId} and category {CategoryId}, using default value", 
                    race?.Id, category?.Id);
                return 0;
            }
        }

        private void AddEventToPlanningStructure(EventViewModel newEvent)
        {
            try
            {
                // Get the current PlanningStructureCompetitionViewModel from the ContentArea
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    // Find the PlanningView and get its PlanningStructureCompetitionView
                    var planningViews = FindVisualChildren<Views.PlanningStructureCompetitionView>(System.Windows.Application.Current.MainWindow);
                    var planningStructureView = planningViews.FirstOrDefault();
                    
                    if (planningStructureView?.DataContext is PlanningStructureCompetitionViewModel viewModel)
                    {
                        viewModel.Events.Add(newEvent);
                        _logger.LogInformation("Added event {EventName} to planning structure", newEvent.Name);
                    }
                    else
                    {
                        _logger.LogWarning("Could not find PlanningStructureCompetitionViewModel to add event");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding event to planning structure");
                throw;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(System.Windows.DependencyObject depObj) where T : System.Windows.DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void OnPrevious()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
                SelectedStep = Steps[CurrentStep - 1];
                OnPropertyChanged(nameof(SelectedStep));
            }
        }

        private void OnPlanification()
        {
            if (CurrentStep < TotalSteps)
            {
                CurrentStep++;
                SelectedStep = Steps[CurrentStep - 1];
                OnPropertyChanged(nameof(SelectedStep));
            }
        }
    }

    public class PlanningStepViewModel : ObservableObject
    {
        public int StepNumber { get; }
        public string Title { get; }
        public string Subtitle { get; }
        
        private bool _isCompleted;
        public bool IsCompleted 
        { 
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        private bool _isCurrentStep;
        public bool IsCurrentStep
        {
            get => _isCurrentStep;
            set => SetProperty(ref _isCurrentStep, value);
        }

        public PlanningStepViewModel(int stepNumber, string title, string subtitle, bool isCompleted)
        {
            StepNumber = stepNumber;
            Title = title;
            Subtitle = subtitle;
            IsCompleted = isCompleted;
        }
    }
}
