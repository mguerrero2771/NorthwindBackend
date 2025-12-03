using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class ProductsReadEndpoints
{
    public static WebApplication UseNorthWindProductsReadEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw").RequireAuthorization();
        group.MapGet("/products", GetProducts);
        group.MapGet("/products/{id:int}", GetProductById);
        return app;
    }

    public record PagedQuery(string? search, int page = 1, int pageSize = 20, bool? available = null);
    public record ListResponse<T>(int total, IEnumerable<T> items);
    public record ProductDto(
        int ProductID,
        string Name,
        decimal UnitPrice,
        short UnitsInStock,
        int CategoryID,
        int SupplierID,
        string? QuantityPerUnit,
        short UnitsOnOrder,
        short ReorderLevel,
        bool Discontinued
    );

    private static async Task<IResult> GetProducts([AsParameters] PagedQuery q, INorthWindSalesQueriesDataContext db)
    {
        var query = db.Products.OrderBy(p => p.ProductID).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q.search))
        {
            var s = q.search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }
        if (q.available == true)
        {
            query = query.Where(p => p.UnitsInStock > 0);
        }
        var total = await query.CountAsync();
        var items = await query
            .Skip((Math.Max(1, q.page) - 1) * Math.Max(1, q.pageSize))
            .Take(Math.Max(1, q.pageSize))
            .Select(p => new ProductDto(
                p.ProductID,
                p.Name,
                p.UnitPrice,
                p.UnitsInStock,
                p.CategoryID,
                p.SupplierID,
                p.QuantityPerUnit,
                p.UnitsOnOrder,
                p.ReorderLevel,
                p.Discontinued
            ))
            .ToListAsync();
        return Results.Ok(new ListResponse<ProductDto>(total, items));
    }

    private static async Task<IResult> GetProductById(int id, INorthWindSalesQueriesDataContext db)
    {
        var prod = await db.Products
            .Where(p => p.ProductID == id)
            .Select(p => new ProductDto(
                p.ProductID,
                p.Name,
                p.UnitPrice,
                p.UnitsInStock,
                p.CategoryID,
                p.SupplierID,
                p.QuantityPerUnit,
                p.UnitsOnOrder,
                p.ReorderLevel,
                p.Discontinued
            ))
            .FirstOrDefaultAsync();
        return prod is null ? Results.NotFound() : Results.Ok(prod);
    }
}
