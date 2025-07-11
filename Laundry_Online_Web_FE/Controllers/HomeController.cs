using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laundry_Online_Web_FE.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
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

        public ActionResult Login()
        {
            if (Session["customer"] != null)
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang chính
                return RedirectToAction("Index", "Customer");
            }
            return View();
        }
        public ActionResult Logout()
        {
            // Xóa thông tin đăng nhập khỏi session
            Session["customer"] = null;
            // Chuyển hướng về trang đăng nhập
            return RedirectToAction("Login");
        }

        public ActionResult Register()
        {
            return View();
        }
    }
}