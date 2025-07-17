using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews.DTO
{
    public class PayPackageInfor
    {
        public int Id { get; set; } = 0;
        public string Package_Name { get; set; } = "";
        public string Image { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; } = 0;
        public int Value { get; set; } = 0;
        public string Unit { get; set; } = "";
        public int Validity_Day { get; set; } = 30;
        public DateTime StartDate { get; set; }= DateTime.Now;
        public DateTime EndDate { get; set; }= DateTime.Now.AddDays(30);

        // Customer Information for Payment
        public string Customer_Name { get; set; } = "";
        public string Customer_Phone { get; set; } = "";
    }
}