using Microsoft.Extensions.DependencyInjection;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Validators.Entities.CreateOrder;
using NorthWind.Validation.Entities;

namespace NorthWind.Sales.Validators.Entities;

public static class DependencyContainer
{
    public static IServiceCollection AddValidators(
   this IServiceCollection services)
    {
        services.AddModelValidator<CreateOrderDto,
        CreateOrderDtoValidator>();
        services.AddModelValidator<CreateOrderDetailDto,
        CreateOrderDetailDtoValidator>();
        return services;
    }
}
