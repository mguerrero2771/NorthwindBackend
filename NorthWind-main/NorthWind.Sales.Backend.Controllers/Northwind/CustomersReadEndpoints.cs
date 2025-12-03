using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class CustomersReadEndpoints
{
    public static WebApplication UseNorthWindCustomersReadEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw").RequireAuthorization();
        group.MapGet("/customers", GetCustomers);
        group.MapGet("/customers/{id}", GetCustomerById);
        return app;
    }

    public record PagedQuery(string? search, int page = 1, int pageSize = 20);
    public record ListResponse<T>(int total, IEnumerable<T> items);
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
                c.Name, // CompanyName (Name ya viene de CompanyName)
                EF.Property<string>(c, "ContactName") ?? string.Empty,
                EF.Property<string>(c, "ContactTitle") ?? string.Empty,
                EF.Property<string>(c, "Address") ?? string.Empty,
                EF.Property<string>(c, "City") ?? string.Empty,
                EF.Property<string>(c, "Country") ?? string.Empty,
                EF.Property<string>(c, "Phone") ?? string.Empty,
                EF.Property<string>(c, "Fax") ?? string.Empty
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
                EF.Property<string>(c, "ContactName") ?? string.Empty,
                EF.Property<string>(c, "ContactTitle") ?? string.Empty,
                EF.Property<string>(c, "Address") ?? string.Empty,
                EF.Property<string>(c, "City") ?? string.Empty,
                EF.Property<string>(c, "Country") ?? string.Empty,
                EF.Property<string>(c, "Phone") ?? string.Empty,
                EF.Property<string>(c, "Fax") ?? string.Empty
            ))
            .FirstOrDefaultAsync();
        if (cust is null) return Results.NotFound();
        return Results.Ok(cust);
    }
}
