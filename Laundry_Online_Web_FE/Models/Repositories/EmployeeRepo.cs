using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Razor.Tokenizer.Symbols;
using Antlr.Runtime.Tree;
using Laundry_Online_Web_FE.Models.Dao;
using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace Laundry_Online_Web_FE.Models.Repositories
{
    public class EmployeeRepo
    {
        private static EmployeeRepo _instance = null;

        private EmployeeRepo() { }
        public static EmployeeRepo Instance
        {
            get
            {
                if (_instance == null) { _instance = new EmployeeRepo(); }
                return _instance;
            }
        }
        public HashSet<EmployeeView> GetAllEmployees()
        {
            return EmployeeDAO.Instance.GetAllEmployees();
        }

        public HashSet<EmployeeView> GetActiveAdmins()
        {
            return EmployeeDAO.Instance.GetAdmins();
        }

        public HashSet<EmployeeView> GetActiveStaffs()
        {
            return EmployeeDAO.Instance.GetStaffs();
        }

        public EmployeeView LoginEmployee(string phone, string password)
        {
            return EmployeeDAO.Instance.ReturnEmployee(phone, password);
        }

        public bool ToggleEmployee(int id)
        {
            return EmployeeDAO.Instance.SetActive(id);
        }

        public int CountEmployees()
        {
            return EmployeeDAO.Instance.CountEmployees();
        }

        public int CountAdmins()
        {
            return EmployeeDAO.Instance.CountAdmins();
        }

        public EmployeeView GetEmployeeById(int id)
        {
            return EmployeeDAO.Instance.GetById(id);
        }

        public bool ResetEmployeePassword(int id, string newPassword)
        {
            return EmployeeDAO.Instance.ResetPassword(id, newPassword);
        }

        public HashSet<EmployeeView> SearchEmployee(string keyword)
        {
            return EmployeeDAO.Instance.Search(keyword);
        }

        public bool Create(EmployeeView entity)
        {
            return EmployeeDAO.Instance.Create(entity);
        }

        public bool Update(EmployeeView entity)
        {
            return EmployeeDAO.Instance.Update(entity);
        }

        public HashSet<EmployeeView> All()
        {
            return EmployeeDAO.Instance.Search("");
        }

        public HashSet<EmployeeView> Search(string keyword)
        {
            return EmployeeDAO.Instance.Search(keyword);
        }
    }
}