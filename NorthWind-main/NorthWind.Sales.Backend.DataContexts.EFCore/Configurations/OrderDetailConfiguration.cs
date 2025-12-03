
using NorthWind.Sales.Backend.Repositories.Entities;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Configurations;

internal class OrderDetailConfiguration :
IEntityTypeConfiguration<Repositories.Entities.OrderDetail>
{
    public void Configure(
   EntityTypeBuilder<Repositories.Entities.OrderDetail> builder)
    {
        // Tabla y columnas de Northwind clásico
        builder.ToTable("Order Details");
        builder.Property(d => d.OrderId).HasColumnName("OrderID");
        builder.Property(d => d.ProductId).HasColumnName("ProductID");
        builder.Property(d => d.UnitPrice)
        .HasColumnName("UnitPrice")
        .HasPrecision(19, 4);
        builder.Property(d => d.Quantity).HasColumnName("Quantity");
        // Columna clásica obligatoria en Northwind: Discount (float, NOT NULL). Usamos propiedad sombra con valor 0.
        // Columna Discount ahora nullable
        builder.Property<float?>("Discount")
            .HasColumnName("Discount");
        builder.HasKey(d => new { d.OrderId, d.ProductId });
        // Enlazar la relación con Order usando el tipo POCO sin navegación
        builder.HasOne<NorthWind.Sales.Backend.BusinessObjects.POCOEntities.Order>()
            .WithMany()
            .HasForeignKey(d => d.OrderId);
        builder.HasOne<Product>()
 .WithMany()
 .HasForeignKey(p => p.ProductId);

    }
}
