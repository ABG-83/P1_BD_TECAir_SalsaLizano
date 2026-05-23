// =============================================================================
// Archivo  : ServiceCollectionExtensions.cs
// Capa     : TECAir.API → Extensions
// Propósito: Registra todas las capas del sistema en el contenedor de DI.
//            Cada vez que se agrega un repositorio o servicio nuevo al proyecto,
//            se registra aquí para que el DI container sepa qué clase instanciar.
// =============================================================================


using TECAir.Core.Interfaces;
using TECAir.Core.Services;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Repositories;

namespace TECAir.API.Extensions
{
   
    /// Provides extension methods to register core infrastructure, repositories, and services into the DI container.
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all infrastructure, data repositories, and business services required by the TECAir platform.
        /// </summary>
        /// <param name="services">The service collection descriptor instance.</param>
        /// <param name="connectionString">The active database connection string configuration.</param>
        /// <returns>The updated service collection instance for method chaining configuration.</returns>
        public static IServiceCollection AddTECAirCoreLayers(this IServiceCollection services, string connectionString)
        {
            // ── Infrastructure & Factory Configuration ────────────────────────
            services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

            // ── Data Access Layer / Repositories ──────────────────────────────
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAirportRepository, AirportRepository>();
            services.AddScoped<IAirplaneRepository, AirplaneRepository>();  // Issue #14
            services.AddScoped<IFlightRepository, FlightRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>(); // Issue #13

            // ── Business Logic Layer / Services ────────────────────────────────
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAirportService, AirportService>();
            services.AddScoped<IFlightService, FlightService>();
            services.AddScoped<IPromotionService, PromotionService>(); // Issue #13

            return services;
        }
    }
}
