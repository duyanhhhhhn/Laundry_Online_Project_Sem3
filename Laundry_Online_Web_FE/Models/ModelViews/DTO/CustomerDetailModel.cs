using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Org.BouncyCastle.Bcpg;

namespace Laundry_Online_Web_FE.Models.ModelViews.DTO
{
    public class CustomerDetailModel
    {
        public CustomerView Customer { get; set; } = new CustomerView();

        public List<InvoiceView> Invoices { get; set; } = new List<InvoiceView>();

        public List<CustomerPackageDetailView> CustomerPackages { get; set; } = new List<CustomerPackageDetailView>();
    }

    public class CustomerPackageDetailView
    {
        public int Id { get; set; } = 0;
        public int Customer_Id { get; set; } = 0;
        public int Package_Id { get; set; } = 0;
        public string Package_Name { get; set; } = ""; // Lấy từ PackageView
        public DateTime Date_Start { get; set; } = DateTime.Now;
        public DateTime Date_End { get; set; } = DateTime.Now;
        public int Value { get; set; } = 0;
        public string Unite { get; set; } = ""; // Lấy từ PackageView
        public string Payment_Id { get; set; } = "";
        public string Image { get; set; } = ""; // Lấy từ PackageView
    }
}
