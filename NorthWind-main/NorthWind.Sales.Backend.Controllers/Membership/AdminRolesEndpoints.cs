using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NorthWind.Sales.Backend.Controllers.Membership.IdentityLite;

namespace Microsoft.AspNetCore.Builder;

public static class AdminRolesEndpoints
{
    public static WebApplication UseAdminRolesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin/roles").RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapGet("", List);
        group.MapPost("", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
        return app;
    }

    private record PagedQuery(string? q, int page = 1, int pageSize = 10, string? orderBy = null, string? orderDir = null);
    private record PagedResult<T>(int total, IEnumerable<T> items);

    private static async Task<IResult> List([AsParameters] PagedQuery query, [FromServices] RoleManager<IdentityRole> rolesMgr)
    {
        var q = (query.q ?? string.Empty).Trim().ToLowerInvariant();
        var roles = rolesMgr.Roles.AsQueryable();
        if (!string.IsNullOrEmpty(q))
        {
            roles = roles.Where(r => r.Name!.ToLower().Contains(q));
        }
        var total = await roles.CountAsync();
        roles = roles.OrderBy(r => r.Name!)
                     .Skip((Math.Max(1, query.page) - 1) * Math.Max(1, query.pageSize))
                     .Take(Math.Max(1, query.pageSize));
        var items = await roles.Select(r => new { r.Id, r.Name }).ToListAsync();
        return Results.Ok(new PagedResult<object>(total, items));
    }

    private record RoleDto(string Name);

    private static async Task<IResult> Create([FromBody] RoleDto dto, [FromServices] RoleManager<IdentityRole> rolesMgr)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest("Name is required");
        var name = dto.Name.Trim();
        if (await rolesMgr.RoleExistsAsync(name)) return Results.Conflict("Role name already exists");
        var role = new IdentityRole(name);
        var res = await rolesMgr.CreateAsync(role);
        if (!res.Succeeded) return Results.BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));
        return Results.Created($"/admin/roles/{role.Id}", new { role.Id, role.Name });
    }

    private static async Task<IResult> Update(string id, [FromBody] RoleDto dto, [FromServices] RoleManager<IdentityRole> rolesMgr)
    {
        var role = await rolesMgr.FindByIdAsync(id);
        if (role is null) return Results.NotFound();
        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name)) return Results.BadRequest("Name is required");
        var exists = await rolesMgr.FindByNameAsync(name);
        if (exists is not null && exists.Id != id) return Results.Conflict("Role name already exists");
        role.Name = name;
        role.NormalizedName = name.ToUpperInvariant();
        var res = await rolesMgr.UpdateAsync(role);
        if (!res.Succeeded) return Results.BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));
        return Results.Ok(new { role.Id, role.Name });
    }

    private static async Task<IResult> Delete(string id, [FromServices] RoleManager<IdentityRole> rolesMgr, [FromServices] UserManager<ApplicationUser> usersMgr)
    {
        var role = await rolesMgr.FindByIdAsync(id);
        if (role is null) return Results.NotFound();
        // Evitar eliminar si está en uso
        var usersInRole = await usersMgr.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any()) return Results.BadRequest("Role is in use");
        var res = await rolesMgr.DeleteAsync(role);
        if (!res.Succeeded) return Results.BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));
        return Results.NoContent();
    }
}
