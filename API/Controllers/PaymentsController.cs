using System;
using API.Extensions;
using API.SignalR;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;


namespace API.Controllers;

public class PaymentsController(IPaymentService service,
     IUniteOfWork unit, ILogger<PaymentsController> logger,
     IConfiguration config,IHubContext<NotificationHub> hubContext) : BaseApiController
{


    private readonly string _whSecret = config["StripeSettings:whSecret"]!;
    

    [Authorize]
    [HttpPost("{cartId}")]
    public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
    {
        var cart = await service.CreateOrUpdatePaymentIntent(cartId);

        if (cart == null) return BadRequest("Problem with your cart");

        return Ok(cart);
    }

    [HttpGet("testlog")]
public IActionResult TestLog()
{
    logger.LogInformation("🚀 Log stream is working!");
    return Ok("Logged to Azure");
}

    [HttpGet("delivery-methods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> getDeliveryMethods()
    {
        return Ok(await unit.Repository<DeliveryMethod>().ListAllAsync());
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = ConstructStripeEvent(json);

            if (stripeEvent.Data.Object is not PaymentIntent intent)
            {
                return BadRequest("Invalid event data");
            }

            await HandlePaymentSucceded(intent);

            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook error");
            return StatusCode(StatusCodes.Status500InternalServerError, "Webhook error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occured");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occured");
        }

        
    }

    private async Task HandlePaymentSucceded(PaymentIntent intent)
    {
        
        if (intent.Status == "succeeded")
        {
            var spec = new OrderdSpecification(intent.Id, true);

            var order = await unit.Repository<Core.Entities.OrderAggregate.Order>().GetEntityWithSpec(spec)
                ?? throw new Exception("Order not found");

            logger.LogInformation("Resolved connectionId: for email: {Email}", order.buyerEmail);

            if ((long)order.GetTotal() * 100 != intent.Amount)
            {
                order.Status = OrderStatus.PaymentMisMatch;
            }
            else
            {
                order.Status = OrderStatus.PaymentReceived;
            }

            await unit.Complete();

            logger.LogInformation($"database updated");

            var connectionId = NotificationHub.GetConnectionIdByEmail(order.buyerEmail);

            if (!string.IsNullOrEmpty(connectionId))
            {
                logger.LogInformation($"Sending OrderCompleteNotification to connectionId: {connectionId}");
                await hubContext.Clients.Client(connectionId).SendAsync("OrderCompleteNotification", order.ToDto());
            }
        }
    }

    private Event ConstructStripeEvent(string json)
    {
        try
        {
            return EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _whSecret,
                throwOnApiVersionMismatch: false
);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to construct stripe event");
            throw new StripeException("Invalid signature");
        }
    }
}
