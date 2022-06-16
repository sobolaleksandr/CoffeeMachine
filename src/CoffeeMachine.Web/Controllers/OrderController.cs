namespace CoffeeMachine.Web.Controllers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using CoffeeMachine.BL;

using CoffeMachine.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        var idOrders = orders
            .Where(entity => entity.CoffeeId == id)
            .ToList();

        if (!idOrders.Any())
            return NotFound(id);

        var coffee = await _unitOfWork
            .GetRepository<Coffee>()
            .FirstOrDefaultAsync(entity => id == entity.Id)
            .ConfigureAwait(false);

        var order = new OrderDto
        {
            Cache = coffee.Price * idOrders.Count,
            CoffeeId = id,
            Name = coffee.Name,
        };

        return Ok(order);
    }

    [HttpGet]
    public async Task<List<OrderDto>> GetAll()
    {
        var orders = _unitOfWork
            .GetRepository<Order>()
            .DbSetEntity;

        var coffees = _unitOfWork
            .GetRepository<Coffee>()
            .DbSetEntity;

        var items = await orders.Join(coffees, order => order.CoffeeId, coffee => coffee.Id,
            (order, coffee) => new { order, coffee })
            .ToListAsync();

        return new List<OrderDto>();
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] OrderDto order)
    {
        var coffee = await _unitOfWork
            .GetRepository<Coffee>()
            .FirstOrDefaultAsync(entity => order.CoffeeId == entity.Id || order.Name == entity.Name);

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
            });

        await _unitOfWork.SaveChangesAsync();
        return Ok(change);
    }
}