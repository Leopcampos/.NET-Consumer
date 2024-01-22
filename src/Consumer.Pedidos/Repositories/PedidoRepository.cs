using Dapper;
using System.Data.SqlClient;

namespace Consumer.Pedidos.Repositories;

public class PedidoRepository
{
    private readonly string? _connectionString;

    public PedidoRepository(string? connectionString)
    {
        _connectionString = connectionString;
    }

    public void Update(Guid pedidoId, Guid transactionId, int? status)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = @"
                    UPDATE Pedido 
                    SET
                        ProtocoloPagamento = @transactionId,
                        Status = @status
                    WHERE
                        Id = @pedidoId
                ";

            connection.Execute(query, new { pedidoId, transactionId, status });
        }
    }
}