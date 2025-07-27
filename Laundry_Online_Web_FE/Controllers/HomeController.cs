using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Helpers;
using Laundry_Online_Web_FE.Models.Dao;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.ModelViews.DTO;
using Laundry_Online_Web_FE.Models.Repositories;

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
                TempData["Message"] = "Please login to book service.";
                return RedirectToAction("Login");
            }

            return View();
        }

        private string FormatNotesForDisplay(string notes)
        {
            return NotesHelper.FormatNotesForDisplay(notes);
        }

        private string GetNotesTooltip(string notes)
        {
            return NotesHelper.GetNotesTooltip(notes);
        }

        // Updated MyBookings action
        public ActionResult MyBookings()
        {
            // Run auto-cancel before displaying list
            InvoiceRepository.Instance.AutoUpdateExpiredOrders();

            // Check login
            if (Session["customer"] == null)
            {
                TempData["Message"] = "Please login to view bookings.";
                return RedirectToAction("Login", "Home");
            }

            var customer = Session["customer"] as CustomerView;

            // Get all customer bookings (only active ones)
            var allBookings = InvoiceRepository.Instance.GetByCustomerIdUsing(customer.Id)
                .Where(b => b.Status == 1) // Only show active bookings
                .ToList();

            // Sort by newest creation date
            var sortedBookings = allBookings.OrderByDescending(b => b.Invoice_Date).ToList();

            // Add helper functions for View
            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);
            ViewBag.CanEdit = new Func<InvoiceView, bool>(CanEditBooking);

            // ✅ THÊM: Helper functions cho Notes (kiểm tra null safety)
            ViewBag.FormatNotes = new Func<string, string>((notes) => {
                try
                {
                    return FormatNotesForDisplay(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error formatting notes: {ex.Message}");
                    return "Error loading notes";
                }
            });

            ViewBag.GetNotesTooltip = new Func<string, string>((notes) => {
                try
                {
                    return GetNotesTooltip(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error generating tooltip: {ex.Message}");
                    return "Error loading tooltip";
                }
            });

            return View(sortedBookings);
        }


        // Cập nhật helper method CanEditBooking
        private bool CanEditBooking(InvoiceView booking)
        {
            // Sử dụng method từ repository thay vì DateTime.Now
            return InvoiceRepository.Instance.CanEditBooking(booking.Id);
        }

        [HttpPost]
        public JsonResult SubmitBooking()
        {
            try
            {
                var customer = Session["customer"] as CustomerView;
                if (customer == null)
                {
                    return Json(new { success = false, message = "Please login first" });
                }

                string serviceDate = Request.Form["ServiceDate"];
                string serviceTime = Request.Form["ServiceTime"];
                string notes = Request.Form["Notes"] ?? "";

                if (string.IsNullOrEmpty(serviceDate) || string.IsNullOrEmpty(serviceTime))
                {
                    return Json(new { success = false, message = "Please select both date and time" });
                }

                // Parse appointment time
                if (!DateTime.TryParse($"{serviceDate} {serviceTime}", out DateTime appointmentDateTime))
                {
                    return Json(new { success = false, message = "Invalid date/time format" });
                }

                // CẬP NHẬT: Validate với SQL Server time
                if (!InvoiceRepository.Instance.ValidateBookingTime(appointmentDateTime, 2))
                {
                    var sqlSeverTime = InvoiceRepository.Instance.GetSqlServerDateTime();
                    var hoursDiff = (appointmentDateTime - sqlSeverTime).TotalHours;

                    return Json(new
                    {
                        success = false,
                        message = $"Appointment must be at least 2 hours from now. Current server time: {sqlSeverTime:dd/MM/yyyy HH:mm}, Your selection: {appointmentDateTime:dd/MM/yyyy HH:mm}, Difference: {hoursDiff:F1} hours"
                    });
                }

                // ✅ CẬP NHẬT: Format Notes sử dụng helper
                var sqlTime = InvoiceRepository.Instance.GetSqlServerDateTime();
                var systemLog = $"[CREATED] {sqlTime:dd/MM/yyyy HH:mm}: Online booking created";
                var formattedNotes = NotesHelper.FormatNotesForSaving(notes, "", systemLog);

                // Tạo booking mới
                var newBooking = new InvoiceView
                {
                    Customer_Id = customer.Id,
                    Invoice_Date = appointmentDateTime,
                    Delivery_Date = null, // Will be set to null in database
                    Pickup_Date = null,   // Will be set to null in database
                    Total_Amount = 0,
                    Payment_Type = 1,
                    Order_Status = 0, // Pending
                    Invoice_Type = 0, // Online
                    Status = 1, // Active
                    Notes = formattedNotes,
                    Ship_Cost = 0,
                    Delivery_Status = 1,
                    Payment_Id = ""
                };

                bool success = InvoiceRepository.Instance.Add(newBooking);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"[BOOKING-CREATED] Customer #{customer.Id}, Appointment: {appointmentDateTime:yyyy-MM-dd HH:mm}");
                    return Json(new { success = true, message = "Booking created successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create booking. Please try again." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SUBMIT-BOOKING] Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // CẬP NHẬT: Thêm method để get booking với SQL Server time validation
        [HttpGet]
        public ActionResult EditBooking(int id)
        {
            try
            {
                if (Session["customer"] == null)
                {
                    TempData["ErrorMessage"] = "Please login first";
                    return RedirectToAction("Login");
                }

                var customer = Session["customer"] as CustomerView;
                var booking = InvoiceRepository.Instance.GetById(id);

                if (booking == null || booking.Customer_Id != customer.Id || booking.Status != 1)
                {
                    TempData["ErrorMessage"] = "Booking not found";
                    return RedirectToAction("MyBookings");
                }

                // ✅ Kiểm tra có thể edit không với SQL Server time
                if (!InvoiceRepository.Instance.CanEditBooking(id))
                {
                    var sqlTime = InvoiceRepository.Instance.GetSqlServerDateTime();
                    TempData["ErrorMessage"] = $"Cannot edit booking. Either the booking is not pending or the edit window has closed. Current server time: {sqlTime:dd/MM/yyyy HH:mm}";
                    return RedirectToAction("MyBookings");
                }

                // ✅ Thêm SQL Server time vào ViewBag để sử dụng trong View
                ViewBag.SqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();

                return View(booking);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING-GET] Error: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred";
                return RedirectToAction("MyBookings");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBooking(int id, string ServiceTime, string ServiceDate, string Notes)
        {
            try
            {
                if (Session["customer"] == null)
                {
                    TempData["ErrorMessage"] = "Session expired.";
                    return RedirectToAction("Login", "Home");
                }

                var customer = Session["customer"] as CustomerView;
                var booking = InvoiceRepository.Instance.GetById(id);

                if (booking == null || booking.Customer_Id != customer.Id || booking.Status != 1)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("MyBookings");
                }

                if (booking.Order_Status != 0)
                {
                    TempData["ErrorMessage"] = "Only pending bookings can be edited.";
                    return RedirectToAction("MyBookings");
                }

                if (!CanEditBooking(booking))
                {
                    TempData["ErrorMessage"] = "This booking cannot be edited.";
                    return RedirectToAction("MyBookings");
                }

                // Parse new date and time
                DateTime newServiceDateTime;
                if (!DateTime.TryParse(ServiceDate, out newServiceDateTime))
                {
                    TempData["ErrorMessage"] = "Invalid date.";
                    return RedirectToAction("MyBookings");
                }

                if (!string.IsNullOrEmpty(ServiceTime))
                {
                    if (TimeSpan.TryParse(ServiceTime, out TimeSpan timeSpan))
                    {
                        newServiceDateTime = newServiceDateTime.Date.Add(timeSpan);
                    }
                }

                var sqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();
                if (newServiceDateTime <= sqlServerTime.AddHours(12))
                {
                    TempData["ErrorMessage"] = $"Appointment time must be at least 12 hours from now. Current server time: {sqlServerTime:dd/MM/yyyy HH:mm}";
                    return RedirectToAction("MyBookings");
                }

                // Store old values for logging
                var oldDateTime = booking.Invoice_Date;

                // Update information
                booking.Invoice_Date = newServiceDateTime;

                if (booking.Delivery_Date.HasValue && booking.Delivery_Date.Value == DateTime.MinValue)
                {
                    booking.Delivery_Date = null;
                }
                if (booking.Pickup_Date.HasValue && booking.Pickup_Date.Value == DateTime.MinValue)
                {
                    booking.Pickup_Date = null;
                }

                var sqlServerTime2 = InvoiceRepository.Instance.GetSqlServerDateTime();
                var systemLog = $"[UPDATED] {sqlServerTime2:dd/MM/yyyy HH:mm}: Changed appointment time from {oldDateTime:dd/MM/yyyy HH:mm} to {newServiceDateTime:dd/MM/yyyy HH:mm}";

                var formattedNotes = NotesHelper.FormatNotesForSaving(Notes, booking.Notes, systemLog);

                // ✅ NO LIMITS: Store notes as-is
                booking.Notes = formattedNotes;

                bool success = InvoiceRepository.Instance.Update(booking);

                if (success)
                {
                    TempData["SuccessMessage"] = "Booking updated successfully!";
                    return RedirectToAction("MyBookings");
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while updating.";
                    return RedirectToAction("MyBookings");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EDIT-BOOKING] Exception: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("MyBookings");
            }
        }

        private string GetBookingStatusText(int orderStatus)
        {
            switch (orderStatus)
            {
                case 0: return "Pending"; // Chờ xác nhận
                case 1: return "Confirmed"; // Đã xác nhận
                case 2: return "Paid"; // Đã thanh toán
                case 3: return "Cancelled"; // Đã hủy
                default: return "Unknown";
            }
        }

        // Helper method để lấy CSS class cho trạng thái booking - CẬP NHẬT
        private string GetBookingStatusClass(int orderStatus)
        {
            switch (orderStatus)
            {
                case 0: return "warning"; // Pending - màu vàng
                case 1: return "info"; // Confirmed - màu xanh dương
                case 2: return "success"; // Paid - màu xanh lá
                case 3: return "danger"; // Cancelled - màu đỏ
                default: return "secondary";
            }
        }

        public void AutoCancelExpiredBookings()
        {
            try
            {
                var sqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();
                var pendingBookings = InvoiceRepository.Instance.GetAll()
                    .Where(b => b.Status == 1 && b.Order_Status == 0)
                    .ToList();

                int cancelledCount = 0;
                foreach (var booking in pendingBookings)
                {
                    if (booking.Invoice_Date <= sqlServerTime.AddHours(-1))
                    {
                        booking.Order_Status = 3;

                        var cancelLog = $"\n[AUTO CANCELLED] {sqlServerTime:dd/MM/yyyy HH:mm}: Automatically cancelled due to 1 hour past appointment time without confirmation";

                        // ✅ NO LIMITS: Append notes without any truncation
                        booking.Notes = (booking.Notes ?? "") + cancelLog;

                        bool updateSuccess = InvoiceRepository.Instance.Update(booking);

                        if (updateSuccess)
                        {
                            cancelledCount++;
                            Debug.WriteLine($"[AUTO-CANCEL] Successfully cancelled booking ID: {booking.Id}");
                        }
                    }
                }

                Debug.WriteLine($"[AUTO-CANCEL] Completed. Cancelled {cancelledCount} bookings");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTO-CANCEL] Error: {ex.Message}");
            }
        }

        [HttpGet]
        public JsonResult GetServerTime()
        {
            try
            {
                var sqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();

                return Json(new
                {
                    success = true,
                    serverTime = sqlServerTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    displayTime = sqlServerTime.ToString("dd/MM/yyyy HH:mm:ss"),
                    timestamp = ((DateTimeOffset)sqlServerTime).ToUnixTimeSeconds()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    serverTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") // Fallback
                }, JsonRequestBehavior.AllowGet);
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
        public ActionResult Contact_us()
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
                ViewBag.ErrorMessage = "The phone number has already been used! Please use a different phone number to create an account!";
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
                TempData["Message"] = "You need to log in to view customer information.";
                return RedirectToAction("Login");
            }
            var customer = CustomerRepo.Instance.GetCustomerDetailById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
        public ActionResult EditCustomer(int id)
        {
            if (Session["customer"] == null)
            {
                TempData["Message"] = "You need to log in to edit customer information.";
                return RedirectToAction("Login");
            }
            var customer = CustomerRepo.Instance.GetCustomerById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }
        [HttpPost]
        public ActionResult EditCustomer(CustomerView customer)
        {
            if (Session["customer"] == null)
            {
                TempData["Message"] = "You need to log in to edit customer information.";
                return RedirectToAction("Login");
            }
            if (ModelState.IsValid)
            {
                var result = CustomerRepo.Instance.UpdateCusClient(customer);
                if (result)
                {
                    TempData["Message"] = "Customer information updated successfully.";
                    return RedirectToAction("CustomerDetail", new { id = customer.Id });
                }
                else
                {
                    ModelState.AddModelError("", "Error updating customer information. Please try again.");
                }
            }
            return View(customer);
        }
    }
}