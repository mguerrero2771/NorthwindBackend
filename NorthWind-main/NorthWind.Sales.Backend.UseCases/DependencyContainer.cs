using Microsoft.Extensions.DependencyInjection;
using NorthWind.Events.Entities.Interfaces;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces.CreateOrder;
using NorthWind.Sales.Backend.UseCases.CreateOrder;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Validation.Entities;

namespace NorthWind.Sales.Backend.UseCases;

public static class DependencyContainer
{
    public static IServiceCollection AddUseCasesServices(
 this IServiceCollection services)
    {
        services.AddScoped<ICreateOrderInputPort,
       CreateOrderInteractor>();
        services.AddModelValidator<CreateOrderDto,
       CreateOrderCustomerValidator>();
        services.AddModelValidator<CreateOrderDto,
       CreateOrderProductValidator>();
        services.AddScoped<IDomainEventHandler<SpecialOrderCreatedEvent>,
SendEMailWhenSpecialOrderCreatedEventHandler>();

        return services;
    }

}