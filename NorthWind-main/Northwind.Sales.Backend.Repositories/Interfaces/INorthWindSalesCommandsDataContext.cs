
namespace NorthWind.Sales.Backend.Repositories.Interfaces;

public interface INorthWindSalesCommandsDataContext
{
    Task AddOrderAsync(NorthWind.Sales.Backend.BusinessObjects.POCOEntities.Order order);
    Task AddOrderDetailsAsync(IEnumerable<Entities.OrderDetail> orderDetails);
    Task SaveChangesAsync();

    // Categories CRUD
    Task AddCategoryAsync(Entities.Category category);
    Task UpdateCategoryAsync(Entities.Category category);
    Task DeleteCategoryAsync(int categoryId);

    // Suppliers CRUD
    Task AddSupplierAsync(Entities.Supplier supplier);
    Task UpdateSupplierAsync(Entities.Supplier supplier);
    Task DeleteSupplierAsync(int supplierId);

    // Shippers CRUD
    Task AddShipperAsync(Entities.Shipper shipper);
    Task UpdateShipperAsync(Entities.Shipper shipper);
    Task DeleteShipperAsync(int shipperId);

    // Products CRUD
    Task AddProductAsync(Entities.Product product);
    Task UpdateProductAsync(Entities.Product product);
    Task DeleteProductAsync(int productId);

    // Orders CRUD helpers
    Task UpdateOrderAsync(NorthWind.Sales.Backend.BusinessObjects.POCOEntities.Order order);
    Task DeleteOrderAsync(int orderId);
    Task RemoveOrderDetailsAsync(int orderId);
}
