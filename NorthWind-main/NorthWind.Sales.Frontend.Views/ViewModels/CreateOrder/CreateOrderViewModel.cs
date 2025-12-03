using NorthWind.RazorComponents.Validators;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Frontend.BusinessObjects.Interfaces;
using NorthWind.Sales.Frontend.Views.Resources;
using NorthWind.Validation.Entities.Interfaces;
using NorthWind.Validation.Entities.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Sales.Frontend.Views.ViewModels.CreateOrder
{
    public class CreateOrderViewModel(ICreateOrderGateway gateway,
 IModelValidatorHub<CreateOrderViewModel> validator)
    {
        public IModelValidatorHub<CreateOrderViewModel> Validator => validator;
        #region Propiedades relacionadas a CreateOrderDto
        public string CustomerId { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipCountry { get; set; }
        public string ShipPostalCode { get; set; }
        public List<CreateOrderDetailViewModel> OrderDetails { get; set; } = [];
        #endregion
        public string InformationMessage { get; private set; }

        public ModelValidator<CreateOrderViewModel>
 ModelValidatorComponentReference { get; set; }
        public void AddNewOrderDetailItem()
        {
            OrderDetails.Add(
            new CreateOrderDetailViewModel());
        }
        public async Task Send()
        {
            InformationMessage = "";
            try
            {
                var OrderId = await gateway.CreateOrderAsync(
                (CreateOrderDto)this);
                InformationMessage = string.Format(
                CreateOrderMessages.CreatedOrderTemplate, OrderId);
            }
            catch (HttpRequestException ex)
            {
                if (ex.Data.Contains("Errors"))
                {
                    IEnumerable<ValidationError> Errors =
                    ex.Data["Errors"] as IEnumerable<ValidationError>;
                    ModelValidatorComponentReference.AddErrors(Errors);
                }
                else
                {
                    // Mostrar el mensaje real proveniente de la API (ProblemDetails o texto)
                    var status = ex.StatusCode.HasValue ? ((int)ex.StatusCode.Value).ToString() : "";
                    var traceId = ex.Data.Contains("TraceId") ? ex.Data["TraceId"]?.ToString() : null;
                    var raw = ex.Data.Contains("RawResponse") ? ex.Data["RawResponse"]?.ToString() : null;
                    InformationMessage = string.IsNullOrWhiteSpace(traceId)
                        ? ex.Message
                        : $"{ex.Message} (TraceId: {traceId})";
                    Console.Error.WriteLine($"CreateOrder failed. Status: {status} Code: {ex.StatusCode} Source: {ex.Source}\nMessage: {ex.Message}\nTraceId: {traceId}\nRaw: {raw}");
                }
            }
        }

        public static explicit operator CreateOrderDto(
       CreateOrderViewModel model) =>
       new CreateOrderDto(
       model.CustomerId, model.ShipAddress, model.ShipCity,
       model.ShipCountry, model.ShipPostalCode,
       model.OrderDetails.Select(d => new CreateOrderDetailDto(
       d.ProductId, d.UnitPrice, d.Quantity)
       ));
    }
}
