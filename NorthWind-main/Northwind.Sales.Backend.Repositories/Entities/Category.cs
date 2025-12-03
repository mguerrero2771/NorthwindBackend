namespace NorthWind.Sales.Backend.Repositories.Entities
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public byte[]? Picture { get; set; }
    }
}
