using System.ComponentModel;
using System.IO;

using ClosedXML.Excel;

using RescueScoreManager.Comparer;
using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public class ExcelService : IExcelService
{
    #region CONST VALUE
    const string CLUBS = "Clubs";
    const string POSITION = "Position";
    const string POINTS = "Points";
    const string GENERAL = "General";
    const string DETAIL = "Detail";
    const string RESULTS = "Resultats";
    const string COEFFICIENT = "Coefficient";
    const string STARTLIST = "StartList";
    const string NUMBER = "Numéro";
    const string LASTFIRSTNAME = "Nom Prénom";
    const string CATEGORY = "Catégorie";
    const string CLUB = "Club";
    const string TIME = "Temps";
    const string PROGRAM = "Programme";
    const string LANE = "Couloir";
    const string FINALES = "Finales";
    const string CHRONO = "Chrono";
    const string PASSAGEORDER = "Ordre de passage";
    const string REFEREE = "JUGE ";
    const string SCOREOUTOF10 = "Note sur 10";
    const string COEFF = "Coeff";
    const string TOTALUPPER = "TOTAL";
    const string REFEREENAMES = "Nom des Officiel";
    const string COMMENTS = "Commentaires";
    const string RACESHEATS = "Epreuves - Séries";
    const string TIME1 = "Temps 1";
    const string TIME2 = "Temps 2";
    const string TIME3 = "Temps 3";
    const string TIMETOTAL = "Temps Total";
    const string FINISH = "Arrivées";
    const string RACESHEATSPOSITIONS = "Epreuve - Séries / Positions";
    const string DQ = "DQ";
    const string FORFEIT = "Forfait";
    const string FA = "FA";
    const string FB = "FB";
    const string FORFEITFINAL = "Forfait final";
    const string NATIONALITY = "Nationalité";
    const string PROGRAMBEACHHEATS = "Programme séries côtieres";
    const string RACES = "Epreuves";
    const string PARTICIPANTS = "Participants";
    const string MAXGRADE = "Note Max";
    const string DIXPERCENTRESULTS = "Resultats 10 pourcent";
    const string ENTRYTIME = "Temps engagement";
    const string TENPERCENT = "10 %";
    const string DISQUALIFICATION = "Disqualification";
    const string DQCODE = "Code DQ";
    const string HOUR = "Heure";
    const string NAME = "Nom";
    const string FIRSTNAME = "Prénom";
    const string REFEREERACE = "Déclaration des arbitres";
    const string GENDER = "Sexe";
    const string LEVEL = "Niveau";
    const string AVAILABILITIES = "Disponibilités";

    const int VERYBIGSIZE = 20;
    const int BIGSIZE = 16;
    const int MEDIUMSIZE = 14;
    const int NORMALSIZE = 12;
    const int SMALLSIZE = 10;
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
    public string GenerateStartList(Competition competition, List<Race> races,List<Referee> referees)
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
        referees.Sort(new RefereeNameLevelComparer());
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
        races.Sort(new RaceNameAndGenderComparer());
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

                        if (athlete.Category != null)
                            worksheet.Cell(row, column).Value = athlete.Category.Name;
                        else
                            worksheet.Cell(row, column).Value = String.Empty;

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

                            worksheet.Cell(row, 3).Value = athlete.Category.Name;
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
                        if (athlete.Category != null)
                        {
                            worksheet.Cell(row, column).Value = athlete.Category.Name;
                        }

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
                            if (athlete.Category != null)
                            {
                                worksheet.Cell(row, 3).Value = athlete.Category.Name;
                            }
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
}
