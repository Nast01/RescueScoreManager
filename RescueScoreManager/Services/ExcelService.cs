using System.ComponentModel;
using System.IO;

using ClosedXML.Excel;

using RescueScoreManager.Comparer;
using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public class ExcelService : IExcelService
{
    #region CONST VALUE
    private const string CLUBS = "Clubs";
    private const string POSITION = "Position";
    private const string POINTS = "Points";
    private const string GENERAL = "General";
    private const string DETAIL = "Detail";
    private const string RESULTS = "Resultats";
    private const string COEFFICIENT = "Coefficient";
    private const string STARTLIST = "StartList";
    private const string NUMBER = "Numéro";
    private const string LASTFIRSTNAME = "Nom Prénom";
    private const string CATEGORY = "Catégorie";
    private const string CLUB = "Club";
    private const string TIME = "Temps";
    private const string PROGRAM = "Programme";
    private const string LANE = "Couloir";
    private const string FINALES = "Finales";
    private const string CHRONO = "Chrono";
    private const string PASSAGEORDER = "Ordre de passage";
    private const string REFEREE = "JUGE ";
    private const string SCOREOUTOF10 = "Note sur 10";
    private const string COEFF = "Coeff";
    private const string TOTALUPPER = "TOTAL";
    private const string REFEREENAMES = "Nom des Officiel";
    private const string COMMENTS = "Commentaires";
    private const string RACESHEATS = "Epreuves - Séries";
    private const string TIME1 = "Temps 1";
    private const string TIME2 = "Temps 2";
    private const string TIME3 = "Temps 3";
    private const string TIMETOTAL = "Temps Total";
    private const string FINISH = "Arrivées";
    private const string RACESHEATSPOSITIONS = "Epreuve - Séries / Positions";
    private const string DQ = "DQ";
    private const string FORFEIT = "Forfait";
    private const string FA = "FA";
    private const string FB = "FB";
    private const string FORFEITFINAL = "Forfait final";
    private const string NATIONALITY = "Nationalité";
    private const string PROGRAMBEACHHEATS = "Programme séries côtieres";
    private const string RACES = "Epreuves";
    private const string PARTICIPANTS = "Participants";
    private const string MAXGRADE = "Note Max";
    private const string DIXPERCENTRESULTS = "Resultats 10 pourcent";
    private const string ENTRYTIME = "Temps engagement";
    private const string TENPERCENT = "10 %";
    private const string DISQUALIFICATION = "Disqualification";
    private const string DQCODE = "Code DQ";
    private const string HOUR = "Heure";
    private const string NAME = "Nom";
    private const string FIRSTNAME = "Prénom";
    private const string REFEREERACE = "Déclaration des arbitres";
    private const string GENDER = "Sexe";
    private const string LEVEL = "Niveau";
    private const string AVAILABILITIES = "Disponibilités";

    private const int VERYBIGSIZE = 20;
    private const int BIGSIZE = 16;
    private const int MEDIUMSIZE = 14;
    private const int NORMALSIZE = 12;
    private const int SMALLSIZE = 10;
    #endregion

    public bool IsFileExist(EnumRSM.ExcelType excelType, string name)
    {
        FileInfo fi = GetFileName(excelType, name);

        return fi.Exists;
    }

    private FileInfo GetFileName(EnumRSM.ExcelType excelType, string name)
    {
        string dirPath = Path.Combine(Properties.Settings.Default.DirPath, "Documents");
        string fileName = excelType.ToString() + " - " + name + ".xlsx";

        DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }

        FileInfo fi = new FileInfo(dirPath + "\\" + fileName);

        return fi;
    }
    public string GenerateStartList(Competition competition, IReadOnlyList<Race> races, IReadOnlyList<Referee> referees)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(STARTLIST);

        //Print setup
        worksheet.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        //worksheet.PageSetup.PagesWide = 1;
        worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        worksheet.PageSetup.Margins.Top = 1.2;
        worksheet.PageSetup.Margins.Bottom = 0.50;
        worksheet.PageSetup.Margins.Left = 0.25;
        worksheet.PageSetup.Margins.Right = 0.25;
        worksheet.PageSetup.Margins.Header = 0.35;
        worksheet.PageSetup.Margins.Footer = 0.35;
        worksheet.PageSetup.AdjustTo(60);


        //Header Footer
        worksheet.PageSetup.Header.Center.AddText(STARTLIST).SetBold(true).SetFontSize(BIGSIZE);
        worksheet.PageSetup.Header.Center.AddText("\n");
        worksheet.PageSetup.Header.Center.AddText(competition.Name);
        worksheet.PageSetup.Header.Center.AddText("\n");
        worksheet.PageSetup.Header.Center.AddText(competition.Location);
        worksheet.PageSetup.Header.Center.AddText("\n");
        string date = competition.BeginDate.ToShortDateString() + " - " + competition.EndDate.ToShortDateString();
        worksheet.PageSetup.Header.Center.AddText(date);

        worksheet.PageSetup.Footer.Right.AddText(XLHFPredefinedText.PageNumber, XLHFOccurrence.AllPages);
        worksheet.PageSetup.Footer.Right.AddText(" / ", XLHFOccurrence.AllPages);
        worksheet.PageSetup.Footer.Right.AddText(XLHFPredefinedText.NumberOfPages, XLHFOccurrence.AllPages);
        worksheet.PageSetup.Footer.Left.AddText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), XLHFOccurrence.AllPages);

        int row = 2;
        int column = 1;

        #region Referees
        worksheet.Cell(row, column).Value = REFEREERACE;
        worksheet.Cell(row, column).Style.Font.Underline = XLFontUnderlineValues.Single;
        worksheet.Cell(row, column).Style.Font.Bold = true;
        worksheet.Cell(row, column).Style.Font.FontSize = BIGSIZE;

        worksheet.Range(row, column, row, column + 5).Merge();
        ++row;
        column = 2;

        worksheet.Cell(row, column).Value = LASTFIRSTNAME;
        worksheet.Cell(row, column).Style.Font.Bold = true;
        worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

        ++column;
        worksheet.Cell(row, column).Value = GENDER;
        worksheet.Cell(row, column).Style.Font.Bold = true;
        worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

        ++column;
        worksheet.Cell(row, column).Value = CLUB;
        worksheet.Cell(row, column).Style.Font.Bold = true;
        worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

        ++column;
        worksheet.Cell(row, column).Value = LEVEL;
        worksheet.Cell(row, column).Style.Font.Bold = true;
        worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

        ++column;
        worksheet.Cell(row, column).Value = AVAILABILITIES;
        worksheet.Cell(row, column).Style.Font.Bold = true;
        worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;
        
        var rangeHeader = worksheet.Range(row, 2, row, column);
        rangeHeader.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        ++row;
        referees.ToList().Sort(new RefereeNameLevelComparer());
        foreach (Referee referee in referees)
        {
            column = 2;
            worksheet.Cell(row, column).Value = referee.FullName;
            worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

            ++column;
            worksheet.Cell(row, column).Value = EnumRSM.GetEnumDescription(referee.Gender);
            worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

            ++column;
            worksheet.Cell(row, column).Value = referee.Club.Name;
            worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

            ++column;
            worksheet.Cell(row, column).Value = referee.RefereeLevel.ToString();
            worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

            ++column;
            worksheet.Cell(row, column).Value = referee.RefereeAvailabilitiesLabel;
            worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

            ++row;
        }
        #endregion Referees

        #region Athletes
        ++row;
        races.ToList().Sort(new RaceNameAndGenderComparer());
        foreach (Race race in races)
        {
            column = 1;

            worksheet.Cell(row, column).Value = race.Label;
            worksheet.Cell(row, column).Style.Font.Underline = XLFontUnderlineValues.Single;
            worksheet.Cell(row, column).Style.Font.Bold = true;
            worksheet.Cell(row, column).Style.Font.FontSize = BIGSIZE;

            worksheet.Range(row, column, row, column + 5).Merge();
            ++row;

            List<Team> teams;
            if (race.Speciality == EnumRSM.Speciality.EauPlate)
            {
                column = 2;

                worksheet.Cell(row, column).Value = LASTFIRSTNAME;
                worksheet.Cell(row, column).Style.Font.Bold = true;
                worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

                ++column;
                worksheet.Cell(row, column).Value = CATEGORY;
                worksheet.Cell(row, column).Style.Font.Bold = true;
                worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

                ++column;
                worksheet.Cell(row, column).Value = CLUB;
                worksheet.Cell(row, column).Style.Font.Bold = true;
                worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

                ++column;
                worksheet.Cell(row, column).Value = TIME;
                worksheet.Cell(row, column).Style.Font.Bold = true;
                worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

                rangeHeader = worksheet.Range(row, 2, row, column);
                rangeHeader.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                teams = race.GetAvailableTeams();
                teams.Sort(new TeamEntryTimeComparer());

                ++row;
                int adjustStartRow = row;
                foreach (Team team in teams)
                {
                    column = 2;

                    if (race.NumberByTeam == 1)
                    {
                        IndividualTeam indivTeam = team as IndividualTeam;
                        Athlete athlete = indivTeam.Athlete;

                        worksheet.Cell(row, column).Value = athlete.FullName;
                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

                        ++column;

                        //if (athlete.Category != null)
                        //    worksheet.Cell(row, column).Value = athlete.Category.Name;
                        //else
                        //    worksheet.Cell(row, column).Value = String.Empty;

                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

                        ++column;
                        worksheet.Cell(row, column).Value = athlete.Club.Name;
                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

                        ++column;
                        worksheet.Cell(row, column).Value = indivTeam.EntryTimeLabel;
                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

                        ++row;
                    }
                    else
                    {
                        RelayTeam relayTeam = team as RelayTeam;
                        List<Athlete> athletes = new List<Athlete>();

                        int startRow = row;
                        foreach (Athlete athlete in athletes)
                        {
                            worksheet.Cell(row, 2).Value = athlete.FullName;
                            worksheet.Cell(row, 2).Style.Font.FontSize = NORMALSIZE;

                            //worksheet.Cell(row, 3).Value = athlete.Category.Name;
                            worksheet.Cell(row, 3).Style.Font.FontSize = NORMALSIZE;

                            ++row;
                        }
                        row = startRow;

                        column = 4;
                        worksheet.Cell(row, column).Value = relayTeam.GetClub().Name;
                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;
                        worksheet.Range(row, column, row + athletes.Count - 1, column).Merge();

                        ++column;
                        worksheet.Cell(row, column).Value = relayTeam.EntryTimeLabel;
                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;
                        worksheet.Range(row, column, row + athletes.Count - 1, column).Merge();

                        row += athletes.Count;
                    }
                }

                ++row;
            }
            else
            {
                column = 2;

                ++column;
                worksheet.Cell(row, column).Value = LASTFIRSTNAME;
                worksheet.Cell(row, column).Style.Font.Bold = true;
                worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

                ++column;
                worksheet.Cell(row, column).Value = CATEGORY;
                worksheet.Cell(row, column).Style.Font.Bold = true;
                worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

                ++column;
                worksheet.Cell(row, column).Value = CLUB;
                worksheet.Cell(row, column).Style.Font.Bold = true;
                worksheet.Cell(row, column).Style.Font.FontSize = MEDIUMSIZE;

                rangeHeader = worksheet.Range(row, 2, row, column);
                rangeHeader.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                teams = race.GetAvailableTeams();
                teams.Sort(new TeamClubComparer());

                ++row;

                foreach (Team team in teams)
                {
                    column = 2;

                    if (race.NumberByTeam == 1)
                    {
                        Athlete athlete = (team as IndividualTeam).Athlete;

                        ++column;
                        worksheet.Cell(row, column).Value = athlete.FullName;
                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

                        ++column;
                        worksheet.Cell(row, column).Value = "";
                        //if (athlete.Category != null)
                        //{
                        //    worksheet.Cell(row, column).Value = athlete.Category.Name;
                        //}

                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

                        ++column;
                        worksheet.Cell(row, column).Value = athlete.Club.Name;
                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;

                        ++row;
                    }
                    else
                    {
                        List<Athlete> athletes = (team as RelayTeam).Athletes.ToList();
                        int startRow = row;

                        foreach (Athlete athlete in athletes)
                        {
                            worksheet.Cell(row, 2).Value = athlete.FullName;
                            worksheet.Cell(row, 2).Style.Font.FontSize = NORMALSIZE;

                            worksheet.Cell(row, 3).Value = "";
                            //if (athlete.Category != null)
                            //{
                            //    worksheet.Cell(row, 3).Value = athlete.Category.Name;
                            //}
                            worksheet.Cell(row, 3).Style.Font.FontSize = NORMALSIZE;

                            ++row;
                        }
                        row = startRow;

                        column = 4;
                        worksheet.Cell(row, column).Value = (team as RelayTeam).GetClub().Name;
                        worksheet.Cell(row, column).Style.Font.FontSize = NORMALSIZE;
                        worksheet.Range(row, column, row + athletes.Count - 1, column).Merge();

                        row += athletes.Count;
                    }
                }
                ++row;
            }

        }
        #endregion Athletes

        //Adjust column to content
        worksheet.Columns().AdjustToContents();

        try
        {
            FileInfo fi = GetFileName(EnumRSM.ExcelType.STARTLIST, competition.Name);
            workbook.SaveAs(fi.FullName);

            return fi.FullName;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public string GenerateClubList(Competition competition, List<Club> clubs)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(STARTLIST);

        //Print setup
        worksheet.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        //worksheet.PageSetup.PagesWide = 1;
        worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        worksheet.PageSetup.Margins.Top = 1.2;
        worksheet.PageSetup.Margins.Bottom = 0.50;
        worksheet.PageSetup.Margins.Left = 0.25;
        worksheet.PageSetup.Margins.Right = 0.25;
        worksheet.PageSetup.Margins.Header = 0.35;
        worksheet.PageSetup.Margins.Footer = 0.35;
        worksheet.PageSetup.AdjustTo(60);


        //Header Footer
        worksheet.PageSetup.Header.Center.AddText(CLUBS).SetBold(true).SetFontSize(BIGSIZE);
        worksheet.PageSetup.Header.Center.AddText("\n");
        worksheet.PageSetup.Header.Center.AddText(competition.Name);
        worksheet.PageSetup.Header.Center.AddText("\n");
        worksheet.PageSetup.Header.Center.AddText(competition.Location);
        worksheet.PageSetup.Header.Center.AddText("\n");
        string date = competition.BeginDate.ToShortDateString() + " - " + competition.EndDate.ToShortDateString();
        worksheet.PageSetup.Header.Center.AddText(date);

        worksheet.PageSetup.Footer.Right.AddText(XLHFPredefinedText.PageNumber, XLHFOccurrence.AllPages);
        worksheet.PageSetup.Footer.Right.AddText(" / ", XLHFOccurrence.AllPages);
        worksheet.PageSetup.Footer.Right.AddText(XLHFPredefinedText.NumberOfPages, XLHFOccurrence.AllPages);
        worksheet.PageSetup.Footer.Left.AddText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), XLHFOccurrence.AllPages);

        int row = 2;
        int column = 1;




        //Adjust column to content
        worksheet.Columns().AdjustToContents();

        try
        {
            FileInfo fi = GetFileName(EnumRSM.ExcelType.STARTLIST, competition.Name);
            workbook.SaveAs(fi.FullName);

            return fi.FullName;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
