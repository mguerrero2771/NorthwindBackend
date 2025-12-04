using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using NorthWind.Sales.Backend.Controllers.Membership.IdentityLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Builder.Security.SessionBinding;

namespace Microsoft.AspNetCore.Builder;

public static class NorthWindMembershipEndpoints
{
    private record LoginRequest(string Email, string Password, string? Role);
    private record UnlockRequest(string Email);
    private record RegisterRequest(string Email, string Password, string FirstName, string LastName, string PhoneNumber);
    private record AuthResponse(string Token);

    public static WebApplication UseNorthWindMembershipEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", Login);
        app.MapPost("/auth/register", Register);
        app.MapPost("/auth/unlock", Unlock).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        app.MapGet("/auth/me", Me).RequireAuthorization();
        return app;
    }

    private static async Task<IResult> Login(
        HttpContext http,
        IConfiguration cfg,
        [FromServices] SignInManager<ApplicationUser> signIn,
        [FromServices] UserManager<ApplicationUser> users,
        [FromServices] ISessionStore sessions,
        [FromBody] LoginRequest req)
    {
        try
        {
            if (req is null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest("Email and password are required.");

            var input = req.Email.Trim();
            // Encontrar por UserName o Email
            var user = await users.FindByNameAsync(input) ?? await users.FindByEmailAsync(input);
            if (user is null)
                return Results.BadRequest("Invalid credentials.");

            // Verificar contraseña sin provocar excepciones
            var pwdOk = await users.CheckPasswordAsync(user, req.Password);
            if (!pwdOk)
            {
                var result = await signIn.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
                if (result.IsLockedOut) return Results.StatusCode(StatusCodes.Status423Locked);
                return Results.BadRequest("Invalid credentials.");
            }

            // Si la contraseña es correcta pero el usuario está bloqueado, impedir login
            if (await users.IsLockedOutAsync(user))
            {
                return Results.StatusCode(StatusCodes.Status423Locked);
            }

            // Validar rol seleccionado (si se envía)
            if (!string.IsNullOrWhiteSpace(req.Role))
            {
                var desired = req.Role.Trim();
                if (!await users.IsInRoleAsync(user, desired))
                    return Results.Forbid();
            }

            var roles = (await users.GetRolesAsync(user)).ToArray();
            if (roles.Length == 0) roles = new[] { "User" };
            // Device and User-Agent binding
            var userAgent = http.Request.Headers[HeaderNames.UserAgent].ToString() ?? string.Empty;
            var deviceId = http.Request.Headers["X-Device-Id"].ToString();
            // (Optional) enforce single-session: revoke prior sessions
            await sessions.RevokeAllForUserAsync(user.Id);
            var sessionId = await sessions.CreateAsync(user.Id, userAgent, string.IsNullOrWhiteSpace(deviceId) ? null : deviceId);
            var token = IssueJwt(cfg, user.Email ?? input, roles, sessionId);
            return Results.Ok(new AuthResponse(token));
        }
        catch
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> Register(
        HttpContext http,
        IConfiguration cfg,
        [FromServices] UserManager<ApplicationUser> users,
        [FromServices] ISessionStore sessions,
        [FromBody] RegisterRequest req)
    {
        if (req is null)
            return Results.BadRequest(new { message = "El cuerpo de la solicitud es obligatorio." });

        var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Results.BadRequest(new { message = "El correo electrónico es obligatorio y debe tener un formato válido." });

        var firstName = (req.FirstName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(firstName))
            return Results.BadRequest(new { message = "El primer nombre es obligatorio." });

        var lastName = (req.LastName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(lastName))
            return Results.BadRequest(new { message = "El segundo nombre es obligatorio." });

        var phone = (req.PhoneNumber ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(phone))
            return Results.BadRequest(new { message = "El número de teléfono es obligatorio." });

        if (!Regex.IsMatch(phone, "^\\d{7,15}$"))
            return Results.BadRequest(new { message = "El número de teléfono debe contener entre 7 y 15 dígitos." });

        if (string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest(new { message = "La contraseña es obligatoria." });

        if (await users.FindByEmailAsync(email) is not null)
            return Results.Conflict(new { message = "Ya existe un usuario con este correo electrónico." });

        if (await users.Users.AnyAsync(u => u.PhoneNumber == phone))
            return Results.Conflict(new { message = "Ya existe un usuario con este número de teléfono." });

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phone,
            PhoneNumberConfirmed = false,
        };

        var create = await users.CreateAsync(user, req.Password);
        if (!create.Succeeded)
            return Results.BadRequest(new { message = string.Join("; ", create.Errors.Select(e => e.Description)) });

        await users.AddToRoleAsync(user, "User");
        // Crear sesión ligada al navegador para el usuario recién creado
        var userAgent = http.Request.Headers[HeaderNames.UserAgent].ToString() ?? string.Empty;
        var deviceId = http.Request.Headers["X-Device-Id"].ToString();
        var sessionId = await sessions.CreateAsync(user.Id, userAgent, string.IsNullOrWhiteSpace(deviceId) ? null : deviceId);
        var token = IssueJwt(cfg, email, new[] { "User" }, sessionId);
        return Results.Ok(new AuthResponse(token));
    }

    private static async Task<IResult> Unlock(
        [FromServices] UserManager<ApplicationUser> users,
        [FromBody] UnlockRequest req)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.Email))
            return Results.BadRequest("Email is required.");
        var email = req.Email.Trim();
        var user = await users.FindByNameAsync(email) ?? await users.FindByEmailAsync(email);
        if (user is null) return Results.NotFound();
        await users.ResetAccessFailedCountAsync(user);
        await users.SetLockoutEndDateAsync(user, null);
        return Results.Ok();
    }

    private static async Task<IResult> Me(HttpContext http, [FromServices] UserManager<ApplicationUser> users)
    {
        var user = http.User;
        if (user?.Identity?.IsAuthenticated != true) return Results.Unauthorized();
        var email = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.Identity!.Name ?? "";
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        bool isLockedOut = false; DateTimeOffset? lockoutEnd = null;
        var appUser = await users.FindByNameAsync(email) ?? await users.FindByEmailAsync(email);
        if (appUser is not null)
        {
            isLockedOut = await users.IsLockedOutAsync(appUser);
            lockoutEnd = appUser.LockoutEnd;
        }
        return Results.Ok(new { email, roles, isLockedOut, lockoutEnd });
    }

    private static string IssueJwt(IConfiguration cfg, string email, IEnumerable<string> roles, string sessionId)
    {
        var section = cfg.GetSection("JwtOptions");
        var securityKey = section["SecurityKey"]!;
        var issuer = section["ValidIssuer"]!;
        var audience = section["ValidAudience"]!;
        var expireMinutes = int.TryParse(section["ExpireInMinutes"], out var m) ? m : 60;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, email),
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Sid, sessionId)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
