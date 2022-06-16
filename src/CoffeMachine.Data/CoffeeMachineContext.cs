namespace CoffeMachine.Data
{
    using CoffeeMachine.BL;

    using Microsoft.EntityFrameworkCore;

    public sealed class CoffeeMachineContext : DbContext
    {
        public CoffeeMachineContext(DbContextOptions<CoffeeMachineContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<Coffee> Coffees { get; set; }

        public DbSet<Order> Orders { get; set; }
    }
}