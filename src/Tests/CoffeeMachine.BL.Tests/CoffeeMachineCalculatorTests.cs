namespace CoffeeMachine.BL.Tests
{
    using System;

    using Xunit;

    public class CoffeeMachineCalculatorTests
    {
        private static readonly Coffee Coffee = new ()
        {
            Id = new Guid(),
            Name = "Латте",
            Price = 850,
        };

        [Fact]
        public void GetChange_EnoughMoney()
        {
            var calculator = new CoffeeMachineCalculator();
            var change = calculator.GetChange(Coffee, 5000);

            var expected = new decimal[] { 2000, 2000, 100, 50 };
            Assert.Equal(expected, change);
        }

        [Fact]
        public void GetChange_NotEnoughMoney()
        {
            var calculator = new CoffeeMachineCalculator();
            var change = calculator.GetChange(Coffee, 500);

            Assert.Equal(Array.Empty<decimal>(), change);
        }
    }
}