namespace RescueScoreManager.Helper;

internal static class TimeHelper
{
    public static string ConvertMillisecondInString(int? millisecondsTime)
    {
        string formattedTime = string.Empty;
        if (millisecondsTime.HasValue == true)
        {
            // Calculate minutes, seconds, and milliseconds
            int minutes = millisecondsTime.Value / 60000;
            int seconds = (millisecondsTime.Value % 60000) / 1000;
            int milliseconds = millisecondsTime.Value % 1000;

            // Format the result as a string
            formattedTime = $"{minutes}'{seconds}\"{milliseconds}";
        }
        return formattedTime;
    }

    public static string ConvertCentisecondInString(int? centiseconds)
    {
        string formattedTime = string.Empty;
        if (centiseconds.HasValue == true)
        {
            int minutes = centiseconds.Value / 6000;
            int seconds = (centiseconds.Value % 6000) / 100;
            int milliseconds = centiseconds.Value % 100;

            formattedTime = string.Format("{0:D2}'{1:D2}\"{2:D2}", minutes, seconds, milliseconds);
        }
        return formattedTime;
    }

}
