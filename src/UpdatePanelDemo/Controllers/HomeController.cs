using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UpdatePanelDemo.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var @this = this.ViewData.AddOrGetExisting("this", () => this.ViewData);
            return View();
        }

        public ActionResult SubAction()
        {
            return PartialView("IndexPartial");
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult Sleep(int? time)
        {
            time = time ?? 3000;
            System.Threading.Thread.Sleep(time.Value);
            return Content("OK");
        }

    }
}