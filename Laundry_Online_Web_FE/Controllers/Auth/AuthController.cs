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
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login_Employee()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (Session["customer"] != null || Session["employee"] != null)
            {
                // Nếu đã đăng nhập, chuyển hướng đến trang chính
                return RedirectToAction("Admin", "Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult CheckEmployee()
        {
            // Lấy thông tin đăng nhập từ form
            string phone = Request.Form["PhoneNumber"];
            string password = Request.Form["Password"];
            // Kiểm tra thông tin đăng nhập
            var employee = EmployeeRepo.Instance.LoginEmployee(phone, password);
            if (employee != null)
            {
                // Lưu thông tin đăng nhập vào session
                Session["employee"] = employee;
                // Chuyển hướng đến trang chính của admin
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                // Nếu đăng nhập không thành công, hiển thị thông báo lỗi
                ViewBag.ErrorMessage = "Số điện thoại hoặc mật khẩu không đúng.";
                return View("Login_Employee");
            }
        }
        [HttpPost]
        public ActionResult CheckCustomer()
        {
            // Lấy thông tin đăng nhập từ form
            string phone = Request.Form["PhoneNumber"];
            string password = Request.Form["Password"];
            // Kiểm tra thông tin đăng nhập
            var customer = CustomerRepo.Instance.LoginCustomer(phone, password);
            if (customer != null)
            {
                // Lưu thông tin đăng nhập vào session
                Session["customer"] = customer;
                // Chuyển hướng đến trang chính của khách hàng
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Nếu đăng nhập không thành công, hiển thị thông báo lỗi
                ViewBag.ErrorMessage = "Số điện thoại hoặc mật khẩu không đúng.";
                return View("Login","Home");
            }
        }
        public ActionResult AdminLogout()
        {
            // Xóa thông tin đăng nhập khỏi session
            Session["employee"] = null;
            // Chuyển hướng về trang đăng nhập
            return RedirectToAction("Login_Employee");
        }
        
    }
}