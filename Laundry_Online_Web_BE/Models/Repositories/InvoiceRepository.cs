using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laundry_Online_Web_BE.Models.Entities;
using Laundry_Online_Web_BE.Models.ModelViews;

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

        // Lấy tất cả hóa đơn
        public List<InvoiceView> GetAll()
        {
            return _context.Invoices.Select(i => MapToView(i)).ToList();
        }

        // Lấy hóa đơn theo ID
        public InvoiceView GetById(int id)
        {
            var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == id);
            return invoice != null ? MapToView(invoice) : null;
        }

        // Lấy hóa đơn theo ID khách hàng
        public List<InvoiceView> GetByCustomerId(int customerId)
        {
            return _context.Invoices
                .Where(i => i.customer_id == customerId)
                .Select(i => MapToView(i))
                .ToList();
        }

        // Tìm kiếm theo từ khóa trong payment_id hoặc notes
        public List<InvoiceView> Search(string keyword)
        {
            return _context.Invoices
                .Where(i => (i.payment_id ?? "").Contains(keyword) || (i.notes ?? "").Contains(keyword))
                .Select(i => MapToView(i))
                .ToList();
        }

        // Thêm hóa đơn mới
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
            catch
            {
                return false;
            }
        }

        // Cập nhật hóa đơn
        public bool Update(InvoiceView model)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == model.Id);
                if (invoice == null)
                    return false;

                invoice.customer_id = model.Customer_Id;
                invoice.employee_id = model.Employee_Id;
                invoice.invoice_date = model.Invoice_Date;
                invoice.delivery_date = model.Delivery_Date;
                invoice.pickup_date = model.Pickup_Date;
                invoice.total_amount = model.Total_Amount;
                invoice.payment_type = model.Payment_Type;
                invoice.payment_id = model.Payment_Id;
                invoice.order_status = model.Order_Status;
                invoice.invoice_type = model.Invoice_Type;
                invoice.cp_id = model.CustomerPackage_Id;
                invoice.status = model.Status;
                invoice.notes = model.Notes;
                invoice.ship_cost = model.Ship_Cost;
                invoice.delivery_status = model.Delivery_Status;

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Xóa hóa đơn
        public bool Delete(int id)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.invoice_id == id);
                if (invoice == null)
                    return false;

                _context.Invoices.Remove(invoice);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Mapping từ Invoice => InvoiceView
        private InvoiceView MapToView(Invoice i)
        {
            return new InvoiceView
            {
                Id = i.invoice_id,
                Customer_Id = i.customer_id ?? 0,
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
    }

}