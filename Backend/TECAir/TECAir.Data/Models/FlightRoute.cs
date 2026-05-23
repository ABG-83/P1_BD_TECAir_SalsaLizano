// =============================================================================
// Archivo  : FlightRoute.cs
// Capa     : TECAir.Data → Models
// Propósito: Modelo que representa una escala intermedia de un vuelo.
//            Mapea la tabla VUELO_ESCALA de la base de datos.
//            La PK es compuesta: (flight_number, airport_id).
//            Permite registrar la ruta completa de un vuelo:
//            origen → escala 1 → escala 2 → ... → destino.
// =============================================================================

namespace TECAir.Data.Models
{
    public class FlightRoute
    {
        // Número del vuelo al que pertenece esta escala. FK → flights
        public string FlightNumber { get; set; } = string.Empty;

        // ID del aeropuerto de escala. FK → airports
        public int AirportId { get; set; }

        // Posición de esta escala en la ruta: 1 = primera escala, 2 = segunda, etc.
        public int StopOrder { get; set; }
    }
}