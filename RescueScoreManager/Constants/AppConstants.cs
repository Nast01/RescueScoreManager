namespace RescueScoreManager.Constants;

public static class AppConstants
{
    public const string ApplicationName = "RescueScoreManager";
    public const string CompanyName = "FFSS";

    public static class FileFilters
    {
        public const string CompetitionFiles = "Competition Files (*.ffss)|*.ffss";
        public const string AllFiles = "All Files (*.*)|*.*";
        public const string Combined = CompetitionFiles + "|" + AllFiles;
    }

    public static class LocalizationKeys
    {
        public const string DataLoadingInfo = "DataLoadingInfo";
        public const string FileCreationInfo = "FileCreationInfo";
        public const string EndLoadingInfo = "EndLoadingInfo";
        public const string FileLoadingInfo = "FileLoadingInfo";
        public const string OpenFileTitle = "OpenFileTitle";
        public const string SaveTokenError = "SaveTokenError";
        public const string FileLoadedSuccess = "FileLoadedSuccess";
        public const string FileLoadError = "FileLoadError";
        public const string NewCompetitionError = "NewCompetitionError";
        public const string DataLoadedSuccess = "DataLoadedSuccess";
        public const string DataLoadError = "DataLoadError";
    }

    public static class Paths
    {
        public const string AppDataFolder = "FFSSAuthApp";
        public const string AuthFileName = "auth.json";
        public const string KeyFileName = "key.dat";
    }
}