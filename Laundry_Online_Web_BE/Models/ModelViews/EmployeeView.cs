using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_BE.Models.ModelViews
{
    public class EmployeeView
    {
        public int Id { get; set; } = 0;
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public int Role { get; set; } = 0;
        public DateTime HireDate { get; set; } = DateTime.Now;
        public int Salary { get; set; } = 0;
        public string Password { get; set; } = "";
        public int Active { get; set; } = 1;
    }
}