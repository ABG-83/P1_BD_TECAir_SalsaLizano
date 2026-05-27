// =============================================================================
// File    : ServiceCollectionExtensions.cs
// Layer   : TECAir.API → Extensions
// Purpose : Registers dependency injection and application configuration.
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
            services.AddScoped<IReservationRepository, ReservationRepository>();  // Issue #29
            services.AddScoped<IBaggageRepository, BaggageRepository>();          // Issue #29
            services.AddScoped<ICheckInRepository, CheckInRepository>();          // Issue #15
            services.AddScoped<IBaggageRepository, BaggageRepository>();          // 
            services.AddScoped<IPaymentRepository, PaymentRepository>();


            // ── Business Logic Layer / Services ────────────────────────────────
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAirportService, AirportService>();
            services.AddScoped<IFlightService, FlightService>();
            services.AddScoped<IPromotionService, PromotionService>(); // Issue #13
            services.AddScoped<IFlightOpeningService, FlightOpeningService>();     // Issue #29
            services.AddScoped<ICheckInService, CheckInService>();                 // Issue #15
            services.AddScoped<IBaggageService, BaggageService>();          // Issue #16
            services.AddScoped<IFlightClosingService, FlightClosingService>();         // Issue #30
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
