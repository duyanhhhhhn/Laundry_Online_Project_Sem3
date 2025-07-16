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

namespace Laundry_Online_Web_FE.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly InvoiceRepository _invoiceRepository;
        private readonly CustomerPackageRepository _customerPackageRepository;
        private readonly PackageRepository _packageRepository;

        public CheckoutController()
        {
            _vnPayService = new VnPayPaymentService(); // Tạo instance thủ công

            _invoiceRepository = InvoiceRepository.Instance;
            _customerPackageRepository = CustomerPackageRepository.Instance;
            _packageRepository = PackageRepository.Instance;
        }

        // Nếu muốn vẫn giữ constructor nhận IVnPayService để test/unit test thì:
        public CheckoutController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;

            _invoiceRepository = InvoiceRepository.Instance;
            _customerPackageRepository = CustomerPackageRepository.Instance;
            _packageRepository = PackageRepository.Instance;
        }
        // Method để Invoice Controller gọi qua
        public ActionResult PayInvoice(int invoiceId)
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "Invoice");
            }

            // Kiểm tra trạng thái hóa đơn, status = 2 la da thanh toan
            if (invoice.Order_Status == 2)
            {
                TempData["ErrorMessage"] = "Hóa đơn này đã được thanh toán.";
                return RedirectToAction("Index", "Invoice", new { id = invoiceId });
            }
             
            return View(invoice);
        }

        // Method để Package
        public ActionResult PayPackage(int packageId)
        {
            var package = _packageRepository.GetById(packageId);
            if (package == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy gói khách hàng.";
                return RedirectToAction("Index", "CustomerPackage");
            }
            var customer = Session["customer"] as CustomerView;
         
            CustomerPackageView model = new CustomerPackageView();
            model.Package_Id = package.Id;
            model.Customer_Id = customer.Id;
            model.Value = package.Value;
            model.Payment_Id=string.Empty;
            model.Date_Start = DateTime.Now;
            model.Date_End = DateTime.Now.AddDays(30);      
            return View(model);
        }
        // Callback từ VNPay sau khi thanh toán
        [HttpGet]
        public ActionResult PaymentCallbackVnpay()
        {
            try
            {
                // Xử lý callback từ VNPay
                var response = _vnPayService.PaymentExecute(Request.QueryString);

                if (response.Success)
                {
                    // Kiểm tra xem đang thanh toán cho cái gì
                    var invoice = Session["invoice"] as InvoiceView;
                    var customerPackage = Session["customerPackage"] as CustomerPackageView;
                  
                    if (invoice != null)
                    {
                        bool success = false;
                        invoice.Order_Status = 2;
                        if (invoice.CustomerPackage_Id != null)
                        {
                           
                            var cp_id = invoice.CustomerPackage_Id;
                            var customerPackageId = _customerPackageRepository.GetById((int)cp_id);
                            int current_value = customerPackageId.Value - invoice.CustomerPackage_Id.Value;
                            if (current_value > 0)
                            {
                                customerPackageId.Value = current_value;
                                success = _customerPackageRepository.Update(customerPackageId);
                            }
                            else
                            {
                                customerPackageId.Value = 0;
                                success = _customerPackageRepository.Update(customerPackageId);
                                current_value= Math.Abs(current_value);
                            }
                        }

                        if (success)
                        {
                            Session.Remove("invoice");
                            TempData["SuccessMessage"] = "Thanh toán hóa đơn thành công!";
                            return RedirectToAction("PaymentSuccess");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Thanh toán thành công nhưng có lỗi khi cập nhật hóa đơn!";
                            return RedirectToAction("Detail", "Invoice", new { id = invoice.Id });
                        }
                    }
                    else if (customerPackage != null)
                    {
                        
                        var createCustomerPackage=_customerPackageRepository.Add(customerPackage);
                        if (createCustomerPackage)
                        {
                            Session.Remove("customerPackage");
                            TempData["SuccessMessage"] = "Thanh toán gói khách hàng thành công!";
                            return RedirectToAction("PaymentSuccess");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Thanh toán thành công nhưng có lỗi khi cập nhật gói khách hàng!";
                            return RedirectToAction("Detail", "CustomerPackage", new { id = customerPackage.Id });
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán trong session!";
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    // Thanh toán thất bại
                    TempData["ErrorMessage"] = "Thanh toán thất bại!";
                    return RedirectToAction("PaymentFailed");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán: " + ex.Message;
                return RedirectToAction("PaymentFailed");
            }
        }

        // Trang thành công
        public ActionResult PaymentSuccessAdmin()
        {
            return View();
        }
        public ActionResult PaymentSuccessClient()
        {
            return View();
        }

        // Trang thất bại
        public ActionResult PaymentFailedAdmin()
        {
            return View();
        }
        public ActionResult PaymentFailedClient()
        {
            return View();
        }
    }
}