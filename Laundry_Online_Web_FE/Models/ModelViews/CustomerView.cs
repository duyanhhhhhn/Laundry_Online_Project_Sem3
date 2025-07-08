using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class CustomerView
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public string Password { get; set; } = "";
        public int Active { get; set; } = 1;

    }
}