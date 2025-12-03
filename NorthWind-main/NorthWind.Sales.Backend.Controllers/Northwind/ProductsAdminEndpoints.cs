using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class ProductsAdminEndpoints
{
    public static WebApplication UseNorthWindProductsAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw/products").RequireAuthorization();

        group.MapPost("", Create)
             .Produces<int>(StatusCodes.Status200OK)
             .WithTags("Products");

        group.MapPut("/{id:int}", Update)
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status404NotFound)
             .WithTags("Products");

        group.MapDelete("/{id:int}", Delete)
             .Produces(StatusCodes.Status204NoContent)
             .WithTags("Products");

        return app;
    }

    public record CreateProductDto(
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
    public record UpdateProductDto(
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

    private static async Task<IResult> Create([FromBody] CreateProductDto dto,
        INorthWindSalesCommandsDataContext commands)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest("Name is required");
            if (dto.UnitPrice < 0) return Results.BadRequest("UnitPrice must be >= 0");
            if (dto.UnitsInStock < 0) return Results.BadRequest("UnitsInStock must be >= 0");
            if (dto.CategoryID <= 0) return Results.BadRequest("CategoryID is required");
            if (dto.SupplierID <= 0) return Results.BadRequest("SupplierID is required");
            if (dto.UnitsOnOrder < 0) return Results.BadRequest("UnitsOnOrder must be >= 0");
            if (dto.ReorderLevel < 0) return Results.BadRequest("ReorderLevel must be >= 0");

            var entity = new Product
            {
                Name = dto.Name.Trim(),
                UnitPrice = dto.UnitPrice,
                UnitsInStock = dto.UnitsInStock,
                CategoryID = dto.CategoryID,
                SupplierID = dto.SupplierID,
                QuantityPerUnit = string.IsNullOrWhiteSpace(dto.QuantityPerUnit) ? null : dto.QuantityPerUnit.Trim(),
                UnitsOnOrder = dto.UnitsOnOrder,
                ReorderLevel = dto.ReorderLevel,
                Discontinued = dto.Discontinued
            };
            await commands.AddProductAsync(entity);
            await commands.SaveChangesAsync();
            return Results.Ok(entity.ProductID);
        }
        catch (Exception ex)
        {
            return Results.Problem(title: "Error al crear el producto.", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError, instance: "ProblemDetails/CreateProduct");
        }
    }

    private static async Task<IResult> Update(int id, [FromBody] UpdateProductDto dto,
        INorthWindSalesCommandsDataContext commands,
        INorthWindSalesQueriesDataContext queries)
    {
        try
        {
            var prod = await queries.Products.FirstOrDefaultAsync(p => p.ProductID == id);
            if (prod is null) return Results.NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name)) prod.Name = dto.Name.Trim();
            prod.UnitPrice = dto.UnitPrice;
            prod.UnitsInStock = dto.UnitsInStock;
            if (dto.CategoryID > 0) prod.CategoryID = dto.CategoryID;
            if (dto.SupplierID > 0) prod.SupplierID = dto.SupplierID;
            prod.QuantityPerUnit = string.IsNullOrWhiteSpace(dto.QuantityPerUnit) ? null : dto.QuantityPerUnit.Trim();
            if (dto.UnitsOnOrder >= 0) prod.UnitsOnOrder = dto.UnitsOnOrder;
            if (dto.ReorderLevel >= 0) prod.ReorderLevel = dto.ReorderLevel;
            prod.Discontinued = dto.Discontinued;

            await commands.UpdateProductAsync(prod);
            await commands.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(title: "Error al actualizar el producto.", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError, instance: "ProblemDetails/UpdateProduct");
        }
    }

    private static async Task<IResult> Delete(int id, INorthWindSalesCommandsDataContext commands, INorthWindSalesQueriesDataContext queries)
    {
        // Prevenir conflicto de FK: si existen detalles que referencian el producto, devolver 409 con mensaje claro
        var hasRefs = await queries.OrderDetails.AnyAsync(d => d.ProductId == id);
        if (hasRefs)
        {
            return Results.Conflict(new ProblemDetails
            {
                Title = "No se puede eliminar el producto.",
                Detail = "El producto está siendo utilizado en órdenes. Elimine o modifique esas órdenes antes de eliminar el producto.",
                Status = StatusCodes.Status409Conflict,
                Instance = "ProblemDetails/ProductInUse"
            });
        }
        try
        {
            await commands.DeleteProductAsync(id);
            await commands.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (DbUpdateException dbex)
        {
            // Fallback si por alguna razón no detectamos antes
            return Results.Conflict(new ProblemDetails
            {
                Title = "No se puede eliminar el producto.",
                Detail = dbex.InnerException?.Message ?? dbex.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = "ProblemDetails/ProductInUseDbUpdate"
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(title: "Error al eliminar el producto.", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError, instance: "ProblemDetails/DeleteProduct");
        }
    }
}
