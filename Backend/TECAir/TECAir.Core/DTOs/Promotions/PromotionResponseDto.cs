// =============================================================================
// Archivo  : PromotionResponseDto.cs
// Capa     : TECAir.Core → DTOs/Promotions
// Propósito: Define el JSON que la API retorna al consultar promociones.
//            A diferencia del modelo Promotion (que solo guarda IDs de aeropuertos),
//            este DTO incluye nombres y ubicaciones de origen y destino para que
//            el frontend no necesite hacer llamadas adicionales.
// =============================================================================

namespace TECAir.Core.DTOs.Promotions
{
    public class PromotionResponseDto
    {
        public int PromotionId { get; set; }
        public decimal Price { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // Null si la promoción no tiene imagen asociada
        public string? Image { get; set; }
        public bool IsActive { get; set; }

        // Aeropuerto de origen enriquecido con nombre y ubicación
        public PromotionAirportDto Origin { get; set; } = new();

        // Aeropuerto de destino enriquecido con nombre y ubicación
        public PromotionAirportDto Destination { get; set; } = new();
    }

    // Datos resumidos de un aeropuerto dentro de la respuesta de una promoción
    public class PromotionAirportDto
    {
        public int AirportId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}