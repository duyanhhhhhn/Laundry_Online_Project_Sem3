using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.ModelViews.DTO;

namespace Laundry_Online_Web_FE.Models.Repositories.RepoBackup
{
    public class CustomerDAO
    {
        public static CustomerDAO _intance = null;
        public static CustomerDAO Instance
        {
            get
            {
                if (_intance == null)
                {
                    _intance = new CustomerDAO();
                }
                return _intance;
            }
        }
        public bool IsExistCustomer(string phoneNumber)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    var customer = en.Customers.FirstOrDefault(c => c.phone_number == phoneNumber);
                    return customer != null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update Error: " + ex.Message);
            }
            return false;
        }
        public bool CreateCustomer(CustomerView customer)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    if (IsExistCustomer(customer.PhoneNumber))
                    {
                        return false; // Customer already exists
                    }
                    var newCustomer = new Customer()
                    {
                        first_name = customer.FirstName,
                        last_name = customer.LastName,
                        phone_number = customer.PhoneNumber,
                        address = customer.Address,
                        registration_date = DateTime.Now,

                        active = 1
                    };
                    en.Customers.Add(newCustomer);
                    customer.Id = newCustomer.customer_id; // Set the ID of the newly created customer
                    en.SaveChanges();
                    return true; // Customer created successfully
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Create Customer Error: " + ex.Message);
                return false; // An error occurred
            }
        }
        public CustomerView GetCustomerByPhoneNumber(string phoneNumber)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    var customer = en.Customers.FirstOrDefault(c => c.phone_number == phoneNumber);
                    if (customer != null)
                    {
                        return new CustomerView
                        {
                            Id = customer.customer_id,
                            FirstName = customer.first_name,
                            LastName = customer.last_name,
                            PhoneNumber = customer.phone_number,
                            Address = customer.address,
                            RegistrationDate = (DateTime)customer.registration_date,

                            Active = customer.active
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetCustomerByPhoneNumber Error: " + ex.Message);
            }
            return null; // Return null if no customer found or an error occurred
        }
        public bool UpdateCustomer(CustomerView customer)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    var existingCustomer = en.Customers.FirstOrDefault(c => c.customer_id == customer.Id);
                    if (existingCustomer == null)
                    {
                        return false; // Customer not found
                    }
                    existingCustomer.first_name = customer.FirstName;
                    existingCustomer.last_name = customer.LastName;
                    existingCustomer.phone_number = customer.PhoneNumber;
                    existingCustomer.address = customer.Address;
                    existingCustomer.registration_date = customer.RegistrationDate;
                    en.SaveChanges();
                    return true; // Update successful
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update Error: " + ex.Message);
                return false; // An error occurred
            }
        }
        public bool DeleteCustomer(CustomerView customer)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    var existingCustomer = en.Customers.FirstOrDefault(c => c.customer_id == customer.Id);
                    if (existingCustomer == null)
                    {
                        return false; // Customer not found
                    }
                    existingCustomer.active = 0; // Soft delete by setting active to 0
                    en.SaveChanges();
                    return true; // Delete successful
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Delete Error: " + ex.Message);
                return false; // An error occurred
            }
        }
        public HashSet<CustomerView> GetAllCustomer()
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return new HashSet<CustomerView>(en.Customers
                        .Select(c => new CustomerView
                        {
                            Id = c.customer_id,
                            FirstName = c.first_name,
                            LastName = c.last_name,
                            PhoneNumber = c.phone_number,
                            Address = c.address,
                            RegistrationDate = (DateTime)c.registration_date,

                            Active = c.active
                        }).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAllCustomer Error: " + ex.Message);
                return new HashSet<CustomerView>(); // Return an empty set if an error occurs
            }
        }
        public HashSet<CustomerView> GetAllCustomerActive()
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return new HashSet<CustomerView>(en.Customers
                        .Where(c => c.active == 1)
                        .Select(c => new CustomerView
                        {
                            Id = c.customer_id,
                            FirstName = c.first_name,
                            LastName = c.last_name,
                            PhoneNumber = c.phone_number,
                            Address = c.address,
                            RegistrationDate = (DateTime)c.registration_date,

                            Active = c.active
                        }).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAllCusstomerActive Error: " + ex.Message);
                return new HashSet<CustomerView>(); // Return an empty set if an error occurs
            }
        }
        public HashSet<CustomerView> GetAllCustomerInActive()
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return new HashSet<CustomerView>(en.Customers
                        .Where(c => c.active == 0)
                        .Select(c => new CustomerView
                        {
                            Id = c.customer_id,
                            FirstName = c.first_name,
                            LastName = c.last_name,
                            PhoneNumber = c.phone_number,
                            Address = c.address,
                            RegistrationDate = (DateTime)c.registration_date,

                            Active = c.active
                        }).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAllCusstomerInActive Error: " + ex.Message);
                return new HashSet<CustomerView>(); // Return an empty set if an error occurs
            }
        }
        public CustomerView GetCustomerById(int id)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    var customer = en.Customers.FirstOrDefault(c => c.customer_id == id);
                    if (customer != null)
                    {
                        return new CustomerView
                        {
                            Id = customer.customer_id,
                            FirstName = customer.first_name,
                            LastName = customer.last_name,
                            PhoneNumber = customer.phone_number,
                            Address = customer.address,
                            RegistrationDate = (DateTime)customer.registration_date,

                            Active = customer.active
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetCustomerById Error: " + ex.Message);
            }
            return null; // Return null if no customer found or an error occurred
        }
        public HashSet<CustomerView> GetCustomerByRegistrationDatePeriod(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return new HashSet<CustomerView>(en.Customers
                        .Where(c => c.registration_date >= startDate && c.registration_date <= endDate)
                        .Select(c => new CustomerView
                        {
                            Id = c.customer_id,
                            FirstName = c.first_name,
                            LastName = c.last_name,
                            PhoneNumber = c.phone_number,
                            Address = c.address,
                            RegistrationDate = (DateTime)c.registration_date,

                            Active = c.active
                        }).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetCustomerByRegistrationDate Error: " + ex.Message);
                return new HashSet<CustomerView>(); // Return an empty set if an error occurs
            }
        }
        public HashSet<CustomerView> GetCustomerByRegistrationDate(DateTime registrationDate)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return new HashSet<CustomerView>(en.Customers
                        .Where(c => c.registration_date.Value.Date == registrationDate.Date)
                        .Select(c => new CustomerView
                        {
                            Id = c.customer_id,
                            FirstName = c.first_name,
                            LastName = c.last_name,
                            PhoneNumber = c.phone_number,
                            Address = c.address,
                            RegistrationDate = (DateTime)c.registration_date,
                            Active = c.active
                        }).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetCustomerByRegistrationDate Error: " + ex.Message);
                return new HashSet<CustomerView>(); // Return an empty set if an error occurs
            }
        }
        public HashSet<CustomerView> GetCustomerByName(string name)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return new HashSet<CustomerView>(en.Customers
                        .Where(c => c.first_name.Contains(name) || c.last_name.Contains(name))
                        .Select(c => new CustomerView
                        {
                            Id = c.customer_id,
                            FirstName = c.first_name,
                            LastName = c.last_name,
                            PhoneNumber = c.phone_number,
                            Address = c.address,
                            RegistrationDate = (DateTime)c.registration_date,

                            Active = c.active
                        }).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetCustomerByName Error: " + ex.Message);
                return new HashSet<CustomerView>(); // Return an empty set if an error occurs
            }
        }
        public bool ResetPassword(int customerId, string newPassword)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    var customer = en.Customers.FirstOrDefault(c => c.customer_id == customerId);
                    if (customer == null)
                    {
                        return false; // Customer not found
                    }
                    customer.password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    en.SaveChanges();
                    return true; // Password reset successful
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ResetPassword Error: " + ex.Message);
                return false; // An error occurred
            }
        }
        public bool ChangeStatusActive(int CustomerID)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    var customer = en.Customers.FirstOrDefault(c => c.customer_id == CustomerID);
                    if (customer == null)
                    {
                        return false; // Customer not found
                    }
                    customer.active = customer.active == 1 ? 0 : 1; // Toggle active status
                    en.SaveChanges();
                    return true; // Status change successful
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Change Status Active Error: " + ex.Message);
                return false; // An error occurred
            }
        }
        public HashSet<CustomerView> FindByKeyword(string keyword)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return new HashSet<CustomerView>(en.Customers
                        .Where(c => c.first_name.Contains(keyword) || c.last_name.Contains(keyword) || c.phone_number.Contains(keyword))
                        .Select(c => new CustomerView
                        {
                            Id = c.customer_id,
                            FirstName = c.first_name,
                            LastName = c.last_name,
                            PhoneNumber = c.phone_number,
                            Address = c.address,
                            RegistrationDate = (DateTime)c.registration_date,

                            Active = c.active
                        }).ToList());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FindByKeyword Error: " + ex.Message);
                return new HashSet<CustomerView>(); // Return an empty set if an error occurs
            }
        }
        public int CountTotalCustomers()
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return en.Customers.Count();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CountTotalCustomers Error: " + ex.Message);
                return 0; // Return 0 if an error occurs
            }
        }
        public int CountActiveCustomers()
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return en.Customers.Count(c => c.active == 1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CountActiveCustomers Error: " + ex.Message);
                return 0; // Return 0 if an error occurs
            }
        }
        public int CountInactiveCustomers()
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    return en.Customers.Count(c => c.active == 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CountInactiveCustomers Error: " + ex.Message);
                return 0; // Return 0 if an error occurs
            }
        }
        public CustomerView LoginCustomer(string phone, string password)
        {
            try
            {
                using (var en = new OnlineLaundryEntities())
                {
                    var customer = en.Customers.FirstOrDefault(c => c.phone_number == phone && c.active == 1);
                    if (customer != null && BCrypt.Net.BCrypt.Verify(password, customer.password))
                    {
                        return new CustomerView
                        {
                            Id = customer.customer_id,
                            FirstName = customer.first_name,
                            LastName = customer.last_name,
                            PhoneNumber = customer.phone_number,
                            Address = customer.address,
                            RegistrationDate = customer.registration_date ?? DateTime.MinValue,
                            Active = customer.active
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Login Customer Error: " + ex.Message);
            }
            return null;
        }
        public CustomerDetailModel GetCustomerDetail(int customerId)
        {
            var customer = GetCustomerById(customerId);

            // Lấy danh sách hóa đơn theo Customer_Id
            var invoices = InvoiceRepository.Instance.GetAll()
                .Where(i => i.Customer_Id == customerId)
                .ToList();

            // Lấy danh sách gói dịch vụ theo Customer_Id
            var customerPackages = CustomerPackageRepository.Instance.GetAll()
                .Where(cp => cp.Customer_Id == customerId)
                .ToList();

            // Gộp thêm tên gói từ PackageRepo
            var customerPackageDetails = customerPackages.Select(cp =>
            {
                var package = PackageRepository.Instance.GetById(cp.Package_Id);
                return new CustomerPackageDetailView
                {
                    Id = cp.Id,
                    Customer_Id = cp.Customer_Id,
                    Package_Id = cp.Package_Id,
                    Package_Name = package?.Package_Name ?? "",
                    Date_Start = cp.Date_Start,
                    Date_End = cp.Date_End,
                    Value = cp.Value,
                    Unite = package.Unit,
                    Payment_Id = cp.Payment_Id,
                    Image = package?.Image ?? ""
                };
            }).ToList();

            return new CustomerDetailModel
            {
                Customer = customer,
                Invoices = invoices,
                CustomerPackages = customerPackageDetails
            };
        }

    }
}