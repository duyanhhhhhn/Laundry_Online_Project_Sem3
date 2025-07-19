using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;
namespace Laundry_Online_Web_FE.Controllers.Admin
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index(int? year)
        {
            var years = InvoiceRepository.Instance.GetAvailableYears();
            int selectedYear = year ?? (years.Count > 0 ? years[0] : DateTime.Now.Year);

            var revenues = InvoiceRepository.Instance.GetMonthlyRevenueByYear(selectedYear);
            var months = Enumerable.Range(1, 12)
                .Select(m => m.ToString("D2") + "/" + selectedYear)
                .ToList();

            ViewBag.Months = months;
            ViewBag.Revenues = revenues;
            ViewBag.Years = years;
            ViewBag.SelectedYear = selectedYear;
            return View();
        }

        public ActionResult Admin_create_customer()
        {
            return View();
        }
        public ActionResult Admin_create_employee()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateEmployee()
        {
            string firstName = Request.Form["FirstName"];
            string lastName = Request.Form["LastName"];
            string phone = Request.Form["PhoneNumber"];
            string password = Request.Form["Password"];
            string salary = Request.Form["Salary"];
            int role = Request.Form["Role"] == "1" ? 1 : 0;

            var newemp = new EmployeeView
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Password = password,
                HireDate = DateTime.Now,
                Salary = salary != null ? int.Parse(salary) : 0,
                Role = role,
                Active = 1
            };

            bool result = EmployeeRepo.Instance.Create(newemp);
            if (result)
            {
                return RedirectToAction("EmployeeList");
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to create new employee. The phone number may already exist.";
                return View("Admin_create_employee", newemp);
            }
        }

        [HttpPost]
        public ActionResult CreateCustomer()
        {
            string firstName = Request.Form["FirstName"];
            string lastName = Request.Form["LastName"];
            string phone = Request.Form["PhoneNumber"];
            string address = Request.Form["Address"];
            string password = Request.Form["Password"];

            var newCustomer = new CustomerView
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phone,
                Address = address,
                Password = password,
                RegistrationDate = DateTime.Now,
                Active = 1
            };

            bool result = CustomerRepo.Instance.Create(newCustomer);
            if (result)
            {
                return RedirectToAction("CustomerList");
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to create new customer. The phone number may already exist.";
                return View("Admin_create_customer", newCustomer); // return lại view với dữ liệu đã nhập
            }
        }

        public ActionResult EmployeeList()
        {
            HashSet<EmployeeView> listEmp = new HashSet<EmployeeView>();
            var empList = EmployeeRepo.Instance.GetActiveStaffs();
            if (empList != null && empList.Count > 0)
            {
                listEmp = empList;
            }
            ViewBag.Data = listEmp;
            return View();
        }
        public ActionResult AllEmployeeList()
        {
            HashSet<EmployeeView> listEmp = new HashSet<EmployeeView>();
            var empList = EmployeeRepo.Instance.GetAllEmployees();
            if (empList != null && empList.Count > 0)
            {
                listEmp = empList;
            }
            ViewBag.Data = listEmp;
            return View();
        }
        public ActionResult CustomerList()
        {
            HashSet<CustomerView> listCus = new HashSet<CustomerView>();
            var cusList = CustomerRepo.Instance.GetActiveCustomer();
            if (cusList != null && cusList.Count > 0)
            {
                listCus = cusList;
            }
            ViewBag.Data = listCus;
            return View();
        }
        public ActionResult AllCustomerList()
        {
            HashSet<CustomerView> listCus = new HashSet<CustomerView>();
            var cusList = CustomerRepo.Instance.GetAll();
            if (cusList != null && cusList.Count > 0)
            {
                listCus = cusList;
            }
            ViewBag.Data = listCus;
            return View();
        }
        public ActionResult CustomerInActive()
        {
            HashSet<CustomerView> listCus = new HashSet<CustomerView>();
            var cusList = CustomerRepo.Instance.GetInactiveCustomer();
            if (cusList != null && cusList.Count > 0)
            {
                listCus = cusList;
            }
            ViewBag.Data = listCus;
            return View();
        }
        [HttpGet]
        public ActionResult Admin_edit_customer()
        {
            int id = 0;
            if (RouteData.Values["id"] != null)
            {
                int.TryParse(RouteData.Values["id"].ToString(), out id);
            }
            else
            {
                return RedirectToAction("CustomerList");
            }
            var customer = CustomerRepo.Instance.GetCustomerById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.Customer = customer;
            return View();
        }
        [HttpGet]
        public ActionResult Admin_edit_employee()
        {
            int id = 0;
            if (RouteData.Values["id"] != null)
            {
                int.TryParse(RouteData.Values["id"].ToString(), out id);
            }
            else
            {
                return RedirectToAction("EmployeeList");
            }
            var employee = EmployeeRepo.Instance.GetEmployeeById(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            ViewBag.Employee = employee;
            return View();
        }
        [HttpPost]
        public ActionResult EditEmployee()
        {
            int id = int.Parse(Request.Form["EmployeeId"]);
            string firstName = Request.Form["FirstName"];
            string lastName = Request.Form["LastName"];
            string salary = Request.Form["Salary"];
            var isEmp = EmployeeRepo.Instance.GetEmployeeById(id);
            isEmp.FirstName = firstName;
            isEmp.LastName = lastName;
            isEmp.Salary = salary != null ? int.Parse(salary) : 0;
            EmployeeRepo.Instance.Update(isEmp);
            return RedirectToAction("EmployeeList");
        }
        [HttpPost]
        public ActionResult EditCustomer()
        {
            int id = int.Parse(Request.Form["CustomerId"]);
            string firstName = Request.Form["FirstName"];
            string lastName = Request.Form["LastName"];
            string phone = Request.Form["PhoneNumber"];
            string address = Request.Form["Address"];
            DateTime registrationDate = DateTime.Parse(Request.Form["RegistrationDate"]);
            var isCus = CustomerRepo.Instance.GetCustomerById(id);
            isCus.FirstName = firstName;
            isCus.LastName = lastName;
            isCus.PhoneNumber = phone;
            isCus.Address = address;
            isCus.RegistrationDate = registrationDate;
            CustomerRepo.Instance.Update(isCus);
            return RedirectToAction("CustomerList");
        }

        [HttpPost]
        public JsonResult ChangeCustomerActiveStatus(int id)
        {
            var success = CustomerRepo.Instance.ToggleCustomerStatusActive(id);

            return Json(new
            {
                status = success ? "success" : "error",
                message = success ? "Trạng thái đã được cập nhật." : "Cập nhật thất bại."
            });
        }
        public JsonResult ChangeEmployeeActiveStatus(int id)
        {
            var success = EmployeeRepo.Instance.ToggleEmployee(id);
            return Json(new
            {
                status = success ? "success" : "error",
                message = success ? "Trạng thái đã được cập nhật." : "Cập nhật thất bại."
            });
        } // trả về true/false
        public ActionResult ServiceList()
        {
            HashSet<ServiceView> listSer = new HashSet<ServiceView>();
            var serviceList = ServiceRepository.Instance.All();
            if (serviceList != null && serviceList.Count > 0)
            {
                listSer = serviceList;
            }
            ViewBag.Data = listSer;
            return View();
        }
        public JsonResult ChangeServiceActiveStatus(int id)
        {
            var success = ServiceRepository.Instance.Delete(id);

            return Json(new
            {
                status = success ? "success" : "error",
                message = success ? "Trạng thái đã được cập nhật." : "Cập nhật thất bại."
            });
        }
        public ActionResult createService()
        {
            return View();
        }
        [HttpPost]
        public ActionResult createService(HttpPostedFileBase Image, ServiceView model)
        {
            try
            {
                string directoryPath = Server.MapPath("~/Content/client/images");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (Image != null && Image.ContentLength > 0)
                {
                    string safeFileName = Path.GetFileNameWithoutExtension(Image.FileName)
                                            .Replace(" ", "_")
                                            + Path.GetExtension(Image.FileName);
                    string newFileName = $"{DateTime.Now.Ticks}_{safeFileName}";
                    string fullPathSave = Path.Combine(directoryPath, newFileName);

                    Image.SaveAs(fullPathSave);
                    model.Image = newFileName;
                }
                else
                {
                    model.Image = "defaultimage.jpg";
                }

                ServiceRepository.Instance.Create(model);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                System.IO.File.WriteAllText(Server.MapPath("~/Content/log.txt"), ex.ToString());
            }
            return RedirectToAction("ServiceList");
        }
        [HttpGet]
        public ActionResult Admin_edit_service(int id)
        {
            var service = ServiceRepository.Instance.GetById(id);

            if (service == null)
            {
                return HttpNotFound();
            }

            return View(service);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditService(HttpPostedFileBase Image, ServiceView model)
        {
            if (ModelState.IsValid)
            {
                var existingService = ServiceRepository.Instance.GetById(model.Id);

                if (existingService == null)
                {
                    ModelState.AddModelError("", "Dịch vụ không tồn tại.");
                    return View("Admin_edit_service", model);
                }

                try
                {
                    string directoryPath = Server.MapPath("~/Content/client/images");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    if (Image != null && Image.ContentLength > 0)
                    {
                        if (!string.IsNullOrEmpty(existingService.Image) && existingService.Image != "defaultimage.jpg")
                        {
                            string oldImagePath = Path.Combine(directoryPath, existingService.Image);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string safeFileName = Path.GetFileNameWithoutExtension(Image.FileName)
                                                    .Replace(" ", "_")
                                                    + Path.GetExtension(Image.FileName);
                        string newFileName = $"{DateTime.Now.Ticks}_{safeFileName}";
                        string fullPathSave = Path.Combine(directoryPath, newFileName);

                        Image.SaveAs(fullPathSave);
                        model.Image = newFileName;
                    }
                    else
                    {
                        model.Image = existingService.Image;
                    }

                    model.Active = existingService.Active;

                    ServiceRepository.Instance.Update(model);

                    return RedirectToAction("ServiceList");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    System.IO.File.WriteAllText(Server.MapPath("~/Content/log.txt"), ex.ToString());
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dịch vụ: " + ex.Message);
                }
            }
            return View("Admin_edit_service", model);
        }
        [HttpGet]
        public ActionResult Admin_edit_BlogPost(int id)
        {
            var blog = BlogPostRepository.Instance.GetById(id);

            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(blog);
        }

        public ActionResult BookingManagement()
        {
            // Chạy auto-update trước khi hiển thị
            InvoiceRepository.Instance.AutoUpdateExpiredOrders();

            var allBookings = InvoiceRepository.Instance.GetAll()
                .Where(b => b.Invoice_Type == 1) // Chỉ lấy booking online
                .OrderByDescending(b => b.Invoice_Date)
                .ToList();

            // Lấy thông tin khách hàng cho từng booking
            foreach (var booking in allBookings)
            {
                var customer = CustomerRepo.Instance.GetCustomerById(booking.Customer_Id);
                if (customer != null)
                {
                    booking.CustomerName = $"{customer.FirstName} {customer.LastName}";
                    booking.CustomerPhone = customer.PhoneNumber;
                }
            }

            // Thêm helper functions cho View
            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);

            return View(allBookings);
        }

        // GET: Danh sách đơn đặt lịch theo trạng thái
        public ActionResult BookingsByStatus(int status)
        {
            var bookings = InvoiceRepository.Instance.GetAll()
                .Where(b => b.Invoice_Type == 1 && b.Order_Status == status)
                .OrderByDescending(b => b.Invoice_Date)
                .ToList();

            // Lấy thông tin khách hàng cho từng booking
            foreach (var booking in bookings)
            {
                var customer = CustomerRepo.Instance.GetCustomerById(booking.Customer_Id);
                if (customer != null)
                {
                    booking.CustomerName = $"{customer.FirstName} {customer.LastName}";
                    booking.CustomerPhone = customer.PhoneNumber;
                }
            }

            ViewBag.StatusFilter = status;
            ViewBag.StatusText = GetBookingStatusText(status);
            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);

            return View("BookingManagement", bookings);
        }

        // GET: Danh sách đơn đã quá hạn
        public ActionResult ExpiredBookings()
        {
            if (Session["admin"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var expiredBookings = InvoiceRepository.Instance.GetExpiredOrders();

            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);

            return View(expiredBookings);
        }

        // POST: Cập nhật trạng thái đơn đặt lịch
        [HttpPost]
        public JsonResult UpdateBookingStatus(int id, int newStatus)
        {
            try
            {
                // Tạo log message
                var statusText = GetBookingStatusText(newStatus);
                var updateLog = $"\n[ADMIN UPDATE] {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}: Status changed to {statusText}";

                // Sử dụng method chuyên dụng để update status
                bool success = InvoiceRepository.Instance.UpdateOrderStatus(id, newStatus, updateLog);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Status updated successfully",
                        newStatusText = statusText,
                        newStatusClass = GetBookingStatusClass(newStatus)
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Database update error" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateBookingStatus Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // GET: Chi tiết đơn đặt lịch
        public ActionResult BookingDetails(int id)
        {
            var booking = InvoiceRepository.Instance.GetById(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn đặt lịch";
                return RedirectToAction("BookingManagement");
            }

            // Lấy thông tin khách hàng
            var customer = CustomerRepo.Instance.GetCustomerById(booking.Customer_Id);
            if (customer != null)
            {
                booking.CustomerName = $"{customer.FirstName} {customer.LastName}";
                booking.CustomerPhone = customer.PhoneNumber;
            }

            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);

            return View(booking);
        }

        // POST: Chạy auto-update đơn quá hạn
        [HttpPost]
        public JsonResult RunAutoUpdateExpired()
        {
            try
            {
                int updatedCount = InvoiceRepository.Instance.AutoUpdateExpiredOrders();

                return Json(new
                {
                    success = true,
                    message = $"Updated {updatedCount} expired orders",
                    updatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Thống kê đơn đặt lịch
        public ActionResult BookingStatistics()
        {

            var allBookings = InvoiceRepository.Instance.GetAll()
                .Where(b => b.Invoice_Type == 1)
                .ToList();

            var stats = new
            {
                TotalBookings = allBookings.Count,
                PendingBookings = allBookings.Count(b => b.Order_Status == 0),
                ConfirmedBookings = allBookings.Count(b => b.Order_Status == 1),
                CancelledBookings = allBookings.Count(b => b.Order_Status == 2),
                ExpiredBookings = InvoiceRepository.Instance.GetExpiredOrders().Count,
                TodayBookings = allBookings.Count(b => b.Invoice_Date.Date == DateTime.Today)
            };

            ViewBag.Statistics = stats;
            return View();
        }

        // Helper methods
        private string GetBookingStatusText(int status)
        {
            switch (status)
            {
                case 0: return "Pending";
                case 1: return "Confirmed";
                case 2: return "Cancelled";
                default: return "Unknown";
            }
        }

        private string GetBookingStatusClass(int status)
        {
            switch (status)
            {
                case 0: return "warning";
                case 1: return "success";
                case 2: return "danger";
                default: return "secondary";
            }
        }
        public ActionResult CustomerDetail(int id)
        {
            var customer = CustomerRepo.Instance.GetCustomerDetailById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
    }

}