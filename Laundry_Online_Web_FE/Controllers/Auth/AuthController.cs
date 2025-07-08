using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_FE.Models.Repositories;

namespace Laundry_Online_Web_FE.Controllers.Auth
{
    public class AuthController : Controller
    {
        // GET: Auth
        public ActionResult Login_Customer()
        {
            return View();
        }
        public ActionResult Login_Employee()
        {
            return View();
        }
        public ActionResult CheckEmployee()
        {
            string phoneNumber = Request.Form["PhoneNumber"];
            string password = Request.Form["Password"];
            var emp = EmployeeRepo.Instance.LoginEmployee(phoneNumber, password);
            if (emp != null)
            {
                Session["User"] = emp;
                Session["Role"] = emp.Role == 1 ? "Admin" : "Employee";

                return RedirectToAction("Admin",emp);
            }
            ViewBag.Error = "Tài khoản nhân viên không hợp lệ!";
            return RedirectToAction("Login_Employee");
        }
        public ActionResult LoginCustomer()
        {
            string phoneNumber = Request.Form["PhoneNumber"];
            string password = Request.Form["Password"];
            var customer = CustomerRepo.Instance.LoginCustomer(phoneNumber,password);
            if (customer != null)
            {
                Session["User"] = customer;
                Session["Role"] = "Customer";
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Tài khoản khách hàng không hợp lệ!";
            return RedirectToAction("Login_Customer");
        }
        public ActionResult Logout_Customer()
        {
            Session.Clear();
            return RedirectToAction("LoginCustomer");
        }
        public ActionResult Logout_Employee()
        {
            Session.Clear();
            return RedirectToAction("LoginEmployee");
        }
    }
}