using System.Data;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

using RescueScoreManager.Data;
using RescueScoreManager.Model;

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
            Licensee ath1 = new Athlete()
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


            Context.Competitions.RemoveRange(await Context.Competitions.ToListAsync());
            Context.Clubs.RemoveRange(await Context.Clubs.ToListAsync());
            Context.Licensees.RemoveRange(await Context.Licensees.ToListAsync());
            Context.RefereeDates.RemoveRange(await Context.RefereeDates.ToListAsync());

            await Context.SaveChangesAsync();

            Context.Competitions.Add(competition);
            Context.Clubs.AddRange(newClub1, newClub2);
            Context.Licensees.AddRange(ath1, ref1);
            Context.Categories.AddRange(cat1, cat2);
            Context.RefereeDates.Add(refDate1);

            await Context.SaveChangesAsync();
            Competition = await Context.Competitions.FirstOrDefaultAsync();
            List<Licensee> Licensees = await Context.Licensees.ToListAsync();

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
