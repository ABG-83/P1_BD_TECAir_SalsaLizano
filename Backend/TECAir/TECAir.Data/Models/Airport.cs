namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents an airport facility within the TECAir system.
    /// </summary>
    public class Airport
    {
        /// <summary>
        /// Unique 3-letter IATA code representing the airport (e.g., SJO, LIR, LAX) 
        /// which acts as the primary key.
        /// </summary>
        public int AirportId { get; set; }

        /// <summary>
        /// Official commercial name of the airport facility.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Geographic location, specifying the city or region and country.
        /// </summary>
        public string Location { get; set; } = string.Empty;
    }
}
