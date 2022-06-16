using Microsoft.EntityFrameworkCore;

namespace CoffeeMachine.BL.Tests;

public class InMemoryContext : DbContext
{
    public DbSet<Coffee> Coffees { get; set; }

    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("test");
    }
}