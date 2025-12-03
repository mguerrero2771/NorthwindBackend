
using NorthWind.Sales.Backend.Repositories.Entities;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;

internal class NorthWindContext : DbContext
{
    protected override void OnConfiguring(
   DbContextOptionsBuilder optionsBuilder)
    {
        /*optionsBuilder.UseSqlServer(
        "Data Source=SEBAS;Initial Catalog=NorthWindDB;Integrated Security=True;Trust Server Certificate=True");*/

        optionsBuilder.UseSqlServer(
        "workstation id=NorthWind_Moviles.mssql.somee.com;packet size=4096;user id=AZ_developer_SQLLogin_2;pwd=lip1fgttra;data source=NorthWind_Moviles.mssql.somee.com;persist security info=False;initial catalog=NorthWind_Moviles;TrustServerCertificate=True"); 


    }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
        Assembly.GetExecutingAssembly());
    }
}
