using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DocumentFormat.OpenXml.Office2010.Excel;

using Microsoft.Extensions.Logging;

using RescueScoreManager.Data;

using RescueScoreManager.Services;

using static RescueScoreManager.Data.EnumRSM;
using static RescueScoreManager.Modules.Forfeit.ForfeitViewModel;

namespace RescueScoreManager.Modules.Forfeit;

public partial class ForfeitViewModel : ObservableObject
{
    private readonly IXMLService _xmlService;
    private readonly ILocalizationService _localizationService;

    private readonly ILogger<ForfeitViewModel> _logger;

    #region Observable Properties

    [ObservableProperty]
    private string _searchByName = string.Empty;

    [ObservableProperty]
    private string _searchByClub = string.Empty;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isLoading = true;
    [ObservableProperty]
    private bool _hasUnsavedChanges;

    #endregion

    #region Collections

    public ObservableCollection<AthleteViewModel> AllAthletes { get; } = new();

    private ICollectionView? _filteredAthletesView;
    public ICollectionView FilteredAthletes
    {
        get
        {
            if (_filteredAthletesView == null)
            {
                _filteredAthletesView = CollectionViewSource.GetDefaultView(AllAthletes);
                _filteredAthletesView.Filter = FilterAthletes;
            }
            return _filteredAthletesView;
        }
    }

    #endregion

    #region Constructor

    public ForfeitViewModel(
        IXMLService xmlService,
        ILocalizationService localizationService,
        ILogger<ForfeitViewModel> logger)
    {
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to property changes for filtering
        PropertyChanged += OnPropertyChanged;
    }

    #endregion

    #region Initialization

    public async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            //if (_xmlService.IsLoaded())
            //{
            //    StatusText = "No competition data loaded";
            //IsLoading = false;
            //    return;
            //}

            StatusText = _localizationService.GetString("LoadingAthletes");
            await LoadDataAsync();
            StatusText = _localizationService.GetString("Ready");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing athlete race participation view");
            StatusText = _localizationService.GetString("ErrorLoadingData");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadDataAsync()
    {
        await Task.Run(() =>
        {
            // Load athletes with their teams
            var athletes = _xmlService.GetAthletes();

            App.Current.Dispatcher.Invoke(() =>
            {
                AllAthletes.Clear();
                foreach (var athlete in athletes)
                {
                    AthleteViewModel athleteViewModel = new AthleteViewModel(athlete,_localizationService);
                    foreach (var team in athlete.Teams)
                    {
                        var teamViewModel = new TeamViewModel(team);
                        teamViewModel.ForfeitStatusChanged = () =>
                        {
                            athleteViewModel.UpdateForfeitStatus();
                            HasUnsavedChanges = true;
                        };
                        athleteViewModel.Teams.Add(teamViewModel);
                    }

                    // Initial status update
                    athleteViewModel.UpdateForfeitStatus();
                    AllAthletes.Add(athleteViewModel);
                }
            });

        });
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ClearFilters()
    {
        SearchByName = string.Empty;
        SearchByClub = string.Empty;
    }

    [RelayCommand]
    private void ToggleTeamForfeit(TeamViewModel teamViewModel)
    {
        try
        {
            teamViewModel.IsForfeit = !teamViewModel.IsForfeit;
            HasUnsavedChanges = true;

            StatusText = $"{_localizationService.GetString("TeamForfeitToggledFor")} {teamViewModel.TeamLabel} - {teamViewModel.RaceName}";

            _logger.LogInformation("{TeamForfeitToggledFor} {TeamId}: {Status}", _localizationService.GetString("TeamForfeitToggledFor"),
                teamViewModel.Id, teamViewModel.IsForfeit);

            _xmlService.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ErrorTogglingTeamForfeitForTeam} {TeamId}", _localizationService.GetString("ErrorTogglingTeamForfeitForTeam"), teamViewModel.Id);
            StatusText = _localizationService.GetString("ErrorUpdatingTeamForfeitStatus");
        }
    }

    [RelayCommand]
    private void ForfeitAllTeams(AthleteViewModel athleteViewModel)
    {
        try
        {
            foreach (var teamViewModel in athleteViewModel.Teams)
            {
                teamViewModel.IsForfeit = true;
            }

            HasUnsavedChanges = true;
            StatusText = $"{_localizationService.GetString("AllTeamsForfeitedFor")} {athleteViewModel.FullName}";

            _logger.LogInformation("{AllTeamsForfeitedForAthlete} {AthleteId}", _localizationService.GetString("AllTeamsForfeitedForAthlete"), athleteViewModel.Id);

            _xmlService.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forfeiting all teams for athlete {AthleteId}", athleteViewModel.Id);
            StatusText = _localizationService.GetString("ErrorForfeitingAllTeams");
        }
    }

    [RelayCommand]
    private void RestoreAllTeams(AthleteViewModel athleteViewModel)
    {
        try
        {
            foreach (var teamViewModel in athleteViewModel.Teams)
            {
                teamViewModel.IsForfeit = false;
            }

            HasUnsavedChanges = true;
            StatusText = $"{_localizationService.GetString("AllTeamsRestoredFor")} {athleteViewModel.FullName}";

            _logger.LogInformation("All teams restored for athlete {AthleteId}", athleteViewModel.Id);

            _xmlService.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring all teams for athlete {AthleteId}", athleteViewModel.Id);
            StatusText = _localizationService.GetString("ErrorRestoringAllTeams");
        }
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        try
        {
            StatusText = _localizationService.GetString("SavingChanges");

            // Here you would implement the actual save logic
            // This might involve calling a service to persist the changes
            await Task.Run(() =>
            {
                // Simulate save operation
                Thread.Sleep(1000);

                // Update the underlying data models
                foreach (var athleteViewModel in AllAthletes)
                {
                    foreach (var teamViewModel in athleteViewModel.Teams)
                    {
                        // Update the actual Team entity
                        teamViewModel.Team.IsForfeit = teamViewModel.IsForfeit;
                    }
                }
            });

            _xmlService.Save();
            HasUnsavedChanges = false;
            StatusText = _localizationService.GetString("ChangesSavedSuccessfully");

            _logger.LogInformation("All changes saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            StatusText = _localizationService.GetString("ErrorSavingChanges");
        }
    }

    #endregion

    #region Filtering

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchByName) || e.PropertyName == nameof(SearchByClub))
        {
            FilteredAthletes.Refresh();
        }
    }

    private bool FilterAthletes(object item)
    {
        if (item is not AthleteViewModel athlete)
        {
            return false;
        }
        // Filter by name
        if (!string.IsNullOrWhiteSpace(SearchByName))
        {
            string[] searchTerms = SearchByName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string fullName = athlete.FullName.ToLowerInvariant();

            if (!searchTerms.All(term => fullName.Contains(term.ToLowerInvariant())))
            {
                return false;
            }
        }

        // Filter by club
        if (!string.IsNullOrWhiteSpace(SearchByClub))
        {
            string clubName = athlete.Club.Name.ToLowerInvariant();
            string searchTerm = SearchByClub.ToLowerInvariant();

            if (!clubName.Contains(searchTerm))
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
    }

    #endregion
}

#region Helper ViewModels
public partial class AthleteViewModel : ObservableObject
{
    //[ObservableProperty]
    //private Athlete _athlete;
    public int Id { get; }
    public string FullName { get; }
    public string LicenseeNumber { get; }
    public int ClubId { get; }
    public Club Club { get; }
    [ObservableProperty]
    public ObservableCollection<TeamViewModel> _teams = new();

    public bool HasTeams => Teams.Count > 0;

    // Forfeit status indicators
    public int TotalTeamsNumber => Teams.Count;
    public int ForfeitedTeamsNumber => Teams.Count(t => t.IsForfeit);
    public int ActiveTeamsNumber => Teams.Count(t => !t.IsForfeit);

    public bool HasForfeitedTeams => ForfeitedTeamsNumber > 0;
    public bool HasAllTeamsForfeited => TotalTeamsNumber > 0 && ForfeitedTeamsNumber == TotalTeamsNumber;
    public bool HasPartialForfeit => ForfeitedTeamsNumber > 0 && ForfeitedTeamsNumber < TotalTeamsNumber;

    public string ForfeitStatusText => TotalTeamsNumber switch
    {
        0 => _localizationService.GetString("NoTeams"),
        _ when HasAllTeamsForfeited => _localizationService.GetString("AllTeamsForfeited"),
        _ when HasPartialForfeit => $"{ForfeitedTeamsNumber}/{TotalTeamsNumber} {_localizationService.GetString("TeamsForfeited")}",
        _ => _localizationService.GetString("AllTeamsActive")
    };


    private readonly ILocalizationService _localizationService;


    public AthleteViewModel(Athlete athlete, ILocalizationService localizationService)
    {
        //Athlete = athlete;
        Id = athlete.Id;
        FullName = athlete.FullName!;
        LicenseeNumber = athlete.LicenseeNumber;
        ClubId = athlete.ClubId;
        Club = athlete.Club;

        // Subscribe to collection changes to update status
        Teams.CollectionChanged += (s, e) => UpdateForfeitStatus();
        _localizationService = localizationService;
    }

    // Subscribe to team changes to update status
    public void UpdateForfeitStatus()
    {
        OnPropertyChanged(nameof(HasTeams));
        OnPropertyChanged(nameof(TotalTeamsNumber));
        OnPropertyChanged(nameof(ForfeitedTeamsNumber));
        OnPropertyChanged(nameof(ActiveTeamsNumber));
        OnPropertyChanged(nameof(HasForfeitedTeams));
        OnPropertyChanged(nameof(HasAllTeamsForfeited));
        OnPropertyChanged(nameof(HasPartialForfeit));
        OnPropertyChanged(nameof(ForfeitStatusText));
    }
}

public partial class TeamViewModel : ObservableObject
{
    public int Id { get; }
    public string RaceName { get; }
    public string TeamLabel { get; }
    public string CategoryName { get; }
    public Speciality RaceSpeciality { get; }
    public string EntryTimeLabel { get; }

    [ObservableProperty]
    private bool _isForfeit;

    [ObservableProperty]
    public Team _team;

    public TeamViewModel(Team team)
    {
        Id = team.Id;
        RaceName = team.Race?.Name ?? "Unknown Race";
        TeamLabel = team.TeamLabel;
        CategoryName = team.Category?.Name ?? "Unknown Category";
        RaceSpeciality = team.Race?.Speciality ?? Speciality.EauPlate;
        EntryTimeLabel = team.EntryTimeLabel;
        IsForfeit = team.IsForfeit;

        Team = team;

        // Subscribe to our own property changes
        PropertyChanged += OnTeamPropertyChanged;
    }

    private void OnTeamPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsForfeit))
        {
            // Notify parent to update status
            ForfeitStatusChanged?.Invoke();
        }
    }

    public Action? ForfeitStatusChanged { get; set; }
}
#endregion Helper ViewModels
