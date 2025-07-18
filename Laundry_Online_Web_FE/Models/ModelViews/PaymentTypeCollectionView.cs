using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class PaymentTypeCollectionView
    {
        public string PaymentType { get; set; }
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }
    public class PaymentTypeTotalReportModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<PaymentTypeCollectionView> PaymentSummaries { get; set; }
        public string ErrorMessage { get; set; }
    }
}