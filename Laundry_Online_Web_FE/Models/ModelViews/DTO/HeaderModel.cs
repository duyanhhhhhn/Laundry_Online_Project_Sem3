using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.ModelViews.DTO
{
    public class HeaderModel
    {
        public HashSet<ServiceView> Services { get; set; }
        public HashSet<PackageView> Packages { get; set; }
    }
}