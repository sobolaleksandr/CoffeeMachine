namespace CoffeeMachine.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CoffeeMachine.BL;

    using CoffeMachine.Data;

    using Microsoft.AspNetCore.Mvc;

    using Serilog;
    using Serilog.Core;

    [ApiController]
    [Route("[controller]")]
    public class CoffeeController : ControllerBase
    {
        private readonly Logger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CoffeeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = new LoggerConfiguration()
                .CreateLogger();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var coffee = await _unitOfWork
                .GetRepository<Coffee>()
                .FirstOrDefaultAsync(entity => id == entity.Id);

            if (coffee == null)
                return NotFound(id);

            _unitOfWork
                .GetRepository<Coffee>()
                .Delete(coffee);

            await _unitOfWork.SaveChangesAsync();
            return Ok(coffee);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> Get(Guid id)
        {
            var coffee = await _unitOfWork
                .GetRepository<Coffee>()
                .FirstOrDefaultAsync(entity => id == entity.Id);

            return coffee == null ? NotFound(id) : Ok(coffee);
        }

        [HttpGet]
        public async Task<List<Coffee>> GetAll()
        {
            return await _unitOfWork
                .GetRepository<Coffee>()
                .GetAll();
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Coffee coffee)
        {
            if (coffee.Price <= 0)
                return BadRequest(coffee);

            var entity = await _unitOfWork
                .GetRepository<Coffee>()
                .FirstOrDefaultAsync(entity => entity.Id == coffee.Id || entity.Name == coffee.Name);

            if (entity != null)
                return BadRequest(coffee);

            await _unitOfWork
                .GetRepository<Coffee>()
                .Add(coffee);

            await _unitOfWork.SaveChangesAsync();
            return Ok(coffee);
        }

        [HttpPut]
        public async Task<ActionResult> Put([FromBody] Coffee coffee)
        {
            var entity = await _unitOfWork
                .GetRepository<Coffee>()
                .FirstOrDefaultAsync(entity => entity.Id == coffee.Id || entity.Name == coffee.Name);

            if (entity == null)
                return BadRequest(coffee);

            _unitOfWork
                .GetRepository<Coffee>()
                .Update(coffee);

            await _unitOfWork.SaveChangesAsync();
            return Ok(coffee);
        }
    }
}