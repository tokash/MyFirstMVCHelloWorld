using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RacingGame.Models;

namespace RacingGame.Controllers
{
    public class GameController : Controller
    {

        private static readonly string connString = "Server=TOKASHYO-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=RaceGameDB";
        //private static readonly string connString = "Server=tcp:fqw1x1y2s2.database.windows.net,1433;Database=RacingGALLIpkFTF;User Id=tokash@fqw1x1y2s2;Password=Yt043112192;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        //private static readonly string connString = "workstation id=RaceGameDB.mssql.somee.com;packet size=4096;user id=tokash_SQLLogin_1;pwd=vahzmb1why;data source=RaceGameDB.mssql.somee.com;persist security info=False;initial catalog=RaceGameDB";

        //
        // GET: /Game/

        public ActionResult FirstPage()
        {
            //Data to pass through the game life cycle:
            //Randomized speeds array
            //Session.Timeout = 10;
            Session["Game"] = new RacingGame.Models.Game();
            Session["UserID"] = Game.GenerateUniqueID();

            return View();
        }

        [HttpPost]
        public ActionResult FirstPageResult()
        {
            string answer = Request.Form["rbtnAnswer"];
            RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];

            if (answer != "If I save 60 minutes and spend $50")
            {
                //User anserwed wrong answer
            }
            else
            {
                //User answered correctly and should be redirected to next page
            }

            ViewBag.Account = game.Account;

            return View("Bid");
        }

        public ActionResult Bid()
        {
            RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];
            ViewBag.Account = game.Account;
            //ViewBag.TimeLeft = game.TimeLeft;
            ViewBag.TimeSaved = game.TimeSaved;
            ViewBag.CurrentSection = game.CurrentSection;
            ViewBag.RoadSections = game.RoadSections;

            ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;

            ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;

            return View();
        }

        [HttpPost]
        public ActionResult Bid(BidClass model)
        {
            RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];
            ViewBag.Account = game.Account;
            //ViewBag.TimeLeft = game.TimeLeft;
            ViewBag.TimeSaved = game.TimeSaved;
            ViewBag.CurrentSection = game.CurrentSection;
            ViewBag.RoadSections = game.RoadSections;

            ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;

            ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;

            if (!ModelState.IsValidField("Bid"))
            {
                ModelState.AddModelError("Bid", "Please enter a Bid");
            }

            if (!ModelState.IsValid)
            {
                return View("Bid");
            }
            else
            {
                return RedirectToAction("BidResult", model);
            }

            
        }

        public ActionResult BidResult(BidClass model)
        {
            RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];
            double sectionCoverageTime = 0;
            ViewResult result = new ViewResult();

            //Need to calculte a randome number and compare it to the gamer bid
            int userBid = model.Bid;
            ViewBag.UserBid = userBid;

            int randomCost = RandomNumberGenerator.Next();
            ViewBag.RandomTollCost = randomCost;

            if (userBid < randomCost)
            {
                //go by free way
                //reduce time by the following calculation :
                //time passed = section_length / freeway_speed;
                //new time left = old time - time passed
                sectionCoverageTime = Math.Round((10.0 / game.SpeedSet[game.CurrentSection - 1].VelocityFreeway) * 60, 2);

                ViewBag.SectionCoverageTime = sectionCoverageTime;
                game.TimeLeft = game.TimeLeft - sectionCoverageTime;

                //account stays the same
            }
            else
            {
                //go by highway
                //reduce time by the following calculation :
                //time passed = section_length / highway_speed;
                //new time left = old time - time passed
                //account = old_account - highway cost

                sectionCoverageTime = Math.Round((10.0 / game.SpeedSet[game.CurrentSection - 1].VelocityHighway) * 60, 2);

                //calculating time saved
                //time to drive on freeway = 10 / game.SpeedSet[game.CurrentSection - 1].VelocityFreeway
                double freewayTime = Math.Round((double)(10.0 / game.SpeedSet[game.CurrentSection - 1].VelocityFreeway) * 60, 2);

                //time to drive on highway = 10 / game.SpeedSet[game.CurrentSection - 1].VelocityHighway
                double highwayTime = Math.Round((double)((10.0 / game.SpeedSet[game.CurrentSection - 1].VelocityHighway) * 60), 2);

                game.TimeSaved += freewayTime - highwayTime;

                ViewBag.SectionCoverageTime = sectionCoverageTime;
                ViewBag.TimeSaved = game.TimeSaved;

                game.TimeLeft = game.TimeLeft - sectionCoverageTime;
                game.Account = game.Account - randomCost;
            }
            ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;
            ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;
            ViewBag.Account = game.Account;
            ViewBag.TimeLeft = Math.Round(game.TimeLeft, 2);
            ViewBag.CurrentSection = game.CurrentSection;
            ViewBag.RoadSections = game.RoadSections;

            Session["Game"] = game;
            game.GamePlays.Add(new GamePlay()
            {
                UserID = (string)Session["UserID"],
                Section = game.CurrentSection,
                FreewayVelocity = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway,
                TollwayVelocity = game.SpeedSet[game.CurrentSection - 1].VelocityHighway,
                PriceSubject = userBid,
                PriceRandom = randomCost,
                CurrentAccount = game.Account,
                TimeLeft = game.TimeLeft
            }
                                   );

            //Game.AddRecordToDB(game.CurrentSection,
            //                   game.SpeedSet[game.CurrentSection - 1].VelocityFreeway,
            //                   game.SpeedSet[game.CurrentSection - 1].VelocityHighway,
            //                   userBid,
            //                   randomCost,
            //                   game.Account,
            //                   game.TimeLeft);

            game.CurrentSection++;
            if (ViewBag.CurrentSection < game.RoadSections && game.Account > 0 && game.TimeLeft > 0)
            {
                //Go to step1
                result = View();
            }
            else
            {
                //Go to game end;
                if (game.Account <= 0 || game.TimeLeft <= 0)
                {
                    //in this case, the game ended because the user lost all time or money
                }
                ViewBag.Code = game.GenerateGUID();

                //write game results to DB
                foreach (GamePlay play in game.GamePlays)
                {
                    Game.AddRecordToDB(play);
                }


                result = View("GameEnd");
            }


            return result;
        }

    }
}
