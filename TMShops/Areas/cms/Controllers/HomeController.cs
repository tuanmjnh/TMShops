using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TMShops.Areas.cms.Controllers
{
    public class HomeController : Controller
    {
        // GET: cms/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}