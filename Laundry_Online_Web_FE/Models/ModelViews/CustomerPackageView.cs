using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class CustomerPackageView
    {
        public int Id { get; set; } = 0;
        public int Customer_Id { get; set; } = 0;
        public int Package_Id { get; set; } = 0;
        public DateTime Date_Start { get; set; } = DateTime.Now;
        public DateTime Date_End { get; set; } = DateTime.Now;
        public int Value { get; set; } = 0;
        public string Payment_Id { get; set; } = "";
    }
}
