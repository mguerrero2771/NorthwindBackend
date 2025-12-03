
using System.Diagnostics;

namespace NorthWind.Sales.Backend.Repositories.Repositories;

internal class CommandsRepository(INorthWindSalesCommandsDataContext context) : ICommandsRepository
{
    public async Task CreateOrder(OrderAggregate order)
    {
        var sw = Stopwatch.StartNew();

        // 1) Guardar primero la orden para obtener OrderID (identidad)
        await context.AddOrderAsync(order);
        await context.SaveChangesAsync();

        // 2) Insertar detalles usando el OrderId obtenido (evita problema de principal desconocido)
        await context.AddOrderDetailsAsync(
            order.OrderDetails
            .Select(d => new Entities.OrderDetail
            {
                OrderId = order.Id,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice
            }).ToArray());

        sw.Stop();
        Console.WriteLine($"🕒 Tiempo CreateOrder en CommandsRepository: {sw.ElapsedMilliseconds} ms");
    }

    public async Task SaveChanges()
    {
        var sw = Stopwatch.StartNew();

        await context.SaveChangesAsync();

        sw.Stop();
        Console.WriteLine($"🕒 Tiempo SaveChanges en CommandsRepository: {sw.ElapsedMilliseconds} ms");
    }
}


