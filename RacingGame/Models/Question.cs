using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RacingGame.Models
{
    public class Question
    {
        public string CorrectAnswer { get; set; }
        public string AnswerA { get; set; }
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }

        public string ErrorMessage { get; set; }
    }
}