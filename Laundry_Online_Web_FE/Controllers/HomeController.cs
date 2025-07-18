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
using Laundry_Online_Web_FE.Models.ModelViews;

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

        // Updated MyBookings action
        public ActionResult MyBookings()
        {
            // Run auto-cancel before displaying list
            AutoCancelExpiredBookings();

            // Check login
            if (Session["customer"] == null)
            {
                TempData["Message"] = "Please login to view bookings.";
                return RedirectToAction("Login", "Home");
            }

            var customer = Session["customer"] as CustomerView;

            // Get all customer bookings (only active ones)
            var allBookings = InvoiceRepository.Instance.GetByCustomerId(customer.Id)
                .Where(b => b.Status == 1) // Only show active bookings
                .ToList();

            // Sort by newest creation date
            var sortedBookings = allBookings.OrderByDescending(b => b.Invoice_Date).ToList();

            // Add helper functions for View
            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);
            ViewBag.CanEdit = new Func<InvoiceView, bool>(CanEditBooking);

            return View(sortedBookings);
        }

        [HttpPost]
        public JsonResult SubmitBooking(string ServiceTime, string ServiceDate, string Notes)
        {
            try
            {
                // Run auto-cancel before creating new booking
                AutoCancelExpiredBookings();

                // Check login
                if (Session["customer"] == null)
                {
                    return Json(new { success = false, message = "Please login to book a service." });
                }

                var customer = Session["customer"] as Models.ModelViews.CustomerView;
                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer information not found." });
                }

                // Parse date and time
                DateTime serviceDateTime;
                if (!DateTime.TryParse(ServiceDate, out serviceDateTime))
                {
                    return Json(new { success = false, message = "Invalid date format" });
                }

                // Combine date and time
                if (!string.IsNullOrEmpty(ServiceTime))
                {
                    if (TimeSpan.TryParse(ServiceTime, out TimeSpan timeSpan))
                    {
                        serviceDateTime = serviceDateTime.Date.Add(timeSpan);
                    }
                }

                // Check time must be at least 2 hours ahead
                if (serviceDateTime <= DateTime.Now.AddHours(2))
                {
                    return Json(new { success = false, message = "Appointment time must be at least 2 hours from now." });
                }

                // Create Notes with standard format
                string bookingNotes = $"[ONLINE BOOKING] {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}\n" +
                                     $"Customer: {customer.FirstName} {customer.LastName}\n" +
                                     $"Phone: {customer.PhoneNumber}\n" +
                                     $"Address: {customer.Address}\n" +
                                     $"Appointment Time: {serviceDateTime.ToString("dd/MM/yyyy HH:mm")}";

                // Add customer notes if provided
                if (!string.IsNullOrEmpty(Notes) && !string.IsNullOrWhiteSpace(Notes))
                {
                    bookingNotes += $"\n[NOTES]: {Notes.Trim()}";
                }

                // Truncate notes if too long (200 characters)
                if (bookingNotes.Length > 200)
                {
                    bookingNotes = bookingNotes.Substring(0, 200);
                }

                // Create booking invoice with proper nullable field handling
                var bookingInvoice = new Models.ModelViews.InvoiceView
                {
                    Customer_Id = customer.Id,
                    Employee_Id = null,
                    Invoice_Date = serviceDateTime,
                    Delivery_Date = null,
                    Pickup_Date = null,
                    Total_Amount = 0m,
                    Payment_Type = 0,
                    Payment_Id = string.Empty,
                    Order_Status = 0,  // 0 = Chờ xác nhận
                    Invoice_Type = 1,
                    CustomerPackage_Id = null,
                    Status = 1,  // 1 = Active (còn hoạt động)
                    Notes = bookingNotes,
                    Ship_Cost = 0m,
                    Delivery_Status = 0
                };

                System.Diagnostics.Debug.WriteLine($"[SUBMIT-BOOKING] Creating booking: Customer_Id={bookingInvoice.Customer_Id}, Invoice_Date={bookingInvoice.Invoice_Date}");

                bool success = InvoiceRepository.Instance.Add(bookingInvoice);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Booking successful! We will contact you for confirmation as soon as possible.",
                        bookingInfo = new
                        {
                            customerName = customer.FirstName + " " + customer.LastName,
                            phone = customer.PhoneNumber,
                            address = customer.Address,
                            serviceDate = serviceDateTime.ToString("dd/MM/yyyy"),
                            serviceTime = ServiceTime,
                            bookingTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to create booking. Please try again." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Booking error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }
        // GET: Edit Booking
        public ActionResult EditBooking(int id)
        {
            // Check login
            if (Session["customer"] == null)
            {
                TempData["Message"] = "Please login to edit booking.";
                return RedirectToAction("Login", "Home");
            }

            var customer = Session["customer"] as CustomerView;
            var booking = InvoiceRepository.Instance.GetById(id);

            System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING-GET] ID: {id}, Booking found: {booking != null}");

            // Check ownership and active status
            if (booking == null || booking.Customer_Id != customer.Id || booking.Status != 1)
            {
                TempData["ErrorMessage"] = "Booking not found or you don't have permission to edit.";
                return RedirectToAction("MyBookings");
            }

            // Check booking must have order_status = 0 (chờ xác nhận)
            if (booking.Order_Status != 0)
            {
                TempData["ErrorMessage"] = "Only pending bookings can be edited.";
                return RedirectToAction("MyBookings");
            }

            // Check if booking can be edited (before 12 hours)
            if (!CanEditBooking(booking))
            {
                TempData["ErrorMessage"] = "This booking cannot be edited. Bookings must be edited at least 12 hours in advance.";
                return RedirectToAction("MyBookings");
            }

            // Pass current notes to ViewBag for display in form
            ViewBag.CurrentNotes = booking.Notes ?? "";

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBooking(int id, string ServiceTime, string ServiceDate, string Notes)
        {
            try
            {
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] POST received - ID: {id}, Date: {ServiceDate}, Time: {ServiceTime}");

                if (Session["customer"] == null)
                {
                    System.Diagnostics.Debug.WriteLine("[EDIT-BOOKING] Session expired");
                    TempData["ErrorMessage"] = "Session expired.";
                    return RedirectToAction("Login", "Home");
                }

                var customer = Session["customer"] as CustomerView;
                var booking = InvoiceRepository.Instance.GetById(id);

                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Booking found: {booking != null}, Customer match: {booking?.Customer_Id == customer?.Id}");

                if (booking == null || booking.Customer_Id != customer.Id || booking.Status != 1)
                {
                    System.Diagnostics.Debug.WriteLine("[EDIT-BOOKING] Booking not found or access denied");
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("MyBookings");
                }

                // Check order_status must be 0 (chờ xác nhận)
                if (booking.Order_Status != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Invalid status: {booking.Order_Status}");
                    TempData["ErrorMessage"] = "Only pending bookings can be edited.";
                    return RedirectToAction("MyBookings");
                }

                if (!CanEditBooking(booking))
                {
                    System.Diagnostics.Debug.WriteLine("[EDIT-BOOKING] Cannot edit booking - time restriction");
                    TempData["ErrorMessage"] = "This booking cannot be edited.";
                    return RedirectToAction("MyBookings");
                }

                // Parse new date and time
                DateTime newServiceDateTime;
                if (!DateTime.TryParse(ServiceDate, out newServiceDateTime))
                {
                    System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Invalid date format: {ServiceDate}");
                    TempData["ErrorMessage"] = "Invalid date.";
                    return RedirectToAction("MyBookings");
                }

                // Combine date and time
                if (!string.IsNullOrEmpty(ServiceTime))
                {
                    if (TimeSpan.TryParse(ServiceTime, out TimeSpan timeSpan))
                    {
                        newServiceDateTime = newServiceDateTime.Date.Add(timeSpan);
                        System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] New datetime: {newServiceDateTime}");
                    }
                }

                // Check new date must be in the future
                if (newServiceDateTime <= DateTime.Now.AddHours(12))
                {
                    System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Time too close: {newServiceDateTime} vs {DateTime.Now.AddHours(12)}");
                    TempData["ErrorMessage"] = "Appointment time must be at least 12 hours from now.";
                    return RedirectToAction("MyBookings");
                }

                // Store old values for logging
                var oldDateTime = booking.Invoice_Date;

                // Update information - only update necessary fields
                booking.Invoice_Date = newServiceDateTime;

                // Ensure nullable fields are handled correctly
                if (booking.Delivery_Date.HasValue && booking.Delivery_Date.Value == DateTime.MinValue)
                {
                    booking.Delivery_Date = null;
                }
                if (booking.Pickup_Date.HasValue && booking.Pickup_Date.Value == DateTime.MinValue)
                {
                    booking.Pickup_Date = null;
                }

                // Handle new Notes - this is the main change
                string newNotes = Notes ?? ""; // Use notes from form instead of old notes

                // Add time update log
                var updateLog = $"\n[UPDATED] {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}: Changed appointment time from {oldDateTime.ToString("dd/MM/yyyy HH:mm")} to {newServiceDateTime.ToString("dd/MM/yyyy HH:mm")}";
                newNotes += updateLog;

                // Truncate notes if too long (200 characters)
                if (newNotes.Length > 200)
                {
                    newNotes = newNotes.Substring(0, 200);
                }

                booking.Notes = newNotes;

                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] About to update booking {booking.Id}");
                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Values: Invoice_Date={booking.Invoice_Date}, Delivery_Date={booking.Delivery_Date}, Pickup_Date={booking.Pickup_Date}");
                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Notes length: {booking.Notes?.Length ?? 0}");

                bool success = InvoiceRepository.Instance.Update(booking);

                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Update result: {success}");

                if (success)
                {
                    TempData["SuccessMessage"] = "Booking updated successfully!";
                    System.Diagnostics.Debug.WriteLine("[EDIT-BOOKING] Success - redirecting to MyBookings");
                    return RedirectToAction("MyBookings");
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while updating.";
                    System.Diagnostics.Debug.WriteLine("[EDIT-BOOKING] Update failed");
                    return RedirectToAction("MyBookings");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[EDIT-BOOKING] Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("MyBookings");
            }
        }

        // Helper methods cho booking
        private bool CanEditBooking(InvoiceView booking)
        {
            // Chỉ cho phép chỉnh sửa nếu:
            // 1. Status = 1 (active)
            // 2. Order_Status = 0 (chờ xác nhận)
            // 3. Lịch hẹn chưa tới (invoice_date > now + 12h)

            if (booking.Status != 1) return false; // Chỉ cho phép edit booking active
            if (booking.Order_Status != 0) return false; // Chỉ cho phép edit status chờ xác nhận

            var appointmentTime = booking.Invoice_Date;
            return appointmentTime > DateTime.Now.AddHours(12);
        }

        // Helper method để lấy text trạng thái booking - CẬP NHẬT
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

        /// <summary>
        /// Auto-cancel expired bookings - cancel after 1 hour
        /// </summary>
        public void AutoCancelExpiredBookings()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AUTO-CANCEL] Starting auto-cancel check at {DateTime.Now}");

                // Get all active bookings with order_status = 0 (chờ xác nhận)
                var pendingBookings = InvoiceRepository.Instance.GetAll()
                    .Where(b => b.Status == 1 && b.Order_Status == 0 && b.Invoice_Type == 1) // Only active online bookings
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[AUTO-CANCEL] Found {pendingBookings.Count} pending bookings");

                int cancelledCount = 0;
                foreach (var booking in pendingBookings)
                {
                    var timeDiff = DateTime.Now - booking.Invoice_Date;
                    System.Diagnostics.Debug.WriteLine($"[AUTO-CANCEL] Booking {booking.Id}: Invoice_Date={booking.Invoice_Date}, TimeDiff={timeDiff.TotalHours:F2} hours");

                    // Check if appointment is over 1 hour past current time
                    if (booking.Invoice_Date <= DateTime.Now.AddHours(-1))
                    {
                        // Update status to "cancelled" (Order_Status = 3)
                        booking.Order_Status = 3;

                        // Add log to Notes
                        var cancelLog = $"\n[AUTO CANCELLED] {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}: Automatically cancelled due to 1 hour past appointment time without confirmation";
                        string newNotes = (booking.Notes ?? "") + cancelLog;

                        // Truncate notes if too long (200 characters)
                        if (newNotes.Length > 200)
                        {
                            newNotes = newNotes.Substring(0, 200);
                        }

                        booking.Notes = newNotes;

                        // Update to database
                        bool updateSuccess = InvoiceRepository.Instance.Update(booking);

                        if (updateSuccess)
                        {
                            cancelledCount++;
                            System.Diagnostics.Debug.WriteLine($"[AUTO-CANCEL] Successfully cancelled booking ID: {booking.Id}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AUTO-CANCEL] Failed to cancel booking ID: {booking.Id}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[AUTO-CANCEL] Completed. Cancelled {cancelledCount} bookings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTO-CANCEL] Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[AUTO-CANCEL] Stack trace: {ex.StackTrace}");
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
                ViewBag.ErrorMessage =  "Register Error. Try again";
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
    }
}