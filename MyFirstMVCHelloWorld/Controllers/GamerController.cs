using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyFirstMVCHelloWorld.Controllers
{
    public class GamerController : Controller
    {
        //
        // GET: /Gamer/

        public ActionResult Index()
        {
            return View();
        }

    }
}
