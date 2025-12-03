using NorthWind.Sales.Backend.Repositories.Entities;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Configurations
{
    internal class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");
            builder.HasKey(s => s.SupplierID);
            builder.Property(s => s.SupplierID).HasColumnName("SupplierID");
            builder.Property(s => s.CompanyName).HasMaxLength(40).IsRequired();
        }
    }
}
