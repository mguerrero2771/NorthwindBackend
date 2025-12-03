using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NorthWind.Entities.Interfaces;
using NorthWind.Sales.Backend.DataContexts.EFCore.Guards;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Services;

internal partial class NorthWindSalesCommandsDataContext(IOptions<DBOptions> dbOptions, IUserService userService)
    : NorthWindSalesContext(dbOptions), INorthWindSalesCommandsDataContext
{
    //  Agrega un objeto "Order" al contexto para ser persistido en la BD.
    public async Task AddOrderAsync(Order order)
    {
        // Validar existencia de Customer antes de insertar la Orden (evita violación de FK con mensaje críptico)
        var customer = await Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId);
        if (customer is null)
        {
            Console.WriteLine($"Cliente inexistente CustomerID={order.CustomerId}");
            throw new UpdateException(new KeyNotFoundException($"CustomerID {order.CustomerId} not found"), new[] { nameof(Customer) });
        }
        // Asignar automáticamente el EmployeeID de la orden usando la información del usuario autenticado
        var entry = Entry(order);
        if (userService?.EmployeeId is int employeeIdValue)
        {
            entry.Property<int?>("EmployeeID").CurrentValue = employeeIdValue;
        }
        await AddAsync(order);
    }

    //  Agrega múltiples "OrderDetail" al contexto.
    public async Task AddOrderDetailsAsync(
        IEnumerable<Repositories.Entities.OrderDetail> orderDetails)
    {
        // Asegurar Discount = 0 si la BD lo requiere NOT NULL (compatibilidad esquema clásico)
        foreach (var d in orderDetails)
        {
            // Validar existencia de producto y completar UnitPrice si no viene
            var product = await Products.FirstOrDefaultAsync(p => p.ProductID == d.ProductId);
            if (product is null)
            {
                Console.WriteLine($"Producto inexistente ProductID={d.ProductId}");
                throw new UpdateException(new KeyNotFoundException($"ProductID {d.ProductId} not found"), new[] { nameof(Product) });
            }
            if (d.UnitPrice <= 0)
            {
                d.UnitPrice = product.UnitPrice;
            }
            // Disminuir existencias
            if (product.UnitsInStock < d.Quantity)
            {
                throw new UpdateException(new InvalidOperationException($"Insufficient stock for ProductID {d.ProductId}. Required {d.Quantity}, InStock {product.UnitsInStock}"), new[] { nameof(Product) });
            }
            product.UnitsInStock = (short)(product.UnitsInStock - d.Quantity);
            var entry = Entry(d);
            if (entry.Property<float?>("Discount").CurrentValue is null)
            {
                entry.Property<float?>("Discount").CurrentValue = 0f;
            }
        }
        await AddRangeAsync(orderDetails);
    }

    //  Persiste todos los cambios en la base de datos en una sola transacción (unidad de trabajo).
    //public async Task SaveChangesAsync() => await base.SaveChangesAsync();
    public async Task SaveChangesAsync() => await GuardDBContext.AgainstSaveChangesErrorAsync(this);
}

// Category operations
partial class NorthWindSalesCommandsDataContext
{
    public async Task AddCategoryAsync(Repositories.Entities.Category category)
    {
        await AddAsync(category);
    }

    public async Task UpdateCategoryAsync(Repositories.Entities.Category category)
    {
        Entry(category).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        var cat = await Categories.FirstOrDefaultAsync(c => c.CategoryID == categoryId);
        if (cat is not null)
        {
            Remove(cat);
        }
    }

    // Suppliers
    public async Task AddSupplierAsync(Repositories.Entities.Supplier supplier)
    {
        await AddAsync(supplier);
    }

    public async Task UpdateSupplierAsync(Repositories.Entities.Supplier supplier)
    {
        Entry(supplier).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteSupplierAsync(int supplierId)
    {
        var sup = await Suppliers.FirstOrDefaultAsync(s => s.SupplierID == supplierId);
        if (sup is not null) Remove(sup);
    }

    // Shippers
    public async Task AddShipperAsync(Repositories.Entities.Shipper shipper)
    {
        await AddAsync(shipper);
    }

    public async Task UpdateShipperAsync(Repositories.Entities.Shipper shipper)
    {
        Entry(shipper).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteShipperAsync(int shipperId)
    {
        var shp = await Shippers.FirstOrDefaultAsync(s => s.ShipperID == shipperId);
        if (shp is not null) Remove(shp);
    }
}

// Orders helpers
partial class NorthWindSalesCommandsDataContext
{
    public async Task UpdateOrderAsync(Order order)
    {
        Entry(order).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        var order = await Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is not null) Remove(order);
    }

    public async Task RemoveOrderDetailsAsync(int orderId)
    {
        var details = await OrderDetails.Where(d => d.OrderId == orderId).ToListAsync();
        if (details.Count > 0) RemoveRange(details);
    }
}

// Products operations
partial class NorthWindSalesCommandsDataContext
{
    public async Task AddProductAsync(NorthWind.Sales.Backend.Repositories.Entities.Product product)
    {
        await AddAsync(product);
    }

    public async Task UpdateProductAsync(NorthWind.Sales.Backend.Repositories.Entities.Product product)
    {
        Entry(product).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteProductAsync(int productId)
    {
        var prod = await Products.FirstOrDefaultAsync(p => p.ProductID == productId);
        if (prod is not null) Remove(prod);
    }
}
