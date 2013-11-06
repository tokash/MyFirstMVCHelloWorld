using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyFirstMVCHelloWorld.Models
{
    public class Gamer
    {
        private int _Account {get; set;}
        private int _Time { get; set; }
        private List<GamePlay> _GamePlays = new List<GamePlay>();
    }
}