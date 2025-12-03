namespace NorthWind.Sales.Backend.Repositories.Entities
{
    public class Shipper
    {
        public int ShipperID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }
}
