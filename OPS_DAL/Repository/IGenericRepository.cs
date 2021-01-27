using System.Collections.Generic;

namespace OPS_DAL.Responsive
{
    interface IGenericRepository<T>
    {
        int Add(T item);
        int Update(T item);
        int Delete(T item);
        int DeleteById(string id);
        IEnumerable<T> GetAll();
        T GetByID(string id);
        T GetByItem(T item);
    }
}
