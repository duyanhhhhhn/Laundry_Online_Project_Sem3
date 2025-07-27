
using Laundry_Online_Web_FE.Services.VnPay;

namespace Laundry_Online_Web_FE.Controllers.Admin
{
    using System.Web.Mvc;
    using Laundry_Online_Web_FE.Models.ModelViews;



    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;

        // Constructor mặc định để tránh lỗi "No parameterless constructor"
        public PaymentController()
        {
            _vnPayService = new VnPayPaymentService(); // Tạo instance thủ công
        }

        // Nếu bạn muốn dùng cho test/unit test, vẫn có thể giữ constructor này
        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        public ActionResult CreatePaymentUrlVnpay(PaymentInformation model, InvoiceView ctr)
        {
            var url = _vnPayService.CreatePaymentUrl(model, System.Web.HttpContext.Current);

            return Redirect(url);
        }
        public ActionResult CreatePaymentUrlVnpayForPackage(PaymentInformation model,CustomerPackageView  package)
        {
            var url = _vnPayService.CreatePaymentUrl(model, System.Web.HttpContext.Current);

            return Redirect(url);
        }

    }

}