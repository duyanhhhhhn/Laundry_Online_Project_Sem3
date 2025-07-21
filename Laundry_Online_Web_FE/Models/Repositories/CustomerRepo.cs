using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Models.Dao;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.ModelViews.DTO;
using Laundry_Online_Web_FE.Models.Repositories.RepoBackup;
using CustomerDAO = Laundry_Online_Web_FE.Models.Repositories.RepoBackup.CustomerDAO;

namespace Laundry_Online_Web_FE.Models.Repositories
{
    public class CustomerRepo
    {
        public static CustomerRepo _instance = null;
        public static CustomerRepo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CustomerRepo();
                }
                return _instance;
            }
        }
        public bool Create(CustomerView entity)
        {
            return CustomerDAO.Instance.CreateCustomer(entity);
        }
        public bool Update(CustomerView entity)
        {
            return CustomerDAO.Instance.UpdateCustomer(entity);
        }
        public bool Delete(CustomerView entity)
        {
            return CustomerDAO.Instance.DeleteCustomer(entity);
        }
        public HashSet<CustomerView> GetAll()
        {
            return CustomerDAO.Instance.GetAllCustomer();
        }
        public HashSet<CustomerView> GetActiveCustomer()
        {
            return CustomerDAO.Instance.GetAllCustomerActive();
        }
        public HashSet<CustomerView> GetInactiveCustomer()
        {
            return CustomerDAO.Instance.GetAllCustomerInActive();
        }
        public HashSet<CustomerView> FindByKeyword(string keyword)
        {
            return CustomerDAO.Instance.FindByKeyword(keyword);
        }
        public CustomerView GetCustomerById(int id)
        {
            return CustomerDAO.Instance.GetCustomerById(id);
        }
        public CustomerView FindCustomerByPhoneNumber(string phoneNumber)
        {
            return CustomerDAO.Instance.GetCustomerByPhoneNumber(phoneNumber);
        }
        public HashSet<CustomerView> FindCustomerByRegistrationDate(DateTime startDate, DateTime endDate)
        {
            return CustomerDAO.Instance.GetCustomerByRegistrationDatePeriod(startDate, endDate);
        }
        public HashSet<CustomerView> FindCustomerByRegistrationDate(DateTime date)
        {
            return CustomerDAO.Instance.GetCustomerByRegistrationDate(date);
        }
        public HashSet<CustomerView> FindCusomterByName(string name)
        {
            return CustomerDAO.Instance.GetCustomerByName(name);
        }
        public bool ToggleCustomerStatusActive(int id)
        {
            return CustomerDAO.Instance.ChangeStatusActive(id);
        }
        public int CountTotalCustomers()
        {
            return CustomerDAO.Instance.CountTotalCustomers();
        }
        public int CountActiveCustomers()
        {
            return CustomerDAO.Instance.CountActiveCustomers();
        }
        public int CountInactiveCustomers()
        {
            return CustomerDAO.Instance.CountInactiveCustomers();
        }
        public CustomerView LoginCustomer(string phoneNumber, string password)
        {
            return CustomerDAO.Instance.LoginCustomer(phoneNumber, password);
        }
        public CustomerDetailModel GetCustomerDetailById(int id)
        {
            return CustomerDAO.Instance.GetCustomerDetail(id);
        }
        public bool UpdateCusClient(CustomerView entity)
        {
            return CustomerDAO.Instance.UpdateCusClient(entity);
        }
    }
}