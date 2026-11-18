using System.Security.Claims;
using ECommerce.Api.Entities;
using ECommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class CheckoutController(ICheckoutService checkoutService, IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResult>> Checkout([FromBody] CheckoutRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.PaymentMethod == PaymentMethod.MockCard && !environment.IsDevelopment())
        {
            ModelState.AddModelError(nameof(CheckoutRequest.PaymentMethod), "Mock card payments are only available in Development.");
            return ValidationProblem(ModelState);
        }

        if (request.PaymentMethod == PaymentMethod.MockCard && request.MockCardSucceeded is null)
        {
            ModelState.AddModelError(nameof(CheckoutRequest.MockCardSucceeded), "Choose whether the mock card payment succeeds.");
            return ValidationProblem(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await checkoutService.CheckoutAsync(userId, request);
            return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
        }
        catch (ArgumentException ex)
        {
            return Problem(title: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyCollection<OrderDetailsResponse>>> GetOrders()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var orders = await checkoutService.GetOrdersForUserAsync(userId);
        return Ok(orders);
    }

    [HttpGet("orders/{id:int}")]
    public async Task<ActionResult<OrderDetailsResponse>> GetOrder(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var order = await checkoutService.GetOrderForUserAsync(userId, id);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}
