
namespace NorthWind.Sales.Backend.Repositories.Entities;

public class OrderDetail
{
    public int OrderId { get; set; } // Relación con la Orden (FK)
    public int ProductId { get; set; } // Id del Producto
    public decimal UnitPrice { get; set; }
    public short Quantity { get; set; }
}
