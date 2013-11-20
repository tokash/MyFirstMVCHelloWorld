using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyFirstMVCHelloWorld.Models;

namespace MyFirstMVCHelloWorld.Controllers
{
    public class GameController : Controller
    {

        private static readonly string connString = "Server=TOKASHYOS-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=RaceGameDB";
        //private static readonly string connString = "Server=tcp:fqw1x1y2s2.database.windows.net,1433;Database=RacingGALLIpkFTF;User Id=tokash@fqw1x1y2s2;Password=Yt043112192;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        //private static readonly string connString = "workstation id=RaceGameDB.mssql.somee.com;packet size=4096;user id=tokash_SQLLogin_1;pwd=vahzmb1why;data source=RaceGameDB.mssql.somee.com;persist security info=False;initial catalog=RaceGameDB";

        //
        // GET: /Game/

        public ActionResult Index()
        {
            //Data to pass through the game life cycle:
            //Randomized speeds array
            Session.Timeout = 10;
            Session["Game"] = new MyFirstMVCHelloWorld.Models.Game();
            Session["UserID"] = Game.GenerateUniqueID();

            return View();
        }

        public ActionResult Bid()
        {
            MyFirstMVCHelloWorld.Models.Game game = (MyFirstMVCHelloWorld.Models.Game)Session["Game"];
            ViewBag.Account = game.Account;
            ViewBag.TimeLeft = game.TimeLeft;
            ViewBag.CurrentSection = game.CurrentSection;
            ViewBag.RoadSections = game.RoadSections;

            ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;
            ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;

            return View("Bid");
        }

        [HttpPost]
        public ActionResult BidResult()
        {
            MyFirstMVCHelloWorld.Models.Game game = (MyFirstMVCHelloWorld.Models.Game)Session["Game"];
            double sectionCoverageTime = 0;
            ViewResult result = new ViewResult();

            //Need to calculte a randome number and compare it to the gamer bid
            int userBid = int.Parse(Request.Form["txtbxBid"]);
            ViewBag.UserBid = userBid;

            int randomCost = RandomNumberGenerator.Next();
            ViewBag.RandomTollCost = randomCost;

            if (userBid < randomCost)
            {
                //go by free way
                //reduce time by the following calculation :
                //time passed = section_length / freeway_speed;
                //new time left = old time - time passed
                sectionCoverageTime = (10.0 / game.SpeedSet[game.CurrentSection - 1].VelocityFreeway) * 60;

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

                sectionCoverageTime = (10.0 / game.SpeedSet[game.CurrentSection - 1].VelocityHighway) * 60;

                ViewBag.SectionCoverageTime = sectionCoverageTime;
                game.TimeLeft = game.TimeLeft - sectionCoverageTime;
                game.Account = game.Account - randomCost;
            }
            ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;
            ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;
            ViewBag.Account = game.Account;
            ViewBag.TimeLeft = game.TimeLeft;
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
