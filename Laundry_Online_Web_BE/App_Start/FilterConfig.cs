using System.Web;
using System.Web.Mvc;

namespace Laundry_Online_Web_BE
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
