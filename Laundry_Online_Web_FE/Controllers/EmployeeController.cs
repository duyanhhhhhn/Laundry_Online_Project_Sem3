using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;

namespace Laundry_Online_Web_FE.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: Employee
        public ActionResult Index()
        {
            var list = EmployeeRepo.Instance.GetActiveStaffs();
            return View(list);
        }

        // 🔍 Tìm kiếm
        public ActionResult Search(string keyword)
        {
            var results = EmployeeRepo.Instance.SearchEmployee(keyword);
            return View("Index", results);
        }

        // 🧑 Hiển thị chi tiết nhân viên
        public ActionResult Details(int id)
        {
            var emp = EmployeeRepo.Instance.GetEmployeeById(id);
            if (emp == null || emp.Id == 0)
                return HttpNotFound();

            return View(emp);
        }

        // 🛠 Tạo nhân viên mới
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(EmployeeView model)
        {
            if (ModelState.IsValid)
            {
                EmployeeRepo.Instance.Create(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // 📝 Chỉnh sửa thông tin
        public ActionResult Edit(int id)
        {
            var emp = EmployeeRepo.Instance.GetEmployeeById(id);
            return View(emp);
        }

        [HttpPost]
        public ActionResult Edit(EmployeeView model)
        {
            if (ModelState.IsValid)
            {
                EmployeeRepo.Instance.Update(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // 🔁 Đổi trạng thái hoạt động
        public ActionResult Toggle(int id)
        {
            EmployeeRepo.Instance.ToggleEmployee(id);
            return RedirectToAction("Index");
        }

        // 🔐 Đặt lại mật khẩu
        public ActionResult ResetPassword(int id)
        {
            var emp = EmployeeRepo.Instance.GetEmployeeById(id);
            return View(emp);
        }

        [HttpPost]
        public ActionResult ResetPassword(int id, string newPassword)
        {
            EmployeeRepo.Instance.ResetEmployeePassword(id, newPassword);
            return RedirectToAction("Index");
        }
    }
}