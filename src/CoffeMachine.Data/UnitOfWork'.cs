namespace CoffeMachine.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    /// <summary>
    /// Represents the default implementation of the <see cref="IUnitOfWork"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the db context.</typeparam>
    public sealed class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        private bool _disposed;
        private Dictionary<Type, object> _repositories;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public UnitOfWork(TContext context)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the specified repository for the <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="hasCustomRepository"><c>True</c> if providing custom repositry</param>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>An instance of type inherited from <see cref="IGenericRepository{TEntity}"/> interface.</returns>
        public IGenericRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false)
            where TEntity : class
        {
            _repositories ??= new Dictionary<Type, object>();

            // what's the best way to support custom reposity?
            if (hasCustomRepository)
            {
                var customRepo = _dbContext.GetService<IGenericRepository<TEntity>>();
                if (customRepo != null)
                    return customRepo;
            }

            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
                _repositories[type] = new GenericRepository<TEntity>(_dbContext);

            return (IGenericRepository<TEntity>)_repositories[type];
        }

        /// <summary>
        /// Asynchronously saves all changes made in this unit of work to the database.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Db context.
        /// </summary>
        /// <returns>The instance of type <typeparamref name="TContext"/>.</returns>
        private readonly TContext _dbContext;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                {
                    // clear repositories
                    _repositories?.Clear();

                    // dispose the db context.
                    _dbContext.Dispose();
                }

            _disposed = true;
        }
    }
}