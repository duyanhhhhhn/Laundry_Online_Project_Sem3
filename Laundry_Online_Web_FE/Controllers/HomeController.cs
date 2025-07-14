using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_FE.Models.ModelViews.DTO;
using Laundry_Online_Web_FE.Models.Dao;
using Laundry_Online_Web_FE.Models.Repositories;
using Laundry_Online_Web_BE.Models.Repositories;

namespace Laundry_Online_Web_FE.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var services = ServiceRepository.Instance.All();
            ViewBag.Services = services;
            var backages = PackageRepository.Instance.GetAll();
            ViewBag.Packages = backages;
            var model = new HeaderModel
            {
                Services = services,
                Packages = backages
            };
            ViewBag.Model = model;
            return View(model);
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
        public async Task<ActionResult> Create_Customer()
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

                try
                {
                  
                    var smsService = new eSmsService();
                    string welcomeMessage = "Cam on quy khach da su dung dich vu cua chung toi. Chuc quy khach mot ngay tot lanh!";

                    string smsResult = await smsService.SendAsync(phone, welcomeMessage);

                    System.Diagnostics.Debug.WriteLine("Ket qua gui SMS: " + smsResult);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LOI GUI SMS: " + ex.Message);
                }

                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.ErrorMessage = "Register Error. Try again";
                return View("Register");
            }
        }

        public ActionResult DetailService(int id)
        {
            var services = ServiceRepository.Instance.All();
            ViewBag.Services = services;
            var backages = PackageRepository.Instance.GetAll();
            ViewBag.Packages = backages;
            var model = new HeaderModel
            {
                Services = services,
                Packages = backages
            };
            ViewBag.Model = model;
            var service = ServiceRepository.Instance.GetById(id);
            if (service == null)
            {
                TempData["ErrorMessage"] = "Not found service!";
                return RedirectToAction("Index");
            }
            ViewBag.Service = service;
            return View(model);
        }
        public ActionResult DetailPackage(int id)
        {
            var services = ServiceRepository.Instance.All();
            ViewBag.Services = services;
            var backages = PackageRepository.Instance.GetAll();
            ViewBag.Packages = backages;
            var model = new HeaderModel
            {
                Services = services,
                Packages = backages
            };
            ViewBag.Model = model;
            var package = PackageRepository.Instance.GetById(id);
            if (package == null)
            {
                TempData["ErrorMessage"] = "Not found package!";
                return RedirectToAction("Index");
            }
            ViewBag.Package = package;
            return View(model);
        }
    }
}