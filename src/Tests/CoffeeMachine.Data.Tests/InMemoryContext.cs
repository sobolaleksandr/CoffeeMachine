namespace CoffeeMachine.Data.Tests;

using CoffeeMachine.BL;

using Microsoft.EntityFrameworkCore;

public class InMemoryContext : DbContext
{
    public DbSet<Coffee> Coffees { get; set; }

    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("test");
    }
}