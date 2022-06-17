namespace CoffeMachine.Data;

using System.Threading.Tasks;

/// <summary>
/// Defines the interface(s) for unit of work.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Gets the specified repository for the <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>An instance of type inherited from <see cref="IGenericRepository{TEntity}"/> interface.</returns>
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the database.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
    Task<int> SaveChangesAsync();
}