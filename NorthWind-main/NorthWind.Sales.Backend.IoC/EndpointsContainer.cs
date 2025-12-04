using NorthWind.Sales.Backend.Controllers.System;
namespace Microsoft.AspNetCore.Builder;
public static class EndpointsContainer
{
    public static WebApplication MapNorthWindSalesEndpoints(
   this WebApplication app)
    {
        app.UseCreateOrderController();
        app.UseNorthWindMembershipEndpoints();
        app.UseAdminRolesEndpoints();
        app.UseAdminUsersEndpoints();
        app.UseNorthWindReadEndpoints();
        app.UseNorthWindDashboardEndpoints();
        app.UseNorthWindCategoriesEndpoints();
        app.UseNorthWindProductsAdminEndpoints();
        app.UseNorthWindSuppliersEndpoints();
        app.UseNorthWindShippersEndpoints();
        app.UseNorthWindOrdersEndpoints();
        app.UseNorthWindCustomersAdminEndpoints();
        app.UseErrorReportsEndpoints();

        return app;
    }
}
