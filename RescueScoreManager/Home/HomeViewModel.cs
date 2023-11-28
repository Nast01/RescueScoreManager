using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

using RescueScoreManager.Data;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Home;

public partial class HomeViewModel : ObservableObject
{
    private RescueScoreManagerContext Context { get; }

    [ObservableProperty]
    private Competition _competition;

    public HomeViewModel(RescueScoreManagerContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [RelayCommand]
    public async Task OpenFile()
    {
        if (GetFile() is { } file)
        {
            Context.DbPath = file;
            Context.Database.Migrate();

            Competition competition = new Competition()
            {
                Id = 1,
                BeginDate = DateTime.Now,
                EndDate = DateTime.Now,
                EntryLimitDate = DateTime.Now,
                Name = "New Competition",
                Location = "location",
                Description = "description",
                IsEligibleToNationalRecord = true,
                PriceByAthlete = 0,
                PriceByClub = 0,
                PriceByEntry = 0,
                SwimType = EnumRSM.SwimType.Bassin_25m,
                Speciality = EnumRSM.Speciality.EauPlate,
                ChronoType = EnumRSM.ChronoType.Manual
            };

            Club newClub1 = new Club()
            {
                Id = 1,
                Name = "premier club",
            };
            Club newClub2 = new Club()
            {
                Id = 2,
                Name = "deuxieme club",
            };

            competition.Organizer = newClub1.Name;
            competition.Clubs.Add(newClub1);
            competition.Clubs.Add(newClub2);

            Category cat1 = new Category()
            {
                Id = 1,
                Name = "Cat1",
            };
            Category cat2 = new Category()
            {
                Id = 2,
                Name = "Cat2",
            };
            Athlete ath1 = new Athlete()
            {
                Id = "ath1",
                BirthYear = 1984,
                FirstName = "Stanislas",
                LastName = "Krzywda",
                Gender = EnumRSM.Gender.Man,
                IsForfeit = false,
                IsGuest = false,
                IsLicencee = true,
                OrderNumber = 1,
                Club = newClub1,
                Category = cat1,
            };
            Athlete ath2 = new Athlete()
            {
                Id = "ath2",
                BirthYear = 1985,
                FirstName = "Test",
                LastName = "Test",
                Gender = EnumRSM.Gender.Man,
                IsForfeit = false,
                IsGuest = false,
                IsLicencee = true,
                OrderNumber = 1,
                Club = newClub1,
                Category = cat1,
            };

            Licensee ref1 = new Referee()
            {
                Id = "ref1",
                BirthYear = 1980,
                FirstName = "Baron",
                LastName = "Guillaume",
                Gender = EnumRSM.Gender.Man,
                IsGuest = false,
                IsLicencee = true,
                Club = newClub1,
                RefereeLevel = EnumRSM.RefereeLevel.A,
                Category = cat2,
            };

            RefereeDate refDate1 = new RefereeDate()
            {
                Id = 1,
                Availability = DateTime.Now,
                Referee = (Referee)ref1
            };

            List<Category> categories = new List<Category>() { cat1, cat2 };
            Race race = new Race()
            {
                Id = 1,
                Discipline = 1,
                Distance = 50,
                Gender = Gender.Mixte,
                IsRelay = false,
                Name = "Race1",
                NumberByTeam = 1,
                Speciality = Speciality.EauPlate,
            };

            race.Categories.Add(cat1);
            competition.Races.Add(race);
            //RaceCategory raceCategory = new RaceCategory()
            //{
            //    Race = race,
            //    Category = cat1,
            //};

            IndividualTeam team = new IndividualTeam()
            {
                Id = 1,
                Race = race,
                IsForfeit = false,
                IsForfeitFinal = false,
                Number = 1,
                EntryTime = 9000,
                Athlete = ath1
            };
            RelayTeam relayTeam = new RelayTeam()
            {
                Id = 2,
                Race = race,
                IsForfeit = false,
                IsForfeitFinal = false,
                Number = 1,
                EntryTime = 9000,
            };

            relayTeam.Athletes.Add(ath1);
            relayTeam.Athletes.Add(ath2);

            Meeting meeting = new Meeting()
            {
                Competition = competition,
                HeatType = HeatType.Heats,
                Label = "meeting1",
                Number = 1,
                StartHour = DateTime.Now,
                EndHour = DateTime.Now,
                MeetingType = Speciality.EauPlate
            };

            MeetingElement me1 = new MeetingElement()
            {
                Label = "meetingelement1",
                StartHour = DateTime.Now,
                EndHour = DateTime.Now,
                isFinalA = false,
                isFinalB = false,
                Meeting = meeting,
                Order = 1,
                Race = race,
                Type = HeatType.Heats
            };

            Round round = new Round()
            {
                Category = cat2,
                HeatType = HeatType.Heats,
                Label = "round 1",
                Number = 1,
                MeetingElement = me1,
            };

            Heat heat = new Heat()
            {
                Label = "heat 1",
                StartHour = DateTime.Now,
                EndHour = DateTime.Now,
                Number = 1,
                Round = round,
            };

            SwimHeatResult heatResult = new SwimHeatResult()
            {
                Lane = 1,
                Heat = heat,
                Team = team,
                Time = 8000,
            };

            meeting.MeetingElements.Add(me1);
            me1.Categories.Add(cat2);

            Context.Competitions.RemoveRange(await Context.Competitions.ToListAsync());
            Context.Clubs.RemoveRange(await Context.Clubs.ToListAsync());
            Context.Licensees.RemoveRange(await Context.Licensees.ToListAsync());
            Context.RefereeDates.RemoveRange(await Context.RefereeDates.ToListAsync());
            Context.Categories.RemoveRange(await Context.Categories.ToListAsync());
            Context.Races.RemoveRange(await Context.Races.ToListAsync());
            Context.Teams.RemoveRange(await Context.Teams.ToListAsync());
            Context.Meetings.RemoveRange(await Context.Meetings.ToListAsync());
            Context.MeetingElements.RemoveRange(await Context.MeetingElements.ToListAsync());
            Context.Rounds.RemoveRange(await Context.Rounds.ToListAsync());
            Context.Heats.RemoveRange(await Context.Heats.ToListAsync());
            Context.HeatResults.RemoveRange(await Context.HeatResults.ToListAsync());


            await Context.SaveChangesAsync();

            Context.Competitions.Add(competition);
            Context.Clubs.AddRange(newClub1, newClub2);
            Context.Licensees.AddRange(ath1, ref1);
            Context.Categories.AddRange(cat1, cat2);
            Context.RefereeDates.Add(refDate1);
            Context.Races.Add(race);
            Context.Teams.Add(team);
            Context.Teams.Add(relayTeam);
            Context.Meetings.Add(meeting);
            Context.MeetingElements.Add(me1);
            Context.Rounds.Add(round);
            Context.Heats.Add(heat);
            Context.HeatResults.Add(heatResult);

            await Context.SaveChangesAsync();
            Competition = await Context.Competitions.FirstOrDefaultAsync();
            List<Licensee> Licensees = await Context.Licensees.ToListAsync();
            List<Race> Races = await Context.Races.ToListAsync();
            List<Team> Teams = await Context.Teams.ToListAsync();
            List<Meeting> Meetings = await Context.Meetings.ToListAsync();
        }
        else
        {
            string path = "C:\\Users\\nast0\\Documents\\RescueScore\\RescueScoreManager\\rsm.ffss";
            Context.DbPath = new FileInfo(path);
            Context.Database.Migrate();
        }
    }

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
}


