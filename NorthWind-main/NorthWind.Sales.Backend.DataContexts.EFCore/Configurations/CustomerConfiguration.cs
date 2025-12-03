using NorthWind.Sales.Backend.Repositories.Entities;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Configurations
{
    internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder
            .Property(c => c.Id)
            .HasColumnName("CustomerID")
            .HasMaxLength(5)
            .IsFixedLength();
            builder.Property(c => c.Name)
            .HasColumnName("CompanyName")
            .IsRequired()
            .HasMaxLength(40);
            builder.Property(c => c.Address)
             .HasColumnName("Address")
             .HasMaxLength(60);
            builder.Property(c => c.Phone)
             .HasColumnName("Phone")
             .HasMaxLength(24);
            builder.Property(c => c.ContactName)
             .HasColumnName("ContactName")
             .HasMaxLength(30);
            builder.Property(c => c.ContactTitle)
             .HasColumnName("ContactTitle")
             .HasMaxLength(30);
            builder.Property(c => c.City)
             .HasColumnName("City")
             .HasMaxLength(15);
            builder.Property(c => c.Country)
             .HasColumnName("Country")
             .HasMaxLength(15);
            builder.Property(c => c.Fax)
             .HasColumnName("Fax")
             .HasMaxLength(24);
            // En Northwind clásico no existe la columna CurrentBalance.
            // Ignorar explícitamente para que EF no intente mapearla ni consultarla.
            builder.Ignore(c => c.CurrentBalance);
        }
    }

}
