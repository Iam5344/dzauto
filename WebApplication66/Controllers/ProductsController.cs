using Microsoft.AspNetCore.Mvc;
using WebApplication66.Models;

namespace WebApplication66.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private static readonly List<Product> Products = new List<Product>
        {
            new Product { Id = 1, Name = "Ноутбук", Price = 25000m },
            new Product { Id = 2, Name = "Смартфон", Price = 15000m },
            new Product { Id = 3, Name = "Навушники", Price = 2000m }
        };

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            return Ok(Products);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetProductById(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            return Ok(product);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult SearchProducts([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Параметр 'name' є обов'язковим.");
            }

            var results = Products
                .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(results);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult CreateProduct([FromBody] ProductDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Поле 'Name' є обов'язковим.");
            }

            int newId = Products.Count > 0 ? Products.Max(p => p.Id) + 1 : 1;

            var newProduct = new Product
            {
                Id = newId,
                Name = dto.Name,
                Price = dto.Price
            };

            Products.Add(newProduct);

            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult UpdateProduct(int id, [FromBody] ProductDto dto)
        {
            var existingProduct = Products.FirstOrDefault(p => p.Id == id);
            if (existingProduct == null)
            {
                return NotFound("Product not found");
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Поле 'Name' є обов'язковим.");
            }

            existingProduct.Name = dto.Name;
            existingProduct.Price = dto.Price;

            return Ok(existingProduct);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult DeleteProduct(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            Products.Remove(product);

            return NoContent();
        }
    }
}
