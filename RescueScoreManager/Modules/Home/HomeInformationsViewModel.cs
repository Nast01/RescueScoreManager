using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Constants;
using RescueScoreManager.Data;
using RescueScoreManager.Services;
using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Modules.Home;

public partial class HomeInformationsViewModel : ObservableObject, IDisposable
{
    private readonly IXMLService _xmlService;
    private readonly ILogger<HomeInformationsViewModel> _logger;
    private bool _disposed = false;

    [ObservableProperty]
    private int _athletesCount = 0;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private List<RefereeLevelCountStruct> _refereeLevels = new();

    [ObservableProperty]
    private List<Category> _categories = new();

    [ObservableProperty]
    private List<Referee> _referees = new();

    public HomeInformationsViewModel(IXMLService xmlService, ILogger<HomeInformationsViewModel> logger)
    {
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Update()
    {
        try
        {
            Categories = _xmlService.GetCategories().ToList();
            AthletesCount = _xmlService.GetAthletes().Count();
            Referees = _xmlService.GetReferees().ToList();

            UpdateRefereeLevels();
            UpdateTitle();

            _logger.LogInformation("Home information updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating home information");
            throw;
        }
    }

    private void UpdateRefereeLevels()
    {
        var refereeGroups = Referees.GroupBy(r => r.RefereeLevel).ToList();

        RefereeLevels = new List<RefereeLevelCountStruct>
        {
            new() { Level = RefereeLevel.A, Count = GetRefereeCount(refereeGroups, RefereeLevel.A) },
            new() { Level = RefereeLevel.B, Count = GetRefereeCount(refereeGroups, RefereeLevel.B) },
            new() { Level = RefereeLevel.C, Count = GetRefereeCount(refereeGroups, RefereeLevel.C) },
            new() { Level = RefereeLevel.ND, Count = GetRefereeCount(refereeGroups, RefereeLevel.ND) }
        };
    }

    private static int GetRefereeCount(IEnumerable<IGrouping<RefereeLevel, Referee>> refereeGroups, RefereeLevel level)
    {
        return refereeGroups.FirstOrDefault(g => g.Key == level)?.Count() ?? 0;
    }

    private void UpdateTitle()
    {
        var competition = _xmlService.GetCompetition();
        Title = competition != null ? $" - {competition.Name}" : string.Empty;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                Categories?.Clear();
                RefereeLevels?.Clear();
                Referees?.Clear();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public readonly struct RefereeLevelCountStruct : IEquatable<RefereeLevelCountStruct>
{
    public RefereeLevel Level { get; init; }
    public int Count { get; init; }

    public bool Equals(RefereeLevelCountStruct other)
    {
        return Level == other.Level && Count == other.Count;
    }

    public override bool Equals(object? obj)
    {
        return obj is RefereeLevelCountStruct other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Level, Count);
    }

    public static bool operator ==(RefereeLevelCountStruct left, RefereeLevelCountStruct right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RefereeLevelCountStruct left, RefereeLevelCountStruct right)
    {
        return !left.Equals(right);
    }
}