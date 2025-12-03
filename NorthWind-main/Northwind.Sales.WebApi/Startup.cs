using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Northwind.Sales.WebApi.Extensions;
using NorthWind.Membership.Backend.AspNetIdentity.Options;
using NorthWind.Membership.Backend.Core.Options;
using NorthWind.Sales.Backend.DataContexts.EFCore.Options;
using NorthWind.Sales.Backend.IoC;
using NorthWind.Sales.Backend.SmtpGateways.Options;
using Microsoft.EntityFrameworkCore;
using NorthWind.Sales.Backend.Controllers.Membership;
using Microsoft.AspNetCore.Identity;
using NorthWind.Sales.Backend.Controllers.Membership.IdentityLite;
using System.Text;

namespace Northwind.Sales.WebApi;

// Esto expone 2 metodos de extension para configurar los servicios web
// y agregar los middlewares y endpoints de la Web API

internal static class Startup
{
    //  Agregar soporte para documentación Swagger.
    public static WebApplication CreateWebApplication(this WebApplicationBuilder builder)
    {

        // Esto registra los servicios necesarios para generar la documentación automática Swagger de la API.
        // Configurar APIExplorer para descubrir y exponer los metadatos de los endpoints de la aplicación.    
        builder.Services.AddEndpointsApiExplorer();

        //  Habilita la documentación de la API.
        builder.Services.AddSwaggerGenBearer();

        //  Registrar servicios con Inyección de Dependencias.
        //  Registrar los servicios de la aplicación.
        //  Esto utiliza el contenedor de IoC (DependencyContainer) para registrar todas las dependencias
        //  del dominio NorthWind Sales, incluyendo:
        //  Use Cases, Repositories, Data Contexts, Presenters.
        //  Aquí "DBOptions" representa un objeto que contiene el "ConnectionString" y se carga
        //  desde "appsettings.json".
        builder.Services.AddNorthWindSalesServices(dbObtions =>
            builder.Configuration.GetSection(DBOptions.SectionKey).Bind(dbObtions),
            smtpOptions => builder.Configuration.GetSection(SmtpOptions.SectionKey).Bind(smtpOptions),
            membershipDBOptions =>
builder.Configuration.GetSection(MembershipDBOptions.SectionKey)
.Bind(membershipDBOptions),
            jwtOptions =>
builder.Configuration
.GetSection(JwtOptions.SectionKey)
.Bind(jwtOptions)
        );

        //  Configurar CORS.
        //  Esto permite que cualquier cliente (como un frontend en Angular, React o Blazor WebAssembly)
        //  pueda consumir la API sin restricciones de origen, método o cabecera.
        //  Habilita el acceso desde otros dominios (útil para frontend).
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(config =>
            {
                config.AllowAnyMethod();
                config.AllowAnyHeader();
                config.AllowAnyOrigin();
            });
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     // Establecer la configuración del Token.
     builder.Configuration.GetSection(JwtOptions.SectionKey)
     .Bind(options.TokenValidationParameters);
     // Establecer la llave para validación de la firma.
     var securityKey = builder.Configuration
     .GetSection(JwtOptions.SectionKey)[nameof(JwtOptions.SecurityKey)];
     if (string.IsNullOrWhiteSpace(securityKey))
     {
         throw new InvalidOperationException("JwtOptions.SecurityKey is missing or empty in configuration");
     }
     byte[] SecurityKeyBytes = Encoding.UTF8.GetBytes(securityKey);
     options.TokenValidationParameters.IssuerSigningKey =
     new SymmetricSecurityKey(SecurityKeyBytes);
 });

        builder.Services.AddAuthorization();

        // In-memory cache for temporary lockout tracking
        builder.Services.AddMemoryCache();

        // Membership DbContext (Users & Roles)
        // ASP.NET Identity sobre las tablas AspNet*
        var membershipConn = builder.Configuration.GetSection(MembershipDBOptions.SectionKey)[nameof(MembershipDBOptions.ConnectionString)];
        if (string.IsNullOrWhiteSpace(membershipConn))
        {
            throw new InvalidOperationException("MembershipDBOptions.ConnectionString is missing or empty in configuration");
        }

        builder.Services.AddDbContext<IdentityAppDbContext>(options =>
            options.UseSqlServer(membershipConn));

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Bloqueo: más suave en Development
                if (builder.Environment.IsDevelopment())
                {
                    options.Lockout.MaxFailedAccessAttempts = 6;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                }
                else
                {
                    options.Lockout.MaxFailedAccessAttempts = 4;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                }
                options.Password.RequiredLength = 4; // validación detallada en capa de negocio si aplica
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<IdentityAppDbContext>()
            .AddSignInManager();


        //  Construye la instancia "WebApplication" con todos los servicios configurados.
        return builder.Build();
    }

    //  Este método se encarga de:
    //  -Habilitar Swagger solo en desarrollo
    //  -Mapear los endpoints de la aplicación
    public static WebApplication ConfigureWebApplication(this WebApplication app)
    {
        //  Middleware que permite ejecutar un delegado luego que se haya ejecutado
        //  los manejadores de excepciones.
        app.UseExceptionHandler(builder => { });

        //  Solo cuando el entorno es "Development", se activa Swagger para ver la documentación
        //  de la API y la interfaz UI de Swagger en el navegador.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            //  Activa la interfaz (UI) de Swagger para probar la API en desarrollo
            app.UseSwaggerUI();
        }

        //  Registra todos los servicios necesarios usando Clean Architecture (casos de uso,
        //  repositorios, presenters, etc.)
        //  Mapea los controladores implementados, como el de crear órdenes "CreateOrders"
        app.MapNorthWindSalesEndpoints();

        //  Agregar el Middleware CORS
        //  Habilita CORS en tiempo de ejecución para aceptar solicitudes de cualquier origen.
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        // Seed membership data (roles/users)
        app.SeedMembership();

        return app;
    }
}
