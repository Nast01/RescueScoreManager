using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IExcelService
{
    public string GenerateStartList(Competition competition,List<Race> races,List<Referee> referees);
    public bool IsFileExist(EnumRSM.ExcelType excelType, string name);
}
