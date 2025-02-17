using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Ecommerce_site.Repo.IRepo;

public interface IGenericRepo<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<T> GetByIdAsync(long id);
    Task<T> GetByConditionAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true);

    Task<T> GetByConditionAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool asNoTracking = true);

    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAllByConditionAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>> include);

    Task<List<TResult>> GetSelectedColumnsListsAsync<TResult>(
        Expression<Func<T, TResult>> selector);

    Task<TResult> GetSelectedColumnsAsync<TResult>(
        Expression<Func<T, TResult>> selector);

    Task<List<TResult>> GetSelectedColumnsListsByConditionAsync<TResult>(Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector);

    Task<List<TResult>> GetSelectedColumnsListsByConditionAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool asNoTracking = true);

    Task<TResult> GetSelectedColumnsAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool asNoTracking = true);

    Task<TResult> GetSelectedColumnsByConditionAsync<TResult>(Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector);

    Task<TResult> GetSelectedColumnsByConditionAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool asNoTracking = true);

    Task<bool> EntityExistByConditionAsync(Expression<Func<T, bool>> predicate);

    Task<bool> EntityExistByConditionAsync(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include);

    Task<IList<T>> AddBulkAsync(IList<T> entities);
    Task<IList<T>> UpdateBulk(IList<T> entities);
    Task<IList<T>> DeleteBulk(IList<T> entities);
    Task<int> CountByConditionAsync(Expression<Func<T, bool>> predicate);
}