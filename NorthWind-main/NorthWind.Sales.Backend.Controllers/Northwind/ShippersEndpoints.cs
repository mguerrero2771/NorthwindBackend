using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class ShippersEndpoints
{
    public static WebApplication UseNorthWindShippersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw/shippers").RequireAuthorization();
        group.MapGet("", List);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("", Create).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapPut("/{id:int}", Update).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapDelete("/{id:int}", Delete).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        return app;
    }

    private record PagedQuery(string? q, int page = 1, int pageSize = 20);

    private static async Task<IResult> List([AsParameters] PagedQuery query, [FromServices] INorthWindSalesQueriesDataContext db)
    {
        var q = (query.q ?? string.Empty).Trim().ToLowerInvariant();
        var itemsQ = db.Shippers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) itemsQ = itemsQ.Where(s => s.CompanyName.ToLower().Contains(q));
        var total = await itemsQ.CountAsync();
        var items = await itemsQ.OrderBy(s => s.ShipperID)
                                .Skip((Math.Max(1, query.page) - 1) * Math.Max(1, query.pageSize))
                                .Take(Math.Max(1, query.pageSize))
                                .ToListAsync();
        return Results.Ok(new { total, items });
    }

    private static async Task<IResult> GetById(int id, [FromServices] INorthWindSalesQueriesDataContext db)
        => (await db.Shippers.FirstOrDefaultAsync(s => s.ShipperID == id)) is Shipper s
            ? Results.Ok(s) : Results.NotFound();

    private static async Task<IResult> Create([FromBody] Shipper dto, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        await ctx.AddShipperAsync(dto);
        await ctx.SaveChangesAsync();
        return Results.Created($"/nw/shippers/{dto.ShipperID}", dto);
    }

    private static async Task<IResult> Update(int id, [FromBody] Shipper dto, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        if (dto.ShipperID == 0) dto.ShipperID = id;
        if (dto.ShipperID != id) return Results.BadRequest("Mismatched id");
        await ctx.UpdateShipperAsync(dto);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(int id, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        await ctx.DeleteShipperAsync(id);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }
}
