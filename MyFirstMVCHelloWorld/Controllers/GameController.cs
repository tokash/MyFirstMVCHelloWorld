﻿using System;
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
            Session["Game"] = new MyFirstMVCHelloWorld.Models.Game();

            return View();
        }

        public ActionResult Bid()
        {
            MyFirstMVCHelloWorld.Models.Game game = (MyFirstMVCHelloWorld.Models.Game)Session["Game"];

            return View("Bid");
        }

        [HttpPost]
        public ActionResult BidResult()
        {
            //Need to calculte a randome number and compare it to the gamer bid
            int userBid = int.Parse(Request.Form["Bid"]);
            int random = RandomNumberGenerator.Next();
            int cost = -1;
            if (userBid < random)
            {
                cost = random;
            }
            else
            {
                cost = 0; //freeway cost - need to read from somewhere
            }

            return View();
        }

    }
}
