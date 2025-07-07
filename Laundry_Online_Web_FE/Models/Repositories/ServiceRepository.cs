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
        public HashSet<ServiceView> All()
        {
            var list = ServiceDAO.Instance.GetAllServices();
            return new HashSet<ServiceView>(list);
        }

        public bool Create(ServiceView entity)
        {
            return ServiceDAO.Instance.CreateService(entity);
        }

        public bool Delete(ServiceView entity)
        {
            return ServiceDAO.Instance.DeleteService(entity.Id);
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
    }
}