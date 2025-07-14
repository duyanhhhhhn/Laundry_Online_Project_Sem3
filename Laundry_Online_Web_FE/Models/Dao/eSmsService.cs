using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Laundry_Online_Web_FE.Models.Dao
{
    public class eSmsService
    {

        private readonly string _apiKey = ConfigurationManager.AppSettings["eSmsApiKey"];
        private readonly string _secretKey = ConfigurationManager.AppSettings["eSmsSecretKey"];
        private readonly HttpClient _client = new HttpClient();

        public async Task<string> SendAsync(string phone, string content)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                return "Lỗi: Vui lòng cấu hình ApiKey và SecretKey trong Web.config.";
            }

            var apiUrl = "https://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_post_json/";

            var requestData = new
            {
                ApiKey = _apiKey,
                SecretKey = _secretKey,
                Content = content,
                Phone = phone,
                SmsType = "2", 
                Brandname = "Baotrixemay", 
                IsUnicode = "0" 
            };

            string jsonPayload = JsonConvert.SerializeObject(requestData);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _client.PostAsync(apiUrl, httpContent);

                string responseString = await response.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObject.CodeResult == "100")
                {
                    return $"Thành công! ID tin nhắn: {responseObject.SMSID}";
                }
                else
                {
                    return $"Thất bại. Lỗi: {responseObject.ErrorMessage} (Code: {responseObject.CodeResult})";
                }
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống: Không thể kết nối tới eSMS API. " + ex.Message;
            }
        }
    }
}