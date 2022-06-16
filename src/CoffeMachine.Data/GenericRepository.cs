namespace CoffeMachine.Data;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

public sealed class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(DbContext context)
    {
        _dbSet = context.Set<TEntity>();
        DbSetEntity = context.Set<TEntity>();
    }

    public DbSet<TEntity> DbSetEntity { get; }

    public async Task Add(TEntity entity)
    {
        await _dbSet.AddAsync(entity).ConfigureAwait(false);
    }

    public void Delete(TEntity entityToDelete)
    {
        _dbSet.Remove(entityToDelete);
    }

    public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate).ConfigureAwait(false);
    }

    public async Task<List<TEntity>> GetAll()
    {
        return await _dbSet.ToListAsync().ConfigureAwait(false);
    }

    public void Update(TEntity entityToUpdate)
    {
        _dbSet.Update(entityToUpdate);
    }
}