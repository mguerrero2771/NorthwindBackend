using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Configurations;

internal class OrderConfiguration :
IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Tabla clásica
        builder.ToTable("Orders");
        builder.Property(o => o.Id)
        .HasColumnName("OrderID")
        .UseIdentityColumn();
        builder.Property(o => o.CustomerId)
        .HasColumnName("CustomerID")
        .HasMaxLength(5)
        .IsFixedLength();
        builder.Property(o => o.ShipAddress)
        .HasMaxLength(60);
        builder.Property(o => o.ShipCity)
        .HasMaxLength(15);
        builder.Property(o => o.ShipCountry)
            .HasMaxLength(15);
        builder.Property(o => o.ShipPostalCode)
        .HasMaxLength(10);
        // Mapear la propiedad real OrderDate del POCO
        builder.Property(o => o.OrderDate)
            .HasColumnName("OrderDate");
        // Propiedades de dominio que NO existen en la tabla clásica
        builder.Ignore(o => o.Discount);
        builder.Ignore(o => o.DiscountType);
        builder.Ignore(o => o.ShippingType);
        builder.Property<int?>("EmployeeID")
            .HasColumnName("EmployeeID")
            .HasDefaultValue(1); // Northwind clásico requiere EmployeeID NOT NULL
        builder.Property<int?>("ShipVia")
            .HasColumnName("ShipVia")
            .HasDefaultValue(1);
        builder.Property<decimal>("Freight")
            .HasColumnName("Freight")
            .HasPrecision(19,4)
            .HasDefaultValue(0m);
        builder.HasOne<NorthWind.Sales.Backend.Repositories.Entities.Customer>()
        .WithMany()
        .HasForeignKey(o => o.CustomerId);

    }
}
