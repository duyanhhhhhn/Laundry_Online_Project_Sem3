using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_FE.Models.Dao
{
    public class EmployeeDAO
    {
        private static readonly object _lock = new object();
        private static EmployeeDAO _instance = null;

        private EmployeeDAO() { }

        public static EmployeeDAO Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EmployeeDAO();
                    }
                }
                return _instance;
            }
        }

        public bool Create(EmployeeView emp)
        {
            try
            {
                using (var en = new Entities.OnlineLaundryEntities())
                {
                    var newEmp = new Employee
                    {
                        first_name = emp.FirstName,
                        last_name = emp.LastName,
                        phone_number = emp.Phone,
                        role = emp.Role,
                        hire_date = emp.HireDate,
                        salary = emp.Salary,
                        active = emp.Active,
                        password = BCrypt.Net.BCrypt.HashPassword(emp.Password)
                    };

                    en.Employees.Add(newEmp);
                    en.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Create Error: " + ex.Message);
            }

            return false;
        }

        public bool Update(EmployeeView emp)
        {
            try
            {
                using (var en = new Entities.OnlineLaundryEntities())
                {
                    var existing = en.Employees.Find(emp.Id);
                    if (existing != null)
                    {
                        existing.first_name = emp.FirstName;
                        existing.last_name = emp.LastName;
                        existing.phone_number = emp.Phone;
                        existing.role = emp.Role;
                        existing.salary = emp.Salary;
                        existing.hire_date = emp.HireDate;
                        en.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update Error: " + ex.Message);
            }

            return false;
        }

        public HashSet<EmployeeView> GetAdmins()
        {
            using (var en = new Entities.OnlineLaundryEntities())
            {
                return en.Employees
                         .Where(e => e.active == 1 && e.role == 0)
                         .Select(e => new EmployeeView
                         {
                             Id = e.employee_id,
                             FirstName = e.first_name,
                             LastName = e.last_name,
                             Phone = e.phone_number,
                             Role = (int)e.role,
                             HireDate = (DateTime)e.hire_date,
                             Salary = (int)(e.salary ?? 0),
                             Active = (int)e.active
                         }).ToHashSet();
            }
        }

        public HashSet<EmployeeView> GetStaffs()
        {
            using (var en = new Entities.OnlineLaundryEntities())
            {
                return en.Employees
                         .Where(e => e.active == 1 && e.role == 1)
                         .Select(e => new EmployeeView
                         {
                             Id = e.employee_id,
                             FirstName = e.first_name,
                             LastName = e.last_name,
                             Phone = e.phone_number,
                             Role = (int)e.role,
                             HireDate = (DateTime)e.hire_date,
                             Salary = (int)(e.salary ?? 0),
                             Active = (int)e.active
                         }).ToHashSet();
            }
        }

        public HashSet<EmployeeView> GetByRole(int role, bool onlyActive = true)
        {
            using (var en = new OnlineLaundryEntities())
            {
                var query = en.Employees.Where(e => e.role == role);

                if (onlyActive)
                    query = query.Where(e => e.active == 1);

                return query.Select(e => new EmployeeView
                {
                    Id = e.employee_id,
                    FirstName = e.first_name,
                    LastName = e.last_name,
                    Phone = e.phone_number,
                    Role = (int)e.role,
                    HireDate = (DateTime)e.hire_date,
                    Salary = (int)(e.salary ?? 0),
                    Active = (int)e.active
                }).ToHashSet();
            }
        }


        public EmployeeView ReturnEmployee(string phone, string pwd)
        {
            try
            {
                using (Entities.OnlineLaundryEntities en = new Entities.OnlineLaundryEntities())
                {
                    var emp = en.Employees.FirstOrDefault(e => e.phone_number == phone && e.active == 1);

                    if (emp != null && BCrypt.Net.BCrypt.Verify(pwd, emp.password))
                    {
                        return new EmployeeView
                        {
                            Id = emp.employee_id,
                            FirstName = emp.first_name,
                            LastName = emp.last_name,
                            Phone = emp.phone_number,
                            Role = (int)emp.role,
                            HireDate = (DateTime)emp.hire_date,
                            Salary = (int)(emp.salary ?? 0),
                            Active = (int)emp.active
                        };
                    }

                    Debug.WriteLine("Wrong phone number or password!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            return new EmployeeView();

        }

        public EmployeeView GetById(int id)
        {
            using (var en = new Entities.OnlineLaundryEntities())
            {
                var emp = en.Employees.Find(id);
                if (emp != null)
                {
                    return new EmployeeView
                    {
                        Id = emp.employee_id,
                        FirstName = emp.first_name,
                        LastName = emp.last_name,
                        Phone = emp.phone_number,
                        Role = (int)emp.role,
                        HireDate = (DateTime)emp.hire_date,
                        Salary = (int)(emp.salary ?? 0),
                        Active = (int)emp.active
                    };
                }
            }
            return new EmployeeView();
        }

        public bool SetActive(int id)
        {
            try
            {
                using (var en = new Entities.OnlineLaundryEntities())
                {
                    var emp = en.Employees.Find(id);
                    if (emp != null)
                    {
                        emp.active = emp.active == 1 ? 0 : 1;
                        en.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SetActive Error: " + ex.Message);
            }

            return false;
        }


        public int CountEmployees()
        {
            using (var en = new Entities.OnlineLaundryEntities())
            {
                return en.Employees.Count(e => e.active == 1 && e.role == 0);
            }
        }

        public int CountAdmins()
        {
            using (var en = new Entities.OnlineLaundryEntities())
            {
                return en.Employees.Count(e => e.active == 1 && e.role == 1);
            }
        }

        public bool ResetPassword(int id, string newPassword)
        {
            using (var en = new Entities.OnlineLaundryEntities())
            {
                var emp = en.Employees.Find(id);
                if (emp != null)
                {
                    emp.password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    en.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public HashSet<EmployeeView> Search(string keyword)
        {
            using (var en = new Entities.OnlineLaundryEntities())
            {
                return en.Employees
                         .Where(e => (e.first_name + " " + e.last_name).Contains(keyword) ||
                                     e.phone_number.Contains(keyword))
                         .Select(e => new EmployeeView
                         {
                             Id = e.employee_id,
                             FirstName = e.first_name,
                             LastName = e.last_name,
                             Phone = e.phone_number,
                             Role = (int)e.role,
                             HireDate = (DateTime)e.hire_date,
                             Salary = (int)(e.salary ?? 0),
                             Active = (int)e.active
                         })
                         .ToHashSet();
            }
        }

    }
}