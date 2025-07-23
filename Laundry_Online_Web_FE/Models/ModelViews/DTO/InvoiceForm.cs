using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web;


namespace Laundry_Online_Web_FE.Models.ModelViews.DTO
{
    public class InvoiceForm
    {
        public int Id { get; set; } = 0;
        public int Invoice_Id { get; set; } = 0;

        [Display(Name = "Customer")]
        public int Customer_Id { get; set; } = 0;

        [Display(Name = "Employee")]
        public int Employee_Id { get; set; } = 0;

        [Display(Name = "Delivery Date")]
        [DataType(DataType.Date)]
        public DateTime Delivery_Date { get; set; } = DateTime.Now.AddDays(1);

        [Display(Name = "Pickup Date")]
        [DataType(DataType.Date)]
        public DateTime Pickup_Date { get; set; } = DateTime.Now.AddDays(3);

        [Display(Name = "Payment Type")]
        public int Payment_Type { get; set; } = 1;

        [Display(Name = "Invoice Type")]
        public int Invoice_Type { get; set; } = 1;

        [Display(Name = "Customer Package")]
        public int? CustomerPackage_Id { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string Notes { get; set; } = "";

        [Display(Name = "Shipping Cost")]
        public decimal Ship_Cost { get; set; } = 0m;

        [Display(Name = "Item List")]
        public List<InvoiceItemForm> InvoiceItems { get; set; } = new List<InvoiceItemForm>();

        [Display(Name = "Total Amount")]
        public decimal Total_Amount => InvoiceItems.Sum(x => x.Sub_Total) + Ship_Cost;
        public decimal TotalAmountFromDb { get; set; }
        public decimal TotalAmountInvoice { get; set; }
        public string Customer_Name { get; set; } = "";
        public string Employee_Name { get; set; } = "";
        public DateTime Invoice_Date { get; set; } = DateTime.Now;
        public int Order_Status { get; set; } = 1;
        public int Delivery_Status { get; set; } = 1;
        public int Status { get; set; } = 1;
        public string Payment_Id { get; set; } = "";
        public string Service_name { get; set; } = "";
        // Helper methods for dropdown lists

        public static List<SelectListItem> GetPaymentTypes()
        {
            return new List<SelectListItem>
            {
               
                new SelectListItem { Value = "1", Text = "Cash" },
                new SelectListItem { Value = "2", Text = "VNPay" },
                 new SelectListItem { Value = "3", Text = "QR Code" },
            };
        }

        public static List<SelectListItem> GetInvoiceTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Kg" },
                new SelectListItem { Value = "1", Text = "Piece" },
                new SelectListItem { Value = "2", Text = "Pair" }
            };
        }

        public static List<SelectListItem> GetOrderStatuses()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Scheduled" },
                new SelectListItem { Value = "1", Text = "Pending" },
                new SelectListItem { Value = "2", Text = "Paid" },
                new SelectListItem { Value = "3", Text = "Cancelled" },
                new SelectListItem { Value = "5", Text = "Cancelled" }
            };
        }

        public static List<SelectListItem> GetDeliveryStatuses()
        {
            return new List<SelectListItem>
            {

                new SelectListItem { Value = "0", Text = "No Delivery" },
                new SelectListItem { Value = "1", Text = "Pending Delivery" },
                new SelectListItem { Value = "2", Text = "In Transit" },
                new SelectListItem { Value = "3", Text = "Delivered" },
                new SelectListItem { Value = "4", Text = "Failed Delivery" }
            };
        }

        public static List<SelectListItem> GetStatusOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Inactive" },
                new SelectListItem { Value = "1", Text = "Active" },
                new SelectListItem { Value = "2", Text = "Archived" }
            };
        }
    }
}

