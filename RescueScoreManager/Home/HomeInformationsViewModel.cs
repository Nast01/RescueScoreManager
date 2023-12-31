using CommunityToolkit.Mvvm.ComponentModel;

using RescueScoreManager.Data;
using RescueScoreManager.Services;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Home;

public partial class HomeInformationsViewModel : ObservableObject
{
    private readonly IXMLService _xmlService;

    [ObservableProperty]
    private int _athletesCount = 0;
    [ObservableProperty]
    private string _title = "";
    [ObservableProperty]
    private List<RefereeLevelCountStruct> _refereeLevels;
    [ObservableProperty]
    private List<Category> _categories;
    [ObservableProperty]
    private List<Referee> _referees;

    public HomeInformationsViewModel(IXMLService xmlService)
    {
        _xmlService = xmlService;
        Categories = new List<Category>();
        RefereeLevels = new List<RefereeLevelCountStruct>();

        Referees = new List<Referee>();
    }

    public void Update()
    {
        Categories = _xmlService.GetCategories();
        AthletesCount = _xmlService.GetAthletes().Count();
        Referees = _xmlService.GetReferees();
        
        RefereeLevelCountStruct rlsA = new RefereeLevelCountStruct() { Level = RefereeLevel.A, Count = Referees.Where(r => r.RefereeLevel == RefereeLevel.A).Count() };
        RefereeLevelCountStruct rlsB = new RefereeLevelCountStruct() { Level = RefereeLevel.B, Count = Referees.Where(r => r.RefereeLevel == RefereeLevel.B).Count() };
        RefereeLevelCountStruct rlsC = new RefereeLevelCountStruct() { Level = RefereeLevel.C, Count = Referees.Where(r => r.RefereeLevel == RefereeLevel.C).Count() };
        RefereeLevelCountStruct rlsND = new RefereeLevelCountStruct() { Level = RefereeLevel.ND, Count = Referees.Where(r => r.RefereeLevel == RefereeLevel.ND).Count() };

        RefereeLevels.Clear();
        RefereeLevels.Add(rlsA); 
        RefereeLevels.Add(rlsB);
        RefereeLevels.Add(rlsC);
        RefereeLevels.Add(rlsND);

        Title = " - " + _xmlService.GetCompetition().Name;
    }
}


public struct RefereeLevelCountStruct
{
    public RefereeLevel Level { get; set; }
    public int Count { get; set; }
}
