using AutoMapper;
using FluentAssertions;
using GeekBurger.Products.Controllers;
using GeekBurger.Products.Model;
using GeekBurger.Products.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GeekBurger.Products.UnitTests
{
    public class ProductsControllerUnitTests
    {
        private readonly MockRepository _mockRepository;
        private readonly Mock<IProductsRepository> _productsRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProductsController _productsController;

        public ProductsControllerUnitTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _productsRepositoryMock = _mockRepository.Create<IProductsRepository>();
            _mapperMock = _mockRepository.Create<IMapper>();
            _productsController = new ProductsController(_productsRepositoryMock.Object, _mapperMock.Object);

        }

        [Fact]
        public void OnGetProductsByStoreName_WhenNoDataFound_ShouldReturnNotFound()
        {
            //arrange
            var storeName = "Paulista";
            var productList = new List<Product>();

            _productsRepositoryMock.Setup(_ => _.GetProductsByStoreName(storeName)).Returns(productList);

            //act
            var response = _productsController.GetProductsByStoreName(storeName);

            var expected = new NotFoundObjectResult("Nenhum dado encontrado");

            //assert
            Assert.IsType<NotFoundObjectResult>(response);
            response.Should().BeEquivalentTo(expected);
            _mockRepository.VerifyAll();
        }
    }
}