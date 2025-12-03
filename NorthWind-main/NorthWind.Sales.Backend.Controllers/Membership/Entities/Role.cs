using System.ComponentModel.DataAnnotations;

namespace NorthWind.Sales.Backend.Controllers.Membership.Entities;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
