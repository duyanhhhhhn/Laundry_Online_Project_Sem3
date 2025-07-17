using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class PaymentInformation
    {
        public string OrderType { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
    }
}