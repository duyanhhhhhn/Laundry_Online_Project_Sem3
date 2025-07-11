using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_FE.Models.Repositories;

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
                return RedirectToAction("Index");
            }
            ViewBag.Message = TempData["Message"];
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
            if (Session["customer"] != null)
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang chính
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Create_Customer()
        {
            string firstName = Request.Form["FirstName"];
            string lastName = Request.Form["LastName"];
            string phone = Request.Form["PhoneNumber"];
            string address = Request.Form["Address"];
            string password = Request.Form["Password"];
            var customer = new Models.ModelViews.CustomerView
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phone,
                Address = address,
                Password = password,
                RegistrationDate = DateTime.Now,
                Active = 1
            };
            var result = CustomerRepo.Instance.Create(customer);
            if (result)
            {
                TempData["Message"] = "Register success!";
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.ErrorMessage = "Register Error. Try again";
                return View("Register");
            }
        }

    }
}