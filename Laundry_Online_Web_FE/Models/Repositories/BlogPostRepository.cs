using Laundry_Online_Web_FE.Models.Dao;
using Laundry_Online_Web_FE.Models.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.Repositories
{
    public class BlogPostRepository
    {
            public static BlogPostRepository _instance = null;
            public static BlogPostRepository Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new BlogPostRepository();
                    }
                    return _instance;
                }
            }
            public HashSet<BlogPostView> All()
            {
                var list = BlogPostDao.Instance.GetAllBlogPosts();

                return new HashSet<BlogPostView>(list);
            }

            public bool Create(BlogPostView entity)
            {
                return BlogPostDao.Instance.Create(entity);
            }

            public bool Delete(int id)
            {
                return BlogPostDao.Instance.Delete(id);
            }


            public bool Update(BlogPostView entity)
            {
                return BlogPostDao.Instance.Update(entity);

            }
            public BlogPostView GetById(int id)
            {
                return BlogPostDao.Instance.GetById(id);
            }
          
    }
}