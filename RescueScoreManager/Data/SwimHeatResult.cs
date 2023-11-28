using RescueScoreManager.Helper;

namespace RescueScoreManager.Data;

public partial class SwimHeatResult : HeatResult
{
    public int Time { get; set; } //centiseconds
    public string TimeLabel => TimeHelper.ConvertCentisecondInString(Time);
    public bool IsForfeitFinal { get; set; }

}
