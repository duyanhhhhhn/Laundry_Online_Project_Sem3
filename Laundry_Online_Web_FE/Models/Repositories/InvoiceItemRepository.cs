using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using global::Laundry_Online_Web_FE.Models.Entities;
    using global::Laundry_Online_Web_FE.Models.ModelViews;

    namespace Laundry_Online_Web_FE.Models.Repositories
    {
        public class InvoiceItemRepository
        {
            private static InvoiceItemRepository _instance = null;
            private static readonly object _lock = new object();
            private readonly OnlineLaundryEntities _context;

            private InvoiceItemRepository()
            {
                _context = new OnlineLaundryEntities();
            }

            public static InvoiceItemRepository Instance
            {
                get
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new InvoiceItemRepository();
                        return _instance;
                    }
                }
            }

            public HashSet<InvoiceItemView> GetAll()
            {
                try
                {
                    var data = _context.InvoiceItems
                        .Select(i => new InvoiceItemView
                        {
                            Item_Id = i.item_id,
                            Invoice_Id = i.invoice_id,
                            Package_Name = i.item_name,
                            Quantity = i.quantity,
                            Unit_Price = i.unit_price,
                            Sub_Price = i.sub_total,
                            barcode = i.barcode,
                            Status = i.item_status ?? 0,
                            Service_Id = i.s_id ?? 0
                        }).ToHashSet();

                    return data;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetAll InvoiceItem Error: " + ex.Message);
                    return new HashSet<InvoiceItemView>();
                }
            }

            public InvoiceItemView GetById(int id)
            {
                var i = _context.InvoiceItems.FirstOrDefault(x => x.item_id == id);
                if (i == null) return null;

                return new InvoiceItemView
                {
                    Item_Id = i.item_id,
                    Invoice_Id = i.invoice_id,
                    Package_Name = i.item_name,
                    Quantity = i.quantity,
                    Unit_Price = i.unit_price,
                    Sub_Price = i.sub_total,
                    barcode = i.barcode,
                    Status = i.item_status ?? 0,
                    Service_Id = i.s_id ?? 0
                };
            }

            public bool Add(InvoiceItemView model)
            {
                try
                {
                    var entity = new InvoiceItem
                    {
                        invoice_id = model.Invoice_Id,
                        item_name = model.Package_Name,
                        quantity = model.Quantity,
                        unit_price = model.Unit_Price,
                        sub_total = model.Sub_Price,
                        barcode = model.barcode,
                        item_status = model.Status,
                        s_id = model.Service_Id
                    };

                    _context.InvoiceItems.Add(entity);
                    _context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Add InvoiceItem Error: " + ex.Message);
                    return false;
                }
            }

            public bool Update(InvoiceItemView model)
            {
                try
                {
                    var entity = _context.InvoiceItems.FirstOrDefault(x => x.item_id == model.Item_Id);
                    if (entity == null) return false;

                    entity.invoice_id = model.Invoice_Id;
                    entity.item_name = model.Package_Name;
                    entity.quantity = model.Quantity;
                    entity.unit_price = model.Unit_Price;
                    entity.sub_total = model.Sub_Price;
                    entity.barcode = model.barcode;
                    entity.item_status = model.Status;
                    entity.s_id = model.Service_Id;

                    _context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Update InvoiceItem Error: " + ex.Message);
                    return false;
                }
            }

            public bool Delete(int id)
            {
                try
                {
                    var entity = _context.InvoiceItems.FirstOrDefault(x => x.item_id == id);
                    if (entity == null) return false;

                    _context.InvoiceItems.Remove(entity);
                    _context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Delete InvoiceItem Error: " + ex.Message);
                    return false;
                }
            }
        }
    }
}