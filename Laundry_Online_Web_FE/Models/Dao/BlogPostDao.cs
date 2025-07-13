using Laundry_Online_Web_FE.Models.Entities; 
using Laundry_Online_Web_FE.Models.ModelViews; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.Dao
{
    public class BlogPostDao
    {
        private static BlogPostDao _instance = null;
        private BlogPostDao() { }
        public static BlogPostDao Instance
        {
            get
            {
                if (_instance == null) { _instance = new BlogPostDao(); }
                return _instance;
            }
        }

        public bool Create(BlogPostView blogPostView)
        {
            try
            {
                using (var db = new OnlineLaundryEntities()) 
                {
                    var blogPost = new BlogPost()
                    {
                        Title = blogPostView.Title,
                        Description = blogPostView.Description,
                        Content = blogPostView.Content,
                        Image = blogPostView.Image,
                        CreateDate = DateTime.Now 
                    };

                    db.BlogPosts.Add(blogPost);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating BlogPost: " + ex.Message);
                return false;
            }
        }

        public HashSet<BlogPostView> GetAllBlogPosts()
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    return db.BlogPosts
                             .OrderByDescending(b => b.CreateDate)
                             .Select(b => new BlogPostView
                             {
                                 ID = b.ID,
                                 Title = b.Title,
                                 Description = b.Description,
                                 Image = b.Image,
                                 Content = b.Content,
                                 CreateDate = b.CreateDate
                             })
                             .ToHashSet();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAllBlogPosts error: " + ex.Message);
                return new HashSet<BlogPostView>();
            }
        }

        public BlogPostView GetById(int id)
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    var blogPostEntity = db.BlogPosts.FirstOrDefault(b => b.ID == id);

                    if (blogPostEntity == null) return null;

                    return new BlogPostView
                    {
                        ID = blogPostEntity.ID,
                        Title = blogPostEntity.Title,
                        Description = blogPostEntity.Description,
                        Image = blogPostEntity.Image,
                        Content = blogPostEntity.Content,
                        CreateDate = blogPostEntity.CreateDate
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetBlogPostById error for ID {id}: {ex.Message}");
                return null;
            }
        }

        public bool Update(BlogPostView blogPostView)
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    var blogPost = db.BlogPosts.FirstOrDefault(b => b.ID == blogPostView.ID);
                    if (blogPost == null) return false; 

                    blogPost.Title = blogPostView.Title;
                    blogPost.Description = blogPostView.Description;
                    blogPost.Content = blogPostView.Content;
                    blogPost.Image = blogPostView.Image;

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateBlogPost error: " + ex.Message);
                return false;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    var blogPost = db.BlogPosts.FirstOrDefault(b => b.ID == id);
                    if (blogPost == null) return false;

                    db.BlogPosts.Remove(blogPost); 
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeleteBlogPost error: " + ex.Message);
                return false;
            }
        }

   
    }
}