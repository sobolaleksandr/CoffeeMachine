namespace CoffeeMachine.Web.Tests;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using CoffeeMachine.BL;
using CoffeeMachine.Web.Controllers;

using CoffeMachine.Data;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

public class OrderControllerTests
{
    public OrderControllerTests()
    {
        _coffeeRepository = new Mock<IGenericRepository<Coffee>>();
        _orderRepository = new Mock<IGenericRepository<Order>>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(uof => uof.GetRepository<Order>())
            .Returns(_orderRepository.Object)
            .Verifiable();

        _unitOfWork.Setup(uof => uof.GetRepository<Coffee>())
            .Returns(_coffeeRepository.Object)
            .Verifiable();
    }

    private readonly Mock<IGenericRepository<Coffee>> _coffeeRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IGenericRepository<Order>> _orderRepository;

    [Theory]
    [MemberData(nameof(GetData))]
    public async Task GetAll(List<Order> orders, List<Coffee> coffees, List<OrderDto> expected)
    {
        _orderRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(orders)
            .Verifiable();

        _coffeeRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(coffees)
            .Verifiable();

        var controller = new OrderController(_unitOfWork.Object);
        var actual = await controller.GetAll().ConfigureAwait(false);

        actual.Should().BeEquivalentTo(expected);

        _unitOfWork.Verify(uof => uof.GetRepository<Order>(), Times.Once);
        _orderRepository.Verify(repo => repo.GetAll(), Times.Once);
        _coffeeRepository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
            Times.Never);
    }

    public static IEnumerable<object[]> GetData()
    {
        return new List<object[]>
        {
            new object[] { new List<Order>(), new List<Coffee>(), new List<OrderDto>() },
            new object[]
            {
                new List<Order> { new() { CoffeeId = Coffee.Id }, new() { CoffeeId = Coffee.Id } },
                new List<Coffee> { Coffee },
                new List<OrderDto> { new() { Cache = Coffee.Price * 2, CoffeeId = Coffee.Id, Name = Coffee.Name } }
            },
        };
    }

    private static readonly Coffee Coffee = new() { Id = new Guid(), Name = "Капучино", Price = 850 };

    [Fact]
    public async Task Get_ReturnsNotFound()
    {
        _orderRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(new List<Order>())
            .Verifiable();

        _coffeeRepository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
            .ReturnsAsync((Coffee)null!)
            .Verifiable();

        var controller = new OrderController(_unitOfWork.Object);
        var guid = new Guid();
        var result = await controller.Get(guid).ConfigureAwait(false);

        var viewResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, viewResult.StatusCode);
        Assert.Equal(guid, viewResult.Value);

        _unitOfWork.Verify(uof => uof.GetRepository<Order>(), Times.Once);
        _orderRepository.Verify(repo => repo.GetAll(), Times.Once);
        _coffeeRepository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
            Times.Never);
    }

    [Fact]
    public async Task Get_ReturnsOk()
    {
        _orderRepository.Setup(repo => repo.GetAll())
            .ReturnsAsync(new List<Order> { new() { CoffeeId = Coffee.Id }, new() { CoffeeId = Coffee.Id } })
            .Verifiable();

        _coffeeRepository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
            .ReturnsAsync(Coffee)
            .Verifiable();

        var controller = new OrderController(_unitOfWork.Object);
        var result = await controller.Get(Coffee.Id);

        var viewResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, viewResult.StatusCode);

        var orderDto = Assert.IsType<OrderDto>(viewResult.Value);
        Assert.Equal(Coffee.Price * 2, orderDto.Cache);
        Assert.Equal(Coffee.Id, orderDto.CoffeeId);
        Assert.Equal(Coffee.Name, orderDto.Name);

        _unitOfWork.Verify(uof => uof.GetRepository<Order>(), Times.Once);
        _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
        _orderRepository.Verify(repo => repo.GetAll(), Times.Once);
        _coffeeRepository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task Post_ReturnsBadRequest()
    {
        _coffeeRepository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
            .ReturnsAsync(Coffee)
            .Verifiable();

        _orderRepository.Setup(repo => repo.Add(It.IsAny<Order>()))
            .Verifiable();

        _unitOfWork.Setup(uof => uof.SaveChangesAsync())
            .ReturnsAsync(1)
            .Verifiable();

        var controller = new OrderController(_unitOfWork.Object);
        var orderDto = new OrderDto
        {
            Cache = 200,
        };

        var result = await controller.Post(orderDto).ConfigureAwait(false);

        var viewResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, viewResult.StatusCode);
        Assert.Equal(orderDto, viewResult.Value);

        _unitOfWork.Verify(uof => uof.GetRepository<Order>(), Times.Never);
        _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
        _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Never);
        _orderRepository.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Never);
        _coffeeRepository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task Post_ReturnsNotFound()
    {
        _coffeeRepository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
            .ReturnsAsync((Coffee)null!)
            .Verifiable();

        _orderRepository.Setup(repo => repo.Add(It.IsAny<Order>()))
            .Verifiable();

        _unitOfWork.Setup(uof => uof.SaveChangesAsync())
            .ReturnsAsync(1)
            .Verifiable();

        var controller = new OrderController(_unitOfWork.Object);
        var orderDto = new OrderDto
        {
            Cache = 4000,
        };

        var result = await controller.Post(orderDto);

        var viewResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, viewResult.StatusCode);
        Assert.Equal(orderDto, viewResult.Value);

        _unitOfWork.Verify(uof => uof.GetRepository<Order>(), Times.Never);
        _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
        _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Never);
        _orderRepository.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Never);
        _coffeeRepository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task Post_ReturnsOk()
    {
        _coffeeRepository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
            .ReturnsAsync(Coffee)
            .Verifiable();

        _orderRepository.Setup(repo => repo.Add(It.IsAny<Order>()))
            .Verifiable();

        _unitOfWork.Setup(uof => uof.SaveChangesAsync())
            .ReturnsAsync(1)
            .Verifiable();

        var controller = new OrderController(_unitOfWork.Object);
        var result = await controller.Post(new OrderDto
        {
            Cache = 4000,
        }).ConfigureAwait(false);

        var viewResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, viewResult.StatusCode);
        Assert.Equal(new decimal[] { 2000, 1000, 100, 50 }, viewResult.Value);

        _unitOfWork.Verify(uof => uof.GetRepository<Order>(), Times.Once);
        _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
        _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Once);
        _orderRepository.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Once);
        _coffeeRepository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
            Times.Once);
    }
}