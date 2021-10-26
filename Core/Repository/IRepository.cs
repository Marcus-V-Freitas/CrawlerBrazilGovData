using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Repository
{
    public interface IRepository<T> : IDisposable where T : class, new()
    {
        Task<T> InsertAsync(T model);

        Task<bool> EditAsync(T model, int id);

        Task<bool> DeleteAsync(Expression<Func<T, bool>> where);

        Task<List<T>> FindAllAsync(Expression<Func<T, bool>> where);

        Task<List<T>> FindAllAsync();

        Task<T> FindAsync(Expression<Func<T, bool>> where);

        IQueryable<T> Query();

        IQueryable<T> Query(params Expression<Func<T, object>>[] includes);

        IQueryable<T> QueryFast();

        IQueryable<T> QueryFast(params Expression<Func<T, object>>[] includes);

        Task<bool> SaveAsync();

        string ConnectionString { get; }
    }
}