using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_FE.Models.Repositories
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

        public HashSet<CustomerPackageView> GetAll()
        {
            try
            {
                var data = _context.Customer_package.Select(cp => new CustomerPackageView
                {
                    Id = cp.cp_id,
                    Customer_Id = cp.cus_id ?? 0,
                    Package_Id = cp.p_id ?? 0,
                    Date_Start = cp.date_start ?? DateTime.Now,
                    Date_End = cp.date_end ?? DateTime.Now,
                    Value = cp.current_value ?? 0,
                    Payment_Id = cp.payment_id ?? ""
                }).ToHashSet();

                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAll CustomerPackage Error: " + ex.Message);
                return new HashSet<CustomerPackageView>();
            }
        }

        public CustomerPackageView GetById(int id)
        {
            var cp = _context.Customer_package.FirstOrDefault(x => x.cp_id == id);
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
                var entity = new Customer_package
                {
                    cus_id = model.Customer_Id,
                    p_id = model.Package_Id,
                    date_start = model.Date_Start,
                    date_end = model.Date_End,
                    current_value = model.Value,
                    payment_id = model.Payment_Id
                };

                _context.Customer_package.Add(entity);
                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool Update(CustomerPackageView model)
        {
            try
            {
                var entity = _context.Customer_package.FirstOrDefault(x => x.cp_id == model.Id);
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
                var entity = _context.Customer_package.FirstOrDefault(x => x.cp_id == id);
                if (entity == null) return false;

                _context.Customer_package.Remove(entity);
                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public List<CustomerPackageView> GetValidPackagesByCustomerId(int customerId)
        {
            try
            {
                var today = DateTime.Now;

                var validPackages = _context.Customer_package
                    .Where(cp => cp.cus_id == customerId
                                 && cp.date_end >= today
                                 && cp.current_value > 0)
                    .Select(cp => new CustomerPackageView
                    {
                        Id = cp.cp_id,
                        Customer_Id = cp.cus_id ?? 0,
                        Package_Id = cp.p_id ?? 0,
                        Date_Start = cp.date_start ?? today,
                        Date_End = cp.date_end ?? today,
                        Value = cp.current_value ?? 0,
                        Payment_Id = cp.payment_id ?? ""
                    })
                    .ToList();

                return validPackages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetValidPackagesByCustomerId Error: " + ex.Message);
                return new List<CustomerPackageView>();
            }
        }



    }
}