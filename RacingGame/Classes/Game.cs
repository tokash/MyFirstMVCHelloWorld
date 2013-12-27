using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using SQLServerCommon;

namespace RacingGame.Models
{

    public class Game
    {
        //internal static readonly string connStringInitial = "Server=TOKASHYOS-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=master";
        //internal static readonly string connString = "Server=TOKASHYOS-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=RaceGameDB";
        //private static readonly string connString = "Server=tcp:fqw1x1y2s2.database.windows.net,1433;Database=GameRaceDB;User ID=tokash@fqw1x1y2s2;Password=Yt043112192;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        private static readonly string dbName = "RaceGameDB";//"GameRaceDB";
        //"Server=tcp:fqw1x1y2s2.database.windows.net,1433;Database=RacingGALLIpkFTF;User ID=tokash@fqw1x1y2s2;Password={your_password_here};Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"

        private static readonly string sqlCommandCreateDB = "CREATE DATABASE " + dbName + " ON PRIMARY " +
                "(NAME = " + dbName + ", " +
                "FILENAME = 'D:\\" + dbName + ".mdf', " +
                "SIZE = 3MB, MAXSIZE = 10MB, FILEGROWTH = 10%) " +
                "LOG ON (NAME = " + dbName + "_LOG, " +
                "FILENAME = 'D:\\" + dbName + ".ldf', " +
                "SIZE = 1MB, " +
                "MAXSIZE = 100MB, " +
                "FILEGROWTH = 10%)";

        internal static readonly string gameplaysTableSchema = "CREATE TABLE GamePlays (ID int IDENTITY(1,1), UserID varchar(50) NOT NULL, Section int NOT NULL, VelocityFreeway int NOT NULL, VelocityTollway int NOT NULL, PriceSubject int NOT NULL, PriceRandom int NOT NULL, Account int NOT NULL, TimeSavedForSection real NOT NULL , PRIMARY KEY (ID))";
        internal static readonly string[] GamePlaysTableColumns = { "UserID", "Section", "VelocityFreeway", "VelocityTollway", "PriceSubject", "PriceRandom", "Account", "TimeSavedForSection" };

        internal static readonly string siteStateTableSchema = "CREATE TABLE SiteState (UserID varchar(50) NOT NULL , PageNumber int NOT NULL, PageName varchar(30) NOT NULL, IsVisited BIT NOT NULL)";
        internal static readonly string[] siteStateTableColumns = { "UserID", "PageNumber", "PageName", "IsVisited" };

        internal static readonly string commentsTableSchema = "CREATE TABLE Comments (UserID varchar(50) NOT NULL , Comments varchar(4000) NOT NULL, ConfirmationCode varchar(8) NOT NULL)";
        internal static readonly string[] commentsTableColumns = { "UserID", "Comments", "ConfirmationCode" };

        private static readonly string[] tableNames = { "GamePlays", "SiteState", "Comments" };
        private static readonly string[] tableSchemas = { "CREATE TABLE GamePlays (ID int IDENTITY(1,1), UserID varchar(50) NOT NULL, Section int NOT NULL, VelocityFreeway int NOT NULL, VelocityTollway int NOT NULL, PriceSubject int NOT NULL, PriceRandom int NOT NULL, Account int NOT NULL, TimeSavedForSection real NOT NULL , PRIMARY KEY (ID))",
                                                          "CREATE TABLE SiteState (UserID varchar(50) NOT NULL , PageNumber int NOT NULL, PageName varchar(30) NOT NULL, IsVisited BIT NOT NULL)",
                                                          "CREATE TABLE Comments (UserID varchar(50) NOT NULL , Comments varchar(4000) NOT NULL, ConfirmationCode varchar(8) NOT NULL)" };
        internal static readonly int maxCharactersinComment = 4000;

        public Game(string iName = "Racing Game")
        {
            _Name = iName;
            _GameData = (NameValueCollection)ConfigurationManager.GetSection("GameConfiguration");
            _MasterConnectionString = ConfigurationManager.ConnectionStrings["MasterConnection"].ConnectionString;
            _DefaultConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            _Account = int.Parse(_GameData["StartingGamerCash"]);
            _StartingAccount = _Account;
            //_TimeLeft = double.Parse(_GameData["StartingGamerTime"]);
            //_StartingTime = _TimeLeft;
            _TimePassed = 0;
            _RoadSections = int.Parse(_GameData["RoadSections"]);
            _GameSections = _RoadSections;
            _GamePlays = new List<GamePlay>();
            _TimeSaved = 0;

            _CurrentSection = 1;

            RandomizeSpeedset();

            //CreateEmptyDB();
        }

        #region Members
        private string _Name { get; set; }

        private int _RoadSections;
        public int RoadSections
        {
            get
            {
                return _RoadSections;
            }
        }

        private int _Account;
        public int Account
        {
            get
            {
                return _Account;
            }
            set
            {
                _Account = value;
            }
        }

        //private double _TimeLeft;
        //public double TimeLeft
        //{
        //    get
        //    {
        //        return _TimeLeft;
        //    }
        //    set
        //    {
        //        _TimeLeft = value;
        //    }
        //}

        private double _TimePassed;
        public double TimePassed
        {
            get
            {
                return _TimePassed;
            }
            set
            {
                _TimePassed = value;
            }
        }

        private double _TimeSaved;
        public double TimeSaved
        {
            get
            {
                return _TimeSaved;
            }

            set
            {
                _TimeSaved = value;
            }
        }

        private int _CurrentSection;
        public int CurrentSection
        {
            get
            {
                return _CurrentSection;
            }

            set
            {
                _CurrentSection = value;
            }
        }

        private NameValueCollection _GameData;

        private List<SpeedSet> _SpeedSet = new List<SpeedSet>();
        public List<SpeedSet> SpeedSet
        {
            get
            {
                return _SpeedSet;
            }
        }

        private List<GamePlay> _GamePlays = new List<GamePlay>();
        public List<GamePlay> GamePlays
        {
            get
            {
                return _GamePlays;
            }
            //set
            //{

            //}
        }

        private static double _StartingAccount;
        public double StartingAccount { get { return _StartingAccount; } }

        private static double _StartingTime;
        public double StartingTime { get { return _StartingTime; } }

        private static int _GameSections;
        public int GameSections { get { return _GameSections; } }

        private string _MasterConnectionString;
        public string MasterConnectionString { get { return _MasterConnectionString; } }

        private string _DefaultConnectionString;
        public string DefaultConnectionString { get { return _DefaultConnectionString; } }
        #endregion

        private void RandomizeSpeedset()
        {
            string freewaySpeed = string.Empty;
            string highwaySpeed = string.Empty;


            //read speedset from config file
            //NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("GameConfiguration");
            freewaySpeed = _GameData["FreewaySpeedset"];
            highwaySpeed = _GameData["HighwaySpeedset"];

            string[] freewaySplit = freewaySpeed.Split(',');
            string[] highwaySplit = highwaySpeed.Split(',');

            //Random rnd = new Random();
            //freewaySplit = freewaySplit.OrderBy(x => rnd.Next()).ToArray();
            //highwaySplit = highwaySplit.OrderBy(x => rnd.Next()).ToArray();

            for (int i = 0; i < freewaySplit.Length; i++)
            {
                _SpeedSet.Add(new SpeedSet() {VelocityFreeway = int.Parse(freewaySplit[i]), VelocityHighway = int.Parse(highwaySplit[i]) });
            }

            _SpeedSet.Shuffle();
        }

        public static string GenerateGUID()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            return result;
        }

        private void CreateEmptyDB()
        {
            try
            {
                //Create DB
                if (!SQLServerCommon.SQLServerCommon.IsDatabaseExists(_MasterConnectionString, dbName))//connStringInitial, dbName))
                {
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(sqlCommandCreateDB, _MasterConnectionString);

                    //Create tables upon DB creation
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(gameplaysTableSchema, _DefaultConnectionString);
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(siteStateTableSchema, _DefaultConnectionString);
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(commentsTableSchema, _DefaultConnectionString);
                }
                else
                {
                    //Check if all tables exist, if not, create them
                    int i = 0;
                    foreach (string tableName in tableNames)
                    {
                        if (SQLServerCommon.SQLServerCommon.IsTableExists(_DefaultConnectionString, dbName, tableName) == false)
                        {
                            SQLServerCommon.SQLServerCommon.ExecuteNonQuery(tableSchemas[i], _DefaultConnectionString);
                        }
                        i++;
                    }
                }

                ////LocalDB = "RaceGameDB"
                //if (!SQLServerCommon.SQLServerCommon.IsDatabaseExists(connString, dbName))
                //{
                //    //Create tables upon DB creation
                //    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(gameplaysTableSchema, connString);
                //}

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddRecordToDB(string iUserID,
                                         int iSection,
                                         int iFreewayVelocity,
                                         int iTollwayVelocity,
                                         int iPriceSubject,
                                         int iPriceRandom,
                                         int iAccount,
                                         double iTimeSaved)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //foreach (string column in GamePlaysTableColumns)
            //{
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[0]), iUserID);
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[1]), iSection.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[2]), iFreewayVelocity.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[3]), iTollwayVelocity.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[4]), iPriceSubject.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[5]), iPriceRandom.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[6]), iAccount.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[7]), iTimeSaved.ToString());
            //}

            try
            {
                //DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select 1 from {0} where {1} = {2};", "GamePlays", "ID", "'" + iDrug.Name.Replace("'", "") + "'"), connString);
                //if (dt.Rows.Count == 0)
                //{
                SQLServerCommon.SQLServerCommon.Insert("GamePlays", _DefaultConnectionString, GamePlaysTableColumns, parameters);
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void AddRecordToDB(GamePlay iGamePlay)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //foreach (string column in GamePlaysTableColumns)
            //{
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[0]), iGamePlay.UserID.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[1]), iGamePlay.Section.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[2]), iGamePlay.FreewayVelocity.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[3]), iGamePlay.TollwayVelocity.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[4]), iGamePlay.PriceSubject.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[5]), iGamePlay.PriceRandom.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[6]), iGamePlay.CurrentAccount.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[7]), iGamePlay.TimeSaved.ToString());
            //}

            try
            {
                //DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select 1 from {0} where {1} = {2};", "GamePlays", "ID", "'" + iDrug.Name.Replace("'", "") + "'"), connString);
                //if (dt.Rows.Count == 0)
                //{
                SQLServerCommon.SQLServerCommon.Insert("GamePlays", _DefaultConnectionString, GamePlaysTableColumns, parameters);
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string GenerateUniqueID()
        {
            string s = string.Empty;

            s = DateTime.Now.ToString("dd.M.yyyy_hh.mm.ss.fffffff");

            return s;
        }

        public void AddStateRecordToDB(string iUserID,
                                         int iPageNumber,
                                         string iPageName,
                                         bool iIsVisited)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", Game.siteStateTableColumns[0]), iUserID);
            parameters.Add(String.Format("@{0}", Game.siteStateTableColumns[1]), iPageNumber.ToString());
            parameters.Add(String.Format("@{0}", Game.siteStateTableColumns[2]), iPageName.ToString());

            if (iIsVisited == true)
            {
                parameters.Add(String.Format("@{0}", Game.siteStateTableColumns[3]), "1");
            }
            else
            {
                parameters.Add(String.Format("@{0}", Game.siteStateTableColumns[3]), "0");
            }


            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4} and {5} = {6};", "SiteState", "UserID", "'" + iUserID + "'", "PageNumber", iPageNumber, "PageName", "'" + iPageName + "'"), _DefaultConnectionString);
                if (dt.Rows.Count == 0)
                {
                    SQLServerCommon.SQLServerCommon.Insert("siteState", _DefaultConnectionString, Game.siteStateTableColumns, parameters);
                }
                else
                {
                    //this means that the record for the user and page already exists
                    //need to update current record

                    if (dt.Rows.Count == 1)
                    {
                        bool value = (bool)dt.Rows[0]["IsVisited"];
                        if (value != iIsVisited)
                        {
                            //we need to update the value
                            SQLServerCommon.SQLServerCommon.Update("siteState", _DefaultConnectionString, Game.siteStateTableColumns, parameters);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void AddCommentsRecordToDB(string iUserID,
                                         string iComment,
                                         string iConfirmationCode)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string comment = string.Empty;

            parameters.Add(String.Format("@{0}", Game.commentsTableColumns[0]), iUserID);

            if (iComment.Length > Game.maxCharactersinComment)
            {
                comment = iComment.Substring(0, Game.maxCharactersinComment);
            }
            else
            {
                comment = iComment;
            }
            parameters.Add(String.Format("@{0}", Game.commentsTableColumns[1]), comment);

            parameters.Add(String.Format("@{0}", Game.commentsTableColumns[2]), iConfirmationCode);

            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2};", "Comments", "UserID", "'" + iUserID + "'"), _DefaultConnectionString);
                if (dt.Rows.Count == 0)
                {
                    SQLServerCommon.SQLServerCommon.Insert("comments", _DefaultConnectionString, Game.commentsTableColumns, parameters);
                }
                else
                {
                    //do nothing - user may leave a comment only once
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void ClearUserStateDataFromDB(string iUserID, string iTableName, string iWhereClause)
        {
            SQLServerCommon.SQLServerCommon.Delete(iTableName, _DefaultConnectionString, iWhereClause);
        }

        public bool IsVisitedPage(string iUserID, int iPageNumber, string iPageName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            bool isVisited = false;

            parameters.Add(String.Format("@{0}", Game.siteStateTableColumns[0]), iUserID);
            parameters.Add(String.Format("@{0}", Game.siteStateTableColumns[1]), iPageNumber.ToString());
            parameters.Add(String.Format("@{0}", Game.siteStateTableColumns[2]), iPageName);

            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4} and {5} = {6};", "SiteState", "UserID", "'" + iUserID + "'", "PageNumber", iPageNumber.ToString(), "PageName", "'" + iPageName + "'"), _DefaultConnectionString);
                if (dt.Rows.Count == 1)
                {
                    bool value = (bool)dt.Rows[0]["IsVisited"];
                    if (value)
                    {
                        isVisited = true;
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }

            return isVisited;
        }

        public bool IsGuidGeneratedForUser(string iUserID, ref string oGUID)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            bool isGuidGenerated = false;

            parameters.Add(String.Format("@{0}", Game.commentsTableColumns[0]), iUserID);
            
            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2};", "Comments", "UserID", "'" + iUserID + "'"), _DefaultConnectionString);
                if (dt.Rows.Count > 0)
                {
                    isGuidGenerated = true;
                    oGUID = (string)dt.Rows[0]["ConfirmationCode"];
                }
            }
            catch (Exception)
            {

                throw;
            }

            return isGuidGenerated;
        }

        public List<GamePlay> GetGamePlaysForUser(string iUserID)
        {
            List<GamePlay> userGamePlays = new List<GamePlay>();
            DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2};", "GamePlays", "UserID", "'" + iUserID + "'"), _DefaultConnectionString);

            foreach (DataRow dr in dt.Rows)
            {
                userGamePlays.Add(new GamePlay() { UserID = dr[1].ToString(),
                                                   Section = int.Parse(dr[2].ToString()),
                                                   FreewayVelocity = int.Parse(dr[3].ToString()),
                                                   TollwayVelocity = int.Parse(dr[4].ToString()),
                                                   PriceSubject = int.Parse(dr[5].ToString()),
                                                   PriceRandom = int.Parse(dr[6].ToString()),
                                                   CurrentAccount = int.Parse(dr[7].ToString()),
                                                   TimeSaved = double.Parse(dr[8].ToString())
                                                 });
            }       

            return userGamePlays;
        }
    }
}