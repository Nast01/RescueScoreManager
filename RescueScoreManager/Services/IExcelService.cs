using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IExcelService
{
    string GenerateStartList(Competition competition,List<Race> races,List<Referee> referees);
    string GenerateClubList(Competition competition, List<Club> clubs);
    bool IsFileExist(EnumRSM.ExcelType excelType, string name);
}
