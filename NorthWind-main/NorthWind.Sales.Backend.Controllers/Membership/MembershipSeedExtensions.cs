using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NorthWind.Sales.Backend.Controllers.Membership.IdentityLite;

namespace Microsoft.AspNetCore.Builder;

public static class MembershipSeedExtensions
{
    public static WebApplication SeedMembership(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Roles mínimos
        var roles = new[] { "Admin", "User" };
        foreach (var r in roles)
        {
            if (!roleMgr.RoleExistsAsync(r).GetAwaiter().GetResult())
            {
                roleMgr.CreateAsync(new IdentityRole(r)).GetAwaiter().GetResult();
            }
        }

        // Usuario admin opcional (solo si no existe ninguno)
        var adminEmail = "admin@example.com";
        var existing = userMgr.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (existing is null)
        {
            var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, FirstName = "System", LastName = "Admin" };
            var create = userMgr.CreateAsync(user, "Aa1!1").GetAwaiter().GetResult();
            if (create.Succeeded)
            {
                userMgr.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
            }
        }

        return app;
    }
}
