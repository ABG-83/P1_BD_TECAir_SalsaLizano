// =============================================================================
// Archivo  : PromotionService.cs
// Capa     : TECAir.Core → Services
// Propósito: Implementa la lógica de negocio para la gestión de promociones.
//            Coordina IPromotionRepository e IAirportRepository para enriquecer
//            las respuestas con nombres de aeropuertos y validar reglas del dominio.
//
//            Reglas de negocio aplicadas:
//              - StartDate debe ser anterior a EndDate
//              - Origen y destino deben ser aeropuertos distintos y existentes en la BD
//              - No se puede editar ni eliminar una promoción que no existe
// =============================================================================

using TECAir.Core.DTOs.Promotions;
using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{
    public class PromotionService(
        IPromotionRepository promotionRepository,
        IAirportRepository airportRepository) : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository = promotionRepository;
        private readonly IAirportRepository   _airportRepository   = airportRepository;

        // ── Consultas ──────────────────────────────────────────────────────────

        // Obtiene todas las promociones y enriquece cada una con los nombres de sus aeropuertos
        public async Task<IEnumerable<PromotionResponseDto>> GetAllPromotionsAsync()
        {
            var promotions = await _promotionRepository.GetAllAsync();

            var result = new List<PromotionResponseDto>();
            foreach (var promo in promotions)
                result.Add(await BuildResponseDtoAsync(promo));

            return result;
        }

        // Obtiene solo las activas y las enriquece con datos de aeropuertos
        public async Task<IEnumerable<PromotionResponseDto>> GetActivePromotionsAsync()
        {
            var promotions = await _promotionRepository.GetActiveAsync();

            var result = new List<PromotionResponseDto>();
            foreach (var promo in promotions)
                result.Add(await BuildResponseDtoAsync(promo));

            return result;
        }

        // Busca una promoción por ID; el controlador convierte null en 404
        public async Task<PromotionResponseDto?> GetPromotionByIdAsync(int promotionId)
        {
            var promotion = await _promotionRepository.GetByIdAsync(promotionId);
            if (promotion is null) return null;

            return await BuildResponseDtoAsync(promotion);
        }

        // ── Creación ───────────────────────────────────────────────────────────

        // Valida aeropuertos y fechas antes de insertar; toda promoción nueva inicia activa
        public async Task<int> CreatePromotionAsync(CreatePromotionDto dto)
        {
            await ValidarAeropuertosAsync(dto.OriginAirportId, dto.DestinationAirportId);
            ValidarFechas(dto.StartDate, dto.EndDate);

            var promotion = new Promotion
            {
                Price                = dto.Price,
                StartDate            = dto.StartDate,
                EndDate              = dto.EndDate,
                Image                = dto.Image,
                IsActive             = true,
                OriginAirportId      = dto.OriginAirportId,
                DestinationAirportId = dto.DestinationAirportId
            };

            return await _promotionRepository.CreateAsync(promotion);
        }

        // ── Edición ────────────────────────────────────────────────────────────

        // Verifica que la promoción exista, valida reglas y aplica los cambios
        public async Task UpdatePromotionAsync(int promotionId, UpdatePromotionDto dto)
        {
            // Lanza KeyNotFoundException si no existe; el middleware lo convierte en 404
            var existing = await _promotionRepository.GetByIdAsync(promotionId)
                ?? throw new KeyNotFoundException($"No existe una promoción con ID {promotionId}.");

            await ValidarAeropuertosAsync(dto.OriginAirportId, dto.DestinationAirportId);
            ValidarFechas(dto.StartDate, dto.EndDate);

            // Sobrescribe todos los campos editables sobre el objeto recuperado de la BD
            existing.Price                = dto.Price;
            existing.StartDate            = dto.StartDate;
            existing.EndDate              = dto.EndDate;
            existing.Image                = dto.Image;
            existing.IsActive             = dto.IsActive;
            existing.OriginAirportId      = dto.OriginAirportId;
            existing.DestinationAirportId = dto.DestinationAirportId;

            await _promotionRepository.UpdateAsync(existing);
        }

        // ── Eliminación ────────────────────────────────────────────────────────

        // Verifica que la promoción exista antes de intentar eliminarla
        public async Task DeletePromotionAsync(int promotionId)
        {
            var existing = await _promotionRepository.GetByIdAsync(promotionId)
                ?? throw new KeyNotFoundException($"No existe una promoción con ID {promotionId}.");

            await _promotionRepository.DeleteAsync(existing.PromotionId);
        }

        // ── Métodos privados de apoyo ──────────────────────────────────────────

        // Construye el DTO de respuesta resolviendo los nombres de origen y destino desde la BD
        private async Task<PromotionResponseDto> BuildResponseDtoAsync(Promotion promotion)
        {
            var origin      = await _airportRepository.GetByIdAsync(promotion.OriginAirportId);
            var destination = await _airportRepository.GetByIdAsync(promotion.DestinationAirportId);

            return new PromotionResponseDto
            {
                PromotionId = promotion.PromotionId,
                Price       = promotion.Price,
                StartDate   = promotion.StartDate,
                EndDate     = promotion.EndDate,
                Image       = promotion.Image,
                IsActive    = promotion.IsActive,
                Origin = new PromotionAirportDto
                {
                    AirportId = origin?.AirportId ?? 0,
                    Name      = origin?.Name      ?? "Desconocido",
                    Location  = origin?.Location  ?? "Desconocida"
                },
                Destination = new PromotionAirportDto
                {
                    AirportId = destination?.AirportId ?? 0,
                    Name      = destination?.Name      ?? "Desconocido",
                    Location  = destination?.Location  ?? "Desconocida"
                }
            };
        }

        // Verifica que ambos aeropuertos existan en la BD y que sean distintos entre sí
        private async Task ValidarAeropuertosAsync(int originId, int destinationId)
        {
            if (originId == destinationId)
                throw new InvalidOperationException("El aeropuerto de origen y destino no pueden ser el mismo.");

            _ = await _airportRepository.GetByIdAsync(originId)
                ?? throw new InvalidOperationException($"No existe el aeropuerto de origen con ID {originId}.");

            _ = await _airportRepository.GetByIdAsync(destinationId)
                ?? throw new InvalidOperationException($"No existe el aeropuerto de destino con ID {destinationId}.");
        }

        // Verifica que la fecha de fin sea posterior a la fecha de inicio
        private static void ValidarFechas(DateOnly startDate, DateOnly endDate)
        {
            if (endDate <= startDate)
                throw new InvalidOperationException("La fecha de fin debe ser posterior a la fecha de inicio.");
        }
    }
}