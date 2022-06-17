namespace CoffeeMachine.Data.Tests;

using System.Threading.Tasks;

using CoffeeMachine.BL;

using CoffeMachine.Data;

using Xunit;

public class UnitOfWorkTests
{
    [Fact]
    public void GetRepository()
    {
        var inMemoryContext = new InMemoryContext();
        var unitOfWork = new UnitOfWork<InMemoryContext>(inMemoryContext);
        var coffeeRepository = unitOfWork.GetRepository<Coffee>();
        Assert.IsType<GenericRepository<Coffee>>(coffeeRepository);
        Assert.NotNull(coffeeRepository);

        var orderRepository = unitOfWork.GetRepository<Order>();
        Assert.IsType<GenericRepository<Order>>(orderRepository);
        Assert.NotNull(orderRepository);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesNone()
    {
        var inMemoryContext = new InMemoryContext();
        var unitOfWork = new UnitOfWork<InMemoryContext>(inMemoryContext);

        var actual = await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesOneEntity()
    {
        var inMemoryContext = new InMemoryContext();
        var unitOfWork = new UnitOfWork<InMemoryContext>(inMemoryContext);
        inMemoryContext.Coffees.Add(new Coffee());

        var actual = await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesTwoEntity()
    {
        var inMemoryContext = new InMemoryContext();
        var unitOfWork = new UnitOfWork<InMemoryContext>(inMemoryContext);
        inMemoryContext.Coffees.Add(new Coffee());
        inMemoryContext.Orders.Add(new Order());

        var actual = await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        Assert.Equal(2, actual);
    }
}