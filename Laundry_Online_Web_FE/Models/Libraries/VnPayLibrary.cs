using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using Laundry_Online_Web_FE.Models.ModelViews.Payments;

namespace Laundry_Online_Web_FE.Models.Librabries
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public PaymentResponseModel GetFullResponseData(NameValueCollection collection, string hashSecret)
        {
            var vnPay = new VnPayLibrary();
            foreach (string key in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, collection[key]);
                }
            }

            var orderId = vnPay.GetResponseData("vnp_TxnRef");
            var vnPayTranId = vnPay.GetResponseData("vnp_TransactionNo");
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnpSecureHash = collection["vnp_SecureHash"];
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");

            var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);
            if (!checkSignature)
                return new PaymentResponseModel()
                {
                    Success = false
                };

            return new PaymentResponseModel()
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId,
                PaymentId = vnPayTranId,
                TransactionId = vnPayTranId,
                Token = vnpSecureHash,
                VnPayResponseCode = vnpResponseCode
            };
        }

        public string GetIpAddress(HttpContext context)
        {
            try
            {
                var ipAddress = context?.Request?.UserHostAddress;
                return string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress;
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var kv in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }

            var querystring = data.ToString();
            if (querystring.Length > 0)
            {
                querystring = querystring.Remove(querystring.Length - 1, 1); // remove last &
            }

            var signData = querystring;
            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            var fullUrl = baseUrl + "?" + querystring + "&vnp_SecureHash=" + vnpSecureHash;

            return fullUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rawData = GetResponseRawData();
            var myChecksum = HmacSha512(secretKey, rawData);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashBytes)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        private string GetResponseRawData()
        {
            var data = new StringBuilder();

            // Loại bỏ các thông tin không cần thiết trước khi hash
            _responseData.Remove("vnp_SecureHash");
            _responseData.Remove("vnp_SecureHashType");

            foreach (var kv in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1); // remove last '&'
            }

            return data.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var comparer = CompareInfo.GetCompareInfo("en-US");
            return comparer.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
