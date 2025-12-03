using NorthWind.Sales.Backend.Repositories.Entities;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Configurations
{
    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.CategoryID);
            builder.Property(c => c.CategoryID).HasColumnName("CategoryID");
            builder.Property(c => c.CategoryName)
                   .HasColumnName("CategoryName")
                   .HasMaxLength(15)
                   .IsRequired();
            builder.Property(c => c.Description)
                   .HasColumnName("Description")
                   .HasMaxLength(1024);
            builder.Property(c => c.Picture)
                   .HasColumnName("Picture");
        }
    }
}
