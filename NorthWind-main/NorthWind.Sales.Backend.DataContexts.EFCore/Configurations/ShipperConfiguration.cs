using NorthWind.Sales.Backend.Repositories.Entities;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Configurations
{
    internal class ShipperConfiguration : IEntityTypeConfiguration<Shipper>
    {
        public void Configure(EntityTypeBuilder<Shipper> builder)
        {
            builder.ToTable("Shippers");
            builder.HasKey(s => s.ShipperID);
            builder.Property(s => s.ShipperID).HasColumnName("ShipperID");
            builder.Property(s => s.CompanyName).HasMaxLength(40).IsRequired();
        }
    }
}
