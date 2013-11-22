using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RacingGame.Models
{

    public class GamePlay
    {
        public string UserID { get; set; }
        public int Section { get; set; }
        public int FreewayVelocity { get; set; }
        public int TollwayVelocity { get; set; }
        public int PriceSubject { get; set; }
        public int PriceRandom { get; set; }
        public int CurrentAccount { get; set; }
        public double TimeLeft { get; set; }
    }
}
