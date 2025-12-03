
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Frontend.BusinessObjects.Interfaces;
using NorthWind.Sales.Entities.ValueObjects;
using System.Net.Http.Json;

namespace NorthWind.Sales.Frontend.WebApiGateways;

public class CreateOrderGateway(HttpClient client) : ICreateOrderGateway
{
    //HttpClient _client;

    //public CreateOrderGateway(HttpClient client)
    //{
    //    _client = client;
    //}

    public async Task<int> CreateOrderAsync(CreateOrderDto order)
    {
        var Response = await client.PostAsJsonAsync(
       Endpoints.CreateOrder, order);
        return await Response.Content.ReadFromJsonAsync<int>();
    }
}
