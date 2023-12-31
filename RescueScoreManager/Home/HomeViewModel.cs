using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Win32;

using RescueScoreManager.Data;
using RescueScoreManager.Login;
using RescueScoreManager.Messages;
using RescueScoreManager.SelectNewCompetition;
using RescueScoreManager.Services;

namespace RescueScoreManager.Home;

public partial class HomeViewModel : ObservableObject, IRecipient<LoginMessage>, IRecipient<SelectNewCompetitionMessage>
{
    //private RescueScoreManagerContext _context { get; }
    private LoginViewModel _loginViewModel { get; }
    private SelectNewCompetitionViewModel _selectNewCompetitionViewModel { get; }
    private HomeGraphsViewModel _homeGraphsViewModel { get; }
    private IDialogService _dialogService { get; }
    private IWSIRestService _wsiService { get; }
    private IXMLService _xmlService { get; }
    private IMessenger _messenger { get; }

    [ObservableProperty]
    private Competition? _competition = null;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(NewCompetitionCommand))]
    private bool _isLoaded = false;

    public HomeViewModel(LoginViewModel loginViewModel,
                            SelectNewCompetitionViewModel selectNewCompetitionViewModel,
                            HomeGraphsViewModel homeGraphsViewModel,
                            IDialogService dialogService,
                            IWSIRestService wsiService,
                            IXMLService xmlService,
                            IMessenger messenger)//RescueScoreManagerContext context,
    {
        //_context = context ?? throw new ArgumentNullException(nameof(context));
        _loginViewModel = loginViewModel ?? throw new ArgumentNullException(nameof(_loginViewModel));
        _selectNewCompetitionViewModel = selectNewCompetitionViewModel;
        _homeGraphsViewModel = homeGraphsViewModel;
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(_dialogService));
        _wsiService = wsiService;
        _xmlService = xmlService;

        _messenger = messenger;
        messenger.RegisterAll(this);
    }


    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    public async Task OpenFile()
    {
        if (GetFile() is { } file)
        {

            _xmlService.SetPath(file);
            _xmlService.Load();
            IsLoaded = true;

            _messenger.Send(new OpenCompetitionMessage());
            CurrentViewModel = _homeGraphsViewModel;
            _homeGraphsViewModel.Update();
            #region entity framework
            //    _context.DbPath = file;
            //    _context.Database.Migrate();

            //    Competition competition = new Competition()
            //    {
            //        Id = 1,
            //        BeginDate = DateTime.Now,
            //        EndDate = DateTime.Now,
            //        EntryLimitDate = DateTime.Now,
            //        Name = "New Competition",
            //        Location = "location",
            //        Description = "description",
            //        IsEligibleToNationalRecord = true,
            //        PriceByAthlete = 0,
            //        PriceByClub = 0,
            //        PriceByEntry = 0,
            //        SwimType = EnumRSM.SwimType.Bassin_25m,
            //        Speciality = EnumRSM.Speciality.EauPlate,
            //        ChronoType = EnumRSM.ChronoType.Manual
            //    };

            //    Club newClub1 = new Club()
            //    {
            //        Id = 1,
            //        Name = "premier club",
            //    };
            //    Club newClub2 = new Club()
            //    {
            //        Id = 2,
            //        Name = "deuxieme club",
            //    };

            //    competition.Organizer = newClub1.Name;
            //    competition.Clubs.Add(newClub1);
            //    competition.Clubs.Add(newClub2);

            //    Category cat1 = new Category()
            //    {
            //        Id = 1,
            //        Name = "Cat1",
            //    };
            //    Category cat2 = new Category()
            //    {
            //        Id = 2,
            //        Name = "Cat2",
            //    };
            //    Athlete ath1 = new Athlete()
            //    {
            //        Id = "ath1",
            //        BirthYear = 1984,
            //        FirstName = "Stanislas",
            //        LastName = "Krzywda",
            //        Gender = EnumRSM.Gender.Man,
            //        IsForfeit = false,
            //        IsGuest = false,
            //        IsLicensee = true,
            //        OrderNumber = 1,
            //        Club = newClub1,
            //        Category = cat1,
            //    };
            //    Athlete ath2 = new Athlete()
            //    {
            //        Id = "ath2",
            //        BirthYear = 1985,
            //        FirstName = "Test",
            //        LastName = "Test",
            //        Gender = EnumRSM.Gender.Man,
            //        IsForfeit = false,
            //        IsGuest = false,
            //        IsLicensee = true,
            //        OrderNumber = 1,
            //        Club = newClub1,
            //        Category = cat1,
            //    };

            //    Licensee ref1 = new Referee()
            //    {
            //        Id = "ref1",
            //        BirthYear = 1980,
            //        FirstName = "Baron",
            //        LastName = "Guillaume",
            //        Gender = EnumRSM.Gender.Man,
            //        IsGuest = false,
            //        IsLicencee = true,
            //        Club = newClub1,
            //        RefereeLevel = EnumRSM.RefereeLevel.A,
            //        Category = cat2,
            //    };

            //    RefereeDate refDate1 = new RefereeDate()
            //    {
            //        Id = 1,
            //        Availability = DateTime.Now,
            //        Referee = (Referee)ref1
            //    };

            //    List<Category> categories = new List<Category>() { cat1, cat2 };
            //    Race race = new Race()
            //    {
            //        Id = 1,
            //        Discipline = 1,
            //        Distance = 50,
            //        Gender = Gender.Mixte,
            //        IsRelay = false,
            //        Name = "Race1",
            //        NumberByTeam = 1,
            //        Speciality = Speciality.EauPlate,
            //    };

            //    race.Categories.Add(cat1);
            //    competition.Races.Add(race);
            //    //RaceCategory raceCategory = new RaceCategory()
            //    //{
            //    //    Race = race,
            //    //    Category = cat1,
            //    //};

            //    IndividualTeam team = new IndividualTeam()
            //    {
            //        Id = 1,
            //        Race = race,
            //        IsForfeit = false,
            //        IsForfeitFinal = false,
            //        Number = 1,
            //        EntryTime = 9000,
            //        Athlete = ath1
            //    };
            //    RelayTeam relayTeam = new RelayTeam()
            //    {
            //        Id = 2,
            //        Race = race,
            //        IsForfeit = false,
            //        IsForfeitFinal = false,
            //        Number = 1,
            //        EntryTime = 9000,
            //    };

            //    relayTeam.Athletes.Add(ath1);
            //    relayTeam.Athletes.Add(ath2);

            //    Meeting meeting = new Meeting()
            //    {
            //        Competition = competition,
            //        HeatType = HeatType.Heats,
            //        Label = "meeting1",
            //        Number = 1,
            //        StartHour = DateTime.Now,
            //        EndHour = DateTime.Now,
            //        MeetingType = Speciality.EauPlate
            //    };

            //    MeetingElement me1 = new MeetingElement()
            //    {
            //        Label = "meetingelement1",
            //        StartHour = DateTime.Now,
            //        EndHour = DateTime.Now,
            //        isFinalA = false,
            //        isFinalB = false,
            //        Meeting = meeting,
            //        Order = 1,
            //        Race = race,
            //        Type = HeatType.Heats
            //    };

            //    Round round = new Round()
            //    {
            //        Category = cat2,
            //        HeatType = HeatType.Heats,
            //        Label = "round 1",
            //        Number = 1,
            //        MeetingElement = me1,
            //    };

            //    Heat heat = new Heat()
            //    {
            //        Label = "heat 1",
            //        StartHour = DateTime.Now,
            //        EndHour = DateTime.Now,
            //        Number = 1,
            //        Round = round,
            //    };

            //    SwimHeatResult heatResult = new SwimHeatResult()
            //    {
            //        Lane = 1,
            //        Heat = heat,
            //        Team = team,
            //        Time = 8000,
            //    };

            //    meeting.MeetingElements.Add(me1);
            //    me1.Categories.Add(cat2);

            //    _context.Competitions.RemoveRange(await _context.Competitions.ToListAsync());
            //    _context.Clubs.RemoveRange(await _context.Clubs.ToListAsync());
            //    _context.Licensees.RemoveRange(await _context.Licensees.ToListAsync());
            //    _context.RefereeDates.RemoveRange(await _context.RefereeDates.ToListAsync());
            //    _context.Categories.RemoveRange(await _context.Categories.ToListAsync());
            //    _context.Races.RemoveRange(await _context.Races.ToListAsync());
            //    _context.Teams.RemoveRange(await _context.Teams.ToListAsync());
            //    _context.Meetings.RemoveRange(await _context.Meetings.ToListAsync());
            //    _context.MeetingElements.RemoveRange(await _context.MeetingElements.ToListAsync());
            //    _context.Rounds.RemoveRange(await _context.Rounds.ToListAsync());
            //    _context.Heats.RemoveRange(await _context.Heats.ToListAsync());
            //    _context.HeatResults.RemoveRange(await _context.HeatResults.ToListAsync());


            //    await _context.SaveChangesAsync();

            //    _context.Competitions.Add(competition);
            //    _context.Clubs.AddRange(newClub1, newClub2);
            //    _context.Licensees.AddRange(ath1, ref1);
            //    _context.Categories.AddRange(cat1, cat2);
            //    _context.RefereeDates.Add(refDate1);
            //    _context.Races.Add(race);
            //    _context.Teams.Add(team);
            //    _context.Teams.Add(relayTeam);
            //    _context.Meetings.Add(meeting);
            //    _context.MeetingElements.Add(me1);
            //    _context.Rounds.Add(round);
            //    _context.Heats.Add(heat);
            //    _context.HeatResults.Add(heatResult);

            //    await _context.SaveChangesAsync();
            //    Competition = await _context.Competitions.FirstOrDefaultAsync();
            //    List<Licensee> Licensees = await _context.Licensees.ToListAsync();
            //    List<Race> Races = await _context.Races.ToListAsync();
            //    List<Team> Teams = await _context.Teams.ToListAsync();
            //    List<Meeting> Meetings = await _context.Meetings.ToListAsync();
            //}
            //else
            //{
            //    string path = "C:\\Users\\nast0\\Documents\\RescueScore\\RescueScoreManager\\rsm.ffss";
            //    _context.DbPath = new FileInfo(path);
            //    _context.Database.Migrate();
            //} 
            #endregion entity framework
        }
    }
    
    private bool CanOpenFile() => _isLoaded == false;


    private static FileInfo? GetFile()
    {
        OpenFileDialog openFileDialog = new()
        {
            Title = "Open File",
            Filter = "All Files (*.ffss)|*.*",
            CheckFileExists = true,
            CheckPathExists = true,
            RestoreDirectory = true,
        };
        if (openFileDialog.ShowDialog() == true)
        {
            return new FileInfo(openFileDialog.FileName);
        }
        return null;
    }

    [RelayCommand(CanExecute = nameof(CanNewCompetition))]
    private void NewCompetition()
    {
        if (_wsiService.HasToken() == false)
            CurrentViewModel = _loginViewModel;
        //_dialogService.ShowLoginView(_loginViewModel);
        else
            CurrentViewModel = _selectNewCompetitionViewModel;
        //_dialogService.ShowSelectNewCompetition(_selectNewCompetitionViewModel);
    }

    private bool CanNewCompetition() => IsLoaded == false;

    public async void Receive(LoginMessage message)
    {
        if (message.IsConnected == true)
        {
            CurrentViewModel = _selectNewCompetitionViewModel;
            //_dialogService.ShowSelectNewCompetition(_selectNewCompetitionViewModel);
            //List<Competition> comps = await _wsiService.GetCompetitions(DateTime.Now);
            //comps = comps.OrderBy(c => c.BeginDate).ToList();
        }
        else
        {
            CurrentViewModel = null;
            IsLoaded = true;
        }
    }
    public async void Receive(SelectNewCompetitionMessage message)
    {
        if (message.NewCompetition != null)
        {
            CurrentViewModel = _homeGraphsViewModel; 
            _homeGraphsViewModel.Update();
            IsLoaded = true;
        }
    }
}


