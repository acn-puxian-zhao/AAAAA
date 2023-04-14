using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Intelligent.OTC.WebApi.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult HttpError()
        {
            ViewBag.NoBreadcrumb = true;
            return View();
        }

        public ActionResult NotFound()
        {
            ViewBag.NoBreadcrumb = true;
            return View();
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}