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
        public ActionResult BookService()
        {
            // Kiểm tra đăng nhập
            if (Session["customer"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để đặt lịch dịch vụ.";
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public JsonResult SubmitBooking(string ServiceTime, string ServiceDate, string Notes)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["customer"] == null)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để đặt lịch." });
                }

                // Lấy thông tin customer từ session
                var customer = Session["customer"] as Models.ModelViews.CustomerView;
                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
                }

                // Parse date and time
                DateTime serviceDateTime;
                if (!DateTime.TryParse(ServiceDate, out serviceDateTime))
                {
                    return Json(new { success = false, message = "Ngày không hợp lệ" });
                }

                // Combine date and time
                if (!string.IsNullOrEmpty(ServiceTime))
                {
                    if (TimeSpan.TryParse(ServiceTime, out TimeSpan timeSpan))
                    {
                        serviceDateTime = serviceDateTime.Date.Add(timeSpan);
                    }
                }

                // Create booking invoice using customer from session
                var bookingInvoice = new Models.ModelViews.InvoiceView
                {
                    Customer_Id = customer.Id, // Lấy từ session
                    Employee_Id = null, // Will be assigned later
                    Invoice_Date = serviceDateTime,
                    Delivery_Date = null, // Use service date as delivery date
                    Pickup_Date = null,
                    Total_Amount = 0, // Will be calculated when service is confirmed
                    Payment_Type = 0, // Pay by cash 
                    Payment_Id = null,
                    Order_Status = 0, // Processing
                    Invoice_Type = 1,
                    CustomerPackage_Id = null,
                    Status = 0, // No ship
                    Notes = Notes ?? "Đặt lịch dịch vụ online",
                    Ship_Cost = 0,
                    Delivery_Status = 0 // Pending
                };

                bool success = InvoiceRepository.Instance.Add(bookingInvoice);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đặt lịch thành công! Chúng tôi sẽ liên hệ với bạn sớm nhất.",
                        bookingInfo = new
                        {
                            customerName = customer.FirstName + " " + customer.LastName,
                            phone = customer.PhoneNumber,
                            serviceDate = serviceDateTime.ToString("dd/MM/yyyy"),
                            serviceTime = ServiceTime
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể tạo đơn đặt lịch. Vui lòng thử lại." });
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine("Booking error: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
            }
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
            Session["customer"] = null;
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
            
            var service = ServiceRepository.Instance.GetById(id);
            if (service == null)
            {
                TempData["ErrorMessage"] = "Not found service!";
                return RedirectToAction("Index");
            }
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
            ViewBag.Service = service;
            return View(model);
        }
        public ActionResult DetailPackage(int id)
        {
            
            var package = PackageRepository.Instance.GetById(id);
            if (package == null)
            {
                TempData["ErrorMessage"] = "Not found package!";
                return RedirectToAction("Index");
            }
            ViewBag.Package = package;
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
        [ChildActionOnly]
        public PartialViewResult HeaderPartial()
        {
            var services = ServiceRepository.Instance.All();
            var packages = PackageRepository.Instance.GetAll();
            var model = new HeaderModel
            {
                Services = services,
                Packages = packages
            };
            return PartialView("~/Views/Shared/Client/_PartialHeader.cshtml", model);
        }

        public ActionResult CustomerDetail(int id)
        {
            if (Session["customer"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để xem thông tin khách hàng.";
                return RedirectToAction("Login");
            }
            var customer = CustomerRepo.Instance.GetCustomerDetailById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
    }
}