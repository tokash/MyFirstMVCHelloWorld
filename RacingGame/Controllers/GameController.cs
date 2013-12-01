using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RacingGame.Classes;
using RacingGame.Models;

namespace RacingGame.Controllers
{
    [NoCacheAttribute]
    public class GameController : Controller
    {

        private static readonly string connString = "Server=TOKASHYOS-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=RaceGameDB";
        //private static readonly string connString = "Server=tcp:fqw1x1y2s2.database.windows.net,1433;Database=RacingGALLIpkFTF;User Id=tokash@fqw1x1y2s2;Password=Yt043112192;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        //private static readonly string connString = "workstation id=RaceGameDB.mssql.somee.com;packet size=4096;user id=tokash_SQLLogin_1;pwd=vahzmb1why;data source=RaceGameDB.mssql.somee.com;persist security info=False;initial catalog=RaceGameDB";

        //
        // GET: /Game/

        private static readonly string siteStateTableSchema = "CREATE TABLE SiteState (UserID varchar(30) NOT NULL , PageNumber int NOT NULL, PageName varchar(30) NOT NULL, IsVisited BIT NOT NULL)";
        private static readonly string[] siteStateTableColumns = { "UserID", "PageNumber", "PageName", "IsVisited" };

        public ActionResult FirstPage(string id)
        {
            //Data to pass through the game life cycle:
            //Randomized speeds array
            //Session.Timeout = 10;

            if (id != null && id != "")
            {
                Session["Game"] = new RacingGame.Models.Game();
                ViewData["UserID"] = id;
                //Session["UserID"] = Game.GenerateUniqueID();
                Question q = new Question()
                {
                    CorrectAnswer = "If I save 60 minutes and spend $50",
                    AnswerA = "If I save 50 minutes and spend $100+",
                    AnswerB = "If I save 60 minutes and spend $100",
                    AnswerC = "If I save 50 minutes and spend $50",
                    AnswerD = "If I save 60 minutes and spend $50",
                    ErrorMessage = "Your answer is incorrect.  Please read the instructions and try again."
                };
                Session["Question1"] = q;

                return View(); 
            }
            else
            {
                return RedirectToAction("IDRequired");
            }
        }

        [HttpPost]
        public ActionResult FirstPage(string id, Question model)
        {
            Question q = (Question)Session["Question1"];
            string answer = Request.Form["rbtnAnswer"];

            if (answer != q.CorrectAnswer)
            {
                ViewBag.ErrorMessage = q.ErrorMessage;
                return View();
            }
            else
            {
                RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];
                ViewBag.Account = game.Account;
                //ViewBag.TimeLeft = game.TimeLeft;
                ViewBag.TimeSaved = game.TimeSaved;
                ViewBag.CurrentSection = game.CurrentSection;
                ViewBag.RoadSections = game.RoadSections;

                ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;

                ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;

                return RedirectToAction("SecondPage", new RouteValueDictionary(new { controller = "Game", action = "SecondPage", Id = id }));
            }
        }

        public ActionResult SecondPage(string id)
        {
            Session["Game"] = new RacingGame.Models.Game();
            ViewData["UserID"] = id;

            Question q = new Question()
            {
                CorrectAnswer = "If the toll road’s price is less than or equal to $10",
                AnswerA = "If the toll road’s price is exactly $10",
                AnswerB = "If the toll road’s price is less than $10",
                AnswerC = "If the toll road’s price is less than or equal to $10",
                AnswerD = "If the toll road’s price is higher than $10",
                ErrorMessage = "Your answer is incorrect.  Please read the instructions and try again."
            };
            Session["Question2"] = q;

            return View();
        }

        [HttpPost]
        public ActionResult SecondPage(string id, Question model)
        {
            Question q = (Question)Session["Question2"];
            string answer = Request.Form["rbtnAnswer"];

            if (answer != q.CorrectAnswer)
            {
                ViewBag.ErrorMessage = q.ErrorMessage;
                return View();
            }
            else
            {
                return RedirectToAction("WarningBeforeGame", new RouteValueDictionary(new { controller = "Game", action = "WarningBeforeGame", Id = id }));
            }
        }

        public ActionResult WarningBeforeGame(string id)
        {
            ViewData["UserID"] = id;
            return View();
        }

        [HttpPost]
        public ActionResult WarningBeforeGamePost(string id)
        {
            RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];
            ViewBag.Account = game.Account;
            //ViewBag.TimeLeft = game.TimeLeft;
            ViewBag.TimeSaved = game.TimeSaved;
            ViewBag.CurrentSection = game.CurrentSection;
            ViewBag.RoadSections = game.RoadSections;

            ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;

            ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;

            Session["Game"] = game;

            return RedirectToAction("Bid", new { id = id, pagenumber = 1, pagename = "Bid" });
        }

        public ActionResult Bid(string id, int? pagenumber, string pagename)
        {
            RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];

            if (!IsVisitedPage(id, (int)pagenumber, pagename))
            {
                ViewData["UserID"] = id;
                ViewData["PageNumber"] = pagenumber;
                ViewData["PageName"] = pagename;

                AddStateRecordToDB(id, (int)pagenumber, pagename, true);

                ViewBag.Account = game.Account;
                //ViewBag.TimeLeft = game.TimeLeft;
                ViewBag.TimeSaved = game.TimeSaved;
                ViewBag.CurrentSection = game.CurrentSection;
                ViewBag.RoadSections = game.RoadSections;

                ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;

                ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;

                return View();
            }
            else
            {
                return RedirectToAction("PageAlreadyVisited",
                                        new { id = id, pagenumber = pagenumber }
                                       );
            }
        }

        [HttpPost]
        public ActionResult Bid(string id, int? pagenumber, string pagename, BidClass model)
        {
            RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];
            ViewData["UserID"] = id;
            ViewData["PageNumber"] = pagenumber;
            ViewData["PageName"] = pagename;

            //if (!IsVisitedPage(id, game.CurrentSection))
            //{
                ViewBag.Account = game.Account;

                //ViewBag.TimeSaved = game.TimeSaved;
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
                    TempData["BidModel"] = model;
                    return RedirectToAction("BidResult", new { id = id, pagenumber = pagenumber, pagename = "BidResult"});
                } 
            //}
            //else
            //{
            //    return RedirectToAction("PageAlreadyVisited",
            //                            new RouteValueDictionary(new { controller = "Game", action = "PageAlreadyVisited", Id = id })
            //                           );
            //}

            
        }

        public ActionResult BidResult(string id, int? pagenumber, string pagename)
        {
            RacingGame.Models.Game game = (RacingGame.Models.Game)Session["Game"];
            double sectionCoverageTime = 0;
            double timeSaved = 0.0;
            ViewResult result = null;// new ViewResult();
            ViewData["UserID"] = id;
            ViewData["PageNumber"] = pagenumber + 1;
            BidClass model = (BidClass)TempData["BidModel"];

            if (!IsVisitedPage(id, (int)pagenumber, pagename))
            {
                AddStateRecordToDB(id, (int)pagenumber, pagename, true);
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
                    timeSaved = freewayTime - highwayTime;

                    game.TimeSaved += timeSaved;
                    game.Account = game.Account - randomCost;
                }

                game.TimePassed += sectionCoverageTime;
                ViewBag.SectionCoverageTime = sectionCoverageTime;
                ViewBag.FreewaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway;
                ViewBag.HighwaySpeed = game.SpeedSet[game.CurrentSection - 1].VelocityHighway;
                ViewBag.Account = game.Account;
                ViewBag.CurrentSection = game.CurrentSection;
                ViewBag.RoadSections = game.RoadSections;

                Session["Game"] = game;
                //string userID = (string)Session["UserID"];

                game.GamePlays.Add(new GamePlay()
                {
                    UserID = id,
                    Section = game.CurrentSection,
                    FreewayVelocity = game.SpeedSet[game.CurrentSection - 1].VelocityFreeway,
                    TollwayVelocity = game.SpeedSet[game.CurrentSection - 1].VelocityHighway,
                    PriceSubject = userBid,
                    PriceRandom = randomCost,
                    CurrentAccount = game.Account,
                    TimeSaved = timeSaved
                });

                #region Write play to DB every play
                Game.AddRecordToDB(id,
                                   game.CurrentSection,
                                   game.SpeedSet[game.CurrentSection - 1].VelocityFreeway,
                                   game.SpeedSet[game.CurrentSection - 1].VelocityHighway,
                                   userBid,
                                   randomCost,
                                   game.Account,
                                   timeSaved); 
                #endregion

                game.CurrentSection++;

                if (game.CurrentSection <= game.RoadSections && game.Account > 0)
                {
                    //Go to Bid page
                    result = View();
                }
                else
                {
                    //Go to game end;
                    if (game.Account <= 0)
                    {
                        //in this case, the game ended because the user lost all money
                        //need to calculate how long it took the user to ride by the freeway

                        double time = 0;
                        int currentSection = game.CurrentSection + 1;

                        if (currentSection < game.GameSections)
                        {
                            while (currentSection < game.RoadSections)
                            {
                                time += 10 / game.SpeedSet[currentSection - 1].VelocityFreeway;
                                currentSection++;
                            } 
                        }

                        game.TimePassed += time;
                    }

                    ViewBag.MoneySaved = game.Account;
                    ViewBag.MoneySpent = game.StartingAccount - game.Account;
                    ViewBag.Sections = game.GameSections;
                    ViewBag.StartingAccount = game.StartingAccount;
                    ViewBag.StartingTime = game.StartingTime;
                    @ViewBag.TimeSaved = game.TimeSaved;

                    double moneyTimeRatio = 0;
                    if (game.StartingAccount - game.Account > 0)
                    {
                        moneyTimeRatio = Math.Round(game.TimeSaved / (game.StartingAccount - game.Account), 2);
                    }
                    else
                    {
                        moneyTimeRatio = Math.Round(game.TimeSaved / (game.StartingAccount), 2);
                    }

                    ViewBag.MoneyTimeRatio = moneyTimeRatio;
                    ViewBag.Bonus = moneyTimeRatio * 0.5;
                    ViewBag.Code = game.GenerateGUID();

                    //write game results to DB
                    //foreach (GamePlay play in game.GamePlays)
                    //{
                    //    Game.AddRecordToDB(play);
                    //}


                    result = View("GameEnd");
                }
            }
            else
            {
                return RedirectToAction("PageAlreadyVisited",
                                        new { id = id, pagenumber = pagenumber }
                                       );
            }

            return result;
        }

        [HttpPost]
        public ActionResult BidResultPost(string id, int? pagenumber)
        {
            return RedirectToAction("Bid", new { id = id, pagenumber = pagenumber, pagename = "Bid" });
        }

        public ActionResult AfterEndGame(string id)
        {
            ViewData["UserID"] = id;

            return View();
        }

        [HttpPost]
        public ActionResult AfterEndGamePost(string id)
        {
            string comments = Request.Form["txtbxComments"];

            if (comments != null && comments != string.Empty)
            {
                //write comments to db
            }

            ClearUserStateDataFromDB(id, "SiteState", "");

            return Redirect("http://www.google.com");
        }

        public ActionResult PageAlreadyVisited(string id, int? pagenumber)
        {
            if (pagenumber != null)
            {
                ViewBag.Page = pagenumber;
            }
            return View();
        }

        public ActionResult StartOver(string id)
        {
            ClearUserStateDataFromDB(id, "siteState", "");
            Session.Abandon();
            return RedirectToAction("FirstPage",
                                        new RouteValueDictionary(new { controller = "Game", action = "FirstPage", Id = id})
                                       );
        }

        public ActionResult IDRequired()
        {
            return View();
        }

        public ActionResult IDRequiredPost()
        {
            string id = Request.Form["txtbxUserID"];

            if (id != null && id != string.Empty)
            {
                return RedirectToAction("FirstPage",
                                        new RouteValueDictionary(new { controller = "Game", action = "FirstPage", Id = id })
                                       );
            }
            else
            {
                return View("IDRequired");
            }

            
        }

        private void AddStateRecordToDB(string iUserID,
                                         int iPageNumber,
                                         string iPageName,
                                         bool iIsVisited)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", siteStateTableColumns[0]), iUserID);
            parameters.Add(String.Format("@{0}", siteStateTableColumns[1]), iPageNumber.ToString());
            parameters.Add(String.Format("@{0}", siteStateTableColumns[2]), iPageName.ToString());

            if (iIsVisited == true)
            {
                parameters.Add(String.Format("@{0}", siteStateTableColumns[3]), "1");
            }
            else
            {
                parameters.Add(String.Format("@{0}", siteStateTableColumns[3]), "0");
            }


            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4} and {5} = {6};", "SiteState", "UserID", "'" + iUserID + "'", "PageNumber", iPageNumber, "PageName", "'" + iPageName + "'"), connString);
                if (dt.Rows.Count == 0)
                {
                    SQLServerCommon.SQLServerCommon.Insert("siteState", connString, siteStateTableColumns, parameters);
                }
                else
                {
                    //this means that the record for the user and page already exists
                    //need to update current record

                    if (dt.Rows.Count == 1)
                    {
                        bool value = (bool)dt.Rows[0]["IsVisited"];
                        if (value != iIsVisited)
                        {
                            //we need to update the value
                            SQLServerCommon.SQLServerCommon.Update("siteState", connString, siteStateTableColumns, parameters);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void ClearUserStateDataFromDB(string iUserID, string iTableName, string iWhereClause)
        {
            SQLServerCommon.SQLServerCommon.Delete(iTableName, connString, iWhereClause);
        }

        private bool IsVisitedPage(string iUserID, int iPageNumber, string iPageName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            bool isVisited = false;

            parameters.Add(String.Format("@{0}", siteStateTableColumns[0]), iUserID);
            parameters.Add(String.Format("@{0}", siteStateTableColumns[1]), iPageNumber.ToString());
            parameters.Add(String.Format("@{0}", siteStateTableColumns[2]), iPageName);

            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4} and {5} = {6};", "SiteState", "UserID", "'" + iUserID + "'", "PageNumber", iPageNumber.ToString(), "PageName", "'" + iPageName + "'"), connString);
                if (dt.Rows.Count == 1)
                {
                    bool value = (bool)dt.Rows[0]["IsVisited"];
                    if (value)
                    {
                        isVisited = true;
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }

            return isVisited;
        }

    }
}
