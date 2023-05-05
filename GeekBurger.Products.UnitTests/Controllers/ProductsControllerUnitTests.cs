using AutoMapper;
using FluentAssertions;
using GeekBurger.Products.Contract;
using GeekBurger.Products.Controllers;
using GeekBurger.Products.Model;
using GeekBurger.Products.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBurger.Products.UnitTests.Controllers
{

    public class ProductsControllerUnitTests
    {
        private readonly ProductsController _productsController;
        private Mock<IProductsRepository> _productRepositoryMock;
        private Mock<IMapper> _mapperMock;
        private MockRepository _mockRepository;

        public ProductsControllerUnitTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _productRepositoryMock = _mockRepository.Create<IProductsRepository>();
            _mapperMock = _mockRepository.Create<IMapper>();
            _productsController = new ProductsController(_productRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public void OnGetProductsByStoreName_WhenListIsEmpty_ShouldReturnNotFound()
        {
            //arrange
            var storeName = "Paulista";
            var productList = new List<Product>();
            _productRepositoryMock.Setup(_ => _.GetProductsByStoreName(storeName)).Returns(productList);
            var expected = new NotFoundObjectResult("Nenhum dado encontrado");

            //act
            var response = _productsController.GetProductsByStoreName(storeName);

            //assert            
            Assert.IsType<NotFoundObjectResult>(response);
            response.Should().BeEquivalentTo(expected);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void OnGetProductsByStoreName_WhenProductIsFound_ShouldReturnProduct()
        {
            //arrange
            var storeName = "Paulista";            
            var product = new Product
            {
                ProductId = Guid.NewGuid(),
            };

            var productList = new List<Product> { product };
            _productRepositoryMock.Setup(_ => _.GetProductsByStoreName(storeName)).Returns(productList);            

            //act
            var response = _productsController.GetProductsByStoreName(storeName);

            //assert            
            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public void OnGetProductsByStoreName_WhenProductIsFound_ShouldReturnProductToGet()
        {
            //arrange
            var storeName = "Paulista";
            var guid = Guid.NewGuid();

            var product = new Product
            {
                ProductId = guid
            };

            var productToGet = new ProductToGet
            {
                ProductId = guid
            };

            var productList = new List<Product> { product };
            var productToGetList = new List<ProductToGet> { productToGet };

            _productRepositoryMock.Setup(_ => _.GetProductsByStoreName(storeName)).Returns(productList);
            _mapperMock.Setup(_ => _.Map<IEnumerable<ProductToGet>>(productList)).Returns(productToGetList);
            var expected = new OkObjectResult(productToGetList);

            //act
            var response = _productsController.GetProductsByStoreName(storeName);

            //assert            
            Assert.IsType<OkObjectResult>(response);
            response.Should().BeEquivalentTo(expected);
            _mockRepository.VerifyAll();
        }
    }
}
