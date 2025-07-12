using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews.DTO
{
    public class CustomerPackageForm
    {
        public int Id { get; set; }
        public string Customer_Name { get; set; }
        public string Package_Name { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public int Value { get; set; }
        public string Payment_Id { get; set; }
        public int Customer_Id { get; set; }
        public int Package_Id { get; set; }
    }
}