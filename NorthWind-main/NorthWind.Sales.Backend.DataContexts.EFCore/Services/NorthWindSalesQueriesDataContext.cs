using NorthWind.Sales.Backend.Repositories.Entities;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.Services;

internal class NorthWindSalesQueriesDataContext :
 NorthWindSalesContext,
 INorthWindSalesQueriesDataContext
{
    public NorthWindSalesQueriesDataContext(IOptions<DBOptions> dbOptions)
   : base(dbOptions)
    {
        ChangeTracker.QueryTrackingBehavior =
        QueryTrackingBehavior.NoTracking;
    }
    public new IQueryable<Customer> Customers => base.Customers;
    public new IQueryable<Product> Products => base.Products;
    public new IQueryable<Category> Categories => base.Categories;
    public new IQueryable<Supplier> Suppliers => base.Suppliers;
    public new IQueryable<Shipper> Shippers => base.Shippers;
    public new IQueryable<Order> Orders => base.Orders;
    public new IQueryable<OrderDetail> OrderDetails => base.OrderDetails;
    public Task<ReturnType> FirstOrDefaultAync<ReturnType>(
   IQueryable<ReturnType> queryable) =>
   queryable.FirstOrDefaultAsync();
    public async Task<IEnumerable<ReturnType>> ToListAsync<ReturnType>(
IQueryable<ReturnType> queryable) =>
await queryable.ToListAsync();
}

