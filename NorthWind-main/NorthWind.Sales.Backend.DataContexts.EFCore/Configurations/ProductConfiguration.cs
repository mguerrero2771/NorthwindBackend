using NorthWind.Sales.Backend.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Configurations
{
    internal class ProductConfiguration :
 IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.Property(p => p.ProductID)
            .HasColumnName("ProductID");
            builder.Property(p => p.Name)
            .HasColumnName("ProductName")
            .IsRequired()
            .HasMaxLength(40);
            builder.Property(p => p.UnitPrice)
            .HasPrecision(19, 4);
            builder.HasData(
            new Product
            {
                ProductID = 1,
                Name = "Chai",
                UnitPrice = 35,
                UnitsInStock = 20
            },
            new Product
            {
                ProductID = 2,
                Name = "Chang",
                UnitPrice = 55,
                UnitsInStock = 0
            },
            new Product
            {
                ProductID = 3,
                Name = "Aniseed Syrup",
                UnitPrice = 65,
                UnitsInStock = 20
            },
            new Product
            {
                ProductID = 4,
                Name = "Chef Anton's Cajun Seasoning",
                UnitPrice = 75,
                UnitsInStock = 40
            },
            new Product
            {
                ProductID = 5,
                Name = "Chef Anton's Gumbo Mix",
                UnitPrice = 50,
                UnitsInStock = 20
            });
        }
    }

}
