using Laundry_Online_Web_FE.Models.Dao;
using Laundry_Online_Web_FE.Models.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laundry_Online_Web_FE.Models.Repositories
{
    public sealed class ServiceRepository 
    {
        public static ServiceRepository _instance = null;
        public static ServiceRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceRepository();
                }
                return _instance;
            }
        }
        public HashSet<ServiceView> All()
        {
            var list = ServiceDAO.Instance.GetAllServices();
            return new HashSet<ServiceView>(list);
        }

        public bool Create(ServiceView entity)
        {
            return ServiceDAO.Instance.CreateService(entity);
        }

        public bool Delete(int id)
        {
            return ServiceDAO.Instance.DeleteService(id);
        }

        public HashSet<ServiceView> FindByKeyword(string keyword)
        {
            var result = ServiceDAO.Instance.FindServicesByKeyword(keyword);
            return new HashSet<ServiceView>(result);
        }

        public bool Update(ServiceView entity)
        {
            return ServiceDAO.Instance.UpdateService(entity);

        }
        public ServiceView GetById(int id)
        {
            return ServiceDAO.Instance.GetById(id);
        }
        public HashSet<ServiceView> GetInactiveServices()
        {
             var list = ServiceDAO.Instance.GetInactiveServices();
             return new HashSet<ServiceView>(list);
         }
}
}