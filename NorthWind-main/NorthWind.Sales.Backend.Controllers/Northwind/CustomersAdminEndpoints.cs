using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class CustomersAdminEndpoints
{
    public static WebApplication UseNorthWindCustomersAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw/customers").RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapPost("", Create)
             .Produces<string>(StatusCodes.Status200OK)
             .WithTags("Customers");
        group.MapPut("/{id}", Update)
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status404NotFound)
             .WithTags("Customers");
        group.MapDelete("/{id}", Delete)
             .Produces(StatusCodes.Status204NoContent)
             .WithTags("Customers");
        return app;
    }

    public record CustomerCreateDto(
        string Id,
        string Name,
        decimal CurrentBalance,
        string? CompanyName,
        string? ContactName,
        string? ContactTitle,
        string? Address,
        string? City,
        string? Country,
        string? Phone,
        string? Fax
    );
    public record CustomerUpdateDto(
        string Name,
        decimal CurrentBalance,
        string? CompanyName,
        string? ContactName,
        string? ContactTitle,
        string? Address,
        string? City,
        string? Country,
        string? Phone,
        string? Fax
    );

    private static async Task<IResult> Create([FromBody] CustomerCreateDto dto, INorthWindSalesCommandsDataContext ctx)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.Name))
            return Results.BadRequest("Id and Name are required");
        var entity = new Customer
        {
            Id = dto.Id.Trim(),
            Name = dto.Name.Trim(),
            CurrentBalance = dto.CurrentBalance,
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            ContactName = string.IsNullOrWhiteSpace(dto.ContactName) ? null : dto.ContactName.Trim(),
            ContactTitle = string.IsNullOrWhiteSpace(dto.ContactTitle) ? null : dto.ContactTitle.Trim(),
            City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim(),
            Country = string.IsNullOrWhiteSpace(dto.Country) ? null : dto.Country.Trim(),
            Fax = string.IsNullOrWhiteSpace(dto.Fax) ? null : dto.Fax.Trim(),
        };
        await ctx.AddCustomerAsync(entity);
        await ctx.SaveChangesAsync();
        return Results.Ok(entity.Id);
    }

    private static async Task<IResult> Update(string id, [FromBody] CustomerUpdateDto dto, INorthWindSalesCommandsDataContext ctx, INorthWindSalesQueriesDataContext qctx)
    {
        if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Id required");
        var cust = await qctx.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (cust is null) return Results.NotFound();
        cust.Name = dto.Name?.Trim() ?? cust.Name;
        cust.CurrentBalance = dto.CurrentBalance;
        cust.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
        cust.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        cust.ContactName = string.IsNullOrWhiteSpace(dto.ContactName) ? null : dto.ContactName.Trim();
        cust.ContactTitle = string.IsNullOrWhiteSpace(dto.ContactTitle) ? null : dto.ContactTitle.Trim();
        cust.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
        cust.Country = string.IsNullOrWhiteSpace(dto.Country) ? null : dto.Country.Trim();
        cust.Fax = string.IsNullOrWhiteSpace(dto.Fax) ? null : dto.Fax.Trim();
        await ctx.UpdateCustomerAsync(cust);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(string id, INorthWindSalesCommandsDataContext ctx)
    {
        if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Id required");
        await ctx.DeleteCustomerAsync(id);
        await ctx.SaveChangesAsync();
        return Results.NoContent();
    }
}
