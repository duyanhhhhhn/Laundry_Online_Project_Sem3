using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_FE.Controllers.Admin
{
    public class PackageController : Controller
    {
        private readonly PackageRepository _packageRepository;

        public PackageController()
        {
            _packageRepository = PackageRepository.Instance;
        }
        // GET: Package
        public ActionResult Index()
        {
            HashSet<PackageView> packageSet = new HashSet<PackageView>();
            var packageList = PackageRepository.Instance.GetAll();
            if (packageList != null && packageList.Count > 0)
            {
                packageSet = new HashSet<PackageView>(packageList);
            }

            ViewBag.Data = packageSet;
            return View();
        }

        // GET: Package/Detail/5
        public ActionResult Detail(int id)
        {
            var package = PackageRepository.Instance.GetById(id);
            if (package == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy gói.";
                return RedirectToAction("Index");
            }
            return View(package);
        }
        // GET: Package/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Package/Create
        [HttpPost]
        public ActionResult CreatePackage()
        {
            string name = Request.Form["Package_Name"];
            string desc = Request.Form["Description"];
            string price = Request.Form["Price"];
            string value = Request.Form["Value"];
            string unit = Request.Form["Unit"];
            string validityDay = Request.Form["Validity_Day"];

            var newPackage = new PackageView
            {
                Package_Name = name,
                Description = desc,
                Price = string.IsNullOrEmpty(price) ? 0 : Convert.ToDecimal(price),
                Value = string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value),
                Unit = unit,
                Validity_Day = string.IsNullOrEmpty(validityDay) ? 0 : Convert.ToInt32(validityDay)
            };

            bool created = PackageRepository.Instance.Create(newPackage);

            if (created)
                TempData["SuccessMessage"] = "Tạo gói dịch vụ thành công!";
            else
                TempData["ErrorMessage"] = "Tạo thất bại!";

            return RedirectToAction("Index","Package");
        }



        // GET: Package/Edit/5
        public ActionResult Edit(int id)
        {
            var package = PackageRepository.Instance.GetById(id);
            if (package == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy gói.";
                return RedirectToAction("Index");
            }
            return View(package);
        }
        // POST: Package/Edit/5
        [HttpPost]
        public ActionResult EditPackage(int id)
        {
            string name = Request.Form["Package_Name"];
            string desc = Request.Form["Description"];
            string price = Request.Form["Price"];
            string value = Request.Form["Value"];
            string unit = Request.Form["Unit"];
            string validityDay = Request.Form["Validity_Day"];

            var updatedPackage = new PackageView
            {
                Id = id,
                Package_Name = name,
                Description = desc,
                Price = string.IsNullOrEmpty(price) ? 0 : Convert.ToDecimal(price),
                Value = string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value),
                Unit = unit,
                Validity_Day = string.IsNullOrEmpty(validityDay) ? 0 : Convert.ToInt32(validityDay)
            };

            bool result = PackageRepository.Instance.Update(updatedPackage);

            if (result)
                TempData["SuccessMessage"] = "Cập nhật gói thành công!";
            else
                TempData["ErrorMessage"] = "Cập nhật gói thất bại!";

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var result = _packageRepository.Delete(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Xóa gói dịch vụ thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa gói dịch vụ!";
            }
            return RedirectToAction("Index");
        }

    }
}
