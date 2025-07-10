using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.ModelViews.DTO;
using Laundry_Online_Web_FE.Models.Repositories;
using Laundry_Online_Web_FE.Models.Repositories.RepoBackup;

namespace Laundry_Online_Web_FE.Controllers.Admin
{
    public class CustomerPackageController : Controller
    {
        private readonly CustomerPackageRepository _customerPackageRepository;
        private readonly CustomerRepo _customerRepository;
        private readonly PackageRepository _packageRepository;

        public CustomerPackageController()
        {
            _customerPackageRepository = CustomerPackageRepository.Instance;
            _customerRepository = CustomerRepo.Instance;
            _packageRepository = PackageRepository.Instance;
        }

        // GET: CustomerPackage
        public ActionResult Index()
        {
            var customerPackageList = _customerPackageRepository.GetAll();
            var customerList = _customerRepository.GetAll();
            var packageList = _packageRepository.GetAll();

            var customerPackageViewList = new List<CustomerPackageForm>();

            if (customerPackageList != null && customerPackageList.Count > 0)
            {
                foreach (var cp in customerPackageList)
                {
                    var customer = customerList?.FirstOrDefault(c => c.Id == cp.Customer_Id);
                    var package = packageList?.FirstOrDefault(p => p.Id == cp.Package_Id);

                    var viewModel = new CustomerPackageForm
                    {
                        Id = cp.Id,
                        Customer_Name = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Không tìm thấy",
                        Package_Name = package?.Package_Name ?? "Không tìm thấy",
                        Date_Start = cp.Date_Start,
                        Date_End = cp.Date_End,
                        Value = cp.Value,
                        Payment_Id = cp.Payment_Id,
                        Customer_Id = cp.Customer_Id,
                        Package_Id = cp.Package_Id
                    };

                    customerPackageViewList.Add(viewModel);
                }
            }

            ViewBag.Data = customerPackageViewList;

            return View();
        }
        // GET: CustomerPackage/Detail/5
        public ActionResult Detail(int id)
        {
            var customerPackage = _customerPackageRepository.GetById(id);
            if (customerPackage == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy gói khách hàng.";
                return RedirectToAction("Index");
            }

            var customer = _customerRepository.GetCustomerById(customerPackage.Customer_Id);
            var package = _packageRepository.GetById(customerPackage.Package_Id);

            var customerPackageDetail = new CustomerPackageForm

            {
                Id = customerPackage.Id,
                Customer_Name = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Không tìm thấy",
                Package_Name = package?.Package_Name ?? "Không tìm thấy",
                Date_Start = customerPackage.Date_Start,
                Date_End = customerPackage.Date_End,
                Value = customerPackage.Value,
                Payment_Id = customerPackage.Payment_Id,
                Customer_Id = customerPackage.Customer_Id,
                Package_Id = customerPackage.Package_Id
            };

            return View(customerPackageDetail);
        }

        // GET: CustomerPackage/Create
        public ActionResult Create()
        {
            var customerList = _customerRepository.GetActiveCustomer();
            var packageList = _packageRepository.GetAll();

            ViewBag.CustomerList = customerList;
            ViewBag.PackageList = packageList;

            return View();
        }

        // POST: CustomerPackage/Create
        [HttpPost]
        public ActionResult CreateCustomerPackage()
        {
            string customerId = Request.Form["Customer_Id"];
            string packageId = Request.Form["Package_Id"];
            string dateStart = Request.Form["Date_Start"];
            string dateEnd = Request.Form["Date_End"];
            string value = Request.Form["Value"];
            string paymentId = Request.Form["Payment_Id"];

            var newCustomerPackage = new CustomerPackageView
            {
                Customer_Id = string.IsNullOrEmpty(customerId) ? 0 : Convert.ToInt32(customerId),
                Package_Id = string.IsNullOrEmpty(packageId) ? 0 : Convert.ToInt32(packageId),
                Date_Start = string.IsNullOrEmpty(dateStart) ? DateTime.Now : Convert.ToDateTime(dateStart),
                Date_End = string.IsNullOrEmpty(dateEnd) ? DateTime.Now : Convert.ToDateTime(dateEnd),
                Value = string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value),
                Payment_Id = paymentId ?? ""
            };

            bool created = _customerPackageRepository.Add(newCustomerPackage);

            if (created)
                TempData["SuccessMessage"] = "Tạo gói khách hàng thành công!";
            else
                TempData["ErrorMessage"] = "Tạo thất bại!";

            return RedirectToAction("Index");
        }

        // GET: CustomerPackage/Edit/5
        public ActionResult Edit(int id)
        {
            var customerPackage = _customerPackageRepository.GetById(id);
            if (customerPackage == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy gói khách hàng.";
                return RedirectToAction("Index");
            }

            var customerList = _customerRepository.GetActiveCustomer();
            var packageList = _packageRepository.GetAll();

            ViewBag.CustomerList = customerList;
            ViewBag.PackageList = packageList;

            return View(customerPackage);
        }

        // POST: CustomerPackage/Edit/5
        [HttpPost]
        public ActionResult EditCustomerPackage(int id)
        {
            string customerId = Request.Form["Customer_Id"];
            string packageId = Request.Form["Package_Id"];
            string dateStart = Request.Form["Date_Start"];
            string dateEnd = Request.Form["Date_End"];
            string value = Request.Form["Value"];
            string paymentId = Request.Form["Payment_Id"];

            var updatedCustomerPackage = new CustomerPackageView
            {
                Id = id,
                Customer_Id = string.IsNullOrEmpty(customerId) ? 0 : Convert.ToInt32(customerId),
                Package_Id = string.IsNullOrEmpty(packageId) ? 0 : Convert.ToInt32(packageId),
                Date_Start = string.IsNullOrEmpty(dateStart) ? DateTime.Now : Convert.ToDateTime(dateStart),
                Date_End = string.IsNullOrEmpty(dateEnd) ? DateTime.Now : Convert.ToDateTime(dateEnd),
                Value = string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value),
                Payment_Id = paymentId ?? ""
            };

            bool result = _customerPackageRepository.Update(updatedCustomerPackage);

            if (result)
                TempData["SuccessMessage"] = "Cập nhật gói khách hàng thành công!";
            else
                TempData["ErrorMessage"] = "Cập nhật gói khách hàng thất bại!";

            return RedirectToAction("Index");
        }

        // POST: CustomerPackage/Delete/5
        [HttpPost]
        public ActionResult Delete()
        {
            int id = Convert.ToInt32(Request.Form["id"]);

            var result = _customerPackageRepository.Delete(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Xóa gói khách hàng thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa gói khách hàng!";
            }
            return RedirectToAction("Index");
        }

    }
}