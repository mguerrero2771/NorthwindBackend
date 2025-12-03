using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NorthWind.Sales.Backend.Controllers.Membership.IdentityLite;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Builder;

public static class AdminUsersEndpoints
{
    public static WebApplication UseAdminUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/admin/users").RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapGet("", List);
        group.MapGet("/locked", Locked);
        group.MapPost("", Create);
        group.MapPut("/{id}", Update);
        group.MapDelete("/{id}", Delete);
        group.MapPut("/{id}/roles", SetRoles);
        return app;
    }

    private record PagedQuery(string? q, int page = 1, int pageSize = 10);
    private record PagedResult<T>(int total, IEnumerable<T> items);
    private record UserDto(string Email, string FirstName, string LastName, string? Password, string? PhoneNumber);
    private record RolesDto(IEnumerable<string> RoleNames);

    private static async Task<IResult> List([AsParameters] PagedQuery query, [FromServices] UserManager<ApplicationUser> userMgr)
    {
        var q = (query.q ?? string.Empty).Trim().ToLowerInvariant();
        var users = userMgr.Users.AsQueryable();
        if (!string.IsNullOrEmpty(q))
        {
            users = users.Where(u => u.Email!.ToLower().Contains(q) || u.UserName!.ToLower().Contains(q));
        }
        var total = await users.CountAsync();
        users = users.OrderBy(u => u.Email!)
            .Skip((Math.Max(1, query.page) - 1) * Math.Max(1, query.pageSize))
            .Take(Math.Max(1, query.pageSize));
        var items = await users.Select(u => new { u.Id, u.Email, u.UserName, u.FirstName, u.LastName, u.PhoneNumber, u.AccessFailedCount, u.LockoutEnd })
                               .ToListAsync();
        return Results.Ok(new PagedResult<object>(total, items));
    }

    private static async Task<IResult> Locked([FromServices] UserManager<ApplicationUser> userMgr)
    {
        var now = DateTimeOffset.UtcNow;
        var items = await userMgr.Users
            .Where(u => u.LockoutEnd.HasValue && u.LockoutEnd!.Value > now)
            .OrderBy(u => u.Email!)
            .Select(u => new { u.Id, u.Email, u.UserName, u.FirstName, u.LastName, u.AccessFailedCount, u.LockoutEnd })
            .ToListAsync();
        return Results.Ok(new { total = items.Count, items });
    }

    private static async Task<IResult> Create([FromBody] UserDto dto, [FromServices] UserManager<ApplicationUser> userMgr)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)) return Results.BadRequest("Email is required");
        var email = dto.Email.Trim().ToLowerInvariant();
        if (!email.Contains('@')) return Results.BadRequest("Invalid email");
        if (await userMgr.FindByEmailAsync(email) is not null) return Results.Conflict("Email already exists");
        if (string.IsNullOrWhiteSpace(dto.Password)) return Results.BadRequest("Password is required");
        var phone = (dto.PhoneNumber ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(phone)) return Results.BadRequest("Phone is required");
        if (!Regex.IsMatch(phone, "^\\d{7,15}$")) return Results.BadRequest("Phone must be 7-15 digits");

        if (await userMgr.Users.AnyAsync(u => u.PhoneNumber == phone)) return Results.Conflict("Phone already exists");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = (dto.FirstName ?? string.Empty).Trim(),
            LastName  = (dto.LastName  ?? string.Empty).Trim(),
            PhoneNumber = phone,
        };
        var res = await userMgr.CreateAsync(user, dto.Password!);
        if (!res.Succeeded) return Results.BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));
        return Results.Created($"/admin/users/{user.Id}", new { user.Id, user.Email, user.UserName });
    }

    private static async Task<IResult> Update(string id, [FromBody] UserDto dto, [FromServices] UserManager<ApplicationUser> userMgr)
    {
        var user = await userMgr.FindByIdAsync(id);
        if (user is null) return Results.NotFound();
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(email) || !email.Contains('@')) return Results.BadRequest("Invalid email");
        var dup = await userMgr.FindByEmailAsync(email);
        if (dup is not null && dup.Id != id) return Results.Conflict("Email already exists");
        user.Email = email;
        user.UserName = email;
        user.FirstName = (dto.FirstName ?? string.Empty).Trim();
        user.LastName  = (dto.LastName  ?? string.Empty).Trim();
        var phone = (dto.PhoneNumber ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(phone))
        {
            if (!Regex.IsMatch(phone, "^\\d{7,15}$")) return Results.BadRequest("Phone must be 7-15 digits");
            var dupPhone = await userMgr.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (dupPhone is not null && dupPhone.Id != id) return Results.Conflict("Phone already exists");
            user.PhoneNumber = phone;
        }
        var resUpdate = await userMgr.UpdateAsync(user);
        if (!resUpdate.Succeeded) return Results.BadRequest(string.Join("; ", resUpdate.Errors.Select(e => e.Description)));
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var token = await userMgr.GeneratePasswordResetTokenAsync(user);
            var resPwd = await userMgr.ResetPasswordAsync(user, token, dto.Password!);
            if (!resPwd.Succeeded) return Results.BadRequest(string.Join("; ", resPwd.Errors.Select(e => e.Description)));
            await userMgr.ResetAccessFailedCountAsync(user);
            await userMgr.SetLockoutEndDateAsync(user, null);
        }
        return Results.Ok(new { user.Id, user.Email, user.UserName });
    }

    private static async Task<IResult> Delete(string id, [FromServices] UserManager<ApplicationUser> userMgr)
    {
        var user = await userMgr.FindByIdAsync(id);
        if (user is null) return Results.NotFound();
        var res = await userMgr.DeleteAsync(user);
        if (!res.Succeeded) return Results.BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));
        return Results.NoContent();
    }

    private static async Task<IResult> SetRoles(string id, [FromBody] RolesDto dto, [FromServices] UserManager<ApplicationUser> userMgr, [FromServices] RoleManager<IdentityRole> roleMgr)
    {
        var user = await userMgr.FindByIdAsync(id);
        if (user is null) return Results.NotFound();
        var roleNames = (dto.RoleNames ?? Enumerable.Empty<string>()).Distinct().ToArray();
        // Validar existencia
        foreach (var rn in roleNames)
        {
            if (!await roleMgr.RoleExistsAsync(rn))
                return Results.BadRequest($"Role '{rn}' does not exist");
        }
        var current = await userMgr.GetRolesAsync(user);
        var toRemove = current.Where(r => !roleNames.Contains(r)).ToArray();
        var toAdd = roleNames.Where(r => !current.Contains(r)).ToArray();
        if (toRemove.Length > 0) await userMgr.RemoveFromRolesAsync(user, toRemove);
        if (toAdd.Length > 0)
        {
            var resAdd = await userMgr.AddToRolesAsync(user, toAdd);
            if (!resAdd.Succeeded) return Results.BadRequest(string.Join("; ", resAdd.Errors.Select(e => e.Description)));
        }
        return Results.Ok();
    }
}
