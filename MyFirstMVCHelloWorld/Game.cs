using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using SQLServerCommon;

namespace MyFirstMVCHelloWorld.Models
{

    public class Game
    {
        private static readonly string connStringInitial = "Server=TOKASHYOS-PC;Integrated security=SSPI;database=master";
        //private static readonly string connString = "Server=TOKASHYOS-PC\\SQLEXPRESS;Integrated security=SSPI;database=GameDB";
        private static readonly string connString = "workstation id=RaceGameDB.mssql.somee.com;packet size=4096;user id=tokash_SQLLogin_1;pwd=vahzmb1why;data source=RaceGameDB.mssql.somee.com;persist security info=False;initial catalog=RaceGameDB";

        private static readonly string sqlCommandCreateDB = "CREATE DATABASE RaceGameDB ON PRIMARY " +
                "(NAME = RaceGameDB, " +
                "FILENAME = 'D:\\RaceGameDB.mdf', " +
                "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%) " +
                "LOG ON (NAME = RaceGameDB_LOG, " +
                "FILENAME = 'D:\\RaceGameDB.ldf', " +
                "SIZE = 1MB, " +
                "MAXSIZE = 100MB, " +
                "FILEGROWTH = 10%)";

        private static readonly string gameplaysTableSchema = "CREATE TABLE GamePlays (ID int IDENTITY(1,1), VelocityFreeway int NOT NULL, PRIMARY KEY (ID))";
        private static readonly string[] GamePlaysTableColumns = { "VelocityFreeway" };

        public Game(string iName = "Racing Game")
        {
            _Name = iName;
            _GameData = (NameValueCollection)ConfigurationManager.GetSection("GameConfiguration");

            _Account = int.Parse(_GameData["StartingGamerCash"]);
            _TimeLeft = double.Parse(_GameData["StartingGamerTime"]);
            _RoadSections = int.Parse(_GameData["RoadSections"]);

            _CurrentSection = 1;

            RandomizeSpeedset();

            CreateEmptyDB();
        }

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

        private double _TimeLeft;
        public double TimeLeft
        {
            get
            {
                return _TimeLeft;
            }
            set
            {
                _TimeLeft = value;
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
        public List<GamePlay> GamePlays { get; set; }

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

        public string GenerateGUID()
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
                if (!SQLServerCommon.SQLServerCommon.IsDatabaseExists(connStringInitial, "RaceGameDB"))
                {
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(sqlCommandCreateDB, connStringInitial);

                    //Create tables upon DB creation
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(gameplaysTableSchema, connString);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void AddRecordToDB(int iVelocity)//Drug iDrug)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (string column in GamePlaysTableColumns)
            {
                parameters.Add(String.Format("@{0}", column), iVelocity.ToString());
            }

            try
            {
                //DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select 1 from {0} where {1} = {2};", "GamePlays", "ID", "'" + iDrug.Name.Replace("'", "") + "'"), connString);
                //if (dt.Rows.Count == 0)
                //{
                    SQLServerCommon.SQLServerCommon.Insert("GamePlays", connString, GamePlaysTableColumns, parameters);
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}