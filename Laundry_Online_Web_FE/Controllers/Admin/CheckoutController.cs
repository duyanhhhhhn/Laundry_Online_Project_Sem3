using System;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;
using Laundry_Online_Web_FE.Services;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Services.VnPay;
using Microsoft.Ajax.Utilities;
using Laundry_Online_Web_FE.Models.Librabries;
using Laundry_Online_Web_FE.Models.ModelViews.DTO;
using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews.Payments;
using System.Linq;
using Laundry_Online_Web_FE.Models.ModelViews.DTO.Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_FE.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly InvoiceRepository _invoiceRepository;
        private readonly CustomerPackageRepository _customerPackageRepository;
        private readonly PackageRepository _packageRepository;
        private readonly InvoiceItemRepo _invoiceItemRepository;
        private readonly ServiceRepository _serviceRepository;

        // IMPROVEMENT: Use dependency injection instead of manual instantiation
        public CheckoutController()
        {
            _vnPayService = new VnPayPaymentService();
            _invoiceRepository = InvoiceRepository.Instance;
            _customerPackageRepository = CustomerPackageRepository.Instance;
            _packageRepository = PackageRepository.Instance;
            _invoiceItemRepository = InvoiceItemRepo.Instance;
            _serviceRepository = ServiceRepository.Instance;
        }

        // Constructor for dependency injection (better for testing)
        public CheckoutController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
            _invoiceRepository = InvoiceRepository.Instance;
            _customerPackageRepository = CustomerPackageRepository.Instance;
            _packageRepository = PackageRepository.Instance;
        }

        // Method for Invoice Controller to call

        public ActionResult PayInvoice(int invoiceId)
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {

                TempData["ErrorMessage"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "Invoice");
            }

            var employee = Session["employee"] as EmployeeView;
            if (employee == null)
            {
                Session["employee"] = new EmployeeView
                {
                    Id = 1,
                    FirstName = "Test",
                    LastName = "Employee"
                };
                Session["employee"] = employee;
                //TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục.";
                //return RedirectToAction("Login", "Home");
            }

            if (invoice.Order_Status == 2)
            {
                TempData["ErrorMessage"] = "Hóa đơn này đã được thanh toán.";
                return RedirectToAction("Index", "Invoice", new { id = invoiceId });
            }

            var invoiceItems = _invoiceItemRepository
                .GetItemsByInvoiceId(invoiceId)
                .Select(item =>
                {
                    var service = _serviceRepository.GetById(item.ServiceId);
                    return new InvoiceItemForm
                    {
                        Id = item.Id,
                        ItemName = item.ItemName,
                        Invoice_Id = item.InvoiceId,
                        Quantity = (int)item.Quantity,
                        Unit_Price = item.UnitPrice,
                        Item_Status = item.ItemStatus,
                        Service_Id = item.ServiceId,
                        Service_Name = service.Title ?? "",
                        Service_Description = service?.Description ?? "",
                        Service_Price = service?.Price ?? 0,
                        ItemUnit = service?.Unit ?? ""
                    };
                }).ToList();

            var model= new InvoiceForm
            {
                Id = invoice.Id,
                Customer_Name = invoice.CustomerName,
                Customer_Id = invoice.Customer_Id,
               // Employee_Name = employee.LastName + " " + employee.FirstName,
                Employee_Id = (int)invoice.Employee_Id,
                Delivery_Date = (DateTime)invoice.Delivery_Date,
                Pickup_Date = (DateTime)invoice.Pickup_Date,
                InvoiceItems = invoiceItems, // This is where Total_Amount is calculated via InvoiceForm
                Invoice_Type = invoice.Invoice_Type,
                Payment_Type = invoice.Payment_Type,
                CustomerPackage_Id = invoice.CustomerPackage_Id,
                Ship_Cost = invoice.Ship_Cost,
                Notes = invoice.Notes,
                Order_Status = invoice.Order_Status
            };

            Session["invoice"] = model;

            return View(model);
        }



        // Method for Package payment
        public ActionResult PayPackage(int packageId)
        {
            var package = _packageRepository.GetById(packageId);
            if (package == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy gói khách hàng.";
                return RedirectToAction("Index", "CustomerPackage");
            }

            var customer = Session["customer"] as CustomerView;
            // IMPROVEMENT: Add null check for customer
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Home");
            }

            var model = new PayPackageInfor
            {
                Id = package.Id,
                Package_Name = package.Package_Name,
                Image = package.Image,
                Description = package.Description,
                Price = package.Price,
                Value = package.Value,
                Unit = package.Unit,
                Validity_Day = 30,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),

                Customer_Name = customer.FirstName + " " + customer.LastName,
                Customer_Phone = customer.PhoneNumber
            };
            // IMPROVEMENT: Store customer package in session for payment callback
            Session["customerPackage"] = model;

            return View(model);
        }

        // Callback from VNPay after payment
        [HttpGet]
        public ActionResult PaymentCallbackVnpay()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(Request.QueryString);

                if (response.Success)
                {
                    var invoice = Session["invoice"] as InvoiceForm;
                    var package = Session["customerPackage"] as PayPackageInfor; // ✅ Đúng tên session

                    if (invoice != null)
                    {
                        return ProcessInvoicePayment(invoice,response);
                    }
                    else if (package != null)
                    {
                        return ProcessPackagePayment(package, response); // ✅ Truyền thêm response để lấy transactionId
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán trong session!";
                        return RedirectToAction("PaymentFailedAdmin");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"Thanh toán thất bại: {response.VnPayResponseCode}";
                    return RedirectToAction("PaymentFailedAdmin");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán: " + ex.Message;
                return RedirectToAction("PaymentFailedAdmin");
            }
        }


        // IMPROVEMENT: Extract invoice payment logic to separate method
        private ActionResult ProcessInvoicePayment(InvoiceForm invoice, PaymentResponseModel response)
        {
            var employee = Session["employee"] as EmployeeView;
            bool success = false;

            //invoice.Employee_Id = employee.Id;
            if (invoice.CustomerPackage_Id != null)
            {
                var cp_id = invoice.CustomerPackage_Id;
                var customerPackage = _customerPackageRepository.GetById((int)cp_id);

                if (customerPackage != null)
                {
                    // Get the package information to know the unit
                    var package = _packageRepository.GetById(customerPackage.Package_Id);

                    if (package != null)
                    {
                        // Calculate total quantity for services with matching unit
                        int totalQuantityForUnit = CalculateQuantityForMatchingUnit(invoice.Id, package.Unit);

                        // Calculate remaining value after deducting
                        int remainingValue = customerPackage.Value - totalQuantityForUnit;

                        if (remainingValue >= 0)
                        {
                            // Sufficient balance in package
                            customerPackage.Value = remainingValue;
                            success = _customerPackageRepository.Update(customerPackage);
                        }
                        else
                        {
                            // Insufficient balance - set to 0 and handle the deficit
                            customerPackage.Value = 0;
                            success = _customerPackageRepository.Update(customerPackage);

                            // IMPROVEMENT: You might want to handle the deficit (negative balance)
                            // For example, charge the remaining amount separately or notify the customer
                            int deficit = Math.Abs(remainingValue);
                            // Log or handle the deficit as needed
                            // TempData["WarningMessage"] = $"Gói không đủ, còn thiếu {deficit} {package.Unit}";
                        }
                    }
                    else
                    {
                        // Package not found, still try to update invoice
                        TempData["WarningMessage"] = "Không tìm thấy thông tin gói khách hàng.";
                    }
                }
                else
                {
                    // Customer package not found
                    TempData["WarningMessage"] = "Không tìm thấy gói khách hàng của bạn.";
                }
            }


            // IMPROVEMENT: Update invoice status regardless of package update
            var invoiceUpdateSuccess = _invoiceRepository.ConfirmInvoicePayment(invoice.Id, response.TransactionId);

            if (invoiceUpdateSuccess)
            {
                Session.Remove("invoice");
                TempData["SuccessMessage"] = "Thanh toán hóa đơn thành công!";
                return RedirectToAction("PaymentSuccess");
            }
            else
            {
                TempData["ErrorMessage"] = "Thanh toán thành công nhưng có lỗi khi cập nhật hóa đơn!";
                return RedirectToAction("Index", "Invoice", new { id = invoice.Id });
            }
        }

        // Helper method to calculate total quantity for services with matching unit
        private int CalculateQuantityForMatchingUnit(int invoiceId, string packageUnit)
        {
            try
            {
                // Get all invoice items for this invoice
                var invoiceItems = _invoiceItemRepository.GetInvoiceItemsByInvoiceId(invoiceId);

                if (invoiceItems != null)
                {
                    int totalQuantity = 0;

                    foreach (var item in invoiceItems)
                    {
                        // Get service information for this invoice item
                        var service = _serviceRepository.GetById(item.ServiceId);

                        if (service != null && service.Unit == packageUnit)
                        {
                            // Add quantity of items with matching unit
                            totalQuantity += (int)item.Quantity;
                        }
                    }

                    return totalQuantity;
                }

                return 0;
            }
            catch (Exception ex)
            {
                // Log error and return 0 as fallback
                // Logger.LogError(ex, "Error calculating matching unit quantity for invoice {InvoiceId}, unit {Unit}", invoiceId, packageUnit);
                return 0;
            }
        }
        // IMPROVEMENT: Extract customer package payment logic to separate method
        private ActionResult ProcessPackagePayment(PayPackageInfor payModel, PaymentResponseModel response)
        {
            var customer = Session["customer"] as CustomerView;

            if (payModel == null || customer == null)
            {
                TempData["ErrorMessage"] = "Thiếu thông tin khách hàng hoặc gói dịch vụ để xử lý.";
                return RedirectToAction("Index", "CustomerPackage");
            }

            var customerPackage = new CustomerPackageView
            {
                Customer_Id = customer.Id,
                Package_Id = payModel.Id,
                Value = payModel.Value,
                Date_Start = payModel.StartDate,
                Date_End = payModel.EndDate,
                Payment_Id = response.TransactionId // ✅ Lấy từ response
            };

            var created = _customerPackageRepository.Add(customerPackage);

            if (created)
            {
                Session.Remove("customerPackage");
                TempData["SuccessMessage"] = "Thanh toán gói dịch vụ thành công!";
                return RedirectToAction("PaymentSuccess");
            }
            else
            {
                TempData["ErrorMessage"] = "Thanh toán thành công nhưng lỗi khi lưu gói dịch vụ.";
                return RedirectToAction("Index", "CustomerPackage");
            }
        }



        // IMPROVEMENT: Consolidate success/failure pages or make them more specific
        public ActionResult PaymentSuccess()
        {
            return View();
        }

        public ActionResult PaymentFailed()
        {
            return View();
        }

    }
}