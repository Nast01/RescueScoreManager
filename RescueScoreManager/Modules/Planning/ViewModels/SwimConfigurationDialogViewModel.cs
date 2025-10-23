using System;
using System.Collections.Generic;

using CommunityToolkit.Mvvm.Messaging;

using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class SwimConfigurationDialogViewModel : BaseConfigurationDialogViewModel
    {
        public SwimConfigurationDialogViewModel(ILocalizationService localizationService,
                                               IXMLService xmlService,
                                               IApiService apiService,
                                               IAuthenticationService authService,
                                               IMessenger messenger,
                                               List<Race> races)
            : base(localizationService, xmlService, apiService, authService, messenger, races)
        {
        }

        protected override PhaseViewModel CreateNewPhase(CategoryConfigurationViewModel category)
        {
            // Swimming-specific default values
            return new PhaseViewModel
            {
                Order = category.Phases.Count + 1,
                Name = _localizationService.GetString("Series") ?? "Series",
                Level = EnumRSM.HeatLevel.Heat,
                QualificationLogic = EnumRSM.QualificationType.Course,
                NumberOfRaces = 1,
                PlacesPerRace = 10, // Swimming pools typically have 10 lanes
                QualifyingPlaces = 8, // Swimming events typically qualify 8 swimmers
                RemoveCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<PhaseViewModel>(OnRemovePhase)
            };
        }

        // TODO: Add swimming-specific methods and properties here
        // Examples:
        // - Pool configuration (lanes, length)
        // - Timing system setup
        // - Qualification times
        // - Pool temperature requirements
        // - Starting block configuration
        // - Electronic timing validation
    }
}