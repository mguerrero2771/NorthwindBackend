using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Repositories.Interfaces;

namespace Microsoft.AspNetCore.Builder;

public static class DashboardEndpoints
{
    public static WebApplication UseNorthWindDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/nw").RequireAuthorization();
        group.MapGet("/dashboard", GetDashboard);
        return app;
    }

    public record DashboardCardsDto(int products, int orders, int customers, decimal sales);
    public record DashboardRecentOrderDto(int id, string customerId, DateTime date);
    public record DashboardResponseDto(DashboardCardsDto cards, IEnumerable<DashboardRecentOrderDto> recentOrders);

    private static async Task<IResult> GetDashboard(INorthWindSalesQueriesDataContext db)
    {
        var totalProducts = await db.Products.CountAsync();
        var totalCustomers = await db.Customers.CountAsync();
        var totalOrders = await db.Orders.CountAsync();
        // Ventas totales: suma de UnitPrice * Quantity de todos los detalles
        decimal totalSales = 0m;
        totalSales = await db.OrderDetails.SumAsync(od => (decimal)od.UnitPrice * (decimal)od.Quantity);

        var recentOrders = await db.Orders
            .OrderByDescending(o => o.Id)
            .Take(5)
            .Select(o => new DashboardRecentOrderDto(o.Id, o.CustomerId, o.OrderDate))
            .ToListAsync();

        var cards = new DashboardCardsDto(products: totalProducts, orders: totalOrders, customers: totalCustomers, sales: totalSales);
        var payload = new DashboardResponseDto(cards, recentOrders);
        return Results.Ok(payload);
    }
}
