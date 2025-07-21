using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;
using Newtonsoft.Json;
using Laundry_Online_Web_FE.Models.ModelViews.DTO;
using Laundry_Online_Web_FE.Models.Entities;


namespace Laundry_Online_Web_FE.Controllers.Admin
{
    [RoutePrefix("Admin/Invoice")]
    public class InvoiceController : Controller
    {
        private readonly InvoiceRepository _invoiceRepository;
        private readonly CustomerRepo _customerRepository;
        private readonly EmployeeRepo _employeeRepository;
        private readonly CustomerPackageRepository _customerPackageRepository;
        private readonly PackageRepository _packageRepository;
        private readonly InvoiceItemRepo _invoiceItemRepository;
        private readonly ServiceRepository _serviceRepository;
        // private readonly VnPayPaymentService _paymentService = new VnPayPaymentService();

        public InvoiceController()
        {
            _invoiceRepository = InvoiceRepository.Instance;
            _customerRepository = CustomerRepo.Instance;
            _employeeRepository = EmployeeRepo.Instance;
            _customerPackageRepository = CustomerPackageRepository.Instance;
            _packageRepository = PackageRepository.Instance;
            // Initialize other repositories as needed
            _invoiceItemRepository = InvoiceItemRepo.Instance;
            _serviceRepository = ServiceRepository.Instance;
        }

        [HttpGet]
        [Route("")]
        // GET: Invoice
        public ActionResult Index(int? orderStatus)
        {
            var invoiceList = _invoiceRepository.GetAll();
            var customerList = _customerRepository.GetAll();
            var employeeList = _employeeRepository.All();

            // Filter by status if provided
            if (orderStatus.HasValue)
            {
                invoiceList = invoiceList.Where(i => i.Order_Status == orderStatus.Value).ToHashSet();
            }

            var invoiceViewList = new List<InvoiceForm>();

            if (invoiceList != null && invoiceList.Count > 0)
            {
                foreach (var invoice in invoiceList)
                {
                    var customer = customerList?.FirstOrDefault(c => c.Id == invoice.Customer_Id);
                    var employee = employeeList?.FirstOrDefault(e => e.Id == invoice.Employee_Id);

                    var viewModel = new InvoiceForm
                    {
                        Id = invoice.Id,
                        Customer_Id = invoice.Customer_Id,
                        Employee_Id = (int)invoice.Employee_Id,
                        Customer_Name = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Not Found",
                        Employee_Name = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Not Found",
                        Invoice_Date = invoice.Invoice_Date,
                        Delivery_Date = (DateTime)invoice.Delivery_Date,
                        Pickup_Date = (DateTime)invoice.Pickup_Date,
                        Payment_Type = invoice.Payment_Type,
                        Order_Status = invoice.Order_Status,
                        Invoice_Type = invoice.Invoice_Type,
                        CustomerPackage_Id = invoice.CustomerPackage_Id,
                        Status = invoice.Status,
                        Notes = invoice.Notes,
                        Ship_Cost = invoice.Ship_Cost,
                        Delivery_Status = invoice.Delivery_Status,
                        Payment_Id = invoice.Payment_Id,
                        TotalAmountFromDb = invoice.Total_Amount

                    };

                    invoiceViewList.Add(viewModel);
                }
            }

            ViewBag.Data = invoiceViewList;
            ViewBag.CurrentStatus = orderStatus;

            return View();
        }

        [HttpGet]
        [Route("Details/{id:int}")]
        // GET: Invoice/Details/5
        public ActionResult Details(int id)
        {
            var invoice = _invoiceRepository.GetById(id);
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction("Index");
            }

            // Get customer and employee info
            var customer = _customerRepository.GetCustomerById(invoice.Customer_Id);
            var employee = _employeeRepository.GetEmployeeById((int)invoice.Employee_Id);

            // Get invoice items
            var invoiceItems = _invoiceItemRepository.GetItemsByInvoiceId(id);
            var services = _serviceRepository.All().ToDictionary(s => s.Id, s => s);

            var invoiceDetail = new InvoiceForm
            {
                Id = invoice.Id,
                Customer_Id = invoice.Customer_Id,
                Employee_Id = (int)invoice.Employee_Id,

                // Handle null values - set to "Unknown" if null or empty
                Customer_Name = customer != null
                    ? (!string.IsNullOrEmpty(customer.FirstName) || !string.IsNullOrEmpty(customer.LastName)
                        ? $"{customer.FirstName} {customer.LastName}".Trim()
                        : "Unknown")
                    : "Unknown",

                Employee_Name = employee != null
                    ? (!string.IsNullOrEmpty(employee.FirstName) || !string.IsNullOrEmpty(employee.LastName)
                        ? $"{employee.FirstName} {employee.LastName}".Trim()
                        : "Unknown")
                    : "Unknown",

                Invoice_Date = invoice.Invoice_Date,
                Delivery_Date = invoice.Delivery_Date ?? DateTime.MinValue,
                Pickup_Date = invoice.Pickup_Date ?? DateTime.MinValue,
                Payment_Type = invoice.Payment_Type,
                Order_Status = invoice.Order_Status,
                Invoice_Type = invoice.Invoice_Type,
                CustomerPackage_Id = invoice.CustomerPackage_Id,
                Status = invoice.Status,
                Notes = string.IsNullOrEmpty(invoice.Notes) ? "Unknown" : invoice.Notes,
                Ship_Cost = invoice.Ship_Cost,
                Delivery_Status = invoice.Delivery_Status,
                Payment_Id = string.IsNullOrEmpty(invoice.Payment_Id) ? "Unknown" : invoice.Payment_Id,
                TotalAmountFromDb = invoice.Total_Amount,

                // Load invoice items with barcode
                InvoiceItems = invoiceItems.Select(i => new InvoiceItemForm
                {
                    Id = i.Id,
                    ItemName = string.IsNullOrEmpty(i.ItemName) ? "Unknown" : i.ItemName,
                    Quantity = (int)i.Quantity,
                    Unit_Price = i.UnitPrice,                 
                    Service_Id = i.ServiceId,
                    BarCode = string.IsNullOrEmpty(i.BarCode) ? "Unknown" : i.BarCode,
                    Service_Name = services.ContainsKey(i.ServiceId)
                        ? (string.IsNullOrEmpty(services[i.ServiceId].Title) ? "Unknown" : services[i.ServiceId].Title)
                        : "Unknown"
                }).ToList()
            };

            return View(invoiceDetail);
        }
        [HttpGet]
        [Route("Create")]
        public ActionResult Create()
        {
            var employeeId = Convert.ToInt32(Session["EmployeeId"]); // ví dụ lấy từ session
            var employee = _employeeRepository.GetEmployeeById(employeeId);

            ViewBag.PaymentTypeList = InvoiceForm.GetPaymentTypes().Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text
            }).ToList();

            ViewBag.InvoiceTypeList = InvoiceForm.GetInvoiceTypes().Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text
            }).ToList();

            var model = new InvoiceForm
            {
                Employee_Id = employee.Id,
                Employee_Name = employee.LastName + " " + employee.FirstName,
                Pickup_Date = DateTime.Now,
                Delivery_Date = DateTime.MinValue,
            };

            return View(model);
        }


        [HttpPost]
        [Route("Create")]
        public ActionResult CreateInvoice()
        {
            string customerId = Request.Form["Customer_Id"];
            string employeeId = Request.Form["Employee_Id"];
            string deliveryDate = Request.Form["Delivery_Date"];
            string pickupDate = Request.Form["Pickup_Date"];
            string paymentType = Request.Form["Payment_Type"];
            string invoiceType = Request.Form["Invoice_Type"];
            string customerPackageId = Request.Form["CustomerPackage_Id"];
            string notes = Request.Form["Notes"];
            string shipCost = Request.Form["Ship_Cost"];
            if (string.IsNullOrEmpty(customerId) || customerId == "0")
            {
                TempData["ErrorMessage"] = "Vui lòng chọn khách hàng.";
                return RedirectToAction("Create");
            }

            var newInvoice = new InvoiceView
            {
                Customer_Id = string.IsNullOrEmpty(customerId) ? 0 : Convert.ToInt32(customerId),
                Employee_Id = string.IsNullOrEmpty(employeeId) ? 0 : Convert.ToInt32(employeeId),
                Invoice_Date = DateTime.Now,
                Delivery_Date = string.IsNullOrEmpty(deliveryDate) ? DateTime.Now.AddDays(1) : Convert.ToDateTime(deliveryDate),
                Pickup_Date = string.IsNullOrEmpty(pickupDate) ? DateTime.Now.AddDays(3) : Convert.ToDateTime(pickupDate),
                Total_Amount = 0, // Will be calculated based on invoice items
                Payment_Type = string.IsNullOrEmpty(paymentType) ? 1 : Convert.ToInt32(paymentType),
                Order_Status = 1, // Pending
                Invoice_Type = string.IsNullOrEmpty(invoiceType) ? 1 : Convert.ToInt32(invoiceType),
                CustomerPackage_Id = string.IsNullOrEmpty(customerPackageId) ? (int?)null : Convert.ToInt32(customerPackageId),
                Status = 1, // Active
                Notes = notes ?? "",
                Ship_Cost = string.IsNullOrEmpty(shipCost) ? 0 : Convert.ToDecimal(shipCost),
                Delivery_Status = 1, // Pending
                Payment_Id = ""
            };

            bool created = _invoiceRepository.Add(newInvoice);

            if (created)
                TempData["SuccessMessage"] = "Invoice created successfully!";
            else
                TempData["ErrorMessage"] = "Failed to create invoice!";

            return RedirectToAction("Index");
        }


        [HttpGet]
        [Route("Edit/{id:int}")]
        public ActionResult Edit(int id)
        {
            var invoice = _invoiceRepository.GetById(id);
            if (invoice == null) return RedirectToAction("Index");

            var items = InvoiceItemRepo.Instance.GetItemsByInvoiceId(id);
            var customer = _customerRepository.GetCustomerById(invoice.Customer_Id);
            var employee = _employeeRepository.GetEmployeeById((int)invoice.Employee_Id);
            var services = _serviceRepository.All().ToDictionary(s => s.Id, s => s);

            var validPackages = CustomerPackageRepository.Instance.GetValidPackagesByCustomerId(invoice.Customer_Id);
            var allPackages = _packageRepository.GetAll().ToList();
            var customerPackageSelectList = (from cp in validPackages
                                             join p in allPackages on cp.Package_Id equals p.Id
                                             select new SelectListItem
                                             {
                                                 Value = cp.Id.ToString(),
                                                 Text = p.Package_Name
                                             }).ToList();

            if (!customerPackageSelectList.Any())
            {
                customerPackageSelectList.Add(new SelectListItem
                {
                    Value = "0",
                    Text = "NoPackage"
                });
            }

            ViewBag.CustomerPackageList = customerPackageSelectList;

            // Fixed: Đảm bảo selected value được set đúng
            ViewBag.DeliveryStatusList = InvoiceForm.GetDeliveryStatuses().Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == invoice.Delivery_Status.ToString()
            }).ToList();

            // Fixed: Set selected value cho Payment Type
            ViewBag.PaymentTypeList = InvoiceForm.GetPaymentTypes().Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == invoice.Payment_Type.ToString()
            }).ToList();

            // Fixed: Set selected value cho Invoice Type  
            ViewBag.InvoiceTypeList = InvoiceForm.GetInvoiceTypes().Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == invoice.Invoice_Type.ToString()
            }).ToList();

            ViewBag.ValidCustomerPackages = validPackages.Select(p => new SelectListItem
            {
                Text = $"Gói #{p.Package_Id} - HSD: {p.Date_End:dd/MM/yyyy} - Còn: {p.Value:N0}đ",
                Value = p.Id.ToString()
            }).ToList();

            var viewModel = new InvoiceForm
            {
                Id = invoice.Id,
                Customer_Id = invoice.Customer_Id,
                Customer_Name = customer.LastName + " " + customer.FirstName,
                Employee_Id = (int)invoice.Employee_Id,
                Employee_Name = employee.LastName + " " + employee.FirstName,
                Invoice_Date = invoice.Invoice_Date,
                Delivery_Date = invoice.Delivery_Date ?? DateTime.MinValue,
                Pickup_Date = invoice.Pickup_Date ?? DateTime.Now,
                Notes = invoice.Notes ?? "",
                Order_Status = invoice.Order_Status,
                Delivery_Status = invoice.Delivery_Status,
                Ship_Cost = invoice.Ship_Cost,

                // Fixed: Thêm các trường bị thiếu
                Payment_Type = invoice.Payment_Type,
                Invoice_Type = invoice.Invoice_Type,
                Status = invoice.Status,
                CustomerPackage_Id = invoice.CustomerPackage_Id,
                Payment_Id = invoice.Payment_Id ?? "",

                InvoiceItems = items.Select(i => new InvoiceItemForm
                {
                    Id = i.Id,
                    ItemName = i.ItemName,
                    Quantity = (int)i.Quantity,
                    Unit_Price = i.UnitPrice,
                    Service_Id = i.ServiceId,
                    BarCode = i.BarCode ?? "",
                    Service_Name = services.ContainsKey(i.ServiceId) ? services[i.ServiceId].Title : "",
                }).ToList()
            };

            ViewBag.ServiceList = _serviceRepository.All()
                .Select(s => new SelectListItem { Text = s.Title, Value = s.Id.ToString() }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit/{id:int}")]
        public ActionResult Edit(int id, InvoiceForm model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Model is invalid.";
                return RedirectToAction("Edit", new { id });
            }

            var invoice = _invoiceRepository.GetById(id);
            if (invoice == null)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction("Index");
            }

            // Cập nhật thông tin hóa đơn
            invoice.Notes = model.Notes;
            invoice.Pickup_Date = model.Pickup_Date;
            invoice.Delivery_Date = model.Delivery_Date;
            invoice.Delivery_Status = model.Delivery_Status;

            // Cập nhật các field bị thiếu
            invoice.Ship_Cost = model.Ship_Cost;

            invoice.CustomerPackage_Id = model.CustomerPackage_Id == 0 ? (int?)null : model.CustomerPackage_Id;

            // Xử lý các mục hóa đơn (InvoiceItems)
            var existingItems = InvoiceItemRepo.Instance.GetItemsByInvoiceId(id);
            var updatedItemIds = new HashSet<int>();

            foreach (var item in model.InvoiceItems)
            {
                if (item.Id > 0)
                {
                    var existingItem = existingItems.FirstOrDefault(i => i.Id == item.Id);
                    if (existingItem != null)
                    {
                        existingItem.ItemName = item.ItemName;
                        existingItem.Quantity = item.Quantity;
                        existingItem.UnitPrice = item.Unit_Price;
                        existingItem.SubTotal = item.Quantity * item.Unit_Price;
                        existingItem.ServiceId = item.Service_Id;

                        InvoiceItemRepo.Instance.UpdateInvoiceItem(existingItem);
                        updatedItemIds.Add(existingItem.Id);
                    }
                }
                else
                {
                    var newItem = new InvoiceItem
                    {
                        invoice_id = id,
                        item_name = item.ItemName,
                        quantity = item.Quantity,
                        unit_price = item.Unit_Price,
                        sub_total = item.Quantity * item.Unit_Price,
                        s_id = item.Service_Id,
                        barcode = item.BarCode
                    };
                    InvoiceItemRepo.Instance.AddItem(newItem);
                }
            }

            // Xoá item nào đã bị remove khỏi form
            foreach (var oldItem in existingItems)
            {
                if (!updatedItemIds.Contains(oldItem.Id))
                {
                    InvoiceItemRepo.Instance.DeleteInvoiceItem(oldItem.Id);
                }
            }

            // Tính lại tổng tiền
            var invoiceItems = InvoiceItemRepo.Instance.GetItemsByInvoiceId(id);
            invoice.Total_Amount = invoiceItems.Sum(i => i.SubTotal) + model.Ship_Cost;


            _invoiceRepository.Update(invoice);

            TempData["Success"] = "Invoice updated successfully.";
            return RedirectToAction("Edit", new { id });
        }



        // POST: Invoice/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Route("Edit/{id:int}")]
        //public ActionResult EditInvoice(int id, InvoiceForm form)
        //{
        //    try
        //    {
        //        var existingInvoice = _invoiceRepository.GetById(id);
        //        if (existingInvoice == null)
        //            return Json(new { success = false, message = "Invoice not found." });

        //        // Chỉ cập nhật các trường có thể chỉnh sửa
        //        existingInvoice.Delivery_Date = form.Delivery_Date == DateTime.MinValue ? DateTime.Now.AddDays(1) : form.Delivery_Date;
        //        existingInvoice.Pickup_Date = form.Pickup_Date;
        //        existingInvoice.Notes = form.Notes ?? "";
        //        existingInvoice.Ship_Cost = form.Ship_Cost;
        //        existingInvoice.Payment_Type = form.Payment_Type;
        //        existingInvoice.Invoice_Type = form.Invoice_Type;
        //        existingInvoice.CustomerPackage_Id = form.CustomerPackage_Id ?? 0;
        //        existingInvoice.Order_Status = form.Order_Status;
        //        existingInvoice.Delivery_Status = form.Delivery_Status;
        //        existingInvoice.Status = form.Status;
        //        existingInvoice.Total_Amount = form.Total_Amount;

        //        bool result = _invoiceRepository.Update(existingInvoice);

        //        if (result)
        //        {
        //            if (form.InvoiceItems != null && form.InvoiceItems.Count > 0)
        //            {
        //                SaveInvoiceItems(id, form.InvoiceItems);
        //            }

        //            return Json(new { success = true, message = "Invoice updated successfully!" });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Failed to update invoice!" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error: " + ex.Message });
        //    }
        //}


        // POST: Invoice/Delete/5
        [HttpPost]
        [Route("Delete")]
        public ActionResult Delete()
        {
            int id = Convert.ToInt32(Request.Form["id"]);

            var result = _invoiceRepository.Delete(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Invoice deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the invoice!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Search")]
        // GET: Invoice/Search
        public ActionResult Search(string keyword)
        {
            var invoices = string.IsNullOrEmpty(keyword)
                ? _invoiceRepository.GetAll()
                : _invoiceRepository.Search(keyword);
            ViewBag.Keyword = keyword;
            return View("Index", invoices);
        }


        [HttpGet]
        public ActionResult GetCustomerPackages(int customerId)
        {
            try
            {
                var customerPackages = _customerPackageRepository.GetAll()
                    .Where(cp => cp.Customer_Id == customerId)
                    .ToList();

                var allPackages = _packageRepository.GetAll().ToList();

                var result = (from cp in customerPackages
                              join p in allPackages on cp.Package_Id equals p.Id
                              select new
                              {
                                  cp.Id,
                                  Name = p.Package_Name
                              }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX: Get Services (for invoice items)
        [HttpGet]
        public ActionResult GetServices()
        {
            try
            {
                // This would use your ServiceRepository
                // For now, return sample data
                var services = new List<object>
                {
                    new { Id = 1, Name = "Washing", Price = 15.00 },
                    new { Id = 2, Name = "Dry Cleaning", Price = 25.00 },
                    new { Id = 3, Name = "Ironing", Price = 10.00 },
                    new { Id = 4, Name = "Folding", Price = 5.00 }
                };

                return Json(services, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        private void SaveInvoiceItems(int invoiceId, List<InvoiceItemForm> items)
        {
            if (items == null || !items.Any()) return;

            // Xoá item cũ
            var oldItems = _invoiceItemRepository.GetItemsByInvoiceId(invoiceId);
            foreach (var oldItem in oldItems)
            {
                _invoiceItemRepository.DeleteInvoiceItem(oldItem.Id);
            }

            // Lưu item mới
            foreach (var item in items)
            {
                var newItem = new InvoiceItemView
                {
                    InvoiceId = invoiceId,
                    ItemName = item.ItemName,
                    ServiceId = item.Service_Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Unit_Price,
                    SubTotal = item.Sub_Total,
                    ItemStatus = item.Item_Status
                };

                _invoiceItemRepository.AddInvoiceItem(newItem);
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Update(InvoiceForm model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        // Populate dropdowns lại khi có lỗi validation
        //        ViewBag.PaymentTypeList = InvoiceForm.GetPaymentTypes();
        //        ViewBag.InvoiceTypeList = InvoiceForm.GetInvoiceTypes();
        //        ViewBag.ServiceList = _serviceRepository.All()
        //            .Select(s => new SelectListItem
        //            {
        //                Value = s.Id.ToString(),
        //                Text = s.Title,
        //            }).ToList();

        //        return View("Edit", model);
        //    }

        //    var invoice = _invoiceRepository.GetById(model.Id);
        //    if (invoice == null)
        //        return RedirectToAction("Index");

        //    // Cập nhật các thông tin cơ bản
        //    invoice.Delivery_Date = model.Delivery_Date;
        //    invoice.Pickup_Date = model.Pickup_Date;
        //    invoice.Notes = model.Notes;
        //    invoice.Payment_Type = model.Payment_Type;
        //    invoice.Invoice_Type = model.Invoice_Type;
        //    invoice.Delivery_Status = model.Delivery_Status;
        //    invoice.Ship_Cost = model.Ship_Cost;

        //    _invoiceRepository.Update(invoice);

        //    // Xử lý danh sách InvoiceItems mới được thêm
        //    foreach (var item in model.InvoiceItems)
        //    {
        //        if (item.Id == 0)
        //        {
        //            InvoiceItemRepo.Instance.AddItem(new InvoiceItem
        //            {
        //                invoice_id = invoice.Id,
        //                item_name = item.ItemName,
        //                quantity = item.Quantity,
        //                unit_price = item.Unit_Price,
        //                sub_total = item.Sub_Total,
        //                s_id = item.Service_Id,
        //                item_status = 1
        //            });
        //        }
        //        else
        //        {
        //            // Nếu cần hỗ trợ sửa item cũ thì thêm xử lý update ở đây
        //        }
        //    }
        //    var deletedRaw = Request.Form["DeletedItemIds"];
        //    if (!string.IsNullOrEmpty(deletedRaw))
        //    {
        //        var deletedIds = deletedRaw.Split(',')
        //            .Where(s => int.TryParse(s, out _))
        //            .Select(int.Parse)
        //            .ToList();

        //        foreach (var id in deletedIds)
        //        {
        //            var item = InvoiceItemRepo.Instance.GetInvoiceItemById(id);
        //            if (item != null)
        //            {
        //                InvoiceItemRepo.Instance.DeleteInvoiceItem(id); 
        //            }
        //        }
        //    }

        //    return RedirectToAction("Edit", new { id = model.Id });
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int invoiceId, InvoiceForm model)
        {
            // Thêm logging
            System.Diagnostics.Debug.WriteLine($"ConfirmPayment called for InvoiceId: {invoiceId}");

            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                System.Diagnostics.Debug.WriteLine($"Invoice not found: {invoiceId}");
                return RedirectToAction("Index", "Invoice");
            }

            // Log trước khi update
            System.Diagnostics.Debug.WriteLine($"Updating Invoice {invoiceId} from status {invoice.Order_Status} to 2");

            invoice.Order_Status = 2;
            invoice.Delivery_Status = model.Delivery_Status;
            invoice.Notes = model.Notes;

            bool result = _invoiceRepository.Update(invoice);

            // Log kết quả
            System.Diagnostics.Debug.WriteLine($"Update result for Invoice {invoiceId}: {result}");

            return RedirectToAction("Index", "Invoice");
        }
        // Thêm các method này vào InvoiceController

        [HttpPost]
        public ActionResult AddInvoiceItem(int invoiceId, string itemName, int quantity, decimal unitPrice, int serviceId)
        {
            try
            {
                var service = _serviceRepository.GetById(serviceId);
                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found" });
                }

                var newItem = new InvoiceItemView
                {
                    InvoiceId = invoiceId,
                    ItemName = itemName,
                    ServiceId = serviceId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    SubTotal = quantity * unitPrice,
                    ItemStatus = 1 // Active
                };

                // Add item to database (this will generate barcode inside the repository)
                bool isAdded = _invoiceItemRepository.AddInvoiceItem(newItem);

                if (isAdded)
                {
                    // Get the newly added item with its ID and barcode
                    var addedItem = _invoiceItemRepository.GetLatestItemByInvoiceId(invoiceId);

                    // Update total amount của invoice
                    UpdateInvoiceTotal(invoiceId);

                    return Json(new
                    {
                        success = true,
                        itemId = addedItem.Id, // Return actual item ID
                        serviceName = service.Title,
                        subTotal = newItem.SubTotal,
                        barcode = addedItem.BarCode, // Include barcode in response
                        message = "Item added successfully"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add item" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult UpdateInvoiceItem(int itemId, string itemName, int quantity, decimal unitPrice, int serviceId)
        {
            try
            {
                var item = _invoiceItemRepository.GetInvoiceItemById(itemId);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found" });
                }

                var service = _serviceRepository.GetById(serviceId);
                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found" });
                }

                // Update item properties
                item.ItemName = itemName;
                item.Quantity = quantity;
                item.UnitPrice = unitPrice;
                item.ServiceId = serviceId;
                item.SubTotal = quantity * unitPrice;

                bool updated = _invoiceItemRepository.UpdateInvoiceItem(item);

                if (updated)
                {
                    // Update total amount của invoice
                    UpdateInvoiceTotal(item.InvoiceId);

                    return Json(new
                    {
                        success = true,
                        serviceName = service.Title,
                        subTotal = item.SubTotal,
                        message = "Item updated successfully"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update item" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteInvoiceItem(int itemId)
        {
            try
            {
                var item = _invoiceItemRepository.GetInvoiceItemById(itemId);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found" });
                }

                var invoiceId = item.InvoiceId;
                bool deleted = _invoiceItemRepository.DeleteInvoiceItem(itemId);

                if (deleted)
                {
                    // Update total amount của invoice
                    UpdateInvoiceTotal(invoiceId);

                    return Json(new { success = true, message = "Item deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete item" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetInvoiceTotal(int invoiceId)
        {
            try
            {
                var items = _invoiceItemRepository.GetItemsByInvoiceId(invoiceId);
                var invoice = _invoiceRepository.GetById(invoiceId);

                var itemTotal = items.Sum(i => i.SubTotal);
                var shippingCost = invoice?.Ship_Cost ?? 0;
                var totalAmount = itemTotal + shippingCost;

                return Json(new
                {
                    success = true,
                    itemTotal = itemTotal,
                    shippingCost = shippingCost,
                    totalAmount = totalAmount
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method để update total amount của invoice
        private void UpdateInvoiceTotal(int invoiceId)
        {
            try
            {
                var invoice = _invoiceRepository.GetById(invoiceId);
                var items = _invoiceItemRepository.GetItemsByInvoiceId(invoiceId);

                if (invoice != null)
                {
                    var itemTotal = items.Sum(i => i.SubTotal);
                    invoice.Total_Amount = itemTotal + (invoice?.Ship_Cost ?? 0);
                    _invoiceRepository.Update(invoice);
                }
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                Console.WriteLine($"Error updating invoice total: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("SearchCustomer")]
        public ActionResult SearchCustomer(string term)
        {
            var customers = _customerRepository.GetAll()
                .Where(c => c.Active == 1); // Chỉ chọn khách hàng đang hoạt động

            var matched = customers
                .Where(c =>
                    (!string.IsNullOrEmpty(c.FirstName + " " + c.LastName) &&
                     (c.FirstName + " " + c.LastName).ToLower().Contains(term.ToLower())) ||
                    (!string.IsNullOrEmpty(c.PhoneNumber) && c.PhoneNumber.Contains(term))
                )
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.FirstName + " " + c.LastName,
                    Phone = c.PhoneNumber
                }).ToList();

            return Json(matched, JsonRequestBehavior.AllowGet);
        }


    }
}