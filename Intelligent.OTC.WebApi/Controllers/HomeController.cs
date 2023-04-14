using System;
using System.Web.Mvc;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        [HttpGet]
        public ActionResult Keeping()
        {
            return Json(DateTime.Now.ToString(), JsonRequestBehavior.AllowGet);
        }
    }
}