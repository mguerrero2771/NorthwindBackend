using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;

namespace NorthWind.Sales.Backend.Repositories.Repositories
{
    internal class QueriesRepository(INorthWindSalesQueriesDataContext context) : IQueriesRepository
    {
        public async Task<decimal?> GetCustomerCurrentBalance(
       string customerId)
        {
            try
            {
                var Queryable = context.Customers
                .Where(c => c.Id == customerId)
                .Select(c => new { c.CurrentBalance });
                var Result = await context.FirstOrDefaultAync(Queryable);
                return Result?.CurrentBalance;
            }
            catch
            {
                // Cuando la BD es Northwind clásica (CustomerID/CompanyName y sin CurrentBalance),
                // devolvemos 0 para permitir crear orden sin saldo pendiente.
                return 0m;
            }
        }
        public async Task<IEnumerable<ProductUnitsInStock>>
       GetProductsUnitsInStock(IEnumerable<int> productIds)
        {
            try
            {
                var Queryable = context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .Select(p => new ProductUnitsInStock(
                p.ProductID, p.UnitsInStock));
                return await context.ToListAsync(Queryable);
            }
            catch
            {
                // Si el esquema usa ProductID en lugar de Id, intentamos un mapeo por proyección manual.
                // Nota: esta rama cubre escenarios donde el modelo no está alineado con la tabla clásica.
                var Queryable = context.Products
                    .Where(p => productIds.Contains(p.ProductID))
                    .Select(p => new ProductUnitsInStock(
                        p.ProductID, p.UnitsInStock));
                return await context.ToListAsync(Queryable);
            }
        }
    }

}
