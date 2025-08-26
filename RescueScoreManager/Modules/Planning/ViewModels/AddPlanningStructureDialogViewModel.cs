using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using RescueScoreManager.Services;
using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public class AddPlanningStructureDialogViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<AddPlanningStructureDialogViewModel> _logger;

        public event Action<bool?> CloseRequested;

        private Race _selectedRace;
        private string _selectedGender;
        private Category _selectedCategory;
        private string _validationMessage;
        private bool _hasValidationError;

        public ObservableCollection<Race> AvailableRaces { get; }
        public ObservableCollection<string> AvailableGenders { get; }
        public ObservableCollection<Category> AvailableCategories { get; }

        public Race SelectedRace
        {
            get => _selectedRace;
            set
            {
                if (SetProperty(ref _selectedRace, value))
                {
                    UpdateAvailableCategories();
                    ValidateSelection();
                }
            }
        }

        public string SelectedGender
        {
            get => _selectedGender;
            set
            {
                if (SetProperty(ref _selectedGender, value))
                {
                    ValidateSelection();
                }
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    ValidateSelection();
                }
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public bool HasValidationError
        {
            get => _hasValidationError;
            set => SetProperty(ref _hasValidationError, value);
        }

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public AddPlanningStructureDialogViewModel(
            IXMLService xmlService,
            ILocalizationService localizationService,
            ILogger<AddPlanningStructureDialogViewModel> logger)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AvailableRaces = new ObservableCollection<Race>();
            AvailableGenders = new ObservableCollection<string>();
            AvailableCategories = new ObservableCollection<Category>();

            CreateCommand = new RelayCommand(OnCreate, CanCreate);
            CancelCommand = new RelayCommand(OnCancel);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Load races
                var races = _xmlService.GetRaces();
                AvailableRaces.Clear();
                foreach (var race in races)
                {
                    AvailableRaces.Add(race);
                }

                // Load genders
                AvailableGenders.Clear();
                AvailableGenders.Add(_localizationService.GetString("Men"));
                AvailableGenders.Add(_localizationService.GetString("Women"));
                AvailableGenders.Add(_localizationService.GetString("Mixte"));

                // Categories will be loaded when race is selected
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data for AddPlanningStructureDialog");
            }
        }

        private void UpdateAvailableCategories()
        {
            AvailableCategories.Clear();
            SelectedCategory = null;

            if (SelectedRace != null)
            {
                foreach (var category in SelectedRace.Categories)
                {
                    AvailableCategories.Add(category);
                }
            }
        }

        private void ValidateSelection()
        {
            ValidationMessage = string.Empty;
            HasValidationError = false;

            if (SelectedRace == null)
            {
                ValidationMessage = _localizationService.GetString("PleaseSelectRace");
                HasValidationError = true;
                return;
            }

            if (string.IsNullOrEmpty(SelectedGender))
            {
                ValidationMessage = _localizationService.GetString("PleaseSelectGender");
                HasValidationError = true;
                return;
            }

            if (SelectedCategory == null)
            {
                ValidationMessage = _localizationService.GetString("PleaseSelectCategory");
                HasValidationError = true;
                return;
            }
        }

        private bool CanCreate()
        {
            return SelectedRace != null && 
                   !string.IsNullOrEmpty(SelectedGender) && 
                   SelectedCategory != null && 
                   !HasValidationError;
        }

        private void OnCreate()
        {
            try
            {
                ValidateSelection();
                
                if (!HasValidationError)
                {
                    // The creation logic will be handled by the calling ViewModel
                    CloseRequested?.Invoke(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating planning structure");
                ValidationMessage = _localizationService.GetString("ErrorCreatingPlanningStructure");
                HasValidationError = true;
            }
        }

        private void OnCancel()
        {
            CloseRequested?.Invoke(false);
        }
    }
}