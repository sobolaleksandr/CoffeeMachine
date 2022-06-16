namespace CoffeeMachine.BL
{
    using System;

    public class OrderDto
    {
        public Guid CoffeeId { get; set; }

        public string Name { get; set; }

        public decimal Cache { get; set; }
    }
}