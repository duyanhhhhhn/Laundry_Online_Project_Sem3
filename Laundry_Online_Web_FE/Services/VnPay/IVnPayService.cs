using System.Collections.Specialized;
using System.Web;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.ModelViews.Payments;
namespace Laundry_Online_Web_FE.Services.VnPay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformation model, HttpContext context);
        PaymentResponseModel PaymentExecute(NameValueCollection collections);
    }
}
