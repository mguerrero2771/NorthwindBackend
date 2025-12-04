using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class OrdersEndpoints
{
    public static WebApplication UseNorthWindOrdersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw/orders").RequireAuthorization();
        group.MapGet("", List);
        group.MapGet("/{id:int}", GetById);
        group.MapGet("/{id:int}/details", GetDetails);
        group.MapGet("/next-id", GetNextId);
        group.MapPut("/{id:int}", Update).RequireAuthorization();
        group.MapDelete("/{id:int}", Delete).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        return app;
    }

    public record PagedQuery(string? customerId, int? productId = null, int page = 1, int pageSize = 20);
    public record CreateOrderDto(string CustomerId, IEnumerable<CreateOrderDetailDto> Details, DateTime? OrderDate, string? ShipAddress, string? ShipCity, string? ShipCountry, string? ShipPostalCode);
    public record CreateOrderDetailDto(int ProductId, short Quantity, decimal UnitPrice);
    public record OrderDto(int Id, string? CustomerId, DateTime? OrderDate, string? ShipAddress, string? ShipCity, string? ShipCountry, string? ShipPostalCode);
    public record OrderDetailDto(int OrderId, int ProductId, decimal UnitPrice, short Quantity);
    public record ListResponse<T>(int total, IEnumerable<T> items);
    public record OrderUpdateDto(string CustomerId, DateTime? OrderDate, string? ShipAddress, string? ShipCity, string? ShipCountry, string? ShipPostalCode);

    private static async Task<IResult> List([AsParameters] PagedQuery query, INorthWindSalesQueriesDataContext db)
    {
        var q = db.Orders.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.customerId))
            q = q.Where(o => o.CustomerId == query.customerId);
        if (query.productId.HasValue)
        {
            var pid = query.productId.Value;
            q = q.Where(o => db.OrderDetails.Any(od => od.OrderId == o.Id && od.ProductId == pid));
        }
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(o => o.Id)
                           .Skip((Math.Max(1, query.page) - 1) * Math.Max(1, query.pageSize))
                           .Take(Math.Max(1, query.pageSize))
                           .Select(o => new OrderDto(
                               o.Id,
                               o.CustomerId ?? string.Empty,
                               o.OrderDate,
                               o.ShipAddress ?? string.Empty,
                               o.ShipCity ?? string.Empty,
                               o.ShipCountry ?? string.Empty,
                               o.ShipPostalCode ?? string.Empty
                           ))
                           .ToListAsync();
        return Results.Ok(new ListResponse<OrderDto>(total, items));
    }

    private static async Task<IResult> GetById(int id, INorthWindSalesQueriesDataContext db)
    {
        var o = await db.Orders
            .Where(o => o.Id == id)
            .Select(o => new OrderDto(
                o.Id,
                o.CustomerId ?? string.Empty,
                o.OrderDate,
                o.ShipAddress ?? string.Empty,
                o.ShipCity ?? string.Empty,
                o.ShipCountry ?? string.Empty,
                o.ShipPostalCode ?? string.Empty
            ))
            .FirstOrDefaultAsync();
        return o is null ? Results.NotFound() : Results.Ok(o);
    }

    private static async Task<IResult> GetDetails(int id, INorthWindSalesQueriesDataContext db)
    {
        var d = await db.OrderDetails
            .Where(od => od.OrderId == id)
            .Select(od => new OrderDetailDto(od.OrderId, od.ProductId, od.UnitPrice, od.Quantity))
            .ToListAsync();
        return Results.Ok(d);
    }

    private static async Task<IResult> GetNextId(INorthWindSalesQueriesDataContext db)
    {
        var hasOrders = await db.Orders.AnyAsync();
        var nextId = hasOrders
            ? await db.Orders.MaxAsync(o => o.Id) + 1
            : 1;
        return Results.Ok(nextId);
    }


    private static async Task<IResult> Update(int id, [FromBody] OrderUpdateDto dto, INorthWindSalesCommandsDataContext ctx, INorthWindSalesQueriesDataContext qctx)
    {
        if (dto is null) return Results.BadRequest("Body is required");
        var order = new NorthWind.Sales.Backend.BusinessObjects.POCOEntities.Order
        {
            Id = id,
            CustomerId = dto.CustomerId,
            OrderDate = dto.OrderDate ?? DateTime.UtcNow,
            ShipAddress = dto.ShipAddress,
            ShipCity = dto.ShipCity,
            ShipCountry = dto.ShipCountry,
            ShipPostalCode = dto.ShipPostalCode
        };
        await ctx.UpdateOrderAsync(order);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(int id, INorthWindSalesCommandsDataContext ctx)
    {
        await ctx.RemoveOrderDetailsAsync(id);
        await ctx.DeleteOrderAsync(id);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }
}
