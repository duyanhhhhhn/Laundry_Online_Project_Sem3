using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laundry_Online_Web_FE.Models.Repositories
{
    internal interface IRepository<T> where T : class
    {
        bool Create(T entity);
        bool Update(T entity);
        bool Delete(T entity);
        HashSet<T> All();
        HashSet<T> FindByKeyword(string keyword);
    }
}
