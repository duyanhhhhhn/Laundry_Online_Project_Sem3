using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Laundry_Online_Web_FE.Models.ModelViews
{

    public class InvoiceView
    {
        public int Id { get; set; } = 0;
        public int Customer_Id { get; set; } = 0;
        public int? Employee_Id { get; set; } = 0;
        public DateTime Invoice_Date { get; set; } = DateTime.MinValue;
        public DateTime? Delivery_Date { get; set; } = DateTime.MinValue;
        public DateTime? Pickup_Date { get; set; } = DateTime.MinValue;
        public decimal Total_Amount { get; set; } = 0m;
        public int Payment_Type { get; set; } = 0;
        public string Payment_Id { get; set; } = "";
        public int Order_Status { get; set; } = 0; //2 la da thanh toan
        public int Invoice_Type { get; set; } = 0;
        public int? CustomerPackage_Id { get; set; } = null;
        public int Status { get; set; } = 0;
        public string Notes { get; set; } = "";
        public decimal Ship_Cost { get; set; } = 0m;
        public int Delivery_Status { get; set; } = 0;
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
    }

}