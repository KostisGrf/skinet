

using API.DTOs;
using API.Extensions;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


[Authorize]
public class OrdersController(ICartService cartService, IUniteOfWork unit) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto orderDto)
    {
        var email = User.GetEmail();

        var cart = await cartService.GetCartAsync(orderDto.cartId);

        if (cart == null) return BadRequest("Cart not found");

        if (cart.PaymentIntentId == null) return BadRequest("No payment intent for this order");

        var items = new List<OrderItem>();

        foreach (var item in cart.Items)
        {
            var productItem = await unit.Repository<Product>().GetByIdAsync(item.ProductId);

            if (productItem == null) return BadRequest("Problem with the order");

            var itemOrdered = new ProductItemOrdered
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                PictureUrl = item.PictureUrl
            };

            var OrderItem = new OrderItem
            {
                ItemOrdered = itemOrdered,
                Price = productItem.Price,
                Quantity = item.Quantity
            };
            items.Add(OrderItem);
        }

        var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync(orderDto.DeliveryMethodId);

        if (deliveryMethod == null) return BadRequest("No delivery method selected");

        var order = new Order
        {
            OrderItems = items,
            DeliveryMethod = deliveryMethod,
            ShippingAddress = orderDto.ShippingAddress,
            Subtotal = items.Sum(x => x.Price * x.Quantity),
            PaymentSummary = orderDto.PaymentSummary,
            PaymentIntentId = cart.PaymentIntentId,
            buyerEmail = email
        };

        unit.Repository<Order>().Add(order);

        if (await unit.Complete())
        {
            return order;
        }

        return BadRequest("problem creating order");
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrdersForUser()
    {
        var spec = new OrderdSpecification(User.GetEmail());

        var orders = await unit.Repository<Order>().ListAsync(spec);

        var ordersToReturn = orders.Select(x => x.ToDto()).ToList();

        return Ok(ordersToReturn);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrderForUser(int id)
    {
        var spec = new OrderdSpecification(User.GetEmail(), id);

        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null) return NotFound();

        return order.ToDto();
    }
}
