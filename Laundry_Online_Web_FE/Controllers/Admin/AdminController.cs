using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;

namespace Laundry_Online_Web_FE.Controllers.Admin
{
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
        public ActionResult AllEmployeeList()
        {
            HashSet<EmployeeView> listEmp = new HashSet<EmployeeView>();
            var empList = EmployeeRepo.Instance.GetAllEmployees();
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
        public ActionResult AllCustomerList()
        {
            HashSet<CustomerView> listCus = new HashSet<CustomerView>();
            var cusList = CustomerRepo.Instance.GetAll();
            if (cusList != null && cusList.Count > 0)
            {
                listCus = cusList;
            }
            ViewBag.Data = listCus;
            return View();
        }
        public ActionResult CustomerInActive()
        {
            HashSet<CustomerView> listCus = new HashSet<CustomerView>();
            var cusList = CustomerRepo.Instance.GetInactiveCustomer();
            if (cusList != null && cusList.Count > 0)
            {
                listCus = cusList;
            }
            ViewBag.Data = listCus;
            return View();
        }
        [HttpGet]
        public ActionResult Admin_edit_customer()
        {
            int id = 0;
            if (RouteData.Values["id"] != null)
            {
                int.TryParse(RouteData.Values["id"].ToString(), out id);
            }
            else
            {
                return RedirectToAction("CustomerList");
            }
            var customer = CustomerRepo.Instance.GetCustomerById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.Customer = customer;
            return View();
        }
        [HttpPost]
        public ActionResult EditCustomer()
        {
            int id = int.Parse(Request.Form["CustomerId"]);
            string firstName = Request.Form["FirstName"];
            string lastName = Request.Form["LastName"];
            string phone = Request.Form["PhoneNumber"];
            string address = Request.Form["Address"];
            DateTime registrationDate = DateTime.Parse(Request.Form["RegistrationDate"]);
            var isCus = CustomerRepo.Instance.GetCustomerById(id);
            isCus.FirstName = firstName;
            isCus.LastName = lastName;
            isCus.PhoneNumber = phone;
            isCus.Address = address;
            isCus.RegistrationDate = registrationDate;
            CustomerRepo.Instance.Update(isCus);
            return RedirectToAction("CustomerList");
        }
        [HttpPost]
        public JsonResult ChangeCustomerActiveStatus(int id)
        {
            var success = CustomerRepo.Instance.ToggleCustomerStatusActive(id);

            return Json(new
            {
                status = success ? "success" : "error",
                message = success ? "Trạng thái đã được cập nhật." : "Cập nhật thất bại."
            });
        }
        public JsonResult ChangeEmployeeActiveStatus(int id)
        {
            var success = EmployeeRepo.Instance.ToggleEmployee(id);
            return Json(new
            {
                status = success ? "success" : "error",
                message = success ? "Trạng thái đã được cập nhật." : "Cập nhật thất bại."
            });
        } // trả về true/false
        public ActionResult ServiceList()
        {
            HashSet<ServiceView> listSer = new HashSet<ServiceView>();
            var serviceList = ServiceRepository.Instance.All();
            if (serviceList != null && serviceList.Count > 0)
            {
                listSer = serviceList;
            }
            ViewBag.Data = listSer;
            return View();
        }
        public JsonResult ChangeServiceActiveStatus(int id)
        {
            var success = ServiceRepository.Instance.Delete(id); 

            return Json(new
            {
                status = success ? "success" : "error",
                message = success ? "Trạng thái đã được cập nhật." : "Cập nhật thất bại."
            });
        }
        public ActionResult createService()
        {
            return View();
        }
        [HttpPost]
        public ActionResult createService(HttpPostedFileBase Image, ServiceView model)
        {
            try
            {
                string directoryPath = Server.MapPath("~/Content/client/images");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (Image != null && Image.ContentLength > 0)
                {
                    string safeFileName = Path.GetFileNameWithoutExtension(Image.FileName)
                                            .Replace(" ", "_")
                                            + Path.GetExtension(Image.FileName);
                    string newFileName = $"{DateTime.Now.Ticks}_{safeFileName}";
                    string fullPathSave = Path.Combine(directoryPath, newFileName);

                    Image.SaveAs(fullPathSave);
                    model.Image = newFileName;
                }
                else
                {
                    model.Image = "defaultimage.jpg";
                }

                ServiceRepository.Instance.Create(model);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                System.IO.File.WriteAllText(Server.MapPath("~/Content/log.txt"), ex.ToString());
            }
            return RedirectToAction("ServiceList");
        }
        [HttpGet]
        public ActionResult Admin_edit_service(int id)
        {
            var service = ServiceRepository.Instance.GetById(id);

            if (service == null)
            {
                return HttpNotFound();
            }

            return View(service);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditService(HttpPostedFileBase Image, ServiceView model)
        {
            if (ModelState.IsValid)
            {
                var existingService = ServiceRepository.Instance.GetById(model.Id); 

                if (existingService == null)
                {
                    ModelState.AddModelError("", "Dịch vụ không tồn tại.");
                    return View("Admin_edit_service", model);
                }

                try
                {
                    string directoryPath = Server.MapPath("~/Content/client/images");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    if (Image != null && Image.ContentLength > 0)
                    {
                        if (!string.IsNullOrEmpty(existingService.Image) && existingService.Image != "defaultimage.jpg")
                        {
                            string oldImagePath = Path.Combine(directoryPath, existingService.Image);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string safeFileName = Path.GetFileNameWithoutExtension(Image.FileName)
                                                    .Replace(" ", "_")
                                                    + Path.GetExtension(Image.FileName);
                        string newFileName = $"{DateTime.Now.Ticks}_{safeFileName}";
                        string fullPathSave = Path.Combine(directoryPath, newFileName);

                        Image.SaveAs(fullPathSave);
                        model.Image = newFileName;
                    }
                    else
                    {
                        model.Image = existingService.Image;
                    }

                    model.Active = existingService.Active;

                    ServiceRepository.Instance.Update(model);

                    return RedirectToAction("ServiceList");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    System.IO.File.WriteAllText(Server.MapPath("~/Content/log.txt"), ex.ToString());
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dịch vụ: " + ex.Message);
                }
            }
            return View("Admin_edit_service", model);
        }
}
}