using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class SuppliersEndpoints
{
    public static WebApplication UseNorthWindSuppliersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw/suppliers").RequireAuthorization();
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
        var itemsQ = db.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) itemsQ = itemsQ.Where(s => s.CompanyName.ToLower().Contains(q));
        var total = await itemsQ.CountAsync();
        var items = await itemsQ.OrderBy(s => s.SupplierID)
                                .Skip((Math.Max(1, query.page) - 1) * Math.Max(1, query.pageSize))
                                .Take(Math.Max(1, query.pageSize))
                                .ToListAsync();
        return Results.Ok(new { total, items });
    }

    private static async Task<IResult> GetById(int id, [FromServices] INorthWindSalesQueriesDataContext db)
        => (await db.Suppliers.FirstOrDefaultAsync(s => s.SupplierID == id)) is Supplier s
            ? Results.Ok(s) : Results.NotFound();

    private static async Task<IResult> Create([FromBody] Supplier dto, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        await ctx.AddSupplierAsync(dto);
        await ctx.SaveChangesAsync();
        return Results.Created($"/nw/suppliers/{dto.SupplierID}", dto);
    }

    private static async Task<IResult> Update(int id, [FromBody] Supplier dto, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        if (dto.SupplierID == 0) dto.SupplierID = id;
        if (dto.SupplierID != id) return Results.BadRequest("Mismatched id");
        await ctx.UpdateSupplierAsync(dto);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(int id, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        await ctx.DeleteSupplierAsync(id);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }
}
