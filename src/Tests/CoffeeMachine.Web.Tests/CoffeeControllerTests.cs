namespace CoffeeMachine.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using CoffeeMachine.BL;
    using CoffeeMachine.Web.Controllers;

    using CoffeMachine.Data;

    using Microsoft.AspNetCore.Mvc;

    using Moq;

    using Xunit;

    public class CoffeeControllerTests
    {
        public CoffeeControllerTests()
        {
            _repository = new Mock<IGenericRepository<Coffee>>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _unitOfWork.Setup(uof => uof.GetRepository<Coffee>())
                .Returns(_repository.Object)
                .Verifiable();
        }

        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IGenericRepository<Coffee>> _repository;

        [Theory]
        [MemberData(nameof(GetData))]
        public async Task GetAll(List<Coffee> expected)
        {
            _repository.Setup(repo => repo.GetAll())
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var actual = await controller.GetAll().ConfigureAwait(false);

            Assert.Equal(expected, actual);

            _repository.Verify(repo => repo.GetAll(), Times.Once);
            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
        }

        public static IEnumerable<object[]> GetData()
        {
            return new List<object[]>
            {
                new object[] { new List<Coffee>() },
                new object[] { new List<Coffee> { new() } },
            };
        }

        [Fact]
        public async Task Delete_ReturnsNotFound()
        {
            _repository.Setup(repo => repo.Delete(It.IsAny<Coffee>()))
                .Verifiable();

            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync((Coffee)null!)
                .Verifiable();

            _unitOfWork.Setup(uof => uof.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var guid = new Guid();
            var result = await controller.Delete(guid);

            var viewResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, viewResult.StatusCode);
            Assert.Equal(guid, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
            _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Never);
            _repository.Verify(repo => repo.Delete(It.IsAny<Coffee>()), Times.Never);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsOk()
        {
            _repository.Setup(repo => repo.Delete(It.IsAny<Coffee>()))
                .Verifiable();

            var coffee = new Coffee();
            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync(coffee)
                .Verifiable();

            _unitOfWork.Setup(uof => uof.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var result = await controller.Delete(new Guid()).ConfigureAwait(false);

            var viewResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, viewResult.StatusCode);
            Assert.Equal(coffee, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Exactly(2));
            _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Once);
            _repository.Verify(repo => repo.Delete(It.IsAny<Coffee>()), Times.Once);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Get_ReturnsNotFound()
        {
            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync((Coffee)null!)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var guid = new Guid();
            var result = await controller.Get(guid);

            var viewResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, viewResult.StatusCode);
            Assert.Equal(guid, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Get_ReturnsOk()
        {
            var coffee = new Coffee();
            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync(coffee)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var result = await controller.Get(new Guid()).ConfigureAwait(false);

            var viewResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, viewResult.StatusCode);
            Assert.Equal(coffee, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WithNegativePrice()
        {
            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync((Coffee)null!)
                .Verifiable();

            _repository.Setup(repo => repo.Add(It.IsAny<Coffee>()))
                .Verifiable();

            _unitOfWork.Setup(uof => uof.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var coffee = new Coffee();
            var result = await controller.Post(coffee);

            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, viewResult.StatusCode);
            Assert.Equal(coffee, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Never);
            _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Never);
            _repository.Verify(repo => repo.Add(It.IsAny<Coffee>()), Times.Never);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Never);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WithPositivePrice()
        {
            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync(new Coffee())
                .Verifiable();

            _repository.Setup(repo => repo.Add(It.IsAny<Coffee>()))
                .Verifiable();

            _unitOfWork.Setup(uof => uof.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var coffee = new Coffee { Price = 100 };
            var result = await controller.Post(coffee);

            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, viewResult.StatusCode);
            Assert.Equal(coffee, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
            _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Never);
            _repository.Verify(repo => repo.Add(It.IsAny<Coffee>()), Times.Never);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Post_ReturnsOk()
        {
            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync((Coffee)null!)
                .Verifiable();

            _repository.Setup(repo => repo.Add(It.IsAny<Coffee>()))
                .Verifiable();

            _unitOfWork.Setup(uof => uof.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var coffee = new Coffee { Price = 100 };
            var result = await controller.Post(coffee);

            var viewResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, viewResult.StatusCode);
            Assert.Equal(coffee, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Exactly(2));
            _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Once);
            _repository.Verify(repo => repo.Add(It.IsAny<Coffee>()), Times.Once);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Put_ReturnsBadRequest()
        {
            var coffee = new Coffee();
            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync((Coffee)null!)
                .Verifiable();

            _repository.Setup(repo => repo.Update(It.IsAny<Coffee>()))
                .Verifiable();

            _unitOfWork.Setup(uof => uof.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var result = await controller.Put(coffee);

            var viewResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, viewResult.StatusCode);
            Assert.Equal(coffee, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Once);
            _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Never);
            _repository.Verify(repo => repo.Update(It.IsAny<Coffee>()), Times.Never);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Put_ReturnsOk()
        {
            var coffee = new Coffee();
            _repository.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()))
                .ReturnsAsync(coffee)
                .Verifiable();

            _repository.Setup(repo => repo.Update(It.IsAny<Coffee>()))
                .Verifiable();

            _unitOfWork.Setup(uof => uof.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new CoffeeController(_unitOfWork.Object);
            var result = await controller.Put(coffee);

            var viewResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, viewResult.StatusCode);
            Assert.Equal(coffee, viewResult.Value);

            _unitOfWork.Verify(uof => uof.GetRepository<Coffee>(), Times.Exactly(2));
            _unitOfWork.Verify(uof => uof.SaveChangesAsync(), Times.Once);
            _repository.Verify(repo => repo.Update(It.IsAny<Coffee>()), Times.Once);
            _repository.Verify(repo => repo.FirstOrDefaultAsync(It.IsAny<Expression<Func<Coffee, bool>>>()),
                Times.Once);
        }
    }
}