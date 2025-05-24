using System;

namespace trustesseApp.Core.Repository;
public interface IRepository<T> where T : class
{
    Task<T> getById(int id);
    Task<T> Insert(T entity);
    Task<IList<T>> Insert(IEnumerable<T> entities);
    Task<T> Update(T entity);
    Task<string> Delete(int[] Ids);

    Task Delete(IEnumerable<T> entities);

    Task<IEnumerable<T>> Fetch();

    Task<string> Delete(int Id);

    IQueryable<T> Table { get; }

    IQueryable<T> TableNoTracking { get; }

}

