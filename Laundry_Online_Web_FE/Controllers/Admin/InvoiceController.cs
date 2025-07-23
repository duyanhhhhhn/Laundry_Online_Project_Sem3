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
            try
            {
                var employee = Session["employee"] as EmployeeView;
                var admin = Session["employee"] as EmployeeView;

                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found. Please log in again.";
                    return RedirectToAction("Index");
                }

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
                    Employee_Name = $"{employee.FirstName} {employee.LastName}".Trim(),
                    Pickup_Date = DateTime.Now,
                    Delivery_Date = DateTime.MinValue, // Default to 3 days later
                    Ship_Cost = 0
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the create form.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public ActionResult Create(InvoiceForm model)
        {
            try
            {
                // Validate required fields
                if (model.Customer_Id <= 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn khách hàng.";
                    return RedirectToAction("Create");
                }

                if (model.Employee_Id <= 0)
                {
                    TempData["ErrorMessage"] = "Employee information is missing.";
                    return RedirectToAction("Create");
                }

         
                // Validate customer exists
                var customer = _customerRepository.GetCustomerById(model.Customer_Id);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Selected customer not found.";
                    return RedirectToAction("Create");
                }

                // Validate employee exists
                var employee = _employeeRepository.GetEmployeeById(model.Employee_Id);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found.";
                    return RedirectToAction("Create");
                }

                var newInvoice = new InvoiceView
                {
                    Customer_Id = model.Customer_Id,
                    Employee_Id = model.Employee_Id,
                    Invoice_Date = DateTime.Now,
                    Delivery_Date = model.Delivery_Date,
                    Pickup_Date = model.Pickup_Date,
                    Total_Amount = model.Ship_Cost, // Initially only shipping cost
                    Payment_Type = model.Payment_Type,
                    Order_Status = 1, // Pending
                    Invoice_Type = model.Invoice_Type,
                    CustomerPackage_Id = model.CustomerPackage_Id == 0 ? (int?)null : model.CustomerPackage_Id,
                    Status = 1, // Active
                    Notes = model.Notes?.Trim() ?? "",
                    Ship_Cost = model.Ship_Cost,
                    Delivery_Status = 1, // Pending
                    Payment_Id = "" // Will be set when payment is processed
                };

                bool created = _invoiceRepository.Add(newInvoice);

                if (created)
                {
                    // Get the created invoice ID to redirect to edit page for adding items
                    var createdInvoice = _invoiceRepository.GetAll()
                        .OrderByDescending(i => i.Id)
                        .FirstOrDefault(i => i.Customer_Id == model.Customer_Id &&
                                           i.Employee_Id == model.Employee_Id);

                    if (createdInvoice != null)
                    {
                        TempData["SuccessMessage"] = "Invoice created successfully! You can now add items to the invoice.";
                        return RedirectToAction("Edit", new { id = createdInvoice.Id });
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Invoice created successfully!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create invoice. Please try again.";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                // Log the error if you have logging
                System.Diagnostics.Debug.WriteLine($"Error creating invoice: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = "An error occurred while creating the invoice. Please try again.";
                return RedirectToAction("Create");
            }
        }

        // Keep your existing SearchCustomer method - it looks correct
        [HttpGet]
        [Route("SearchCustomer")]
        public ActionResult SearchCustomer(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                var customers = _customerRepository.GetAll()
                    .Where(c => c.Active == 1) // Only active customers
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.FirstName + " " + c.LastName) &&
                         (c.FirstName + " " + c.LastName).ToLower().Contains(term.ToLower())) ||
                        (!string.IsNullOrEmpty(c.PhoneNumber) && c.PhoneNumber.Contains(term))
                    )
                    .Select(c => new
                    {
                        Id = c.Id,
                        Name = $"{c.FirstName} {c.LastName}".Trim(),
                        Phone = c.PhoneNumber ?? ""
                    })
                    .Take(10) // Limit results to prevent performance issues
                    .ToList();

                return Json(customers, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching customers: {ex.Message}");
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
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
            var itemTotal = items.Sum(i => i.SubTotal);
            var totalAmount = itemTotal + invoice.Ship_Cost;
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
                TotalAmountInvoice = totalAmount,
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
                    SubTotalItem = i.SubTotal,
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
            try
            {
                // Log để debug
                System.Diagnostics.Debug.WriteLine($"Edit Invoice - ID: {id}");
                System.Diagnostics.Debug.WriteLine($"Model Items Count: {model.InvoiceItems?.Count ?? 0}");

                if (!ModelState.IsValid)
                {
                    // Log lỗi validation
                    foreach (var error in ModelState)
                    {
                        System.Diagnostics.Debug.WriteLine($"ModelState Error - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }

                    TempData["Error"] = "Please correct the validation errors and try again.";
                    return RedirectToAction("Edit", new { id });
                }

                var invoice = _invoiceRepository.GetById(id);
                if (invoice == null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction("Index");
                }

                // Cập nhật thông tin hóa đơn cơ bản
                invoice.Notes = model.Notes?.Trim();
                invoice.Pickup_Date = model.Pickup_Date;
                invoice.Delivery_Date = model.Delivery_Date;
                invoice.Delivery_Status = model.Delivery_Status;
                invoice.Ship_Cost = model.Ship_Cost;
                invoice.Payment_Type = model.Payment_Type;
                invoice.Invoice_Type = model.Invoice_Type;

                // Xử lý CustomerPackage_Id
                invoice.CustomerPackage_Id = model.CustomerPackage_Id == 0 ? (int?)null : model.CustomerPackage_Id;

                // Xử lý các mục hóa đơn (InvoiceItems)
                var existingItems = InvoiceItemRepo.Instance.GetItemsByInvoiceId(id);
                var processedItemIds = new HashSet<int>();

                // Xử lý các items từ model
                if (model.InvoiceItems != null && model.InvoiceItems.Count > 0)
                {
                    foreach (var item in model.InvoiceItems)
                    {
                        if (item.Id > 0) // Existing item
                        {
                            var existingItem = existingItems.FirstOrDefault(i => i.Id == item.Id);
                            if (existingItem != null)
                            {
                                // Cập nhật item hiện có
                                existingItem.ItemName = item.ItemName?.Trim();
                                existingItem.Quantity = item.Quantity;
                                existingItem.UnitPrice = item.Unit_Price;
                                existingItem.SubTotal = item.Quantity * item.Unit_Price;
                                existingItem.ServiceId = item.Service_Id;

                                InvoiceItemRepo.Instance.UpdateInvoiceItem(existingItem);
                                processedItemIds.Add(existingItem.Id);

                                System.Diagnostics.Debug.WriteLine($"Updated existing item: {existingItem.Id} - {existingItem.ItemName}");
                            }
                        }
                        else // New item (Id = 0 or negative)
                        {
                            // Tạo item mới
                            var newItem = new InvoiceItem
                            {
                                invoice_id = id,
                                item_name = item.ItemName?.Trim(),
                                quantity = item.Quantity,
                                unit_price = item.Unit_Price,
                                sub_total = item.Quantity * item.Unit_Price,
                                s_id = item.Service_Id,
                                barcode = item.BarCode
                            };

                            var newItemId = InvoiceItemRepo.Instance.AddItem(newItem);
                            System.Diagnostics.Debug.WriteLine($"Added new item: {newItemId} - {newItem.item_name}");
                        }
                    }
                }

                // Xóa các items không còn trong danh sách
                var itemsToDelete = existingItems.Where(i => !processedItemIds.Contains(i.Id)).ToList();
                foreach (var itemToDelete in itemsToDelete)
                {
                    InvoiceItemRepo.Instance.DeleteInvoiceItem(itemToDelete.Id);
                    System.Diagnostics.Debug.WriteLine($"Deleted item: {itemToDelete.Id} - {itemToDelete.ItemName}");
                }

                // Tính lại tổng tiền
                var updatedInvoiceItems = InvoiceItemRepo.Instance.GetItemsByInvoiceId(id);
                decimal itemsTotal = updatedInvoiceItems?.Sum(i => i.SubTotal) ?? 0;
                invoice.Total_Amount = itemsTotal + model.Ship_Cost;

                System.Diagnostics.Debug.WriteLine($"Items Total: {itemsTotal}, Ship Cost: {model.Ship_Cost}, Total Amount: {invoice.Total_Amount}");

                // Lưu cập nhật invoice
                bool updateResult = _invoiceRepository.Update(invoice);

                if (updateResult)
                {
                    TempData["Success"] = "Invoice updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update invoice. Please try again.";
                }

                return RedirectToAction("Edit", new { id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating invoice: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["Error"] = "An error occurred while updating the invoice. Please try again.";
                return RedirectToAction("Edit", new { id });
            }
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
        public ActionResult ConfirmPayment(int invoiceId)
        {
            try
            {
                if (invoiceId <= 0)
                    return Json(new { success = false, message = "Invalid invoice ID." });

                var invoice = _invoiceRepository.GetById(invoiceId);
                if (invoice == null)
                    return Json(new { success = false, message = "Invoice not found." });

                if (invoice.Order_Status != 1)
                    return Json(new { success = false, message = "Invoice is not in a pending state." });

                if (invoice.Payment_Type != 1 && invoice.Payment_Type != 3)
                    return Json(new { success = false, message = "Only QR code and Cash payments can be confirmed via this method." });
               
                var result = _invoiceRepository.UpdateOrderStatus(invoiceId, 2);
                if (result)
                    return Json(new { success = true });
                else
                    return Json(new { success = false, message = "Failed to update status." });
            }
            catch
            {
                return Json(new { success = false, message = "An error occurred during confirmation." });
            }
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



    }
}