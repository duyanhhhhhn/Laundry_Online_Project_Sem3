using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class InvoiceItemView
    {
        public int Item_Id { get; set; } = 0;
        public int Invoice_Id { get; set; } = 0;
        public string Package_Name { get; set; } = "";
        public decimal Quantity { get; set; } = 0;
        public decimal Unit_Price { get; set; } = 0;
        public decimal Sub_Price { get; set; } = 0;
        public string barcode { get; set; } = "";
        public int Status { get; set; } = 0;
        public int Service_Id { get; set; } = 0;
    }
    
}