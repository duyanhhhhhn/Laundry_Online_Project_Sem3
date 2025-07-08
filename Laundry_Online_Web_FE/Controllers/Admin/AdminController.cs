using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;

namespace Laundry_Online_Web_FE.Controllers.Admin
{
    //[Authorize(Roles = "Admin,Employee")]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Admin_create_customer()
        {
            return View();
        }
        public ActionResult Admin_create_employee()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateEmployee()
        {
            string firstName = Request.Form["FirstName"];
            string lastName = Request.Form["LastName"];
            string phone = Request.Form["PhoneNumber"];
            string password = Request.Form["Password"];
            string salary = Request.Form["Salary"];
            int role = Request.Form["Role"] == "1" ? 1 : 0;
            var newemp = new EmployeeView
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Password = password,
                HireDate = DateTime.Now,
                Salary = salary != null ? int.Parse(salary) : 0,
                Role = role,
                Active = 1
            };
            EmployeeRepo.Instance.Create(newemp);
            return RedirectToAction("EmployeeList");
        }
        [HttpPost]
        public ActionResult CreateCustomer()
        {
            string firstName = Request.Form["FirstName"];
            string lastName = Request.Form["LastName"];
            string phone = Request.Form["PhoneNumber"];
            string address = Request.Form["Address"];
            string password = Request.Form["Password"];
            var newCustomer = new CustomerView
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phone,
                Address = address,
                Password = password,
                RegistrationDate = DateTime.Now,
                Active = 1
            };
            CustomerRepo.Instance.Create(newCustomer);
            return RedirectToAction("CustomerList");
        }
        public ActionResult EmployeeList()
        {
            HashSet<EmployeeView> listEmp = new HashSet<EmployeeView>();
            var empList = EmployeeRepo.Instance.GetActiveStaffs();
            if (empList != null && empList.Count > 0)
            {
                listEmp = empList;
            }
            ViewBag.Data = listEmp;
            return View();
        }
        public ActionResult CustomerList()
        {
            HashSet<CustomerView> listCus = new HashSet<CustomerView>();
            var cusList = CustomerRepo.Instance.GetActiveCustomer();
            if (cusList != null && cusList.Count > 0)
            {
                listCus = cusList;
            }
            ViewBag.Data = listCus;
            return View();
        }
    }
}