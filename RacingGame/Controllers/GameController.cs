using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using System.Web.Routing;
using RacingGame.Classes;
using RacingGame.Models;

namespace RacingGame.Controllers
{
    [NoCacheAttribute]
    public class GameController : Controller
    {

        //private static readonly string connString = "Server=TOKASHYO-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=RaceGameDB";
        //private static readonly string connString = "Server=tcp:fqw1x1y2s2.database.windows.net,1433;Database=RacingGALLIpkFTF;User Id=tokash@fqw1x1y2s2;Password=Yt043112192;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        //private static readonly string connString = "workstation id=RaceGameDB.mssql.somee.com;packet size=4096;user id=tokash_SQLLogin_1;pwd=vahzmb1why;data source=RaceGameDB.mssql.somee.com;persist security info=False;initial catalog=RaceGameDB";

        //
        // GET: /Game/

        //private static readonly string siteStateTableSchema = "CREATE TABLE SiteState (UserID varchar(30) NOT NULL , PageNumber int NOT NULL, PageName varchar(30) NOT NULL, IsVisited BIT NOT NULL)";
        //private static readonly string[] siteStateTableColumns = { "UserID", "PageNumber", "PageName", "IsVisited" };

        
        public ActionResult FirstPage(string id)
        {
            //Data to pass through the game life cycle:
            //Randomized speeds array
            Session.Timeout = 60;

            Session["Game"] = new RacingGame.Models.Game();

            if (id == null || id == string.Empty)
            {
                ViewData["UserID"] = Game.GenerateUniqueID();
            }
            else
            {
                Session["UserID"] = id;
                ViewData["UserID"] = id + "_" + Game.GenerateUniqueID();
            }

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

        [HttpPost]
        public ActionResult FirstPage(string id, Question model)
        {
            Question q = (Question)Session["Question1"];
            string answer = Request.Form["rbtnAnswer"];

            if (answer != q.CorrectAnswer)
            {
                if (answer == null)
                {
                    ViewBag.ErrorMessage = "No answer was chosen, please choose an answer and press \"Submit answer\"";
                }
                else
                {
                    ViewBag.ErrorMessage = q.ErrorMessage; 
                }
                ViewData["UserID"] = id;

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
                if (answer == null)
                {
                    ViewBag.ErrorMessage = "No answer was chosen, please choose an answer and press \"Submit answer\"";
                }
                else
                {
                    ViewBag.ErrorMessage = q.ErrorMessage;
                }
                ViewData["UserID"] = id;

                return View();
            }
            else
            {
                return RedirectToAction("WarningBeforeGame", new RouteValueDictionary(new { controller = "Game", action = "WarningBeforeGame", Id = id }));
            }
        }

        public ActionResult WarningBeforeGame(string id)
        {
            //this is the case where the user was redirected after cheating
            //creating new 
            if (Session["Game"] == null)
            {
                Session["Game"] = new RacingGame.Models.Game();
            }

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

            //if ((int)Session["BidSubmitted"] != 1)
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

                if (model.Bid > game.Account)
                {
                    ModelState.AddModelError("Bid", string.Format("Please enter a number between 0 and {0}.", game.Account));
                    return View("Bid");
                }

                if (!ModelState.IsValid)
                {

                    if (model.Bid > game.Account)
                    {
                        ModelState.AddModelError("Bid", string.Format("Please enter a number between 0 and {0}.", game.Account));
                    }

                    return View("Bid");
                }
                else
                {
                    TempData["BidModel"] = model;
                    return RedirectToAction("BidResult", new { id = id, pagenumber = pagenumber, pagename = "BidResult" });
                }
                //}
                //else
                //{
                //    return RedirectToAction("PageAlreadyVisited",
                //                            new RouteValueDictionary(new { controller = "Game", action = "PageAlreadyVisited", Id = id })
                //                           );
                //}


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
                //Need to calculte a random number and compare it to the gamer bid
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
                game.AddRecordToDB(id,
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
                                game.AddRecordToDB(id,
                                   game.CurrentSection,
                                   game.SpeedSet[game.CurrentSection - 1].VelocityFreeway,
                                   game.SpeedSet[game.CurrentSection - 1].VelocityHighway,
                                   0,
                                   0,
                                   0,
                                   0);

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

                    List<GamePlay> gamePlays = game.GetGamePlaysForUser(id);

                    result = View("GameEnd", gamePlays);
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
            string guid = string.Empty;
            Game game = (Game)Session["Game"];


            if (game != null)
            {
                if (!game.IsGuidGeneratedForUser(id, ref  guid))
                {
                    ViewBag.Code = Game.GenerateGUID();
                }
                else
                {
                    ViewBag.Code = guid;
                } 
            }

            return View();
        }        

        [HttpPost]
        public ActionResult AfterEndGamePost(string id, string confirmationcode)
        {
            string comments = Request.Form["txtbxComments"];

            //if (comments != null && comments != string.Empty)
            //{
                //write comments to db
                AddCommentsRecordToDB(id, comments, confirmationcode);
            //}

                return RedirectToAction("AfterSubmitComments", new { id = id });
        }

        public ActionResult PageAlreadyVisited(string id, int? pagenumber)
        {
            if (pagenumber != null)
            {
                ViewBag.Page = pagenumber;
            }

            ViewData["UserID"] = id;

            return View();
        }

        public ActionResult StartOver(string id)
        {
            ClearUserStateDataFromDB(id, "siteState", "");

            string newID = string.Empty;
            string originalUserID = (string)Session["UserID"];
            if (originalUserID != null)
            {
                newID = originalUserID + "_" + Game.GenerateUniqueID();
            }
            else
            {
                newID = Game.GenerateUniqueID();
            }

            Session.Abandon();
            return RedirectToAction("WarningBeforeGame",
                                        new RouteValueDictionary(new { controller = "Game", action = "WarningBeforeGame", Id = newID })
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

        public ActionResult AfterSubmitComments(string id)
        {
            ClearUserStateDataFromDB(id, "SiteState", "");
            return View();
        }

        private void AddStateRecordToDB(string iUserID,
                                         int iPageNumber,
                                         string iPageName,
                                         bool iIsVisited)
        {
            Game game = (Game)Session["Game"];

            if (game != null)
            {
                game.AddStateRecordToDB(iUserID, iPageNumber, iPageName, iIsVisited);
            }
            else
            {
                throw new Exception("Game engine is null");
            }
        }

        private void AddCommentsRecordToDB(string iUserID,
                                         string iComment, 
                                         string iConfirmationCode)
        {
            Game game = (Game)Session["Game"];

            if (game != null)
            {
                game.AddCommentsRecordToDB(iUserID, iComment, iConfirmationCode);
            }
            else
            {
                throw new Exception("Game engine is null");
            }
        }

        private void ClearUserStateDataFromDB(string iUserID, string iTableName, string iWhereClause)
        {

            Game game = (Game)Session["Game"];

            if (game != null)
	        {
		        game.ClearUserStateDataFromDB(iUserID, iTableName, iWhereClause);
	        }
            else
            {
                throw new Exception("Game engine is null");
            }
        }

        private bool IsVisitedPage(string iUserID, int iPageNumber, string iPageName)
        {
            bool isVisited = false;
            Game game = (Game)Session["Game"];

            if (game != null)
            {
                isVisited = game.IsVisitedPage(iUserID, iPageNumber, iPageName);
            }
            else
            {
                throw new Exception("Game engine is null");
            }

            return isVisited;
        }

    }
}
