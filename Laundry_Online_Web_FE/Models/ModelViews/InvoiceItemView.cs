using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class InvoiceItemView
    {
        public int Id { get; set; } = 0;
        public int InvoiceId { get; set; } = 0;
        public string ItemName { get; set; } = "";
        public decimal Quantity { get; set; } = 0;
        public decimal UnitPrice { get; set; } = 0;
        public decimal SubTotal { get; set; } = 0;
        public string BarCode { get; set; } = "";
        public int ItemStatus { get; set; } = 0;
        public int ServiceId { get; set; } = 0;

    }
    
}