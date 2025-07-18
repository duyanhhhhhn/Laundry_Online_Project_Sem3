﻿using System;
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

                // Cập nhật các trường đơn giản
                invoice.customer_id = model.Customer_Id;
                invoice.employee_id = model.Employee_Id > 0 ? (int?)model.Employee_Id : null;
                invoice.invoice_date = model.Invoice_Date;
                invoice.delivery_date = model.Delivery_Date;
                invoice.pickup_date = model.Pickup_Date;

                // CẬP NHẬT: Xử lý Order_Status và Status đúng theo logic mới
                invoice.order_status = model.Order_Status;  // Không set null nữa vì cần chính xác
                invoice.status = model.Status;  // Không set null nữa vì cần chính xác

                invoice.payment_type = model.Payment_Type == 0 ? null : (int?)model.Payment_Type;
                invoice.payment_id = string.IsNullOrEmpty(model.Payment_Id) ? null : model.Payment_Id;
                invoice.invoice_type = model.Invoice_Type == 0 ? null : (int?)model.Invoice_Type;
                invoice.cp_id = model.CustomerPackage_Id == 0 ? null : model.CustomerPackage_Id;

                // Truncate notes to fit database constraint (200 characters max)
                if (!string.IsNullOrEmpty(model.Notes))
                    // Giữ lại total_amount cũ nếu giá trị mới không hợp lệ
                    if (model.Total_Amount > 0)
                    {
                        invoice.total_amount = model.Total_Amount;
                    }

                invoice.payment_type = model.Payment_Type > 0 ? (int?)model.Payment_Type : null;
                invoice.payment_id = string.IsNullOrWhiteSpace(model.Payment_Id) ? null : model.Payment_Id;
                invoice.order_status = model.Order_Status > 0 ? (int?)model.Order_Status : null;
                invoice.invoice_type = model.Invoice_Type > 0 ? (int?)model.Invoice_Type : null;
                invoice.cp_id = model.CustomerPackage_Id > 0 ? (int?)model.CustomerPackage_Id : null;
                invoice.status = model.Status > 0 ? (int?)model.Status : null;

                // Xử lý ghi chú giới hạn 200 ký tự
                if (!string.IsNullOrWhiteSpace(model.Notes))
                {
                    invoice.notes = model.Notes.Length > 200
                        ? model.Notes.Substring(0, 200)
                        : model.Notes;
                }
                else
                {
                    invoice.notes = null;
                }

                invoice.ship_cost = model.Ship_Cost > 0 ? (decimal?)model.Ship_Cost : null;
                invoice.delivery_status = model.Delivery_Status;

                // Validate entity trước khi lưu
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

                // CẬP NHẬT: Chỉ lấy orders với Status = 1 (active) và Order_Status = 0 (pending)
                var expiredOrders = _context.Invoices
                    .Where(i => i.invoice_date.HasValue &&
                               i.invoice_date.Value <= oneHourAgo &&
                               (i.status == 1 || i.status == null) &&  // Active bookings
                               (i.order_status == 0 || i.order_status == null))  // Pending orders
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
        /// Automatically update order_status to 3 (cancelled) for orders expired over 1 hour
        /// CẬP NHẬT: Thay đổi từ status 2 thành status 3 cho cancelled
        /// </summary>
        /// <returns>Number of updated orders</returns>
        public int AutoUpdateExpiredOrders()
        {
            try
            {
                var currentTime = DateTime.Now;
                var oneHourAgo = currentTime.AddHours(-1);

                // CẬP NHẬT: Chỉ lấy active bookings (status = 1) với pending orders (order_status = 0)
                var expiredOrders = _context.Invoices
                    .Where(i => i.invoice_date.HasValue &&
                               i.invoice_date.Value <= oneHourAgo &&
                               (i.status == 1 || i.status == null) &&  // Active bookings
                               (i.order_status == 0 || i.order_status == null))  // Pending orders
                    .ToList();

                int updatedCount = 0;
                foreach (var order in expiredOrders)
                {
                    try
                    {
                        // CẬP NHẬT: Update status to 3 (cancelled) thay vì 2
                        order.order_status = 3;

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
        /// CẬP NHẬT: Phù hợp với logic trạng thái mới
        /// </summary>
        /// <param name="invoiceId">ID của invoice</param>
        /// <param name="newStatus">Trạng thái mới (0=Pending, 1=Confirmed, 2=Paid, 3=Cancelled)</param>
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

                // CẬP NHẬT: Validate trạng thái mới theo logic mới
                if (newStatus < 0 || newStatus > 3)
                {
                    Debug.WriteLine($"UpdateOrderStatus Error: Invalid status {newStatus}. Valid range is 0-3");
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

    
    }
}

     