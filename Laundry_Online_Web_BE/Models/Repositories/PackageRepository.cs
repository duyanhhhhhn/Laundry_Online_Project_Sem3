using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laundry_Online_Web_BE.Models.Entities;
using Laundry_Online_Web_BE.Models.ModelViews;

namespace Laundry_Online_Web_BE.Models.Repositories
{
    public class PackageRepository
    {
        private static PackageRepository _instance = null;
        private static readonly object _lock = new object();
        private readonly OnlineLaundryEntities _context;

        private PackageRepository()
        {
            _context = new OnlineLaundryEntities();
        }

        public static PackageRepository Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new PackageRepository();
                    return _instance;
                }
            }
        }

        public List<PackageView> GetAll()
        {
            return _context.Packages.Select(p => new PackageView
            {
                Id = p.package_id,
                Package_Name = p.package_name,
                Description = p.description,
                Price = p.price,
                Value = p.value ?? 0,
                Unit = p.unit,
                Validity_Day = p.validity_days ?? 30
            }).ToList();
        }

        public PackageView GetById(int id)
        {
            var p = _context.Packages.FirstOrDefault(x => x.package_id == id);
            if (p == null) return null;

            return new PackageView
            {
                Id = p.package_id,
                Package_Name = p.package_name,
                Description = p.description,
                Price = p.price,
                Value = p.value ?? 0,
                Unit = p.unit,
                Validity_Day = p.validity_days ?? 30
            };
        }

        public bool Add(PackageView model)
        {
            try
            {
                var entity = new Package
                {
                    package_name = model.Package_Name,
                    description = model.Description,
                    price = model.Price,
                    value = model.Value,
                    unit = model.Unit,
                    validity_days = model.Validity_Day
                };

                _context.Packages.Add(entity);
                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool Update(PackageView model)
        {
            try
            {
                var entity = _context.Packages.FirstOrDefault(x => x.package_id == model.Id);
                if (entity == null) return false;

                entity.package_name = model.Package_Name;
                entity.description = model.Description;
                entity.price = model.Price;
                entity.value = model.Value;
                entity.unit = model.Unit;
                entity.validity_days = model.Validity_Day;

                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool Delete(int id)
        {
            try
            {
                var entity = _context.Packages.FirstOrDefault(x => x.package_id == id);
                if (entity == null) return false;

                _context.Packages.Remove(entity);
                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }
    }


}