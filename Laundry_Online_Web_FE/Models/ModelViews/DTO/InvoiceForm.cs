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

        [Required(ErrorMessage = "Please select a customer")]
        [Display(Name = "Customer")]
        public int Customer_Id { get; set; } = 0;

        [Display(Name = "Employee")]
        public int? Employee_Id { get; set; } = null; // Cho phép null nếu không có nhân viên

        [Required(ErrorMessage = "Please select delivery date")]
        [Display(Name = "Delivery Date")]
        [DataType(DataType.Date)]
        public DateTime Delivery_Date { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Please select pickup date")]
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
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; } = "";

        [Display(Name = "Shipping Cost")]
        [Range(0, double.MaxValue, ErrorMessage = "Shipping cost must be >= 0")]
        public decimal Ship_Cost { get; set; } = 0m;

        [Display(Name = "Service List")]
        public List<InvoiceItemView> InvoiceItems { get; set; } = new List<InvoiceItemView>();

        [Display(Name = "Total Amount")]
        public decimal Total_Amount => InvoiceItems.Sum(x => x.SubTotal) + Ship_Cost;

        public string Customer_Name { get; set; } = "";
        public string Employee_Name { get; set; } = "";
        public DateTime Invoice_Date { get; set; } = DateTime.Now;

        public int Order_Status { get; set; } = 1;

        // ✅ Default là 0: "No Delivery option"
        public int Delivery_Status { get; set; } = 0;

        public int Status { get; set; } = 1;
        public string Payment_Id { get; set; } = "";

        // Dropdowns
        public static List<SelectListItem> GetPaymentTypes()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Cash" },
            new SelectListItem { Value = "2", Text = "VNPay" }
        };
        }

        public static List<SelectListItem> GetInvoiceTypes()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Regular Invoice" },
            new SelectListItem { Value = "2", Text = "VIP Invoice" }
        };
        }

        public static List<SelectListItem> GetOrderStatuses()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Pending" },
            new SelectListItem { Value = "2", Text = "Paid" },
            new SelectListItem { Value = "3", Text = "Processing" },
            new SelectListItem { Value = "4", Text = "Completed" },
            new SelectListItem { Value = "5", Text = "Cancelled" }
        };
        }

        // ✅ Sửa để có option "No Delivery option"
        public static List<SelectListItem> GetDeliveryStatuses()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "No Delivery option" },
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
