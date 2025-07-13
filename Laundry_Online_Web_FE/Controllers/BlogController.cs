using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Laundry_Online_Web_FE.Controllers
{
    public class BlogController : Controller
    {
        public ActionResult Index()
        {
            HashSet<BlogPostView> listBlog = new HashSet<BlogPostView>();
            var blogList = BlogPostRepository.Instance.All();
            if (blogList != null && blogList.Count > 0)
            {
                listBlog = blogList;
            }
            ViewBag.Data = listBlog;
            return View();
        }
        [HttpGet]
        public ActionResult BlogDetail(int id)
        {
            var blog = BlogPostRepository.Instance.GetById(id);

            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(blog);
        }
        [Route("admin/BlogPostList")]
        public ActionResult BlogPostList()
        {
            HashSet<BlogPostView> listBlog = new HashSet<BlogPostView>();
            var blogList = BlogPostRepository.Instance.All();
            if (blogList != null && blogList.Count > 0)
            {
                listBlog = blogList;
            }
            ViewBag.Data = listBlog;
            return View();
        }
        [Route("admin/createBlogPost")]
        public ActionResult createBlogPost()
        {
            return View();
        }
        [HttpPost]
        public ActionResult create_Blog_Post(HttpPostedFileBase Image, BlogPostView model)
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

                BlogPostRepository.Instance.Create(model);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                System.IO.File.WriteAllText(Server.MapPath("~/Content/log.txt"), ex.ToString());
            }
            return RedirectToAction("BlogPostList");
        }
        [HttpGet]
        public ActionResult Admin_edit_BlogPost(int id)
        {
            var blog = BlogPostRepository.Instance.GetById(id);

            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(blog);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBlogPost(HttpPostedFileBase Image, BlogPostView model)
        {
            if (ModelState.IsValid)
            {
                var existingService = BlogPostRepository.Instance.GetById(model.ID);

                if (existingService == null)
                {
                    ModelState.AddModelError("", "Dịch vụ không tồn tại.");
                    return View("Admin_edit_BlogPost", "Admin", model);
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
                    BlogPostRepository.Instance.Update(model);

                    return RedirectToAction("BlogPostList");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    System.IO.File.WriteAllText(Server.MapPath("~/Content/log.txt"), ex.ToString());
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dịch vụ: " + ex.Message);
                }
            }
            return View("Admin_edit_BlogPost","Admin", model);
        }
        public JsonResult DeleteBlog(int id)
        {
            var success = BlogPostRepository.Instance.Delete(id);

            return Json(new
            {
                status = success ? "success" : "error",
                message = success ? "Xóa thành công" : "Xóa thất bại."
            });
        }
    }
}