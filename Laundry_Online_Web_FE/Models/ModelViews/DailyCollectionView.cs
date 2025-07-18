using Laundry_Online_Web_FE.Models.ModelViews;
using System;
using System.Collections.Generic;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class DailyCollectionItemModel
    {
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime PickupDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentType { get; set; }
        public string Notes { get; set; }
    }

    public class DailyCollectionSummaryModel
    {
        public DateTime ReportDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public List<PaymentTypeCollectionView> PaymentTypeBreakdown { get; set; }
    }


    public class DailyCollectionReportModel
    {
        public DailyCollectionSummaryModel Summary { get; set; }
        public List<DailyCollectionItemModel> Details { get; set; }
        public string ErrorMessage { get; set; }
    }
    

}