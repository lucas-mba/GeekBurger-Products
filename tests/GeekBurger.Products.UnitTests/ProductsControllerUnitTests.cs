using AutoMapper;
using FluentAssertions;
using GeekBurger.Products.Controllers;
using GeekBurger.Products.Model;
using GeekBurger.Products.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;

namespace GeekBurger.Products.UnitTests
{
    public class ProductsControllerUnitTests
    {
        private ProductsController _productsController { get; set; }
        private Mock<IProductsRepository> _productsRepositoryMock { get; set; }
        private Mock<IMapper> _mapperMock { get; set; }

        private MockRepository _mockRepository { get; set; }

        public ProductsControllerUnitTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _productsRepositoryMock = _mockRepository.Create<IProductsRepository>();
            _mapperMock = _mockRepository.Create<IMapper>();

            _productsController = new ProductsController(_productsRepositoryMock.Object, _mapperMock.Object);
        }


        [Fact]
        public void OnGetProductsByStoreName_WhenThereIsNoProduct_ShouldReturnNotFound()
        {
            //Arrange
            var storeName = "Morumbi";
            _productsRepositoryMock.Setup(_ => _.GetProductsByStoreName(storeName)).Returns(new List<Product>());

            var expected = new NotFoundObjectResult("Nenhum dado encontrado");

            //Act
            var response = _productsController.GetProductsByStoreName(storeName);
            //Assert
            _mockRepository.VerifyAll();
            response.Should().BeEquivalentTo(expected);
        }
    }
}