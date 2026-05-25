// =============================================================================
// Archivo  : Airplane.cs
// Capa     : TECAir.Data → Models
// Propósito: Modelo que representa un avión de la flota de TECAir.
//            Mapea la tabla AVION de la base de datos.
//            Se usa para validar que el avión asignado a un vuelo exista
//            y para conocer la capacidad máxima de pasajeros.
// =============================================================================

namespace TECAir.Data.Models
{
    public class Airplane
    {
        // Matrícula única del avión, clave primaria en la BD. Ej: "TEC-001"
        public string PlateNumber { get; set; } = string.Empty;

        // Número máximo de pasajeros que puede transportar
        public int PassengerCapacity { get; set; }

        // Número total de asientos físicos del avión
        public int SeatCount { get; set; }
    }
}