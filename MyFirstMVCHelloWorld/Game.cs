﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MyFirstMVCHelloWorld.Models
{
    public class Game
    {
        public Game(string iName = "Racing Game")
        {
            _Name = iName;
            _GameData = (NameValueCollection)ConfigurationManager.GetSection("GameConfiguration");

            _Account = int.Parse(_GameData["StartingGamerCash"]);
            _TimeLeft = double.Parse(_GameData["StartingGamerTime"]);
            _RoadSections = int.Parse(_GameData["RoadSections"]);

            _CurrentSection = 1;

            RandomizeSpeedset();
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


    }
}