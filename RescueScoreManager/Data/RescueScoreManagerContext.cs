using System.IO;
using System.Reflection.Metadata;

using Microsoft.EntityFrameworkCore;

namespace RescueScoreManager.Data;

public class RescueScoreManagerContext : DbContext
{
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Licensee> Licensees => Set<Licensee>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RefereeDate> RefereeDates => Set<RefereeDate>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingElement> MeetingElements => Set<MeetingElement>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Heat> Heats => Set<Heat>();
    public DbSet<HeatResult> HeatResults => Set<HeatResult>();

    public FileInfo DbPath { get; set; }
    public string DbFolderPath { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Configure Competition-Clubs One-to-Many relation
        modelBuilder.Entity<Competition>()
            .HasMany(c => c.Clubs)
            .WithOne(cl => cl.Competition)
            .HasForeignKey(cl => cl.CompetitionId)
            .IsRequired(true);

        //Configure Referee-RefereeDate One-to-Many relation
        modelBuilder.Entity<RefereeDate>()
            .HasKey(rd => rd.Id);

        modelBuilder.Entity<RefereeDate>()
            .HasOne(rd => rd.Referee)
            .WithMany(r => r.RefereeAvailabilities)
            .HasForeignKey(rd => rd.RefereeId);


        //Configure Licensee mother daughter relation
        modelBuilder.Entity<Licensee>()
            .HasDiscriminator<string>("LicenseeType")
            .HasValue<Athlete>("Athlete")
            .HasValue<Referee>("Referee");

        //Configure Team mother daughter relation
        modelBuilder.Entity<Team>()
            .HasDiscriminator<string>("TeamType")
            .HasValue<IndividualTeam>("Individual")
            .HasValue<RelayTeam>("Relay");

        //Configure Category-Athletes One-to-Many relation
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Licensees)
            .WithOne(l => l.Category)
            .HasForeignKey(l => l.CategoryId)
            .IsRequired(true);

        //Configure Category-Round One-to-Many relation
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Rounds)
            .WithOne(r => r.Category)
            .HasForeignKey(r => r.CategoryId)
            .IsRequired(false);

        //Configure Race-Category Many-to-Many relation
        modelBuilder.Entity<Race>()
            .HasMany(r => r.Categories)
            .WithMany(c => c.Races)
            .UsingEntity( e => e.ToTable("RaceCategory"));

        //Configure Competition-Races One-to-Many relation
        modelBuilder.Entity<Competition>()
            .HasMany(c => c.Races)
            .WithOne(r => r.Competition)
            .HasForeignKey(l => l.CompetitionId);

        //Configure Race-Teams One-to-Many relation
        modelBuilder.Entity<Race>()
            .HasMany(r => r.Teams)
            .WithOne(t => t.Race)
            .HasForeignKey(r => r.RaceId);

        //Configure Athlete-IndividualTeams One-to-Many relation
        modelBuilder.Entity<Athlete>()
            .HasMany(a => a.IndividualTeams)
            .WithOne(it => it.Athlete)
            .HasForeignKey(a => a.AthleteId)
            .IsRequired(false);

        //Configure Athlete-RelayTeam Many-to-Many relation
        modelBuilder.Entity<Athlete>()
            .HasMany(a => a.RelayTeams)
            .WithMany(rt => rt.Athletes)
            .UsingEntity(e => e.ToTable("AthleteRelayTeam"));

        //Configure Meeting-MeetingElements One-to-Many relation
        modelBuilder.Entity<Meeting>()
            .HasMany(m => m.MeetingElements)
            .WithOne(me => me.Meeting)
            .HasForeignKey(me => me.MeetingId)
            .IsRequired(true);

        //Configure Meeting-Meeting One-to-One relation
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.RelatedMeeting)
            .WithOne()
            .HasForeignKey<Meeting>(m => m.RelatedMeetingId)
            .IsRequired(false);

        //Configure Race-MeetingElements One-to-Many relation
        modelBuilder.Entity<Race>()
            .HasMany(r => r.MeetingElements)
            .WithOne(me => me.Race)
            .HasForeignKey(me => me.RaceId)
            .IsRequired(false);

        //Configure MeetingElement-Category Many-to-Many relation
        modelBuilder.Entity<MeetingElement>()
            .HasMany(me => me.Categories)
            .WithMany(c => c.MeetingElements)
            .UsingEntity(e => e.ToTable("MeetingElementCategory"));

        //Configure Round-MeetingElements One-to-Many relation
        modelBuilder.Entity<MeetingElement>()
            .HasMany(me => me.Rounds)
            .WithOne(r => r.MeetingElement)
            .HasForeignKey(r => r.MeetingElementId)
            .IsRequired(true);

        //Configure Round-Heats One-to-Many relation
        modelBuilder.Entity<Round>()
            .HasMany(r => r.Heats)
            .WithOne(h => h.Round)
            .HasForeignKey(h => h.RoundId)
            .IsRequired(true);

        //Configure Heat-HeatResults One-to-Many relation
        modelBuilder.Entity<Heat>()
            .HasMany(h => h.HeatResults)
            .WithOne(hr => hr.Heat)
            .HasForeignKey(hr => hr.HeatId)
            .IsRequired(true);

        //Configure Team-HeatResults One-to-Many relation
        modelBuilder.Entity<Team>()
            .HasMany(t => t.HeatResults)
            .WithOne(hr => hr.Team)
            .HasForeignKey(hr => hr.TeamId)
            .IsRequired(false);

        //Configure HeatResult mother daughter relation
        modelBuilder.Entity<HeatResult>()
            .HasDiscriminator<string>("HeatResultType")
            .HasValue<SwimHeatResult>("SwimHeatResult")
            .HasValue<BeachHeatResult>("BeachHeatResult");


        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlite($"Data Source=C:\\Users\\nast0\\Documents\\RescueScore\\RescueScoreManager\\rsm.ffss");
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
        base.OnConfiguring(optionsBuilder);
    }
}
