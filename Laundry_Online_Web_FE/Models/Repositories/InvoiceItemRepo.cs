using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_FE.Models.Repositories
{
    public class InvoiceItemRepo
    {
        public static InvoiceItemRepo _instance = null;
        public static InvoiceItemRepo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new InvoiceItemRepo();
                }
                return _instance;
            }
        }
        private readonly OnlineLaundryEntities _context;
        public InvoiceItemRepo()
        {
            _context = new OnlineLaundryEntities();
        }
        public List<InvoiceItemView> GetAll()
        {
            try
            {
                return _context.InvoiceItems.Select(ii => new InvoiceItemView
                {
                    Id = ii.item_id,
                    InvoiceId = ii.invoice_id,
                    ItemName = ii.item_name,
                    Quantity = ii.quantity,
                    UnitPrice = ii.unit_price,
                    SubTotal = ii.sub_total,
                    BarCode = ii.barcode,
                    ItemStatus = ii.item_status ?? 0,
                    ServiceId = ii.s_id ?? 0
                }).ToList();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                throw new Exception("Error retrieving invoice items: " + ex.Message);

            }
        }
        public InvoiceItemView GetInvoiceItemById(int id)
        {
            try
            {
                var item = _context.InvoiceItems.FirstOrDefault(ii => ii.item_id == id);
                if (item != null)
                {
                    return new InvoiceItemView
                    {
                        Id = item.item_id,
                        InvoiceId = item.invoice_id,
                        ItemName = item.item_name,
                        Quantity = item.quantity,
                        UnitPrice = item.unit_price,
                        SubTotal = item.sub_total,
                        BarCode = item.barcode,
                        ItemStatus = item.item_status ?? 0,
                        ServiceId = item.s_id ?? 0
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                throw new Exception("Error retrieving invoice item: " + ex.Message);
            }
        }
        public bool AddInvoiceItem(InvoiceItemView model)
        {
            try
            {
                var newItem = new InvoiceItem
                {
                    invoice_id = model.InvoiceId,
                    item_name = model.ItemName,
                    quantity = model.Quantity,
                    unit_price = model.UnitPrice,
                    sub_total = model.SubTotal,
                    barcode = model.BarCode,
                    item_status = model.ItemStatus,
                    s_id = model.ServiceId
                };
                _context.InvoiceItems.Add(newItem);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                throw new Exception("Error adding invoice item: " + ex.Message);
            }
        }
        public bool UpdateInvoiceItem(InvoiceItemView model)
        {
            try
            {
                var item = _context.InvoiceItems.FirstOrDefault(ii => ii.item_id == model.Id);
                if (item != null)
                {
                    item.invoice_id = model.InvoiceId;
                    item.item_name = model.ItemName;
                    item.quantity = model.Quantity;
                    item.unit_price = model.UnitPrice;
                    item.sub_total = model.SubTotal;
                    item.barcode = model.BarCode;
                    item.item_status = model.ItemStatus;
                    item.s_id = model.ServiceId;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                throw new Exception("Error updating invoice item: " + ex.Message);
            }
        }
        public bool DeleteInvoiceItem(int id)
        {
            try
            {
                var item = _context.InvoiceItems.FirstOrDefault(ii => ii.item_id == id);
                if (item != null)
                {
                    _context.InvoiceItems.Remove(item);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                throw new Exception("Error deleting invoice item: " + ex.Message);
            }
        }
        public InvoiceItemView GetItemByBarCode(string barcode) 
        {
            try
            {
                var item = _context.InvoiceItems.FirstOrDefault(i => i.barcode.Contains(barcode));
                if (item != null)
                {
                    return new InvoiceItemView
                    {
                        Id = item.item_id,
                        InvoiceId = item.invoice_id,
                        ItemName = item.item_name,
                        Quantity = item.quantity,
                        UnitPrice = item.unit_price,
                        SubTotal = item.sub_total,
                        BarCode = item.barcode,
                        ItemStatus = item.item_status ?? 0,
                        ServiceId = item.s_id ?? 0
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error found invoice item: " + ex.Message);
            }
        }
    }
}