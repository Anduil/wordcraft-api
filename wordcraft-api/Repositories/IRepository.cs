using Models.Entities;
using System.Collections.Generic;

namespace Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        void Add(T item);
        void Remove(int id);
        void Update(T item);
        T Get(int id);
        IEnumerable<T> GetAll();
    }
}
