// =============================================================================
// File    : PaymentRepository.cs
// Layer   : TECAir.Data → Repositories
// Purpose : Implements payment persistence logic with ADO.NET.
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// SQL-backed implementation of <see cref="IPaymentRepository"/> using native ADO.NET.
    /// </summary>
    public class PaymentRepository(IDbConnectionFactory connectionFactory) : IPaymentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Maps a data reader row into a <see cref="Payment"/> domain object.
        /// </summary>
        private static Payment MapRow(IDataReader reader) => new()
        {
            PaymentId = reader.GetInt32(reader.GetOrdinal("payment_id")),
            ReservationCode = reader.GetString(reader.GetOrdinal("reservation_code")),
            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
            TransactionDate = reader.GetDateTime(reader.GetOrdinal("transaction_date")),
            TransactionReference = reader.GetString(reader.GetOrdinal("transaction_reference")),
            PaymentStatus = Enum.Parse<PaymentStatus>(reader.GetString(reader.GetOrdinal("payment_status")))
        };

        /// <summary>
        /// Creates and adds a command parameter.
        /// </summary>
        private static void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        /// <inheritdoc />
        public async Task<int> CreateAsync(Payment payment)
        {
            const string sql = @"
                INSERT INTO payments (reservation_code, amount, transaction_date, transaction_reference, payment_status)
                VALUES (@ReservationCode, @Amount, @TransactionDate, @TransactionReference, @PaymentStatus)
                RETURNING payment_id;";

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParameter(command, "@ReservationCode", payment.ReservationCode);
            AddParameter(command, "@Amount", payment.Amount);
            AddParameter(command, "@TransactionDate", payment.TransactionDate);
            AddParameter(command, "@TransactionReference", payment.TransactionReference);
            AddParameter(command, "@PaymentStatus", payment.PaymentStatus);

            var result = command.ExecuteScalar();
            if (result == null || result == DBNull.Value)
            {
                throw new InvalidOperationException("Failed to commit and generate a payment sequence identifier.");
            }

            return Convert.ToInt32(result);
        }

        /// <inheritdoc />
        public async Task<Payment?> GetByIdAsync(int paymentId)
        {
            const string sql = "SELECT * FROM payments WHERE payment_id = @PaymentId;";

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@PaymentId", paymentId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapRow(reader);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Payment>> GetByReservationCodeAsync(string reservationCode)
        {
            const string sql = "SELECT * FROM payments WHERE reservation_code = @ReservationCode;";
            var list = new List<Payment>();

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@ReservationCode", reservationCode);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapRow(reader));
            }

            return list;
        }
    }
}
