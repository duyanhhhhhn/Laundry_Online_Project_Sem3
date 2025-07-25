﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;

namespace Laundry_Online_Web_FE.Controllers.Admin
{
    [RoutePrefix("Admin/package")]
    public class PackageController : Controller
    {
        private readonly PackageRepository _packageRepository;

        public PackageController()
        {
            _packageRepository = PackageRepository.Instance;
        }
        // GET: Package
        [Route("")]
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
                TempData["ErrorMessage"] = "Not found package.";
                return RedirectToAction("Index");
            }
            return View(package);
        }
        [Route("AdminDetail")]
        public ActionResult AdminDetail(int id)
        {
            var package = PackageRepository.Instance.GetById(id);
            if (package == null)
            {
                TempData["ErrorMessage"] = "No found Package.";
                return RedirectToAction("Index");
            }
            return View(package);
        }
        [Route("Create")]
        // GET: Package/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreatePackage(HttpPostedFileBase Image, PackageView model)
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

                PackageRepository.Instance.Create(model);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                System.IO.File.WriteAllText(Server.MapPath("~/Content/log.txt"), ex.ToString());
            }
            return RedirectToAction("Index");
        }
        // POST: Package/Create
        //[HttpPost]
        //public ActionResult CreatePackage()
        //{
        //    string name = Request.Form["Package_Name"];
        //    string desc = Request.Form["Description"];
        //    string price = Request.Form["Price"];
        //    string value = Request.Form["Value"];
        //    string unit = Request.Form["Unit"];
        //    string validityDay = Request.Form["Validity_Day"];

        //    var newPackage = new PackageView
        //    {
        //        Package_Name = name,
        //        Description = desc,
        //        Price = string.IsNullOrEmpty(price) ? 0 : Convert.ToDecimal(price),
        //        Value = string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value),
        //        Unit = unit,
        //        Validity_Day = string.IsNullOrEmpty(validityDay) ? 0 : Convert.ToInt32(validityDay)
        //    };

        //    bool created = PackageRepository.Instance.Create(newPackage);

        //    if (created)
        //        TempData["SuccessMessage"] = "Tạo gói dịch vụ thành công!";
        //    else
        //        TempData["ErrorMessage"] = "Tạo thất bại!";

        //    return RedirectToAction("Index","Package");
        //}



        // GET: Package/Edit/5
        [Route("Edit")]


        public ActionResult Edit(int id)
        {
            var package = PackageRepository.Instance.GetById(id);
            if (package == null)
            {
                TempData["ErrorMessage"] = "Not found package.";
                return RedirectToAction("Index");
            }
            return View(package);
        }
        // POST: Package/Edit/5
        [HttpPost]
        public ActionResult EditPackage(HttpPostedFileBase Image, PackageView model)
        {
            if (ModelState.IsValid)
            {
                var existingPackage = PackageRepository.Instance.GetById(model.Id);

                if (existingPackage == null)
                {
                    ModelState.AddModelError("", "The package does not exist..");
                    return View("Edit", model);
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
                        if (!string.IsNullOrEmpty(existingPackage.Image) && existingPackage.Image != "defaultimage.jpg")
                        {
                            string oldImagePath = Path.Combine(directoryPath, existingPackage.Image);
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
                        model.Image = existingPackage.Image;
                    }


                    PackageRepository.Instance.Update(model);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    System.IO.File.WriteAllText(Server.MapPath("~/Content/log.txt"), ex.ToString());
                    ModelState.AddModelError("", "An error occurred while updating the package: " + ex.Message);
                }
            }
            return View("Edit", model);
        }
        //public ActionResult EditPackage(int id)
        //{
        //    string name = Request.Form["Package_Name"];
        //    string desc = Request.Form["Description"];
        //    string price = Request.Form["Price"];
        //    string value = Request.Form["Value"];
        //    string unit = Request.Form["Unit"];
        //    string validityDay = Request.Form["Validity_Day"];

        //    var updatedPackage = new PackageView
        //    {
        //        Id = id,
        //        Package_Name = name,
        //        Description = desc,
        //        Price = string.IsNullOrEmpty(price) ? 0 : Convert.ToDecimal(price),
        //        Value = string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value),
        //        Unit = unit,
        //        Validity_Day = string.IsNullOrEmpty(validityDay) ? 0 : Convert.ToInt32(validityDay)
        //    };

        //    bool result = PackageRepository.Instance.Update(updatedPackage);

        //    if (result)
        //        TempData["SuccessMessage"] = "Cập nhật gói thành công!";
        //    else
        //        TempData["ErrorMessage"] = "Cập nhật gói thất bại!";

        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var result = _packageRepository.Delete(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Package deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the package!";
            }
            return RedirectToAction("Index");
        }


    }
}
