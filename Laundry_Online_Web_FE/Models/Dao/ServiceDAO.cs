using Antlr.Runtime.Misc;
using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.Dao
{
    public class ServiceDAO
    {
        private static ServiceDAO _instance = null;
        private ServiceDAO() { }
        public static ServiceDAO Instance
        {
            get
            {
                if (_instance == null) { _instance = new ServiceDAO(); }
                return _instance;
            }
        }
        public bool CreateService(ServiceView serviceView)
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    var service = new Service()
                    {
                        s_title = serviceView.Title,
                        s_description = serviceView.Description,
                        s_image = serviceView.Image,
                        s_unit = serviceView.Unit,
                        s_price = serviceView.Price,
                        s_active = serviceView.Active
                    };

                    db.Services.Add(service);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating service: " + ex.Message);
                return false;
            }
        }
        public HashSet<ServiceView> GetAllServices()
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    return db.Services
                             .Where(s => s.s_active == 1)
                             .Select(s => new ServiceView
                             {
                                 Id = s.s_id,
                                 Title = s.s_title,
                                 Description = s.s_description,
                                 Image = s.s_image,
                                 Unit = s.s_unit,
                                 Price = s.s_price,
                                 Active = (int)s.s_active,
                             })
                             .ToHashSet();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAllServices error: " + ex.Message);
                return new HashSet<ServiceView>();
            }
        }
        public bool DeleteService(int id)
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    var service = db.Services.FirstOrDefault(s => s.s_id == id);
                    if (service == null) return false;

                    service.s_active = 0;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeleteService error: " + ex.Message);
                return false;
            }
        }
        public bool UpdateService(ServiceView serviceView)
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    var service = db.Services.FirstOrDefault(s => s.s_id == serviceView.Id);
                    if (service == null) return false;

                    service.s_title = serviceView.Title;
                    service.s_description = serviceView.Description;
                    service.s_image = serviceView.Image;
                    service.s_unit = serviceView.Unit;
                    service.s_price = serviceView.Price;

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateService error: " + ex.Message);
                return false;
            }
        }
        public List<ServiceView> FindServicesByKeyword(string keyword)
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    keyword = keyword?.ToLower() ?? "";

                    return db.Services
                             .Where(s => s.s_active == 1 &&
                                (s.s_title.ToLower().Contains(keyword) ||
                                 s.s_description.ToLower().Contains(keyword)))
                             .Select(s => new ServiceView
                             {
                                 Id = s.s_id,
                                 Title = s.s_title,
                                 Description = s.s_description,
                                 Image = s.s_image,
                                 Unit = s.s_unit,
                                 Price = s.s_price,
                                 Active = s.s_active ?? 1
                             })
                             .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FindServicesByKeyword error: " + ex.Message);
                return new List<ServiceView>();
            }
        }
        public HashSet<ServiceView> GetInactiveServices()
        {

            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    return db.Services
                             .Where(s => s.s_active == 0)
                             .Select(s => new ServiceView
                             {
                                 Id = s.s_id,
                                 Title = s.s_title,
                                 Description = s.s_description,
                                 Image = s.s_image,
                                 Unit = s.s_unit,
                                 Price = s.s_price,
                                 Active = (int)s.s_active,
                             })
                             .ToHashSet();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAllServices error: " + ex.Message);
                return new HashSet<ServiceView>();
            }
        }
        public ServiceView GetById(int id)
        {
            try
            {
                using (var db = new OnlineLaundryEntities())
                {
                    var serviceEntity = db.Services
                                          .FirstOrDefault(s => s.s_id == id);

                    if (serviceEntity == null)
                    {
                        return null;
                    }

                    // Project the entity to ServiceView
                    return new ServiceView
                    {
                        Id = serviceEntity.s_id,
                        Title = serviceEntity.s_title,
                        Description = serviceEntity.s_description,
                        Image = serviceEntity.s_image,
                        Unit = serviceEntity.s_unit,
                        Price = serviceEntity.s_price,
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetServiceById error for ID {id}: {ex.Message}");
                // You might also want to re-throw or throw a custom exception,
                return null;
            }
        }
    }
}