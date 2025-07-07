using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laundry_Online_Web_BE.Models.Entities;
using Laundry_Online_Web_BE.Models.ModelViews;

namespace Laundry_Online_Web_BE.Models.Repositories
{
    public class CustomerPackageRepository
    {
        private static CustomerPackageRepository _instance = null;
        private static readonly object _lock = new object();
        private readonly OnlineLaundryEntities _context;

        private CustomerPackageRepository()
        {
            _context = new OnlineLaundryEntities();
        }

        public static CustomerPackageRepository Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new CustomerPackageRepository();
                    return _instance;
                }
            }
        }

        public List<CustomerPackageView> GetAll()
        {
            return _context.CustomerPackages.Select(cp => new CustomerPackageView
            {
                Id = cp.cp_id,
                Customer_Id = cp.cus_id ?? 0,
                Package_Id = cp.p_id ?? 0,
                Date_Start = cp.date_start ?? DateTime.Now,
                Date_End = cp.date_end ?? DateTime.Now,
                Value = cp.current_value ?? 0,
                Payment_Id = cp.payment_id ?? ""
            }).ToList();
        }

        public CustomerPackageView GetById(int id)
        {
            var cp = _context.CustomerPackages.FirstOrDefault(x => x.cp_id == id);
            if (cp == null) return null;

            return new CustomerPackageView
            {
                Id = cp.cp_id,
                Customer_Id = cp.cus_id ?? 0,
                Package_Id = cp.p_id ?? 0,
                Date_Start = cp.date_start ?? DateTime.Now,
                Date_End = cp.date_end ?? DateTime.Now,
                Value = cp.current_value ?? 0,
                Payment_Id = cp.payment_id ?? ""
            };
        }

        public bool Add(CustomerPackageView model)
        {
            try
            {
                var entity = new CustomerPackage
                {
                    cus_id = model.Customer_Id,
                    p_id = model.Package_Id,
                    date_start = model.Date_Start,
                    date_end = model.Date_End,
                    current_value = model.Value,
                    payment_id = model.Payment_Id
                };

                _context.CustomerPackages.Add(entity);
                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool Update(CustomerPackageView model)
        {
            try
            {
                var entity = _context.CustomerPackages.FirstOrDefault(x => x.cp_id == model.Id);
                if (entity == null) return false;

                entity.cus_id = model.Customer_Id;
                entity.p_id = model.Package_Id;
                entity.date_start = model.Date_Start;
                entity.date_end = model.Date_End;
                entity.current_value = model.Value;
                entity.payment_id = model.Payment_Id;

                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool Delete(int id)
        {
            try
            {
                var entity = _context.CustomerPackages.FirstOrDefault(x => x.cp_id == id);
                if (entity == null) return false;

                _context.CustomerPackages.Remove(entity);
                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }
    }



}