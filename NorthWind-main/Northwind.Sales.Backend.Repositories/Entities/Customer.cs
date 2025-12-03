namespace NorthWind.Sales.Backend.Repositories.Entities
{
    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal CurrentBalance { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ContactName { get; set; }
        public string? ContactTitle { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Fax { get; set; }
    }
}
