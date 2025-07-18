using System;
using System.Collections.Generic;
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

        public HashSet<InvoiceView> GetAll()
        {
            try
            {
                // var invoices = _context.Invoices.ToList();
                // return invoices.Select(i => MapToView(i)).ToHashSet();
                var data = _context.Invoices
                    .OrderByDescending(i => i.invoice_id) // 👉 Sắp xếp theo ID mới nhất
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
                    .OrderByDescending(i => i.invoice_date) // Sắp xếp theo ngày đặt mới nhất
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
                    employee_id = model.Employee_Id,
                    invoice_date = model.Invoice_Date,
                    delivery_date = model.Delivery_Date,
                    pickup_date = model.Pickup_Date,
                    total_amount = model.Total_Amount,
                    payment_type = model.Payment_Type,
                    payment_id = model.Payment_Id,
                    order_status = model.Order_Status,
                    invoice_type = model.Invoice_Type,
                    cp_id = model.CustomerPackage_Id,
                    status = model.Status,
                    notes = model.Notes,
                    ship_cost = model.Ship_Cost,
                    delivery_status = model.Delivery_Status
                };

                _context.Invoices.Add(invoice);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Add Invoice Error: " + ex.Message);
                return false;
            }
        }

        public bool Update(InvoiceView model)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == model.Id);
                if (invoice == null) return false;

                // Update fields with proper validation
                invoice.customer_id = model.Customer_Id;
                invoice.employee_id = model.Employee_Id == 0 ? null : (int?)model.Employee_Id;
                invoice.invoice_date = model.Invoice_Date;
                invoice.delivery_date = model.Delivery_Date;
                invoice.pickup_date = model.Pickup_Date;
                // Ensure total_amount is never null or zero for existing invoices
                invoice.total_amount = model.Total_Amount <= 0 ? invoice.total_amount : model.Total_Amount;

                invoice.payment_type = model.Payment_Type == 0 ? null : (int?)model.Payment_Type;
                invoice.payment_id = string.IsNullOrEmpty(model.Payment_Id) ? null : model.Payment_Id;
                invoice.order_status = model.Order_Status == 0 ? null : (int?)model.Order_Status;
                invoice.invoice_type = model.Invoice_Type == 0 ? null : (int?)model.Invoice_Type;
                invoice.cp_id = model.CustomerPackage_Id == 0 ? null : model.CustomerPackage_Id;
                invoice.status = model.Status == 0 ? null : (int?)model.Status;

                // Truncate notes to fit database constraint (200 characters max)
                if (!string.IsNullOrEmpty(model.Notes))
                {
                    invoice.notes = model.Notes.Length > 200 ? model.Notes.Substring(0, 200) : model.Notes;
                }
                else
                {
                    invoice.notes = null;
                }

                invoice.ship_cost = model.Ship_Cost == 0 ? null : (decimal?)model.Ship_Cost;
                invoice.delivery_status = model.Delivery_Status == 0 ? null : (int?)model.Delivery_Status;

                // Validate the entity state before saving
                var validationResults = _context.Entry(invoice).GetValidationResult();
                if (!validationResults.IsValid)
                {
                    Debug.WriteLine("Entity validation failed:");
                    foreach (var error in validationResults.ValidationErrors)
                    {
                        Debug.WriteLine($"  Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                    }
                    return false;
                }

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update Invoice Error: " + ex.Message);
                return false;
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
                    revenues.Add(Math.Floor(total)); // Không có số thập phân cho VNĐ
                }
                return revenues;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetMonthlyRevenueByYear Error: " + ex.Message);
                return new List<decimal>(new decimal[12]); // Trả về 12 tháng với giá trị 0
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
                return new List<int> { DateTime.Now.Year }; // Trả về năm hiện tại nếu có lỗi
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

        public List<InvoiceView> GetExpiredOrders()
        {
            try
            {
                var oneHourAgo = DateTime.Now.AddHours(-1);

                var expiredOrders = _context.Invoices
                    .Where(i => i.invoice_date.HasValue &&
                               i.invoice_date.Value <= oneHourAgo &&
                               (i.order_status == null || i.order_status != 2))
                    .ToList();

                return expiredOrders.Select(i => MapToView(i)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetExpiredOrders Error: {ex.Message}");
                return new List<InvoiceView>();
            }
        }

        /// <summary>
        /// Automatically update order_status to 2 (cancelled) for orders expired over 1 hour
        /// </summary>
        /// <returns>Number of updated orders</returns>
        public int AutoUpdateExpiredOrders()
        {
            try
            {
                var currentTime = DateTime.Now;
                var oneHourAgo = currentTime.AddHours(-1);

                // Get all orders with order_status != 2 (not cancelled) and expired over 1 hour
                var expiredOrders = _context.Invoices
                    .Where(i => i.invoice_date.HasValue &&
                               i.invoice_date.Value <= oneHourAgo &&
                               (i.order_status == null || i.order_status != 2))
                    .ToList();

                int updatedCount = 0;
                foreach (var order in expiredOrders)
                {
                    try
                    {
                        // Update status to 2 (cancelled)
                        order.order_status = 2;

                        // Add auto-cancel note
                        var cancelNote = $"\n[AUTO CANCELLED] {currentTime.ToString("dd/MM/yyyy HH:mm")}: Order automatically cancelled due to 1 hour past expiration";
                        var currentNotes = order.notes ?? "";
                        var newNotes = currentNotes + cancelNote;

                        // Truncate notes if too long
                        if (newNotes.Length > 200)
                        {
                            newNotes = newNotes.Substring(0, 200);
                        }

                        order.notes = newNotes;

                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AUTO-UPDATE] Error updating order ID {order.invoice_id}: {ex.Message}");
                    }
                }

                if (updatedCount > 0)
                {
                    _context.SaveChanges();
                    Debug.WriteLine($"[AUTO-UPDATE] Successfully updated {updatedCount} expired orders");
                }
                else
                {
                    Debug.WriteLine("[AUTO-UPDATE] No expired orders to update");
                }

                return updatedCount;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTO-UPDATE] Error in AutoUpdateExpiredOrders: {ex.Message}");
                Debug.WriteLine($"[AUTO-UPDATE] Stack trace: {ex.StackTrace}");
                return 0;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng - chỉ update order_status và notes
        /// </summary>
        /// <param name="invoiceId">ID của invoice</param>
        /// <param name="newStatus">Trạng thái mới</param>
        /// <param name="additionalNotes">Ghi chú thêm (optional)</param>
        /// <returns>True nếu thành công</returns>
        public bool UpdateOrderStatus(int invoiceId, int newStatus, string additionalNotes = null)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == invoiceId);
                if (invoice == null)
                {
                    Debug.WriteLine($"UpdateOrderStatus Error: Invoice with ID {invoiceId} not found");
                    return false;
                }

                // Cập nhật trạng thái
                invoice.order_status = newStatus;

                // Thêm ghi chú nếu có
                if (!string.IsNullOrEmpty(additionalNotes))
                {
                    var currentNotes = invoice.notes ?? "";
                    var newNotes = currentNotes + additionalNotes;

                    // Truncate notes nếu quá dài
                    if (newNotes.Length > 200)
                    {
                        newNotes = newNotes.Substring(0, 200);
                    }

                    invoice.notes = newNotes;
                }

                _context.SaveChanges();
                Debug.WriteLine($"UpdateOrderStatus: Successfully updated invoice {invoiceId} to status {newStatus}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateOrderStatus Error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
