using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RacingGame.Models
{
    public class BidClass
    {
        [Required(ErrorMessage="Please enter a Bid.")]
        [Range(1, 100, ErrorMessage="Bid must be a number between 0 and 100.")]
        [RegularExpression(@"^[1-9]\d{0,2}$", ErrorMessage = "Bid must be a number between 1 and 100.")]
        public int Bid {get; set;}
    }
}