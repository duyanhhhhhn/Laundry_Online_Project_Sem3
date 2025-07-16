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
        private readonly InvoiceItemRepo _invoiceItemRepository; // Assuming you have this
        private readonly ServiceRepository _serviceRepository; // Assuming you have this
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
        public ActionResult Index(int? status)
        {
            var invoiceList = _invoiceRepository.GetAll();
            var customerList = _customerRepository.GetAll();
            var employeeList = _employeeRepository.All();

            // Filter by status if provided
            if (status.HasValue)
            {
                invoiceList = invoiceList.Where(i => i.Status == status.Value).ToHashSet();
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
                        //Total_
                        Payment_Type = invoice.Payment_Type,
                        Order_Status = invoice.Order_Status,
                        Invoice_Type = invoice.Invoice_Type,
                        CustomerPackage_Id = invoice.CustomerPackage_Id,
                        Status = invoice.Status,
                        Notes = invoice.Notes,
                        Ship_Cost = invoice.Ship_Cost,
                        Delivery_Status = invoice.Delivery_Status,
                        Payment_Id = invoice.Payment_Id
                    };

                    invoiceViewList.Add(viewModel);
                }
            }

            ViewBag.Data = invoiceViewList;
            ViewBag.CurrentStatus = status;

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

            var customer = _customerRepository.GetCustomerById(invoice.Customer_Id);
            var employee = _employeeRepository.GetEmployeeById((int)invoice.Employee_Id);

            var invoiceDetail = new InvoiceForm
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
                Payment_Id = invoice.Payment_Id
            };

            return View(invoiceDetail);
        }
        [HttpGet]
        [Route("Create")]
        // GET: Invoice/Create
        public ActionResult Create()
        {
            var customerList = _customerRepository.GetActiveCustomer();
            var employeeList = _employeeRepository.All();
            var customerPackageList = _customerPackageRepository.GetAll();

            ViewBag.CustomerList = customerList;
            ViewBag.EmployeeList = employeeList;
            ViewBag.CustomerPackageList = customerPackageList;

            return View();
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

        // GET: Invoice/Edit/5
        public ActionResult Edit(int id)
        {
            var invoice = _invoiceRepository.GetById(id);
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction("Index");
            }

            var customerList = _customerRepository.GetActiveCustomer();
            var employeeList = _employeeRepository.All();

            ViewBag.CustomerList = customerList;
            ViewBag.EmployeeList = employeeList;

            // Map to InvoiceForm for editing
            var invoiceForm = new InvoiceForm
            {
                Id = invoice.Id,
                Customer_Id = invoice.Customer_Id,
                Employee_Id = (int)invoice.Employee_Id,
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
                Payment_Id = invoice.Payment_Id
            };

            return View(invoiceForm);
        }

        // POST: Invoice/Edit/5
        [HttpPost]
        [Route("Edit/{id:int}")]
        public ActionResult EditInvoice(int id)
        {
            try
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
                string orderStatus = Request.Form["Order_Status"];
                string deliveryStatus = Request.Form["Delivery_Status"];
                string status = Request.Form["Status"];
                string invoiceItemsJson = Request.Form["InvoiceItems"];

                var existingInvoice = _invoiceRepository.GetById(id);
                if (existingInvoice == null)
                {
                    return Json(new { success = false, message = "Invoice not found." });
                }

                // Calculate total amount from invoice items
                decimal totalAmount = 0;
                if (!string.IsNullOrEmpty(invoiceItemsJson))
                {
                    try
                    {
                        var invoiceItems = JsonConvert.DeserializeObject<List<dynamic>>(invoiceItemsJson);
                        totalAmount = invoiceItems.Sum(item => (decimal)item.subTotal);
                    }
                    catch (Exception ex)
                    {
                        // Handle JSON parsing error
                        System.Diagnostics.Debug.WriteLine("JSON parsing error: " + ex.Message);
                    }
                }

                // Add shipping cost to total
                decimal shippingCost = string.IsNullOrEmpty(shipCost) ? 0 : Convert.ToDecimal(shipCost);
                totalAmount += shippingCost;

                var updatedInvoice = new InvoiceView
                {
                    Id = id,
                    Customer_Id = string.IsNullOrEmpty(customerId) ? 0 : Convert.ToInt32(customerId),
                    Employee_Id = string.IsNullOrEmpty(employeeId) ? 0 : Convert.ToInt32(employeeId),
                    Invoice_Date = existingInvoice.Invoice_Date, // Keep original invoice date
                    Delivery_Date = string.IsNullOrEmpty(deliveryDate) ? DateTime.Now.AddDays(1) : Convert.ToDateTime(deliveryDate),
                    Pickup_Date = string.IsNullOrEmpty(pickupDate) ? DateTime.Now.AddDays(3) : Convert.ToDateTime(pickupDate),
                    Total_Amount = totalAmount,
                    Payment_Type = string.IsNullOrEmpty(paymentType) ? 1 : Convert.ToInt32(paymentType),
                    Invoice_Type = string.IsNullOrEmpty(invoiceType) ? 1 : Convert.ToInt32(invoiceType),
                    CustomerPackage_Id = string.IsNullOrEmpty(customerPackageId) ? 0 : Convert.ToInt32(customerPackageId),
                    Notes = notes ?? "",
                    Ship_Cost = shippingCost,
                    Order_Status = string.IsNullOrEmpty(orderStatus) ? 1 : Convert.ToInt32(orderStatus),
                    Delivery_Status = string.IsNullOrEmpty(deliveryStatus) ? 1 : Convert.ToInt32(deliveryStatus),
                    Status = string.IsNullOrEmpty(status) ? 1 : Convert.ToInt32(status),
                    Payment_Id = existingInvoice.Payment_Id // Keep existing payment ID
                };

                bool result = _invoiceRepository.Update(updatedInvoice);

                if (result)
                {
                    //Handle invoice items if repository exists
                    // This is where you would save invoice items to database
                     if (_invoiceItemRepository != null && !string.IsNullOrEmpty(invoiceItemsJson))
                    {
                        SaveInvoiceItems(id, invoiceItemsJson);
                    }

                    return Json(new { success = true, message = "Invoice updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update invoice!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

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

        // GET: Invoice/ProcessPayment
        //public ActionResult ProcessPayment(int id)
        //{
        //    var invoice = _invoiceRepository.GetById(id);
        //    if (invoice == null)
        //    {
        //        TempData["ErrorMessage"] = "Invoice not found.";
        //        return RedirectToAction("Index");
        //    }

        //    if (!string.IsNullOrEmpty(invoice.Payment_Id) && invoice.Order_Status == 2)
        //    {
        //        TempData["InfoMessage"] = "This invoice has already been paid!";
        //        return RedirectToAction("Edit", new { id });
        //    }

        //    try
        //    {
        //        var invoiceView = new InvoiceView
        //        {
        //            Id = invoice.Id,
        //            Total_Amount = invoice.Total_Amount
        //        };

        //        string ipAddress = GetIpAddress();
        //        string paymentUrl = _paymentService.CreatePaymentUrl(invoiceView, ipAddress);
        //        return Redirect(paymentUrl);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error processing payment: " + ex.Message;
        //        return RedirectToAction("Edit", new { id });
        //    }
        //}

        //public ActionResult PaymentReturn()
        //{
        //    var (isSuccess, message, updatedInvoice) = _paymentService.HandleReturn(Request.QueryString);

        //    if (isSuccess && updatedInvoice != null)
        //    {
        //        var invoice = _invoiceRepository.GetById(updatedInvoice.Id);
        //        if (invoice != null)
        //        {
        //            invoice.Payment_Id = updatedInvoice.Payment_Id;
        //            invoice.Payment_Type = updatedInvoice.Payment_Type;
        //            invoice.Order_Status = updatedInvoice.Order_Status;

        //            if (_invoiceRepository.Update(invoice))
        //            {
        //                TempData["SuccessMessage"] = message;
        //                return RedirectToAction("Edit", new { id = invoice.Id });
        //            }
        //            else
        //            {
        //                TempData["ErrorMessage"] = "Payment successful, but invoice update failed.";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        TempData["ErrorMessage"] = message;
        //    }

        //    return RedirectToAction("Index");
        //}

        // AJAX: Get Customer Packages
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

        // Helper method to get IP address
        private string GetIpAddress()
        {
            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return Request.ServerVariables["REMOTE_ADDR"];
        }

        // Helper method to save invoice items (implement based on your InvoiceItemRepository)
        private void SaveInvoiceItems(int invoiceId, string invoiceItemsJson)
        {
            try
            {
                // Parse JSON and save to database
                var invoiceItems = JsonConvert.DeserializeObject<List<dynamic>>(invoiceItemsJson);

                // Delete existing items first
                _invoiceItemRepository.DeleteInvoiceItem(invoiceId);

                // Add new items
                foreach (var item in invoiceItems)
                {
                    var invoiceItem = new InvoiceItemView
                    {
                        InvoiceId = invoiceId,
                        ServiceId = (int)item.serviceId,
                        Quantity = (int)item.quantity,
                        UnitPrice = (decimal)item.unitPrice,
                        ItemStatus = 1
                    };

                    _invoiceItemRepository.AddInvoiceItem(invoiceItem);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving invoice items: " + ex.Message);
            }
        }
    }
}