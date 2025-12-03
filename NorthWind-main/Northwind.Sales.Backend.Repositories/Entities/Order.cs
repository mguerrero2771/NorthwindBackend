namespace NorthWind.Sales.Backend.Repositories.Entities
{
    // Renombrado para evitar conflicto con BusinessObjects.POCOEntities.Order
    internal class RepoOrderEntity
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public DateTime? OrderDate { get; set; }
        public string? ShipAddress { get; set; }
        public string? ShipCity { get; set; }
        public string? ShipCountry { get; set; }
        public string? ShipPostalCode { get; set; }
        public decimal Discount { get; set; }
        public int DiscountType { get; set; }
        public int ShippingType { get; set; }
    }
}
