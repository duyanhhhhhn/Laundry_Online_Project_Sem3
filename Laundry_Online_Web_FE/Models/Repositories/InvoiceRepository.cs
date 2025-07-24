using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using System.Linq;
using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_BE.Models.Repositories
{
    public class InvoiceRepository
    {
        private static InvoiceRepository _instance = null;
        private static readonly object _lock = new object();
        private readonly OnlineLaundryEntities _context;

        private static readonly DateTime SqlMinDate = new DateTime(1753, 1, 1);

        private InvoiceRepository()
        {
            _context = new OnlineLaundryEntities();
        }

        public static InvoiceRepository Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new InvoiceRepository();
                    return _instance;
                }
            }
        }

        // ✅ ADDED: Helper method to safely handle DateTime values
        private static DateTime? SafeDateTime(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return null; // Return null instead of MinValue
            }
            if (dateTime < SqlMinDate)
            {
                return SqlMinDate; // Use SQL Server minimum date
            }
            return dateTime;
        }

        public HashSet<InvoiceView> GetAll()
        {
            try
            {
                var data = _context.Invoices
                    .OrderByDescending(i => i.invoice_id)
                    .Select(i => new InvoiceView
                    {
                        Id = i.invoice_id,
                        Customer_Id = i.customer_id,
                        Employee_Id = i.employee_id ?? 0,
                        Invoice_Date = i.invoice_date ?? DateTime.MinValue,
                        Delivery_Date = i.delivery_date ?? DateTime.MinValue,
                        Pickup_Date = i.pickup_date ?? DateTime.MinValue,
                        Total_Amount = i.total_amount,
                        Payment_Type = i.payment_type ?? 0,
                        Payment_Id = i.payment_id ?? "",
                        Order_Status = i.order_status ?? 0,
                        Invoice_Type = i.invoice_type ?? 0,
                        CustomerPackage_Id = i.cp_id ?? 0,
                        Status = i.status ?? 0,
                        Notes = i.notes ?? "",
                        Ship_Cost = i.ship_cost ?? 0,
                        Delivery_Status = i.delivery_status ?? 0
                    })
                    .ToHashSet();

                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAll Invoice Error: " + ex.Message);
                return new HashSet<InvoiceView>();
            }
        }

        public InvoiceView GetById(int id)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == id);
                return invoice != null ? MapToView(invoice) : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetById Invoice Error: " + ex.Message);
                return null;
            }
        }

        public HashSet<InvoiceView> GetByCustomerId(int customerId)
        {
            try
            {
                var invoices = _context.Invoices
                    .Where(i => i.customer_id == customerId)
                    .OrderByDescending(i => i.invoice_date)
                    .ToList();

                return invoices.Select(i => MapToView(i)).ToHashSet();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetByCustomerId Invoice Error: " + ex.Message);
                return new HashSet<InvoiceView>();
            }
        }

        public HashSet<InvoiceView> Search(string keyword)
        {
            try
            {
                return _context.Invoices
                    .Where(i => (i.payment_id ?? "").Contains(keyword) || (i.notes ?? "").Contains(keyword))
                    .Select(i => MapToView(i))
                    .ToHashSet();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Search Invoice Error: " + ex.Message);
                return new HashSet<InvoiceView>();
            }
        }

        public bool Add(InvoiceView model)
        {
            try
            {
                var invoice = new Invoice
                {
                    customer_id = model.Customer_Id,
                    employee_id = model.Employee_Id == 0 ? null : (int?)model.Employee_Id,
                    invoice_date = model.Invoice_Date,
                    delivery_date = SafeDateTime(model.Delivery_Date ?? DateTime.MinValue),
                    pickup_date = SafeDateTime(model.Pickup_Date ?? DateTime.MinValue),
                    total_amount = model.Total_Amount,
                    payment_type = model.Payment_Type == 0 ? null : (int?)model.Payment_Type,
                    payment_id = string.IsNullOrEmpty(model.Payment_Id) ? null : model.Payment_Id,
                    order_status = model.Order_Status,
                    invoice_type = model.Invoice_Type == 0 ? null : (int?)model.Invoice_Type,
                    cp_id = model.CustomerPackage_Id == 0 ? null : model.CustomerPackage_Id,
                    status = model.Status,
                    // ✅ NO LIMITS: Store notes as-is since database is nvarchar(max)
                    notes = model.Notes,
                    ship_cost = model.Ship_Cost == 0 ? null : (decimal?)model.Ship_Cost,
                    delivery_status = model.Delivery_Status == 0 ? null : (int?)model.Delivery_Status
                };

                Debug.WriteLine($"[ADD-INVOICE] Creating invoice with notes length: {invoice.notes?.Length ?? 0}");

                _context.Invoices.Add(invoice);
                _context.SaveChanges();

                Debug.WriteLine($"[ADD-INVOICE] Successfully created invoice ID: {invoice.invoice_id}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Add Invoice Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public bool Update(InvoiceView model)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == model.Id);
                if (invoice == null) return false;

                // Update all fields
                invoice.customer_id = model.Customer_Id;
                invoice.employee_id = model.Employee_Id > 0 ? (int?)model.Employee_Id : null;
                invoice.invoice_date = model.Invoice_Date;
                invoice.delivery_date = SafeDateTime(model.Delivery_Date ?? DateTime.MinValue);
                invoice.pickup_date = SafeDateTime(model.Pickup_Date ?? DateTime.MinValue);
                invoice.total_amount = model.Total_Amount <= 0 ? invoice.total_amount : model.Total_Amount;
                invoice.order_status = model.Order_Status;
                invoice.status = model.Status;
                invoice.payment_type = model.Payment_Type == 0 ? null : (int?)model.Payment_Type;
                invoice.payment_id = string.IsNullOrEmpty(model.Payment_Id) ? null : model.Payment_Id;
                invoice.invoice_type = model.Invoice_Type == 0 ? null : (int?)model.Invoice_Type;
                invoice.cp_id = model.CustomerPackage_Id == 0 ? null : model.CustomerPackage_Id;

                // ✅ NO LIMITS: Store notes as-is
                invoice.notes = model.Notes;

                // Ghi nhận ship cost và delivery status
                invoice.ship_cost = model.Ship_Cost > 0 ? (decimal?)model.Ship_Cost : null;
                invoice.delivery_status = model.Delivery_Status;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update Invoice Error: " + ex.Message);
                return false;
            }
        }

        public bool UpdateOrderStatus(int invoiceId, int newStatus, string additionalNotes = null)
        {
            try
            {
                // ✅ USE FRESH CONTEXT to avoid entity tracking issues
                using (var context = new OnlineLaundryEntities())
                {
                    var invoice = context.Invoices.FirstOrDefault(i => i.invoice_id == invoiceId);
                    if (invoice == null)
                    {
                        Debug.WriteLine($"UpdateOrderStatus Error: Invoice with ID {invoiceId} not found");
                        return false;
                    }

                    if (newStatus < 0 || newStatus > 3)
                    {
                        Debug.WriteLine($"UpdateOrderStatus Error: Invalid status {newStatus}. Valid range is 0-3");
                        return false;
                    }

                    // Update status
                    invoice.order_status = newStatus;

                    // Add notes if provided
                    if (!string.IsNullOrEmpty(additionalNotes))
                    {
                        // ✅ GIỚI HẠN ĐỘ DÀI: Kiểm tra và cắt bớt notes nếu quá dài
                        var currentNotes = invoice.notes ?? "";
                        var newNotes = currentNotes + additionalNotes;

                        // Giới hạn độ dài tối đa (ví dụ: 4000 ký tự)
                        if (newNotes.Length > 4000)
                        {
                            invoice.notes = newNotes.Substring(0, 4000);
                            Debug.WriteLine($"UpdateOrderStatus Warning: Notes truncated to 4000 characters");
                        }
                        else
                        {
                            invoice.notes = newNotes;
                        }
                    }

                    context.SaveChanges();
                    Debug.WriteLine($"UpdateOrderStatus: Successfully updated invoice {invoiceId} to status {newStatus}");
                    return true;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                Debug.WriteLine($"UpdateOrderStatus Validation Error:");
                foreach (var validationError in ex.EntityValidationErrors)
                {
                    foreach (var error in validationError.ValidationErrors)
                    {
                        Debug.WriteLine($"  - Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateOrderStatus Error: {ex.Message}");
                return false;
            }
        }

        public int AutoUpdateExpiredOrders()
        {
            try
            {
                using (var context = new OnlineLaundryEntities())
                {
                    var currentSqlTime = GetSqlServerDateTime();

                    Debug.WriteLine($"[AUTO-UPDATE] Current SQL Server time: {currentSqlTime:yyyy-MM-dd HH:mm:ss}");

                    var expiredBookings = context.Invoices
                        .Where(i => i.status == 1
                                && i.order_status == 0
                                && SqlFunctions.DateAdd("hour", 1, i.invoice_date) < currentSqlTime)
                        .ToList();

                    int updatedCount = 0;

                    foreach (var booking in expiredBookings)
                    {
                        var originalStatus = booking.order_status;
                        booking.order_status = 3; // Cancelled

                        var logMessage = $"\n[AUTO-CANCELLED] {currentSqlTime:dd/MM/yyyy HH:mm}: Automatically cancelled due to no confirmation within 1 hour of appointment time.";

                        // ✅ NO LIMITS: Append notes without any truncation
                        booking.notes = (booking.notes ?? "") + logMessage;

                        Debug.WriteLine($"[AUTO-UPDATE] Booking #{booking.invoice_id}: {originalStatus} -> {booking.order_status}");
                        updatedCount++;
                    }

                    if (updatedCount > 0)
                    {
                        context.SaveChanges();
                        Debug.WriteLine($"[AUTO-UPDATE] Updated {updatedCount} expired booking(s)");
                    }

                    return updatedCount;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTO-UPDATE] Error: {ex.Message}");
                return 0;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == id);
                if (invoice == null) return false;

                _context.Invoices.Remove(invoice);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Delete Invoice Error: " + ex.Message);
                return false;
            }
        }

        public List<decimal> GetMonthlyRevenueByYear(int year)
        {
            try
            {
                var revenues = new List<decimal>();
                for (int month = 1; month <= 12; month++)
                {
                    var start = new DateTime(year, month, 1);
                    var end = start.AddMonths(1);
                    var total = _context.Invoices
                        .Where(inv => inv.invoice_date >= start && inv.invoice_date < end)
                        .Sum(inv => (decimal?)inv.total_amount) ?? 0;
                    revenues.Add(Math.Floor(total));
                }
                return revenues;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetMonthlyRevenueByYear Error: " + ex.Message);
                return new List<decimal>(new decimal[12]);
            }
        }

        public List<int> GetAvailableYears()
        {
            try
            {
                return _context.Invoices
                    .Where(inv => inv.invoice_date.HasValue)
                    .Select(inv => inv.invoice_date.Value.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAvailableYears Error: " + ex.Message);
                return new List<int> { DateTime.Now.Year };
            }
        }

        private InvoiceView MapToView(Invoice i)
        {
            return new InvoiceView
            {
                Id = i.invoice_id,
                Customer_Id = i.customer_id,
                Employee_Id = i.employee_id ?? 0,
                Invoice_Date = i.invoice_date ?? DateTime.MinValue,
                // ✅ FIXED: Return DateTime.MinValue for display when null
                Delivery_Date = i.delivery_date ?? DateTime.MinValue,
                Pickup_Date = i.pickup_date ?? DateTime.MinValue,
                Total_Amount = i.total_amount,
                Payment_Type = i.payment_type ?? 0,
                Payment_Id = i.payment_id ?? "",
                Order_Status = i.order_status ?? 0,
                Invoice_Type = i.invoice_type ?? 0,
                CustomerPackage_Id = i.cp_id ?? 0,
                Status = i.status ?? 0,
                Notes = i.notes ?? "",
                Ship_Cost = i.ship_cost ?? 0,
                Delivery_Status = i.delivery_status ?? 0
            };
        }

        /// <summary>
        /// CẬP NHẬT: Thêm method để lấy bookings theo trạng thái
        /// </summary>
        /// <param name="status">Status (0=Inactive, 1=Active)</param>
        /// <param name="orderStatus">Order Status (0=Pending, 1=Confirmed, 2=Paid, 3=Cancelled)</param>
        /// <returns>List of matching invoices</returns>
        public HashSet<InvoiceView> GetByStatus(int? status = null, int? orderStatus = null)
        {
            try
            {
                var query = _context.Invoices.AsQueryable();

                if (status.HasValue)
                {
                    query = query.Where(i => i.status == status.Value);
                }

                if (orderStatus.HasValue)
                {
                    query = query.Where(i => i.order_status == orderStatus.Value);
                }

                var invoices = query.OrderByDescending(i => i.invoice_date).ToList();
                return invoices.Select(i => MapToView(i)).ToHashSet();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetByStatus Error: {ex.Message}");
                return new HashSet<InvoiceView>();
            }
        }
        public bool ConfirmInvoicePayment(int invoiceId, string transactionId)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == invoiceId);
                if (invoice == null)
                {
                    Debug.WriteLine("Invoice not found for ID: " + invoiceId);
                    return false;
                }

                invoice.order_status = 2; // Đã thanh toán
                invoice.payment_id = string.IsNullOrWhiteSpace(transactionId) ? null : transactionId;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConfirmInvoicePayment error: " + ex.Message);
                return false;
            }

        }
        public bool UpdateOrderStatus(int invoiceId, int newStatus)
        {
          
            {
                var invoice = _context.Invoices.Find(invoiceId);
                if (invoice == null)
                    return false;

                invoice.order_status = newStatus;
                return _context.SaveChanges() > 0;
            }
        }

        // Thêm method để lấy thời gian hiện tại từ SQL Server
        public DateTime GetSqlServerDateTime()
        {
            try
            {
                using (var context = new OnlineLaundryEntities())
                {
                    // Lấy thời gian trực tiếp từ SQL Server
                    var sqlDateTime = context.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();
                    return sqlDateTime;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GET-SQL-TIME] Error: {ex.Message}");
                // Fallback về DateTime.Now nếu có lỗi
                return DateTime.Now;
            }
        }

        public HashSet<InvoiceView> GetExpiredOrders()
        {
            try
            {
                using (var context = new OnlineLaundryEntities())
                {
                    var currentSqlTime = GetSqlServerDateTime();

                    var expiredInvoices = context.Invoices
                        .Where(i => i.status == 1
                                && i.order_status == 0
                                && SqlFunctions.DateAdd("hour", 1, i.invoice_date) < currentSqlTime)
                        .Select(i => new InvoiceView
                        {
                            Id = i.invoice_id,
                            Customer_Id = i.customer_id,
                            Employee_Id = i.employee_id ?? 0,
                            Invoice_Date = i.invoice_date ?? DateTime.MinValue,
                            Delivery_Date = i.delivery_date,
                            Pickup_Date = i.pickup_date,
                            Total_Amount = i.total_amount,
                            Payment_Type = i.payment_type ?? 0,
                            Order_Status = i.order_status ?? 0,
                            Invoice_Type = i.invoice_type ?? 0,
                            CustomerPackage_Id = i.cp_id,
                            Status = i.status ?? 0,
                            Notes = i.notes ?? "",
                            Ship_Cost = i.ship_cost ?? 0,
                            Delivery_Status = i.delivery_status ?? 0,
                            Payment_Id = i.payment_id ?? ""
                        })
                        .ToList();

                    return new HashSet<InvoiceView>(expiredInvoices);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GET-EXPIRED] Error: {ex.Message}");
                return new HashSet<InvoiceView>();
            }
        }

        // Thêm method để kiểm tra booking có thể edit không (sử dụng SQL time)
        public bool CanEditBooking(int bookingId)
        {
            try
            {
                using (var context = new OnlineLaundryEntities())
                {
                    var booking = context.Invoices.FirstOrDefault(i => i.invoice_id == bookingId);
                    if (booking == null) return false;

                    // Lấy thời gian từ SQL Server
                    var currentSqlTime = GetSqlServerDateTime();

                    // Chỉ cho phép edit nếu:
                    // 1. Status = 1 (active)
                    // 2. Order_Status = 0 (pending)
                    // 3. Appointment time > current SQL time + 12 hours
                    var canEdit = booking.status == 1 &&
                                 booking.order_status == 0 &&
                                 booking.invoice_date > currentSqlTime.AddHours(12);

                    System.Diagnostics.Debug.WriteLine($"[CAN-EDIT] Booking #{bookingId}: {canEdit} (Appointment: {booking.invoice_date:yyyy-MM-dd HH:mm}, SQL Time: {currentSqlTime:yyyy-MM-dd HH:mm})");

                    return canEdit;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CAN-EDIT] Error: {ex.Message}");
                return false;
            }
        }

        // Thêm method để validate thời gian booking mới (sử dụng SQL time)
        public bool ValidateBookingTime(DateTime appointmentTime, int minimumHours = 2)
        {
            try
            {
                // Lấy thời gian từ SQL Server
                var currentSqlTime = GetSqlServerDateTime();

                var hoursDifference = (appointmentTime - currentSqlTime).TotalHours;
                var isValid = hoursDifference >= minimumHours;

                System.Diagnostics.Debug.WriteLine($"[VALIDATE-TIME] Appointment: {appointmentTime:yyyy-MM-dd HH:mm}, SQL Time: {currentSqlTime:yyyy-MM-dd HH:mm}, Hours diff: {hoursDifference:F1}, Valid: {isValid}");

                return isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VALIDATE-TIME] Error: {ex.Message}");
                return false;
            }
        }

        // ✅ NEW: Update order status with employee tracking using existing employee_id column
        public bool UpdateOrderStatusWithEmployee(int invoiceId, int newStatus, string updateLog, int employeeId)
        {
            try
            {
                using (var context = new OnlineLaundryEntities())
                {
                    var invoice = context.Invoices.FirstOrDefault(i => i.invoice_id == invoiceId);
                    if (invoice == null) return false;

                    // Update status
                    invoice.order_status = newStatus;

                    // Update notes with employee tracking
                    invoice.notes = (invoice.notes ?? "") + updateLog;

                    // ✅ SET EMPLOYEE ID in the existing column
                    invoice.employee_id = employeeId;

                    context.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"[UPDATE-STATUS-EMPLOYEE] Invoice #{invoiceId} updated to status {newStatus} by employee #{employeeId}");

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UPDATE-STATUS-EMPLOYEE] Error: {ex.Message}");
                return false;
            }
        }
    }
}

     