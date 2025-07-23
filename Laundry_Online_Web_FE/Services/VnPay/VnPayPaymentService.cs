using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.ModelViews.Payments;
using Laundry_Online_Web_FE.Models.Librabries;

namespace Laundry_Online_Web_FE.Services.VnPay
{
    public class VnPayPaymentService : IVnPayService
    {
        public string CreatePaymentUrl(PaymentInformation model, HttpContext context)
        {
            var vnpay = new VnPayLibrary();
            var vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            var vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
            var vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
            var vnp_ReturnUrl = ConfigurationManager.AppSettings["vnp_ReturnUrl"];
            var tick = DateTime.Now.Ticks.ToString();

            var pay = "pay";
            var timeZoneId = ConfigurationManager.AppSettings["TimeZoneId"] ?? "SE Asia Standard Time";
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var amount = ((long)(model.Amount * 100)).ToString();
            var ipAddress = vnpay.GetIpAddress(context);


            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", pay);
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", amount);
            vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            vnpay.AddRequestData("vnp_OrderType", model.OrderType);
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(NameValueCollection collections)
        {
            
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, ConfigurationManager.AppSettings["vnp_HashSecret"]);

            return response;
        }

    }
}
