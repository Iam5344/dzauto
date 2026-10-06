using Microsoft.AspNetCore.Mvc;
using WebApplication66.Models;

namespace WebApplication66.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private static readonly List<Order> Orders = new List<Order>
        {
            new Order { Id = 1, Number = "ORD-1001", Total = 1500.50m, CreatedAt = DateTime.Now.AddDays(-3) },
            new Order { Id = 2, Number = "ORD-1002", Total = 3200.00m, CreatedAt = DateTime.Now.AddDays(-2) },
            new Order { Id = 3, Number = "ORD-1003", Total = 450.75m, CreatedAt = DateTime.Now.AddDays(-1) }
        };

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
        public IActionResult GetOrders()
        {
            return Ok(Orders);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetOrderById(int id)
        {
            var order = Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult SearchOrders([FromQuery] string number, [FromQuery] decimal? minTotal)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return BadRequest("Параметр 'number' є обов'язковим.");
            }

            var query = Orders.Where(o => o.Number.Contains(number, StringComparison.OrdinalIgnoreCase));

            if (minTotal.HasValue)
            {
                query = query.Where(o => o.Total >= minTotal.Value);
            }

            return Ok(query.ToList());
        }

        [HttpPost]
        [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult CreateOrder([FromBody] OrderDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Number) || dto.Total <= 0)
            {
                return BadRequest("Поля 'Number' та 'Total' (більше 0) є обов'язковими.");
            }

            int newId = Orders.Count > 0 ? Orders.Max(o => o.Id) + 1 : 1;

            var newOrder = new Order
            {
                Id = newId,
                Number = dto.Number,
                Total = dto.Total,
                CreatedAt = DateTime.Now
            };

            Orders.Add(newOrder);

            return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult UpdateOrder(int id, [FromBody] OrderDto dto)
        {
            var existingOrder = Orders.FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
            {
                return NotFound("Order not found");
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.Number) || dto.Total <= 0)
            {
                return BadRequest("Поля 'Number' та 'Total' (більше 0) є обов'язковими.");
            }

            existingOrder.Number = dto.Number;
            existingOrder.Total = dto.Total;

            return Ok(existingOrder);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult DeleteOrder(int id)
        {
            var order = Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound("Order not found");
            }

            Orders.Remove(order);

            return NoContent();
        }
    }
}
