using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class CustomerView
    {
        public int Id { get; set; } = 0;
        public string Phone { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "No Email";
        public string Address { get; set; } = "No Address";
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public int Active { get; set; } = 1;// 1: active, 0: inactive
    }
}