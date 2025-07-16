using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_FE
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        //protected void Application_AcquireRequestState(object sender, EventArgs e)
        //{
        //    var app = (HttpApplication)sender;
        //    var context = app.Context;
        //    var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
        //    if (routeData == null) return;

        //    string controller = (routeData.Values["controller"] ?? "").ToString().ToLower();
        //    string action = (routeData.Values["action"] ?? "").ToString().ToLower();

        //    string currentPath = $"/{controller}/{action}";
        //    string url = context.Request.Url.AbsolutePath.ToLower();

        //    // Debug
        //    System.Diagnostics.Debug.WriteLine("CurrentPath: " + currentPath);
        //    System.Diagnostics.Debug.WriteLine("Session[customer]: " + (context.Session["customer"] != null));
        //    System.Diagnostics.Debug.WriteLine("Session[employee]: " + (context.Session["employee"] != null));

        //    // =========================
        //    // PHÂN QUYỀN CUSTOMER
        //    // =========================
        //    if (url.Contains("/customer") || url.Contains("/client"))
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
        //        if (context.Session["employee"] == null)
        //        {
        //            context.Response.RedirectToRoute(new { controller = "Auth", action = "Login" });
        //            return;
        //        }

        //        var emp = (EmployeeView)context.Session["employee"];
        //        int role = emp.Role; // 1 = admin, 0 = employee

        //        // Các URL mà EMPLOYEE (role = 0) được phép truy cập
        //        var allowedForEmployee = new List<string>
        //{
        //    "/admin/index",
        //    "/admin/dashboard",
        //    "/admin/profile",
        //    "/admin/orders"
        //    // Thêm các url hợp lệ nếu cần
        //};

        //        if (role == 0) // Là nhân viên thường
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
