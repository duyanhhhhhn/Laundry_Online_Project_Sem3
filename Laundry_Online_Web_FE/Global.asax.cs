using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Controllers;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_FE
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static System.Timers.Timer _autoUpdateTimer;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            _autoUpdateTimer = new System.Timers.Timer(5 * 60 * 1000); 
            _autoUpdateTimer.Elapsed += (sender, e) =>
            {
                try
                {
                    InvoiceRepository.Instance.AutoUpdateExpiredOrders();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Timer auto-update error: {ex.Message}");
                }
            };
            _autoUpdateTimer.Start();
        }


        //protected void Application_AcquireRequestState(object sender, EventArgs e)
        //{
        //    var app = (HttpApplication)sender;
        //    var context = app.Context;
        //    var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
        //    if (routeData == null) return;

        //    string controller = (routeData.Values["controller"] ?? "").ToString().ToLower();
        //    string action = (routeData.Values["action"] ?? "").ToString().ToLower();

        //    string currentPath = $"/{controller}/{action}".ToLower(); // Đưa về chữ thường
        //    string url = context.Request.Url.AbsolutePath.ToLower();

        //    // =========================
        //    // PHÂN QUYỀN CUSTOMER
        //    // =========================
        //    if (url.Contains("/client"))
        //    {
        //        if (context.Session["customer"] == null)
        //        {
        //            context.Response.RedirectToRoute(new { controller = "Home", action = "Login" });
        //            return;
        //        }
        //    }
        //    // =========================
        //    // PHÂN QUYỀN EMPLOYEE/ADMIN
        //    // =========================
        //    else if (url.Contains("/admin"))
        //    {
        //        if (currentPath == "/admin/accessdenied") return;

        //        if (context.Session["employee"] == null)
        //        {
        //            context.Response.RedirectToRoute(new { controller = "Auth", action = "Login_employee" });
        //            return;
        //        }

        //        var emp = (EmployeeView)context.Session["employee"];
        //        int role = emp.Role; // 1 = admin, 0 = employee

        //        var allowedForEmployee = new List<string>
        //{
        //    "/admin/index",
        //    "/admin/dashboard",
        //    "/admin/allcustomerlist",
        //    "/admin/customeractive",
        //    "/admin/customerinacitve",
        //    "/admin/admin_create_customer",
        //    "/admin/admin_edit_customer",
        //    "/admin/customerdetail",
        //    "/admin/orders",
        //    "/admin/invoice",
        //    "/admin/servicelist",
        //    "/admin/admin_create_service",
        //    "/admin/admin_edit_service",
        //    "/admin/packagelist",
        //    "/admin/admin_create_package",
        //    "/admin/admin_edit_package",
        //    "/admin/blogpostlist"
        //};

        //        if (role == 0) // Nhân viên thường
        //        {
        //            if (!allowedForEmployee.Contains(currentPath))
        //            {
        //                context.Response.RedirectToRoute(new { controller = "Admin", action = "AccessDenied" });
        //                return;
        //            }
        //        }
        //    }
        //}
    }
}