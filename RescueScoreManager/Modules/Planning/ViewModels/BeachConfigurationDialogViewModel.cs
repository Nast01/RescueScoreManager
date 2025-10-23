using System;
using System.Collections.Generic;

using CommunityToolkit.Mvvm.Messaging;

using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class BeachConfigurationDialogViewModel : BaseConfigurationDialogViewModel
    {
        public BeachConfigurationDialogViewModel(ILocalizationService localizationService,
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
            // Beach-specific default values
            return new PhaseViewModel
            {
                Order = category.Phases.Count + 1,
                Name = _localizationService.GetString("Series") ?? "Series",
                Level = EnumRSM.HeatLevel.Heat,
                QualificationLogic = EnumRSM.QualificationType.Course,
                NumberOfRaces = 1,
                PlacesPerRace = 8, // Beach events typically have 8 places per race
                QualifyingPlaces = 6, // Beach events typically qualify 6 places
                RemoveCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<PhaseViewModel>(OnRemovePhase)
            };
        }

        // TODO: Add beach-specific methods and properties here
        // Examples:
        // - Beach conditions validation
        // - Tide schedule considerations
        // - Equipment checks
        // - Safety protocols
    }
}