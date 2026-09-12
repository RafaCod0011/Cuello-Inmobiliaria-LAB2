using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class RepositorioPago : RepositorioBase, IRepositorioPago
    {
        public RepositorioPago(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Pago p)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    INSERT INTO Pago 
                    (Concepto, FechaPago, Importe, Anulado, FechaCreacion, IdReserva, IdUsuarioCreacion)
                    VALUES (@concepto, @fechaPago, @importe, @anulado, @fechaCreacion, @idReserva, @idUsuarioCreacion);
                    SELECT LAST_INSERT_ID();
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@concepto", p.Concepto);
                    command.Parameters.AddWithValue("@fechaPago", p.FechaPago);
                    command.Parameters.AddWithValue("@importe", p.Importe);
                    command.Parameters.AddWithValue("@anulado", p.Anulado);
                    command.Parameters.AddWithValue("@fechaCreacion", p.FechaCreacion);
                    command.Parameters.AddWithValue("@idReserva", p.IdReserva);
                    command.Parameters.AddWithValue("@idUsuarioCreacion", p.IdUsuarioCreacion);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    p.IdPago = res;
                    connection.Close();
                }
            }
            return res;
        }

        public int Baja(int id)
        {
            throw new NotImplementedException("Los pagos no se eliminan, se anulan.");
        }

        public int Modificacion(Pago p)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                // Modificar el concepto de pago
                string sql = "UPDATE Pago SET Concepto = @concepto WHERE IdPago = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@concepto", p.Concepto);
                    command.Parameters.AddWithValue("@id", p.IdPago);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Pago> ObtenerLista(int pagina = 1, int tamPagina = 10)
        {
            List<Pago> res = new List<Pago>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = $@"
                    SELECT p.IdPago, p.Concepto, p.FechaPago, p.Importe, p.Anulado, p.FechaCreacion, 
                           p.FechaAnulacion, p.IdReserva, p.IdUsuarioCreacion, p.IdUsuarioAnulacion,
                           u1.Nombre AS NombreCreador, u1.Apellido AS ApellidoCreador,
                           u2.Nombre AS NombreAnulador, u2.Apellido AS ApellidoAnulador
                    FROM Pago p
                    LEFT JOIN Usuario u1 ON p.IdUsuarioCreacion = u1.IdUsuario
                    LEFT JOIN Usuario u2 ON p.IdUsuarioAnulacion = u2.IdUsuario
                    LIMIT {tamPagina} OFFSET {(pagina - 1) * tamPagina}
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapPago(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public int ObtenerCantidad()
        {
            int res = 0;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(IdPago) FROM Pago";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    connection.Close();
                }
            }
            return res;
        }

        public Pago? ObtenerPorId(int id)
        {
            Pago? p = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT p.IdPago, p.Concepto, p.FechaPago, p.Importe, p.Anulado, p.FechaCreacion,
                           p.FechaAnulacion, p.IdReserva, p.IdUsuarioCreacion, p.IdUsuarioAnulacion,
                           u1.Nombre AS NombreCreador, u1.Apellido AS ApellidoCreador,
                           u2.Nombre AS NombreAnulador, u2.Apellido AS ApellidoAnulador
                    FROM Pago p
                    LEFT JOIN Usuario u1 ON p.IdUsuarioCreacion = u1.IdUsuario
                    LEFT JOIN Usuario u2 ON p.IdUsuarioAnulacion = u2.IdUsuario
                    WHERE p.IdPago = @id
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                        p = MapPago(reader);
                    connection.Close();
                }
            }
            return p;
        }

        public IList<Pago> ObtenerPorReserva(int idReserva)
        {
            List<Pago> res = new List<Pago>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT p.IdPago, p.Concepto, p.FechaPago, p.Importe, p.Anulado, p.FechaCreacion,
                           p.FechaAnulacion, p.IdReserva, p.IdUsuarioCreacion, p.IdUsuarioAnulacion,
                           u1.Nombre AS NombreCreador, u1.Apellido AS ApellidoCreador,
                           u2.Nombre AS NombreAnulador, u2.Apellido AS ApellidoAnulador
                    FROM Pago p
                    LEFT JOIN Usuario u1 ON p.IdUsuarioCreacion = u1.IdUsuario
                    LEFT JOIN Usuario u2 ON p.IdUsuarioAnulacion = u2.IdUsuario
                    WHERE p.IdReserva = @idReserva
                    ORDER BY p.FechaPago DESC
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idReserva", idReserva);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                        res.Add(MapPago(reader));
                    connection.Close();
                }
            }
            return res;
        }

        public void Anular(int idPago, int idUsuarioAnulacion)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    UPDATE Pago 
                    SET Anulado = true, FechaAnulacion = @fechaAnulacion, IdUsuarioAnulacion = @idUsuarioAnulacion
                    WHERE IdPago = @idPago AND Anulado = false
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@fechaAnulacion", DateTime.Now);
                    command.Parameters.AddWithValue("@idUsuarioAnulacion", idUsuarioAnulacion);
                    command.Parameters.AddWithValue("@idPago", idPago);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            if (res <= 0)
                throw new Exception("El pago no existe o ya estaba anulado.");
        }

        private Pago MapPago(MySqlDataReader reader)
        {
            var pago = new Pago
            {
                IdPago = reader.GetInt32("IdPago"),
                Concepto = reader.GetString("Concepto"),
                FechaPago = reader.GetDateTime("FechaPago"),
                Importe = reader.GetDecimal("Importe"),
                Anulado = reader.GetBoolean("Anulado"),
                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                FechaAnulacion = reader.IsDBNull(reader.GetOrdinal("FechaAnulacion")) ? (DateTime?)null : reader.GetDateTime("FechaAnulacion"),
                IdReserva = reader.GetInt32("IdReserva"),
                IdUsuarioCreacion = reader.GetInt32("IdUsuarioCreacion"),
                IdUsuarioAnulacion = reader.IsDBNull(reader.GetOrdinal("IdUsuarioAnulacion")) ? (int?)null : reader.GetInt32("IdUsuarioAnulacion")
            };

            // Usuario que creó el pago
            if (!reader.IsDBNull(reader.GetOrdinal("NombreCreador")))
            {
                pago.UsuarioCreacion = new Usuario
                {
                    Id = pago.IdUsuarioCreacion,
                    Nombre = reader.GetString("NombreCreador"),
                    Apellido = reader.GetString("ApellidoCreador")
                };
            }

            // Usuario que anuló el pago (si aplica)
            if (pago.IdUsuarioAnulacion.HasValue && !reader.IsDBNull(reader.GetOrdinal("NombreAnulador")))
            {
                pago.UsuarioAnulacion = new Usuario
                {
                    Id = pago.IdUsuarioAnulacion.Value,
                    Nombre = reader.GetString("NombreAnulador"),
                    Apellido = reader.GetString("ApellidoAnulador")
                };
            }

            return pago;
        }
    }
}