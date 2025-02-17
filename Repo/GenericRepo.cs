using System.Linq.Expressions;
using Ecommerce_site.Data;
using Ecommerce_site.Exception;
using Ecommerce_site.Repo.IRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Ecommerce_site.Repo;

public class GenericRepo<T> : IGenericRepo<T> where T : class
{
    private readonly EcommerceSiteContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepo(EcommerceSiteContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }


    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }


    public async Task<bool> EntityExistByConditionAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().AnyAsync(predicate);
    }

    public async Task<bool> EntityExistByConditionAsync(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
    {
        IQueryable<T> query = _dbSet;

        query = include(query);

        return await query.AsNoTracking().AnyAsync(predicate);
    }

    public async Task<List<TResult>> GetSelectedColumnsListsAsync<TResult>(
        Expression<Func<T, TResult>> selector)
    {
        return await _dbSet.AsNoTracking().Select(selector).ToListAsync();
    }

    public async Task<List<TResult>> GetSelectedColumnsListsByConditionAsync<TResult>(
        Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
    {
        return await _dbSet.AsNoTracking().Where(predicate).Select(selector).ToListAsync();
    }

    public async Task<List<TResult>> GetSelectedColumnsListsByConditionAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool asNoTracking = true)
    {
        IQueryable<T> query = _dbSet;

        query = include(query);

        query = query.Where(predicate);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.Select(selector).ToListAsync();
    }

    public async Task<TResult> GetSelectedColumnsByConditionAsync<TResult>(Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector)
    {
        return await _dbSet.AsNoTracking().Where(predicate).Select(selector).FirstOrDefaultAsync() ??
               throw new EntityNotFoundException(typeof(T), selector);
    }

    public async Task<TResult> GetSelectedColumnsByConditionAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool asNoTracking = true)
    {
        IQueryable<T> query = _dbSet;

        query = include(query);

        query = query.Where(predicate);

        if (asNoTracking) query = query.AsNoTracking();

        return await query.Select(selector).FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(typeof(T), predicate);
    }

    public async Task<TResult> GetSelectedColumnsAsync<TResult>(
        Expression<Func<T, TResult>> selector)
    {
        return await _dbSet.AsNoTracking().Select(selector).FirstOrDefaultAsync() ??
               throw new EntityNotFoundException(typeof(T), selector);
    }

    public async Task<TResult> GetSelectedColumnsAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool asNoTracking = true)
    {
        IQueryable<T> query = _dbSet;

        query = include(query);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.Select(selector).FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(typeof(T), selector);
    }


    public async Task<T> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id) ?? throw new EntityNotFoundException(typeof(T), id);
    }

    public async Task<T> GetByConditionAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true)
    {
        var query = _dbSet.Where(predicate);
        if (asNoTracking) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync() ?? throw new EntityNotFoundException(typeof(T), predicate);
    }

    public async Task<T> GetByConditionAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
        bool asNoTracking = true)
    {
        IQueryable<T> query = _dbSet;

        query = include(query);

        query = query.Where(predicate);

        if (asNoTracking) query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(typeof(T), predicate);
    }


    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
    {
        IQueryable<T> query = _dbSet;

        query = include(query);

        return await query.ToListAsync();
    }

    public async Task<IList<T>> AddBulkAsync(IList<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<IList<T>> UpdateBulk(IList<T> entities)
    {
        _dbSet.UpdateRange(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<IList<T>> DeleteBulk(IList<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<int> CountByConditionAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().Where(predicate).CountAsync();
    }

    public async Task<List<T>> GetAllByConditionAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
}