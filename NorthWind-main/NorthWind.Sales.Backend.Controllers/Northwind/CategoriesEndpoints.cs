using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class CategoriesEndpoints
{
    public static WebApplication UseNorthWindCategoriesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw/categories").RequireAuthorization();

        group.MapGet("", List);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("", Create).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapPut("/{id:int}", Update).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapDelete("/{id:int}", Delete).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        return app;
    }

    private record PagedQuery(string? q, int page = 1, int pageSize = 20);
    private record CategoryDto(int CategoryID, string CategoryName, string? Description, byte[]? Picture);

    private static async Task<IResult> List([AsParameters] PagedQuery query, [FromServices] INorthWindSalesQueriesDataContext db)
    {
        var q = (query.q ?? string.Empty).Trim().ToLowerInvariant();
        var categories = db.Categories.AsQueryable();
        if (!string.IsNullOrEmpty(q))
            categories = categories.Where(c => c.CategoryName.ToLower().Contains(q));
        var total = await categories.CountAsync();
        var items = await categories
            .OrderBy(c => c.CategoryID)
            .Skip((Math.Max(1, query.page) - 1) * Math.Max(1, query.pageSize))
            .Take(Math.Max(1, query.pageSize))
            .Select(c => new CategoryDto(c.CategoryID, c.CategoryName, c.Description, c.Picture))
            .ToListAsync();
        return Results.Ok(new { total, items });
    }

    private static async Task<IResult> GetById(int id, [FromServices] INorthWindSalesQueriesDataContext db)
    {
        var category = await db.Categories
            .Where(c => c.CategoryID == id)
            .Select(c => new CategoryDto(c.CategoryID, c.CategoryName, c.Description, c.Picture))
            .FirstOrDefaultAsync();
        return category is null ? Results.NotFound() : Results.Ok(category);
    }

    private static async Task<IResult> Create([FromBody] Category dto, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        await ctx.AddCategoryAsync(dto);
        await ctx.SaveChangesAsync();
        return Results.Created($"/nw/categories/{dto.CategoryID}", dto);
    }

    private static async Task<IResult> Update(int id, [FromBody] Category dto, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        if (dto.CategoryID == 0) dto.CategoryID = id;
        if (dto.CategoryID != id) return Results.BadRequest("Mismatched id");
        await ctx.UpdateCategoryAsync(dto);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(int id, [FromServices] INorthWindSalesCommandsDataContext ctx)
    {
        await ctx.DeleteCategoryAsync(id);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }
}
