using Microsoft.Extensions.DependencyInjection;
using NorthWind.Membership.Frontend.RazorViews;
using NorthWind.Sales.Frontend.Views;
using NorthWind.Sales.Frontend.WebApiGateways;
using NorthWind.Sales.Validators.Entities;
using NorthWind.ValidationService.FluentValidation;

namespace NorthWind.Sales.Frontend.IoC
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddNorthWindSalesServices(this IServiceCollection services,
        Action<HttpClient> configureHttpClient,
        Action<HttpClient> configureMembershipHttpClient,
        Action<IHttpClientBuilder> configureHttpClientBuilder)
        {
            services.AddWebApiGateways(configureHttpClient, configureHttpClientBuilder)
            .AddViewsServices()
            .AddValidationService()
            .AddValidators()
            .AddMembershipServices(configureMembershipHttpClient);

            return services;
        }
    }

}
