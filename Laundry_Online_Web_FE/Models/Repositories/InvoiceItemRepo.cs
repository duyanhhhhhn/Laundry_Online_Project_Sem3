using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laundry_Online_Web_BE.Models.Repositories;
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
        public List<InvoiceItem> GetListItemHasInvoiceOrderStatus()
        {
            try
            {
                var itemsList = _context.InvoiceItems.ToList();
                var listInvoice = InvoiceRepository.Instance.GetAll();

                var result = itemsList
                    .Where(item =>
                        listInvoice.Any(inv => inv.Id == item.invoice_id && inv.Order_Status > 0))
                    .ToList();

                return result;
                // var result = itemsList // nếu muốn thêm điêu kiện item_status == 0
                //.Where(item =>
                //    item.item_status == 0 &&
                //    listInvoice.Any(inv => inv.Id == item.invoice_id && inv.Order_Status > 0))
                //.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving invoice items with order status: " + ex.Message);
            }
        }
        public List<InvoiceItemView> GetInvoiceItemsByInvoiceId(int invoiceId)
        {
            try
            {
                return _context.InvoiceItems
                    .Where(ii => ii.invoice_id == invoiceId)
                    .Select(item => new InvoiceItemView
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
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving invoice items by invoice ID: " + ex.Message);
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
                var service = ServiceRepository.Instance.GetById(model.ServiceId);
                if (service == null)
                    throw new Exception("Service not found.");

                // Gọi helper để sinh barcode TRƯỚC khi tạo entity
                BarcodeGeneratorHelper.GenerateBarcodesForItem(model, service);

                var newItem = new InvoiceItem
                {
                    invoice_id = model.InvoiceId,
                    item_name = model.ItemName,
                    quantity = model.Quantity,
                    unit_price = model.UnitPrice,
                    sub_total = model.SubTotal,
                    barcode = model.BarCode, // Barcode đã được generate trong helper
                    item_status = model.ItemStatus,
                    s_id = model.ServiceId
                };

                _context.InvoiceItems.Add(newItem);
                _context.SaveChanges();

                // Update the model with the generated ID
                model.Id = newItem.item_id; // Assuming 'id' is the primary key property

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding invoice item: " + ex.Message);
            }
        }

        // Add this method to get the latest item (needed for controller)
        public InvoiceItemView GetLatestItemByInvoiceId(int invoiceId)
        {
            try
            {
                var item = _context.InvoiceItems
                    .Where(i => i.invoice_id == invoiceId)
                    .OrderByDescending(i => i.item_id)
                    .FirstOrDefault();

                if (item == null) return null;

                return new InvoiceItemView
                {
                    Id = item.item_id,
                    InvoiceId = item.invoice_id,
                    ItemName = item.item_name,
                    Quantity = item.quantity,
                    UnitPrice = item.unit_price,
                    SubTotal = item.sub_total,
                    BarCode = item.barcode,
                    ItemStatus = (int)item.item_status,
                    ServiceId = (int)item.s_id
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting latest item: " + ex.Message);
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
                    // THÊM: Xóa barcode files trước khi xóa item
                    if (!string.IsNullOrEmpty(item.barcode))
                    {
                        var barcodes = item.barcode.Split('|');
                        foreach (var barcode in barcodes)
                        {
                            try
                            {
                                var filePath = HttpContext.Current.Server.MapPath($"~/Content/Barcodes/{barcode}.png");
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                }
                            }
                            catch (Exception fileEx)
                            {
                                // Log lỗi nhưng không throw để không block việc xóa item
                                System.Diagnostics.Debug.WriteLine($"Failed to delete barcode file: {fileEx.Message}");
                            }
                        }
                    }

                    _context.InvoiceItems.Remove(item);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
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
        public List<InvoiceItemView> GetItemsByInvoiceId(int invoiceId)
        {
            try
            {
                return _context.InvoiceItems
                    .Where(ii => ii.invoice_id == invoiceId)
                    .Select(ii => new InvoiceItemView
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
                throw new Exception("Error retrieving items by invoice ID: " + ex.Message);
            }
        }
        public bool AddItem(InvoiceItem entity)
        {
            try
            {
                _context.InvoiceItems.Add(entity);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding invoice item: " + ex.Message);
            }
        }

    }
}