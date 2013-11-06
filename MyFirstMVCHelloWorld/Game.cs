using System;
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
        }

        private string _Name { get; set; }

        private List<GamePlay> _GamePlays = new List<GamePlay>();
        public List<GamePlay> GamePlays { get; set; }


    }
}