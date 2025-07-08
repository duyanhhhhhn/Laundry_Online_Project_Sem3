using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var role = Session?["Role"] as string;

            if (!string.IsNullOrEmpty(role))
            {
                var identity = new System.Security.Principal.GenericIdentity(role);
                var principal = new System.Security.Principal.GenericPrincipal(identity, new[] { role });

                HttpContext.Current.User = principal;
            }
        }
    }
}
