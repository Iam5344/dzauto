using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.DTOs;
using ProductManagement.Models;
using Xunit;

namespace ProductManagement.Tests
{
    public class ProductControllerTests
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductReadDto>();
                cfg.CreateMap<ProductCreateDto, Product>();
                cfg.CreateMap<ProductUpdateDto, Product>();
            });
            _mapper = config.CreateMapper();

            _controller = new ProductController(_context, _mapper);
        }

        [Fact]
        public async Task GetProducts_ReturnsOkWithAllProducts()
        {
            _context.Products.AddRange(
                new Product { Id = 1, Name = "Item 1", Price = 10m },
                new Product { Id = 2, Name = "Item 2", Price = 20m }
            );
            await _context.SaveChangesAsync();

            var result = await _controller.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<ProductReadDto>>(okResult.Value);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetProductById_ReturnsNotFound_WhenMissing()
        {
            var result = await _controller.GetProductById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetProductById_ReturnsOk_WhenExists()
        {
            _context.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 1200m });
            await _context.SaveChangesAsync();

            var result = await _controller.GetProductById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var product = Assert.IsType<ProductReadDto>(okResult.Value);
            Assert.Equal("Laptop", product.Name);
        }

        [Fact]
        public async Task CreateProduct_ReturnsCreated_WithSavedProduct()
        {
            var dto = new ProductCreateDto
            {
                Name = "Keyboard",
                Price = 75m,
                ImageUrls = new List<string> { "https://example.com/1.png" }
            };

            var result = await _controller.CreateProduct(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var created = Assert.IsType<ProductReadDto>(createdResult.Value);
            Assert.Equal("Keyboard", created.Name);
            Assert.Equal(1, await _context.Products.CountAsync());
        }

        [Fact]
        public async Task UpdateProduct_ReturnsNoContent_WhenProductExists()
        {
            _context.Products.Add(new Product { Id = 1, Name = "Old Name", Price = 50m });
            await _context.SaveChangesAsync();

            var updateDto = new ProductUpdateDto
            {
                Name = "New Name",
                Price = 65m
            };

            var result = await _controller.UpdateProduct(1, updateDto);

            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Products.FindAsync(1);
            Assert.Equal("New Name", updated!.Name);
            Assert.Equal(65m, updated.Price);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductExists()
        {
            _context.Products.Add(new Product { Id = 1, Name = "To Delete", Price = 30m });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteProduct(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(await _context.Products.ToListAsync());
        }
    }
}
