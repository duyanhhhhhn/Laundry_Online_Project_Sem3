using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laundry_Online_Web_FE.Models.ModelViews;
using System.Web.Mvc;

namespace Laundry_Online_Web_FE.Models.Auth
{
    public class EmployeeAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var employee = httpContext.Session["Employee"] as EmployeeView;
            return employee != null && employee.Active == 1;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Employee/Login");
        }
    }
}