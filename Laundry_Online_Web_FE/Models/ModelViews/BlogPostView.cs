using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laundry_Online_Web_FE.Models.ModelViews
{
    public class BlogPostView
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public string Image { get; set; } = "defaultImage.jpg";

        public DateTime CreateDate { get; set; } = DateTime.Now;

        [AllowHtml] 
        public string Description { get; set; }

        [AllowHtml] 
        public string Content { get; set; }
    }
}