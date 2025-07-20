using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Helpers;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;
using Org.BouncyCastle.Bcpg;
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
        public ActionResult AdminList ()
        {
            HashSet<EmployeeView> listEmp = new HashSet<EmployeeView>();
            var empList = EmployeeRepo.Instance.GetActiveAdmins();
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
            // ✅ Run auto-update using SQL Server time
            InvoiceRepository.Instance.AutoUpdateExpiredOrders();

            // ✅ CHỈ LẤY CÁC ĐỚN CÓ ORDER STATUS 0 VÀ 3
            var allBookings = InvoiceRepository.Instance.GetAll()
                .Where(b => b.Status == 1 && (b.Order_Status == 0 || b.Order_Status == 3)) // Chỉ pending và cancelled
                .OrderByDescending(b => b.Invoice_Date)
                .ToList();

            // Get customer information for each booking    
            foreach (var booking in allBookings)
            {
                var customer = CustomerRepo.Instance.GetCustomerById(booking.Customer_Id);
                if (customer != null)
                {
                    booking.CustomerName = $"{customer.FirstName} {customer.LastName}";
                    booking.CustomerPhone = customer.PhoneNumber;
                }
            }

            // ✅ Add helper functions for View including notes
            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);

            // ✅ NEW: Add notes helper functions
            ViewBag.FormatNotes = new Func<string, string>((notes) => {
                try
                {
                    return NotesHelper.FormatNotesForDisplay(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin - Error formatting notes: {ex.Message}");
                    return "Error loading notes";
                }
            });

            ViewBag.GetNotesTooltip = new Func<string, string>((notes) => {
                try
                {
                    return NotesHelper.GetNotesTooltip(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin - Error generating tooltip: {ex.Message}");
                    return "Error loading tooltip";
                }
            });

            // ✅ NEW: Add SQL Server time for display
            ViewBag.SqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();

            return View(allBookings);
        }

        [HttpPost]
        public JsonResult UpdateBookingStatus(int id, int newStatus)
        {
            try
            {
                if (newStatus < 0 || newStatus > 3 || newStatus == 2)
                {
                    return Json(new { success = false, message = "Invalid status value" });
                }

                // ✅ Use SQL Server time
                var sqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();
                var statusText = GetBookingStatusText(newStatus);
                var updateLog = $"\n[ADMIN UPDATE] {sqlServerTime:dd/MM/yyyy HH:mm}: Status changed to {statusText} by admin";

                bool success = InvoiceRepository.Instance.UpdateOrderStatus(id, newStatus, updateLog);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"[ADMIN-UPDATE] Booking #{id} status updated to {newStatus} by admin at {sqlServerTime:yyyy-MM-dd HH:mm:ss}");

                    // ✅ NẾU XÁC NHẬN ĐỚN (STATUS = 1), TRẢ VỀ URL CHUYỂN SANG EDIT INVOICE
                    if (newStatus == 1)
                    {
                        var editUrl = Url.Action("Edit", "Invoice", new { id = id });

                        return Json(new
                        {
                            success = true,
                            message = "Đơn hàng đã được xác nhận thành công!",
                            newStatusText = statusText,
                            newStatusClass = GetBookingStatusClass(newStatus),
                            updateTime = sqlServerTime.ToString("dd/MM/yyyy HH:mm"),
                            redirectToEdit = true,
                            editUrl = editUrl, // ✅ THÊM URL ĐỂ REDIRECT
                            confirmMessage = "Đơn hàng đã được xác nhận. Bạn sẽ được chuyển đến trang chỉnh sửa hóa đơn."
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Trạng thái đã được cập nhật thành công!",
                            newStatusText = statusText,
                            newStatusClass = GetBookingStatusClass(newStatus),
                            updateTime = sqlServerTime.ToString("dd/MM/yyyy HH:mm"),
                            redirectToEdit = false
                        });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Lỗi cập nhật cơ sở dữ liệu" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateBookingStatus Error: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        public ActionResult BookingsByStatus(int status)
        {
            var bookings = InvoiceRepository.Instance.GetAll()
                .Where(b => b.Status == 1 && b.Order_Status == status)
                .OrderByDescending(b => b.Invoice_Date)
                .ToList();

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

            // ✅ NEW: Add notes helpers
            ViewBag.FormatNotes = new Func<string, string>((notes) => {
                try
                {
                    return NotesHelper.FormatNotesForDisplay(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin - Error formatting notes: {ex.Message}");
                    return "Error loading notes";
                }
            });

            ViewBag.GetNotesTooltip = new Func<string, string>((notes) => {
                try
                {
                    return NotesHelper.GetNotesTooltip(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin - Error generating tooltip: {ex.Message}");
                    return "Error loading tooltip";
                }
            });

            ViewBag.SqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();

            return View("BookingManagement", bookings);
        }

        public ActionResult ExpiredBookings()
        {
            if (Session["admin"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var expiredBookings = InvoiceRepository.Instance.GetExpiredOrders();

            // Get customer information
            foreach (var booking in expiredBookings)
            {
                var customer = CustomerRepo.Instance.GetCustomerById(booking.Customer_Id);
                if (customer != null)
                {
                    booking.CustomerName = $"{customer.FirstName} {customer.LastName}";
                    booking.CustomerPhone = customer.PhoneNumber;
                }
            }

            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);

            // ✅ NEW: Add notes helpers
            ViewBag.FormatNotes = new Func<string, string>((notes) => {
                try
                {
                    return NotesHelper.FormatNotesForDisplay(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin - Error formatting notes: {ex.Message}");
                    return "Error loading notes";
                }
            });

            ViewBag.GetNotesTooltip = new Func<string, string>((notes) => {
                try
                {
                    return NotesHelper.GetNotesTooltip(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin - Error generating tooltip: {ex.Message}");
                    return "Error loading tooltip";
                }
            });

            ViewBag.SqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();

            return View(expiredBookings);
        }

        public ActionResult BookingDetails(int id)
        {
            var booking = InvoiceRepository.Instance.GetById(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found";
                return RedirectToAction("BookingManagement");
            }

            // Get customer information
            var customer = CustomerRepo.Instance.GetCustomerById(booking.Customer_Id);
            if (customer != null)
            {
                booking.CustomerName = $"{customer.FirstName} {customer.LastName}";
                booking.CustomerPhone = customer.PhoneNumber;
            }

            ViewBag.GetStatusText = new Func<int, string>(GetBookingStatusText);
            ViewBag.GetStatusClass = new Func<int, string>(GetBookingStatusClass);

            // ✅ NEW: Add notes helpers for detail view
            ViewBag.FormatNotes = new Func<string, string>((notes) => {
                try
                {
                    return NotesHelper.FormatNotesForDisplay(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin - Error formatting notes: {ex.Message}");
                    return "Error loading notes";
                }
            });

            ViewBag.GetNotesTooltip = new Func<string, string>((notes) => {
                try
                {
                    return NotesHelper.GetNotesTooltip(notes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin - Error generating tooltip: {ex.Message}");
                    return "Error loading tooltip";
                }
            });

            ViewBag.SqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();

            return View(booking);
        }

        [HttpPost]
        public JsonResult RunAutoUpdateExpired()
        {
            try
            {
                var sqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();
                int updatedCount = InvoiceRepository.Instance.AutoUpdateExpiredOrders();

                System.Diagnostics.Debug.WriteLine($"[ADMIN-AUTO-UPDATE] Manual trigger by admin at {sqlServerTime:yyyy-MM-dd HH:mm:ss}, updated {updatedCount} bookings");

                return Json(new
                {
                    success = true,
                    message = $"Updated {updatedCount} expired orders",
                    updatedCount = updatedCount,
                    updateTime = sqlServerTime.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public ActionResult BookingStatistics()
        {
            var sqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();
            var allBookings = InvoiceRepository.Instance.GetAll()
                .Where(b => b.Status == 1) // Only active bookings
                .ToList();

            var stats = new
            {
                TotalBookings = allBookings.Count,
                PendingBookings = allBookings.Count(b => b.Order_Status == 0),
                ConfirmedBookings = allBookings.Count(b => b.Order_Status == 1),
                PaidBookings = allBookings.Count(b => b.Order_Status == 2),
                CancelledBookings = allBookings.Count(b => b.Order_Status == 3),
                ExpiredBookings = InvoiceRepository.Instance.GetExpiredOrders().Count,
                TodayBookings = allBookings.Count(b => b.Invoice_Date.Date == sqlServerTime.Date), // ✅ Use SQL Server date
                CurrentSqlTime = sqlServerTime
            };

            ViewBag.Statistics = stats;
            ViewBag.SqlServerTime = sqlServerTime;
            return View();
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

        // Helper methods - Updated with better logging
        private string GetBookingStatusText(int status)
        {
            switch (status)
            {
                case 0: return "Pending";
                case 1: return "Confirmed";
                case 2: return "Paid";
                case 3: return "Cancelled";
                default: return "Unknown";
            }
        }
        private string GetBookingStatusClass(int status)
        {
            switch (status)
            {
                case 0: return "warning";     // Pending
                case 1: return "info";        // Confirmed
                case 2: return "success";     // Paid
                case 3: return "danger";      // Cancelled
                default: return "secondary";
            }
        }

        [HttpPost]
        public JsonResult SearchBookingsAjax(string keyword)
        {
            try
            {
                // Run auto-update using SQL Server time
                InvoiceRepository.Instance.AutoUpdateExpiredOrders();

                HashSet<InvoiceView> searchResults;

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    // If no keyword, return all bookings
                    searchResults = InvoiceRepository.Instance.GetAll()
                        .Where(b => b.Status == 1 && (b.Order_Status == 0 || b.Order_Status == 3))
                        .ToHashSet();
                }
                else
                {
                    // Search in all bookings, then filter by status
                    var allBookings = InvoiceRepository.Instance.GetAll()
                        .Where(b => b.Status == 1 && (b.Order_Status == 0 || b.Order_Status == 3))
                        .ToList();

                    // Get customer list for searching customer names
                    var customers = CustomerRepo.Instance.GetAll().ToDictionary(c => c.Id, c => c);

                    // Search by multiple criteria
                    searchResults = allBookings.Where(booking =>
                        // Search by booking ID
                        booking.Id.ToString().Contains(keyword) ||
                        // Search by customer name
                        (customers.ContainsKey(booking.Customer_Id) &&
                         (customers[booking.Customer_Id].FirstName + " " + customers[booking.Customer_Id].LastName)
                         .IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        // Search by customer phone
                        (customers.ContainsKey(booking.Customer_Id) &&
                         !string.IsNullOrEmpty(customers[booking.Customer_Id].PhoneNumber) &&
                         customers[booking.Customer_Id].PhoneNumber.Contains(keyword)) ||
                        // Search by notes
                        (!string.IsNullOrEmpty(booking.Notes) &&
                         booking.Notes.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        // Search by payment ID
                        (!string.IsNullOrEmpty(booking.Payment_Id) &&
                         booking.Payment_Id.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToHashSet();
                }

                // Sort by newest first
                var sortedResults = searchResults.OrderByDescending(b => b.Invoice_Date).ToList();

                // Get customer information and format data for JSON
                var formattedResults = sortedResults.Select(booking => {
                    var customer = CustomerRepo.Instance.GetCustomerById(booking.Customer_Id);
                    var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : $"Customer #{booking.Customer_Id}";
                    var customerPhone = customer?.PhoneNumber ?? "";

                    var sqlServerTime = InvoiceRepository.Instance.GetSqlServerDateTime();
                    var daysDiff = (booking.Invoice_Date.Date - sqlServerTime.Date).Days;
                    var timeDiff = (booking.Invoice_Date - sqlServerTime).TotalHours;

                    var timeText = daysDiff == 0 ? "Today" :
                                  daysDiff == 1 ? "Tomorrow" :
                                  daysDiff == -1 ? "Yesterday" :
                                  daysDiff > 0 ? $"In {daysDiff} days" : $"{Math.Abs(daysDiff)} days ago";

                    var timeStatus = timeDiff > 12 ? "text-info" :
                                   timeDiff > 1 ? "text-warning" :
                                   timeDiff > 0 ? "text-danger" : "text-muted";

                    var timeLabel = timeDiff > 0 ? $"In {Math.Abs(timeDiff):F1}h" : $"{Math.Abs(timeDiff):F1}h ago";

                    return new
                    {
                        Id = booking.Id,
                        Customer_Id = booking.Customer_Id,
                        CustomerName = customerName,
                        CustomerPhone = customerPhone,
                        Invoice_Date = booking.Invoice_Date.ToString("dd/MM/yyyy"),
                        Invoice_Time = booking.Invoice_Date.ToString("HH:mm"),
                        TimeText = timeText,
                        TimeLabel = timeLabel,
                        TimeStatus = timeStatus,
                        Order_Status = booking.Order_Status,
                        StatusText = GetBookingStatusText(booking.Order_Status),
                        StatusClass = GetBookingStatusClass(booking.Order_Status),
                        StatusIcon = GetStatusIcon(booking.Order_Status),
                        Notes = booking.Notes ?? "",
                        HasNotes = !string.IsNullOrEmpty(booking.Notes),
                        // Check if search term matches different fields
                        MatchesId = booking.Id.ToString().Contains(keyword ?? ""),
                        MatchesCustomer = customerName.IndexOf(keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0,
                        MatchesPhone = customerPhone.Contains(keyword ?? ""),
                        MatchesNotes = !string.IsNullOrEmpty(booking.Notes) && booking.Notes.IndexOf(keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    };
                }).ToList();

                // Calculate statistics
                var stats = new
                {
                    Total = formattedResults.Count,
                    Pending = formattedResults.Count(b => b.Order_Status == 0),
                    Confirmed = formattedResults.Count(b => b.Order_Status == 1),
                    Cancelled = formattedResults.Count(b => b.Order_Status == 3)
                };

                return Json(new
                {
                    success = true,
                    data = formattedResults,
                    stats = stats,
                    keyword = keyword,
                    serverTime = InvoiceRepository.Instance.GetSqlServerDateTime().ToString("dd/MM/yyyy HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SearchBookingsAjax Error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while searching bookings: " + ex.Message
                });
            }
        }

        // Helper method để get status icon
        private string GetStatusIcon(int status)
        {
            switch (status)
            {
                case 0: return "fa-hourglass-half";
                case 1: return "fa-check-circle";
                case 2: return "fa-money-bill-wave";
                case 3: return "fa-times-circle";
                default: return "fa-question-circle";
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
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}