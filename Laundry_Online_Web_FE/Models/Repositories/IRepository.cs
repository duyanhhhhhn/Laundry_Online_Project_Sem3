using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laundry_Online_Web_FE.Models.Repositories
{
    internal interface IRepository<T> where T : class
    {
        void Create(T entity);
        int Update(T entity);
        int Delete(T entity);
        HashSet<T> All();
        HashSet<T> findAll();
        HashSet<T> FindByKeyword(string keyword);
    }
}
