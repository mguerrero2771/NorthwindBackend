using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using NorthWind.Sales.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Builder;

public static class NorthWindReadEndpoints
{
    public static WebApplication UseNorthWindReadEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw").RequireAuthorization();

        group.MapGet("/products", GetProducts)
            .Produces<ListResponse<ProductDto>>(StatusCodes.Status200OK)
            .WithTags("Products");
        group.MapGet("/products/{id:int}", GetProductById)
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Products");

        group.MapGet("/customers", GetCustomers)
            .Produces<ListResponse<CustomerDto>>(StatusCodes.Status200OK)
            .WithTags("Customers");
        group.MapGet("/customers/{id}", GetCustomerById)
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Customers");

        // Categories and Orders are mapped in dedicated endpoint classes to avoid route duplication

        return app;
    }

    // Enriquecimiento in-memory para clientes (valores clásicos Northwind)

    public record ListResponse<T>(int total, IEnumerable<T> items);
    public record PagedQuery(string? search, int page = 1, int pageSize = 20, bool? available = null, int? categoryId = null);
    public record ProductDto(int ProductID, string Name, decimal UnitPrice, short UnitsInStock, int CategoryID);
    public record CustomerDto(
        string Id,
        string Name,
        decimal CurrentBalance,
        string CompanyName,
        string ContactName,
        string ContactTitle,
        string Address,
        string City,
        string Country,
        string Phone,
        string Fax
    );
    

    private static async Task<IResult> GetProducts([AsParameters] PagedQuery q, INorthWindSalesQueriesDataContext db)
    {
        var query = db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q.search))
        {
            var s = q.search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }
        if (q.available == true)
        {
            query = query.Where(p => p.UnitsInStock > 0);
        }
        if (q.categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryID == q.categoryId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.UnitsInStock)
            .ThenBy(p => p.ProductID)
            .Skip((Math.Max(1, q.page) - 1) * Math.Max(1, q.pageSize))
            .Take(Math.Max(1, q.pageSize))
            .Select(p => new ProductDto(p.ProductID, p.Name, p.UnitPrice, p.UnitsInStock, p.CategoryID))
            .ToListAsync();
        return Results.Ok(new ListResponse<ProductDto>(total, items));
    }

    private static async Task<IResult> GetProductById(int id, INorthWindSalesQueriesDataContext db)
    {
        var prod = await db.Products
            .Where(p => p.ProductID == id)
            .Select(p => new ProductDto(p.ProductID, p.Name, p.UnitPrice, p.UnitsInStock, p.CategoryID))
            .FirstOrDefaultAsync();
        return prod is null ? Results.NotFound() : Results.Ok(prod);
    }

    

    private static async Task<IResult> GetCustomers([AsParameters] PagedQuery q, INorthWindSalesQueriesDataContext db)
    {
        var query = db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q.search))
        {
            var s = q.search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s) || c.Id.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Id)
            .Skip((System.Math.Max(1, q.page) - 1) * System.Math.Max(1, q.pageSize))
            .Take(System.Math.Max(1, q.pageSize))
            .Select(c => new CustomerDto(
                c.Id,
                c.Name,
                0m,
                c.Name,
                c.ContactName ?? string.Empty,
                c.ContactTitle ?? string.Empty,
                c.Address ?? string.Empty,
                c.City ?? string.Empty,
                c.Country ?? string.Empty,
                c.Phone ?? string.Empty,
                c.Fax ?? string.Empty
            ))
            .ToListAsync();

        return Results.Ok(new ListResponse<CustomerDto>(total, items));
    }

    private static async Task<IResult> GetCustomerById(string id, INorthWindSalesQueriesDataContext db)
    {
        var cust = await db.Customers
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto(
                c.Id,
                c.Name,
                0m,
                c.Name,
                c.ContactName ?? string.Empty,
                c.ContactTitle ?? string.Empty,
                c.Address ?? string.Empty,
                c.City ?? string.Empty,
                c.Country ?? string.Empty,
                c.Phone ?? string.Empty,
                c.Fax ?? string.Empty
            ))
            .FirstOrDefaultAsync();

        return cust is null ? Results.NotFound() : Results.Ok(cust);
    }

    
}
