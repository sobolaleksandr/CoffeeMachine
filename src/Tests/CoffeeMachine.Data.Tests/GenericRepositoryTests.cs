namespace CoffeeMachine.Data.Tests
{
    using System;
    using System.Threading.Tasks;

    using CoffeeMachine.BL;

    using CoffeMachine.Data;

    using Xunit;

    public class GenericRepositoryTests
    {
        [Fact]
        public async Task Add()
        {
            var inMemoryContext = new InMemoryContext();
            var coffeeRepository = new GenericRepository<Coffee>(inMemoryContext);

            await coffeeRepository.Add(new Coffee()).ConfigureAwait(false);

            Assert.Single(inMemoryContext.Coffees.Local);
        }

        [Fact]
        public async Task Delete()
        {
            var inMemoryContext = new InMemoryContext();
            var coffeeRepository = new GenericRepository<Coffee>(inMemoryContext);

            var coffee = new Coffee();
            await coffeeRepository.Add(coffee).ConfigureAwait(false);
            Assert.Single(inMemoryContext.Coffees.Local);

            coffeeRepository.Delete(coffee);
            Assert.Empty(inMemoryContext.Coffees.Local);
        }

        [Fact]
        public async Task FirstOrDefaultAsync()
        {
            var inMemoryContext = new InMemoryContext();
            var coffeeRepository = new GenericRepository<Coffee>(inMemoryContext);
            var coffee = new Coffee { Id = new Guid(), Name = "Латте", Price = 850 };

            await coffeeRepository.Add(coffee).ConfigureAwait(false);
            await inMemoryContext.SaveChangesAsync().ConfigureAwait(false);

            var actual = await coffeeRepository
                .FirstOrDefaultAsync(entity => entity.Name == coffee.Name)
                .ConfigureAwait(false);

            Assert.Equal(coffee, actual);
        }
    }
}