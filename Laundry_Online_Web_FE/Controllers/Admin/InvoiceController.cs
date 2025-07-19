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
using Laundry_Online_Web_FE.Models.ModelViews.DTO.Laundry_Online_Web_FE.Models.ModelViews;

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
                        Payment_Id = invoice.Payment_Id
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
    
        public ActionResult EditInvoice(int id, InvoiceForm form)
        {
            try
            {
                var existingInvoice = _invoiceRepository.GetById(id);
                if (existingInvoice == null)
                    return Json(new { success = false, message = "Invoice not found." });

                // Gán lại dữ liệu từ form vào InvoiceView để lưu
                var updatedInvoice = new InvoiceView
                {
                    Id = id,
                    Customer_Id = form.Customer_Id,
                    Employee_Id = form.Employee_Id,
                    Invoice_Date = existingInvoice.Invoice_Date,
                    Delivery_Date = form.Delivery_Date,
                    Pickup_Date = form.Pickup_Date,
                    Total_Amount = form.Total_Amount,
                    Payment_Type = form.Payment_Type,
                    Invoice_Type = form.Invoice_Type,
                    CustomerPackage_Id = form.CustomerPackage_Id ?? 0,
                    Notes = form.Notes ?? "",
                    Ship_Cost = form.Ship_Cost,
                    Order_Status = form.Order_Status,
                    Delivery_Status = form.Delivery_Status,
                    Status = form.Status,
                    Payment_Id = existingInvoice.Payment_Id
                };

                // Cập nhật hóa đơn
                bool result = _invoiceRepository.Update(updatedInvoice);

                if (result)
                {
                    // Cập nhật chi tiết hóa đơn
                    if (_invoiceItemRepository != null && form.InvoiceItems != null)
                    {
                        SaveInvoiceItems(id, form.InvoiceItems); // truyền list object thay vì JSON
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
                    ItemName=item.ItemName,
                    ServiceId = item.Service_Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Unit_Price,
                    SubTotal = item.Sub_Total,  
                    ItemStatus = item.Item_Status
                };

                _invoiceItemRepository.AddInvoiceItem(newItem);
            }
        }

    }
}