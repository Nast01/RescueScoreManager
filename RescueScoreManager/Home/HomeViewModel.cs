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
            };

            Club newClub1 = new Club()
            {
                Id = 1,
                Name  = "premier club",
            };
            Club newClub2 = new Club()
            {
                Id = 2,
                Name = "deuxieme club",
            };

            competition.Organizer = newClub1.Name;
            competition.Clubs.Add(newClub1);
            competition.Clubs.Add(newClub2);
            
            
            Context.Competitions.Add(competition);
            Context.Clubs.AddRange(newClub1,newClub2);
            //Context.Clubs.Add(newClub2);

            await Context.SaveChangesAsync();
            Competition = await Context.Competitions.FirstOrDefaultAsync();

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
