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
        //
        // GET: /Game/

        public ActionResult Index()
        {
            //Data to pass through the game life cycle:
            //Randomized speeds array
            Session.Timeout = 10;
            Session["Game"] = new MyFirstMVCHelloWorld.Models.Game();

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

            game.CurrentSection++;
            Session["Game"] = game;

            if (ViewBag.CurrentSection < game.RoadSections && game.Account > 0 && game.TimeLeft > 0)
            {
                //Go to step1
                return View();
            }
            else
            {
                //Go to game end;
                if (game.Account <= 0)
                {
                    //in this case, the game ended because the user lost all time or money
                }
                ViewBag.Code = game.GenerateGUID();

                return View("GameEnd");
            }
            
        }

    }
}
