using TECAir.Core.DTOs.Payments;

namespace TECAir.Core.Interfaces
{
    /// <summary>
    /// Coordinates transaction processing execution, credit card validation simulation, 
    /// and core ledger tracking management for user bookings.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Orchestrates the end-to-end clearing pipeline for a customer credit card settlement payload.
        /// </summary>
        /// <param name="dto">The data payload context containing valid payment amounts and sensitive card parameters.</param>
        /// <returns>True if the processing settlement finishes with zero anomalous system breaks, otherwise false.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the associated reservation code does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the booking was settled beforehand or the banking rules reject the card profile.</exception>
        Task<bool> ProcessReservationPaymentAsync(ProcessPaymentDto dto);
    }
}
