using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using trustesseApp.Infrastructure.Data;

namespace trustesseApp.Core.Repository;


public class Repository<T> : IRepository<T> where T : class{

    private readonly AppDbContext _appDbContext;
    private readonly ILogger<Repository<T>> _logger;
    private DbSet<T> _entities;
    public Repository(AppDbContext requestDbContext, ILogger<Repository<T>> logger)
    {

        _appDbContext = requestDbContext;
         _logger = logger;
    }
    public IQueryable<T> Table => _appDbContext.Set<T>();

    public IQueryable<T> TableNoTracking => _appDbContext.Set<T>().AsNoTracking();

    public async Task<string> Delete(int Id)
    {
        string errorMsg = string.Empty;
        var entity = await Entities.FindAsync(Id);

        _appDbContext.Set<T>().Remove(entity);
        await _appDbContext.SaveChangesAsync();
        return errorMsg;
    }

    public async Task<string> Delete(int[] Ids)
    {
        string errorMsg = string.Empty;
        foreach (var id in Ids)
        {
            var entity = await Entities.FindAsync(id);
            if (entity == null)
            {
                errorMsg = $"Entity with ID {id} not found.";
                _logger.LogError(errorMsg);
                continue;
            }
             _appDbContext.Set<T>().Remove(entity);

        }
        await _appDbContext.SaveChangesAsync();
        return errorMsg;
    }

    public async Task Delete(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        foreach (var entity in entities)
        {
              _appDbContext.Set<T>().Remove(entity);
        }
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<T>> Fetch()
    {
        return await _appDbContext.Set<T>().AsNoTracking().ToListAsync();
    }

    public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
    {
        return _appDbContext.Set<T>().Where(expression);
    }

    public async Task<T> getById(int id)
    {
        return await _appDbContext.Set<T>()
            .FindAsync(id);
            
    }
    public async Task<T> Insert(T entity)
    {
        try
        {
            var retVal = await _appDbContext.Set<T>().AddAsync(entity);
            await _appDbContext.SaveChangesAsync();
            return retVal.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting entity");
            return null;

        }

    }

    public async Task<IList<T>> Insert(IEnumerable<T> entities)
    {
        try
        {
            foreach (var entity in entities)
            {
                await _appDbContext.Set<T>().AddAsync(entity);
            }
            //await _appDbContext.Set<T>().AddRangeAsync(entities);
            await _appDbContext.SaveChangesAsync();
            return entities.ToList();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting entities");
            return Enumerable.Empty<T>().ToList();
        }

    }
    public async Task<T> Update(T entity)
    {
        try
        {
            var retVal = _appDbContext.Set<T>().Update(entity);
            await _appDbContext.SaveChangesAsync();
            return await Task.FromResult(retVal.Entity);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity");
            throw new Exception(ex.Message);
        }
    }

    protected virtual DbSet<T> Entities
    {
        get
        {
            if (_entities == null)
                _entities = _appDbContext.Set<T>();
            return _entities;
        }
    }

}