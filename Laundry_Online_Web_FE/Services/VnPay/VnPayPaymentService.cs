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
            //var vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
            //if (string.IsNullOrEmpty(vnp_HashSecret))
            //{
            //    throw new Exception("Missing 'vnp_HashSecret' in Web.config.");
            //}

            //var vnpay = new VnPayLibrary();

            //foreach (string key in collections)
            //{
            //    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            //    {
            //        vnpay.AddResponseData(key, collections[key]);
            //    }
            //}

            //var vnp_SecureHash = collections["vnp_SecureHash"];
            //if (string.IsNullOrEmpty(vnp_SecureHash))
            //{
            //    throw new Exception("Missing 'vnp_SecureHash' from VNPay response.");
            //}

            //bool isValid = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

            //var response = new PaymentResponseModel
            //{
            //    OrderId = collections["vnp_TxnRef"],
            //    TransactionId = collections["vnp_TransactionNo"],
            //    PaymentId = collections["vnp_TransactionNo"],
            //    OrderDescription = collections["vnp_OrderInfo"],
            //    Token = vnp_SecureHash,
            //    VnPayResponseCode = collections["vnp_ResponseCode"],
            //    Success = isValid && collections["vnp_ResponseCode"] == "00",
            //    PaymentMethod = "VNPay"
            //};

            //if (decimal.TryParse(collections["vnp_Amount"], out var rawAmount))
            //{
            //    response.Amount = rawAmount / 100;
            //}

            //if (DateTime.TryParseExact(collections["vnp_PayDate"], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var payDate))
            //{
            //    response.PaymentTime = payDate;
            //}

            //return response;
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, ConfigurationManager.AppSettings["vnp_HashSecret"]);

            return response;
        }

    }
}
