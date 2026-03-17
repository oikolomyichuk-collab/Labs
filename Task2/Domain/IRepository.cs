using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(params object[] keyValues);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Save();
    }
}
