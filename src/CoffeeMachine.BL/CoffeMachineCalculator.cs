namespace CoffeeMachine.BL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CoffeeMachineCalculator
    {
        private readonly decimal[] _availableBanknotes = { 5000, 2000, 1000, 500, 100, 50 };

        public decimal[] GetChange(Coffee coffee, decimal cache)
        {
            var price = coffee.Price;
            if (cache < price)
                return Array.Empty<decimal>();

            var change = cache - price;
            var banknotes = CalculateChange(change);
            var banknotesSum = banknotes.Sum();
            if (banknotesSum != change)
                throw new Exception($"Ошибка в расчетах сумма купюр {banknotesSum}, сдача {change}");

            return banknotes;
        }

        private decimal[] CalculateChange(decimal change)
        {
            var output = new List<decimal>();
            foreach (var availableBanknote in _availableBanknotes)
            {
                var result = (int)(change / availableBanknote);
                if (result <= 0)
                    continue;

                change -= availableBanknote * result;
                for (var i = 0; i < result; i++)
                    output.Add(availableBanknote);
            }

            return output.ToArray();
        }
    }
}