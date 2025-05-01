using System.ComponentModel;
using System.IO;
using System.Security.Claims;
using System.Xml.Linq;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using RescueScoreManager.Comparer;
using RescueScoreManager.Data;
using RescueScoreManager.Helpers;
using RescueScoreManager.Properties;

using static System.Reflection.Metadata.BlobBuilder;

namespace RescueScoreManager.Services;

public class XMLService : IXMLService
{
    public bool Loaded { get; set; }
    public required Competition Competition { get; set; }
    public List<Category> Categories { get; set; } = new List<Category>();
    public List<Club> Clubs { get; set; } = new List<Club>();
    public List<Licensee> Licensees { get; set; } = new List<Licensee>();
    public List<Athlete> Athletes { get; set; } = new List<Athlete>();
    public List<Referee> Referees { get; set; } = new List<Referee>();
    public List<Race> Races { get; set; } = new List<Race>();
    public List<Team> Teams { get; set; } = new List<Team>();



    public Competition GetCompetition() => Competition;
    public List<Category> GetCategories()
    {
        Categories.Sort(new CategoryComparer());
        return Categories;
    }
    public List<Club> GetClubs()
    {
        return Clubs;
    }
    public List<Licensee> GetLicensees()
    {
        return Licensees;
    }
    public List<Athlete> GetAthletes()
    {
        return Athletes;
    }
    public List<Referee> GetReferees()
    {
        return Referees;
    }
    public List<Race> GetRaces()
    {
        return Races;
    }
    public List<Team> GetTeams()
    {
        return Teams;
    }
    public void SetPath(string name)
    {
        string cleanedName = TextHelper.CleanedText(name);

        FileInfo fi = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RescueScore", cleanedName, cleanedName + ".ffss"));

        Properties.Settings.Default.FilePath = fi.FullName;
        Properties.Settings.Default.DirPath = fi.DirectoryName;
        Properties.Settings.Default.Save();
    }
    public void SetPath(FileInfo file)
    {
        Properties.Settings.Default.FilePath = file.FullName;
        Properties.Settings.Default.DirPath = file.DirectoryName;
        Properties.Settings.Default.Save();
    }

    public string GetFilePath()
    {
        return Properties.Settings.Default.FilePath;
    }

    public string GetDirPath()
    {
        return Properties.Settings.Default.DirPath;
    }


    public void Initialize(Competition competition, List<Category> categories, List<Club> clubs, List<Licensee> licensees, List<Race> races, List<Team> teams)
    {
        Competition = competition;
        Categories = categories;
        Clubs = clubs;
        Licensees = licensees;

        Clubs.Sort(new ClubNameComparer());
        Licensees.Sort(new LicenseeClubFullNameComparer());

        int orderNumber = 1;
        foreach (Licensee licensee in licensees)
        {
            if (licensee is Athlete)
            {
                ((Athlete)licensee).OrderNumber = orderNumber;
                ++orderNumber;
                Athletes.Add((Athlete)licensee);
            }
            if (licensee is Referee)
            {
                Referees.Add((Referee)licensee);
            }
        }

        Races = races;
        Teams = teams;

        //initialize order number
    }

    public void Reset()
    {
        Categories.Clear();
        Clubs.Clear();
        Licensees.Clear();
        Athletes.Clear();
        Referees.Clear();
        Races.Clear();
        Teams.Clear();
    }

    public void Load()
    {
        XDocument xDoc = XDocument.Load(Properties.Settings.Default.FilePath);

        XElement rootElement = xDoc.Element(Properties.Resources.Root_XMI);

        //get event
        #region Event
        XElement eventElement = rootElement.Element(Properties.Resources.Competition_XMI);
        Competition = new Competition(xElement: eventElement);
        #endregion

        //get setting 
        #region Setting
        //XElement settingElement = rootElement.Element("Setting");
        //AppSetting setting = new AppSetting(settingElement);
        //dataService.Setting = setting;
        #endregion

        //get all the categories
        #region Categories
        IEnumerable<XElement> catElements = rootElement.Descendants(Properties.Resources.Category_XMI);
        foreach (XElement catElement in catElements)
        {
            Category category = new Category(catElement);
            if (Categories.Contains(category) == false)
            {
                Categories.Add(category);
            }
        }
        Categories = Categories.OrderByDescending(cat => cat.AgeMin).ToList();
        #endregion

        //get all the clubs and licensee
        #region Club and Licensee
        IEnumerable<XElement> clubsElement = rootElement.Descendants(Properties.Resources.Club_XMI);
        foreach (XElement clubElement in clubsElement)
        {
            Club club = new Club(clubElement);
            club.Competition = Competition;
            club.CompetitionId = Competition.Id;
            Competition.Clubs.Add(club);

            IEnumerable<XElement> athletesElement = clubElement.Descendants(Properties.Resources.Athlete_XMI);
            IEnumerable<XElement> refereesElement = clubElement.Descendants(Properties.Resources.Referee_XMI);
            foreach (XElement athElement in athletesElement)
            {
                Athlete licensee = new Athlete(athElement, Categories);
                licensee.Club = club;
                licensee.ClubId = club.Id;
                Category category = Categories.Find(cat => cat.Id == licensee.CategoryId);
                category.Athletes.Add(licensee);
                licensee.Category = category;

                club.AddLicensee(licensee);
                Licensees.Add(licensee);
                Athletes.Add(licensee);
            }
            foreach (XElement refElement in refereesElement)
            {
                Referee licensee = new Referee(refElement);
                licensee.Club = club;
                licensee.ClubId = club.Id;

                club.AddLicensee(licensee);
                Licensees.Add(licensee);
                Referees.Add(licensee);
            }
            Clubs.Add(club);
        }
        #endregion

        //get all the races and teams
        #region Races and Teams
        IEnumerable<XElement> racesElement = rootElement.Descendants(Properties.Resources.Race_XMI);

        foreach (XElement raceElement in racesElement)
        {
            Race race = new Race(raceElement, Categories);
            race.Competition = Competition;
            race.CompetitionId = Competition.Id;

            IEnumerable<XElement> indivTeamElement = raceElement.Descendants(Properties.Resources.IndividualTeam_XMI);
            IEnumerable<XElement> relayTeamElement = raceElement.Descendants(Properties.Resources.RelayTeam_XMI);
            IEnumerable<XElement> teamsElement = raceElement.Descendants(Properties.Resources.Team_XMI);
            Team team = null;
            foreach (XElement teamElement in indivTeamElement)
            {
                team = new IndividualTeam(teamElement, Athletes);
                team.Race = race;
                team.RaceId = race.Id;
                race.AddTeam(team);
                Teams.Add(team);
            }
            foreach (XElement teamElement in relayTeamElement)
            {
                team = new RelayTeam(teamElement, Athletes);
                team.Race = race;
                team.RaceId = race.Id;
                race.AddTeam(team);
                Teams.Add(team);
            }

            Races.Add(race);
        }

        #endregion

        //get disqualifications
        #region Disqualifications

        // ImportExcelService.ImportDisqualification();
        #endregion

        //Get all the SERCDefinition
        #region SERCDefinitions

        //IEnumerable<XElement> sercDefinitionsXElement = rootElement.Descendants(Properties.Resources.SERCDefinition_XMI);
        //List<SERCDefinition> sercDefinitions = new List<SERCDefinition>();

        //foreach (XElement sercElement in sercDefinitionsXElement)
        //{
        //    SERCDefinition sercDefinition = new SERCDefinition(sercElement);

        //    IEnumerable<XElement> sercCriteriasXElement = sercElement.Descendants(Properties.Resources.SERCCriteria_XMI);
        //    foreach (XElement sercCriteriaElement in sercCriteriasXElement)
        //    {
        //        SERCCriteria sercCriteria = new SERCCriteria(sercCriteriaElement, sercDefinition);
        //        sercDefinition.SERCCriterias.Add(sercCriteria);
        //    }

        //    sercDefinitions.Add(sercDefinition);
        //}

        //dataService.SERCDefinitions = sercDefinitions;
        #endregion

        //Get all the BeachProgram
        #region BeachProgram

        //IEnumerable<XElement> sercDefinitionsXElement = rootElement.Descendants(Properties.Resources.SERCDefinition_XMI);
        //List<SERCDefinition> sercDefinitions = new List<SERCDefinition>();
        //XElement beachProgramElement = rootElement.Element(Properties.Resources.BeachProgram_XMI);
        //BeachProgram beachProgram = null;

        //if (beachProgramElement != null)
        //{
        //    beachProgram = new BeachProgram();

        //    IEnumerable<XElement> beachProgramItemsXElement = rootElement.Descendants(Properties.Resources.BeachProgramItem_XMI);

        //    foreach (XElement beachProgramItemElement in beachProgramItemsXElement)
        //    {
        //        int raceId = int.Parse(beachProgramItemElement.Attribute(Properties.Resources.Race_XMI).Value);

        //        int catId = 0;
        //        int.TryParse(beachProgramItemElement.Attribute(Properties.Resources.Category_XMI).Value, out catId);

        //        BeachProgramItem beachProgramItem = new BeachProgramItem(beachProgramItemElement, dataService.GetRaceById(raceId), dataService.GetCategoryById(catId));

        //        IEnumerable<XElement> beachProgramDatasXElement = beachProgramItemElement.Descendants(Properties.Resources.BeachProgramData_XMI);

        //        foreach (XElement beachProgramDataXElement in beachProgramDatasXElement)
        //        {
        //            BeachProgramData pData = new BeachProgramData(beachProgramDataXElement);

        //            IEnumerable<XElement> beachProgramDetailsXElement = beachProgramDataXElement.Descendants(Properties.Resources.BeachProgramDetail_XMI);
        //            foreach (XElement beachProgramDetailXElement in beachProgramDetailsXElement)
        //            {
        //                BeachProgramDetail pDetail = new BeachProgramDetail(beachProgramDetailXElement);

        //                pData.BeachProgramDetails.Add(pDetail);
        //            }

        //            pData.Program = " [" + string.Join(";", pData.BeachProgramDetails.Select(pDetail => pDetail.NumberByHeat.ToString()).ToArray()) + "]";
        //            pData.ProgramNumbers = pData.BeachProgramDetails.Select(pDetail => pDetail.NumberByHeat).ToArray();

        //            beachProgramItem.BeachProgramDatas.Add(pData);
        //        }

        //        beachProgram.BeachProgramItems.Add(beachProgramItem);
        //    }
        //}
        //dataService.BeachProgram = beachProgram;
        #endregion

        #region Schedule
        //XElement scheduleElement = rootElement.Element(Properties.Resources.Schedule_XMI);
        //Schedule schedule = null;
        //if (scheduleElement != null)
        //{
        //    schedule = new Schedule(scheduleElement);

        //    ScheduleDay scheduleDay = null;
        //    ScheduleDetail scheduleDetail = null;

        //    IEnumerable<XElement> scheduleDaysXElement = rootElement.Descendants(Properties.Resources.ScheduleDay_XMI);
        //    foreach (XElement scheduleDayXElement in scheduleDaysXElement)
        //    {
        //        scheduleDay = new ScheduleDay(scheduleDayXElement, schedule);

        //        IEnumerable<XElement> scheduleItemsXElement = scheduleDayXElement.Descendants(Properties.Resources.ScheduleItems_XMI);
        //        foreach (XElement scheduleItemXElement in scheduleItemsXElement)
        //        {
        //            foreach (XElement sItemXElement in scheduleItemXElement.Descendants())
        //            {
        //                if (sItemXElement.Name == Properties.Resources.SwimScheduleItem_XMI)
        //                {
        //                    //IEnumerable<XElement> swimScheduleItemsXElement = sItemXElement.Descendants(Properties.Resources.SwimScheduleItem_XMI);
        //                    //foreach (XElement swimScheduleItemXElement in swimScheduleItemsXElement)
        //                    //{
        //                    SwimScheduleItem scheduleItem = new SwimScheduleItem(sItemXElement, scheduleDay);

        //                    IEnumerable<XElement> scheduleDetailsXElement = sItemXElement.Descendants(Properties.Resources.ScheduleDetails_XMI);
        //                    foreach (XElement scheduleDetailXElement in scheduleDetailsXElement)
        //                    {
        //                        IEnumerable<XElement> xElements = scheduleDetailXElement.Elements();
        //                        foreach (XElement xElement in xElements)
        //                        {
        //                            if (xElement.Name == Properties.Resources.SwimRaceScheduleDetail_XMI)
        //                            {
        //                                int raceId = int.Parse(xElement.Attribute(Properties.Resources.Race_XMI).Value);
        //                                Race race = dataService.GetRaceById(raceId);

        //                                int catId = 0;
        //                                int.TryParse(xElement.Attribute(Properties.Resources.Category_XMI).Value, out catId);
        //                                Category cat = dataService.GetCategoryById(catId);

        //                                scheduleDetail = new SwimRaceScheduleDetail(xElement, scheduleItem, race, cat);
        //                            }
        //                            else if (xElement.Name == Properties.Resources.BreakScheduleDetail_XMI)
        //                            {
        //                                scheduleDetail = new BreakScheduleDetail(xElement, scheduleItem);
        //                            }
        //                            scheduleItem.ScheduleDetails.Add(scheduleDetail);
        //                        }
        //                    }
        //                    scheduleDay.ScheduleItems.Add(scheduleItem);
        //                    //}
        //                }
        //                else if (sItemXElement.Name == Properties.Resources.BeachScheduleItem_XMI)
        //                {
        //                    //IEnumerable<XElement> beachScheduleItemsXElement = sItemXElement.Descendants(Properties.Resources.BeachScheduleItem_XMI);

        //                    //foreach (XElement beachScheduleItemXElement in beachScheduleItemsXElement)
        //                    //{
        //                    BeachScheduleItem beachScheduleItem = new BeachScheduleItem(sItemXElement, scheduleDay);

        //                    IEnumerable<XElement> scheduleLocationsXElement = sItemXElement.Descendants(Properties.Resources.ScheduleLocation_XMI);
        //                    foreach (XElement scheduleLocationXElement in scheduleLocationsXElement)
        //                    {
        //                        ScheduleLocation scheduleLocation = new ScheduleLocation(schLocationXElement: scheduleLocationXElement, beachItem: beachScheduleItem);

        //                        IEnumerable<XElement> scheduleDetailsXElement = scheduleLocationXElement.Descendants(Properties.Resources.ScheduleDetails_XMI);
        //                        foreach (XElement scheduleDetailXElement in scheduleDetailsXElement)
        //                        {
        //                            IEnumerable<XElement> xElements = scheduleDetailXElement.Elements();
        //                            foreach (XElement xElement in xElements)
        //                            {
        //                                if (xElement.Name == Properties.Resources.BeachRaceScheduleDetail_XMI)
        //                                {
        //                                    int raceId = int.Parse(xElement.Attribute(Properties.Resources.Race_XMI).Value);
        //                                    Race race = dataService.GetRaceById(raceId);

        //                                    int catId = 0;
        //                                    int.TryParse(xElement.Attribute(Properties.Resources.Category_XMI).Value, out catId);
        //                                    Category cat = dataService.GetCategoryById(catId);

        //                                    scheduleDetail = new BeachRaceScheduleDetail(beachRaceSchDetailXElement: xElement,
        //                                                                                 item: beachScheduleItem,
        //                                                                                 location: scheduleLocation,
        //                                                                                 race: race,
        //                                                                                 category: cat);
        //                                }
        //                                else if (xElement.Name == Properties.Resources.BreakScheduleDetail_XMI)
        //                                {
        //                                    scheduleDetail = new BreakScheduleDetail(xElement, beachScheduleItem);
        //                                }

        //                                scheduleLocation.ScheduleDetails.Add(scheduleDetail);
        //                            }
        //                        }

        //                        beachScheduleItem.ScheduleLocations.Add(scheduleLocation);
        //                    }

        //                    scheduleDay.ScheduleItems.Add(beachScheduleItem);
        //                    //}
        //                }
        //            }
        //        }

        //        schedule.ScheduleDays.Add(scheduleDay);
        //    }
        //}
        //dataService.Schedule = schedule;

        #endregion

        //Get all the Meetings
        #region Meetings
        //IEnumerable<XElement> meetingsXElement = rootElement.Descendants(Properties.Resources.Meeting_XMI);
        //Meetings meetings = new Meetings();
        //foreach (XElement meetingElement in meetingsXElement)
        //{
        //    Meeting meeting = new Meeting(meetingElement);

        //    IEnumerable<XElement> meetingElementsElement = meetingElement.Descendants(Properties.Resources.MeetingElement_XMI);
        //    foreach (XElement mElementXElement in meetingElementsElement)
        //    {
        //        MeetingElement mElement = null;

        //        if (meeting.MeetingType == SpecialityType.EauPlate)
        //            mElement = new SwimMeetingElement(mElementXElement, meeting);
        //        else
        //            mElement = new BeachMeetingElement(mElementXElement, meeting);

        //        IEnumerable<XElement> roundsElement = mElementXElement.Descendants(Properties.Resources.Round_XMI);
        //        foreach (XElement roundXElement in roundsElement)
        //        {
        //            Category cat = null;
        //            string catId = roundXElement.Attribute(Properties.Resources.Category_XMI).Value;
        //            if (!String.IsNullOrEmpty(catId))
        //                cat = dataService.GetCategoryById(int.Parse(catId));

        //            Race race = dataService.GetRaceById(int.Parse(roundXElement.Attribute(Properties.Resources.Race_XMI).Value));
        //            Round round = new Round(roundXElement, cat, mElement, race);
        //            bool isSERC = race.Name.Contains("SERC");

        //            mElement.AddRound(round);

        //            IEnumerable<XElement> heatsElement = roundXElement.Descendants(Properties.Resources.Heat_XMI);
        //            foreach (XElement heatXElement in heatsElement)
        //            {
        //                Heat heat = new Heat(heatXElement);

        //                heat.Round = round;
        //                IEnumerable<XElement> heatResultsElement = heatXElement.Descendants(Properties.Resources.HeatResult_XMI);
        //                foreach (XElement heatResultXElement in heatResultsElement)
        //                {
        //                    int id = int.Parse(heatResultXElement.Attribute(Properties.Resources.Team_XMI).Value);
        //                    Team team = (heat.IsFinalA || heat.IsFinalB) ? dataService.GetTeamById(id) : dataService.GetTeamByIdAndRace(id, mElement.Race);
        //                    HeatResult heatResult = null;
        //                    //Disqualification disq = dataService.GetDisqualificationByCode(heatResultXElement.Attribute(Properties.Resources.Disqualification_XMI).Value);
        //                    if (isSERC)
        //                    {
        //                        Guid criteriaId = Guid.Parse(heatResultXElement.Attribute(Properties.Resources.SERCCriteria_XMI).Value);
        //                        SERCCriteria criteria = dataService.GetSERCCriteriaById(criteriaId);

        //                        heatResult = new SERCHeatResult(heatResultXElement, heat, team, criteria);
        //                    }
        //                    else if (race.RaceType == SpecialityType.EauPlate)
        //                    {
        //                        heatResult = new SwimHeatResult(heatResultXElement, heat, team);
        //                    }
        //                    else
        //                    {
        //                        heatResult = new BeachHeatResult(heatResultXElement, heat, team);
        //                    }
        //                    heat.HeatResults.Add(heatResult);
        //                }

        //                round.Heats.Add(heat);
        //            }
        //        }

        //        meeting.MeetingElements.Add(mElement);
        //    }
        //    meetings.Add(meeting);
        //}

        ////Create link with related meeting
        //foreach (XElement meetingXElement in meetingsXElement)
        //{
        //    Guid meetingId = Guid.Parse(meetingXElement.Attribute(Properties.Resources.Id_XMI).Value);
        //    Meeting meeting = meetings.ToList().Find(m => m.Id.Equals(meetingId));

        //    Meeting relatedMeeting = null;
        //    if (meetingXElement.Attribute(Properties.Resources.RelatedMeeting_XMI).Value != string.Empty)
        //    {
        //        Guid relatedMeetingId = Guid.Parse(meetingXElement.Attribute(Properties.Resources.RelatedMeeting_XMI).Value);
        //        relatedMeeting = meetings.ToList().Find(m => m.Id.Equals(relatedMeetingId));
        //    }

        //    meeting.RelatedMeeting = relatedMeeting;
        //}
        //dataService.Meetings = meetings;
        #endregion
        //dataService.Event = newEvent;

        //if (Settings.Default.IsDesign)
        //    DataAccessService.Instance.IsLoaded = true;
        //Messenger.Default.Send(new IsDataLoadedMessage() { IsDataLoaded = true });
        Loaded = true;
    }

    public bool Save()
    {
        if (Competition != null)
        {
            string xmlFile = Properties.Settings.Default.FilePath;
            if (!String.IsNullOrEmpty(xmlFile) && !String.IsNullOrEmpty(xmlFile))
            {
                XDocument xDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "true")
                    );

                XElement rootXElem = new XElement(Properties.Resources.Root_XMI);

                XElement eventXElem = Competition.WriteXml();
                if (eventXElem != null)
                    rootXElem.Add(eventXElem);
                //XElement settingXElem = dataService.Setting.WriteXml();
                //if (settingXElem != null)
                //    rootXElem.Add(settingXElem);

                //if (dataService.Schedule != null)
                //{
                //    XElement scheduleXElem = dataService.Schedule.WriteXml();
                //    if (scheduleXElem != null)
                //        rootXElem.Add(scheduleXElem);
                //}
                XElement catsXElem = new XElement(Properties.Resources.Categories_XMI);
                foreach (Category category in Categories)
                {
                    XElement catXElem = category.WriteXml();
                    if (catXElem != null)
                        catsXElem.Add(catXElem);
                }
                rootXElem.Add(catsXElem);


                XElement clubsXElem = new XElement(Properties.Resources.Clubs_XMI);
                foreach (Club club in Clubs)
                {
                    XElement clubXElem = club.WriteXml();
                    if (clubXElem != null)
                        clubsXElem.Add(clubXElem);
                }
                rootXElem.Add(clubsXElem);

                XElement racesXElem = new XElement(Properties.Resources.Races_XMI);
                foreach (Race race in Races)
                {
                    XElement raceXElem = race.WriteXml();
                    if (raceXElem != null)
                        racesXElem.Add(raceXElem);
                }
                rootXElem.Add(racesXElem);


                #region SERCDefinitions

                //if (dataService.SERCDefinitions.Count > 0)
                //{
                //    XElement sercElement = new XElement(Properties.Resources.SERCDefinitions_XMI);
                //    foreach (SERCDefinition definition in dataService.SERCDefinitions)
                //    {
                //        sercElement.Add(definition.WriteXml());
                //    }

                //    rootXElem.Add(sercElement);
                //}
                #endregion SERCDefinitions

                #region BeachProgram

                //if (dataService.BeachProgram != null)
                //{
                //    XElement beachProgramElement = dataService.BeachProgram.WriteXml();

                //    rootXElem.Add(beachProgramElement);
                //}
                #endregion BeachProgram


                //XElement meetingsXElem = dataService.Meetings.WriteXml();
                //if (meetingsXElem != null)
                //    rootXElem.Add(meetingsXElem);

                xDoc.Add(rootXElem);

                FileInfo fi = new FileInfo(xmlFile);
                DirectoryInfo di = new DirectoryInfo(fi.DirectoryName);
                if (!di.Exists)
                {
                    di.Create();
                }

                xDoc.Save(fi.FullName);
                return true;
            }
            else
            {
                return false;
                throw new Exception("Error in Save()");// CustomException(Properties.Resources.CE08);
            }
        }
        return false;
    }

    public bool IsLoaded()
    {
        return Loaded;
    }

}
