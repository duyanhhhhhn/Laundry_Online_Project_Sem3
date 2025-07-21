using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Laundry_Online_Web_FE.Models.ModelViews.DTO
{
    public class InvoiceItemForm
    {
        public int Id { get; set; } = 0;
        public string ItemName { get; set; } = "";
        public string ItemUnit { get; set; } = "";
        public int Invoice_Id { get; set; } = 0;
        public string BarCode { get; set; } = "";

        [Required(ErrorMessage = "Please select a service")]
        [Display(Name = "Service")]
        public int Service_Id { get; set; } = 0;

        [Required(ErrorMessage = "Quantity is required")]
        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Unit price is required")]
        [Display(Name = "Unit Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal Unit_Price { get; set; } = 0m;

        [Display(Name = "Sub Total")]
        public decimal Sub_Total => (Quantity * Unit_Price) + Service_Price;
          public decimal SubTotalItem { get; set; }
        [Display(Name = "Status")]
        public int Item_Status { get; set; } = 1;

        [Display(Name = "Notes")]
        [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
        public string Notes { get; set; } = "";

        // Display properties
        public string Service_Name { get; set; } = "";
        public string Service_Description { get; set; } = "";
        public decimal Service_Price { get; set; } = 0m;
        public ServiceView Services { get; set; }
        // Helper methods
        public static List<SelectListItem> GetItemStatuses()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Active" },
                new SelectListItem { Value = "0", Text = "Inactive" }
            };
        }
    }
}
