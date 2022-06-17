namespace CoffeMachine.Data;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

/// <summary>
/// Generic repository provide all base needed methods (CRUD)
/// </summary>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity">Entity object</param>
    Task Add(T entity);

    /// <summary>
    /// Remove entity from database
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Delete(T entity);

    /// <summary>
    /// Get first entity by predicate 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns>T entity</returns>
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    
    Task<List<T>> GetAll();

    /// <summary>
    /// Update entity
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Update(T entity);
}