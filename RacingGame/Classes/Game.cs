﻿using System;
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
        private static readonly string connStringInitial = "Server=TOKASHYO-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=master";
        private static readonly string connString = "Server=TOKASHYO-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=RaceGameDB";
        //private static readonly string connString = "Server=tcp:fqw1x1y2s2.database.windows.net,1433;Database=GameRaceDB;User ID=tokash@fqw1x1y2s2;Password=Yt043112192;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        private static readonly string dbName = "RaceGameDB";//"GameRaceDB";
        //"Server=tcp:fqw1x1y2s2.database.windows.net,1433;Database=RacingGALLIpkFTF;User ID=tokash@fqw1x1y2s2;Password={your_password_here};Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"

        private static readonly string sqlCommandCreateDB = "CREATE DATABASE RaceGameDB ON PRIMARY " +
                "(NAME = RaceGameDB, " +
                "FILENAME = 'D:\\RaceGameDB.mdf', " +
                "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%) " +
                "LOG ON (NAME = RaceGameDB_LOG, " +
                "FILENAME = 'D:\\RaceGameDB.ldf', " +
                "SIZE = 1MB, " +
                "MAXSIZE = 100MB, " +
                "FILEGROWTH = 10%)";

        private static readonly string gameplaysTableSchema = "CREATE TABLE GamePlays (ID int IDENTITY(1,1), UserID varchar(30) NOT NULL, Section int NOT NULL, VelocityFreeway int NOT NULL, VelocityTollway int NOT NULL, PriceSubject int NOT NULL, PriceRandom int NOT NULL, Account int NOT NULL, TimeLeft real NOT NULL , PRIMARY KEY (ID))";
        private static readonly string[] GamePlaysTableColumns = {"UserID", "Section", "VelocityFreeway", "VelocityTollway", "PriceSubject", "PriceRandom", "Account", "TimeLeft" };

        public Game(string iName = "Racing Game")
        {
            _Name = iName;
            _GameData = (NameValueCollection)ConfigurationManager.GetSection("GameConfiguration");

            _Account = int.Parse(_GameData["StartingGamerCash"]);
            _TimeLeft = double.Parse(_GameData["StartingGamerTime"]);
            _RoadSections = int.Parse(_GameData["RoadSections"]);
            _GamePlays = new List<GamePlay>();
            _TimeSaved = 0;

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
                if (!SQLServerCommon.SQLServerCommon.IsDatabaseExists(connStringInitial, dbName))
                {
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(sqlCommandCreateDB, connStringInitial);

                    //Create tables upon DB creation
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(gameplaysTableSchema, connString);
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

        public static void AddRecordToDB(int iSection,
                                         int iFreewayVelocity,
                                         int iTollwayVelocity,
                                         int iPriceSubject,
                                         int iPriceRandom,
                                         int iAccount,
                                         double iTimeLeft)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //foreach (string column in GamePlaysTableColumns)
            //{
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[0]), iSection.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[1]), iFreewayVelocity.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[2]), iTollwayVelocity.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[3]), iPriceSubject.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[4]), iPriceRandom.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[5]), iAccount.ToString());
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[6]), iTimeLeft.ToString());
            //}

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

        public static void AddRecordToDB(GamePlay iGamePlay)
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
            parameters.Add(String.Format("@{0}", GamePlaysTableColumns[7]), iGamePlay.TimeLeft.ToString());
            //}

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

        public static string GenerateUniqueID()
        {
            string s = string.Empty;

            s = DateTime.Now.ToString("dd.mm.yyyy_hh.mm.ss.fffffff");

            return s;
        }
    }
}