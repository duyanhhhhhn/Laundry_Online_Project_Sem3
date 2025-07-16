using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laundry_Online_Web_FE.Controllers.Admin
{
    public class CustomerController : Controller
    {
        // GET: Customer
        [Route("admin/customer")]
        public ActionResult Index()
        {
            return View();
        }
    }
}