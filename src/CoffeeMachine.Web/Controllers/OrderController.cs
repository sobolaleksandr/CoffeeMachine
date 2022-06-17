namespace CoffeeMachine.Web.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CoffeeMachine.BL;

using CoffeMachine.Data;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var orders = await _unitOfWork
            .GetRepository<Order>()
            .GetAll()
            .ConfigureAwait(false);

        var ordersCount = orders.Count(entity => entity.CoffeeId == id);
        if (ordersCount == 0)
            return NotFound(id);

        var coffee = await _unitOfWork
            .GetRepository<Coffee>()
            .FirstOrDefaultAsync(entity => id == entity.Id)
            .ConfigureAwait(false);

        var order = new OrderDto
        {
            Cache = coffee.Price * ordersCount,
            CoffeeId = id,
            Name = coffee.Name,
        };

        return Ok(order);
    }

    [HttpGet]
    public async Task<List<OrderDto>> GetAll()
    {
        var orders = await _unitOfWork
            .GetRepository<Order>()
            .GetAll()
            .ConfigureAwait(false);

        var coffees = await _unitOfWork
            .GetRepository<Coffee>()
            .GetAll()
            .ConfigureAwait(false);

        return orders.Join(coffees, order => order.CoffeeId, coffee => coffee.Id,
                (order, coffee) => new { order, coffee })
            .GroupBy(x => x.coffee)
            .Select(grouping =>
            {
                var coffee = grouping.Key;
                return new OrderDto
                    { Cache = coffee.Price * grouping.Count(), Name = coffee.Name, CoffeeId = coffee.Id };
            })
            .ToList();
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] OrderDto order)
    {
        var coffee = await _unitOfWork
            .GetRepository<Coffee>()
            .FirstOrDefaultAsync(entity => order.CoffeeId == entity.Id || order.Name == entity.Name)
            .ConfigureAwait(false);

        if (coffee == null)
            return NotFound(order);

        if (coffee.Price > order.Cache)
            return BadRequest(order);

        var calculator = new CoffeeMachineCalculator();
        var change = calculator.GetChange(coffee, order.Cache);
        await _unitOfWork
            .GetRepository<Order>()
            .Add(new Order
            {
                CoffeeId = coffee.Id,
            }).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync()
            .ConfigureAwait(false);

        return Ok(change);
    }
}