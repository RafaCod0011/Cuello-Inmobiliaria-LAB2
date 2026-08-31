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
            // Usuario por defecto hasta hacer la autenticacion.
            if (p.IdUsuarioCreacion <= 0)
                p.IdUsuarioCreacion = 1;
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
            return EjecutarAnulacion(id, 0);
        }

        public int Modificacion(Pago p)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
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
                    SELECT IdPago, Concepto, FechaPago, Importe, Anulado, FechaCreacion, 
                           FechaAnulacion, IdReserva, IdUsuarioCreacion, IdUsuarioAnulacion
                    FROM Pago
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
                    SELECT IdPago, Concepto, FechaPago, Importe, Anulado, FechaCreacion,
                           FechaAnulacion, IdReserva, IdUsuarioCreacion, IdUsuarioAnulacion
                    FROM Pago
                    WHERE IdPago = @id
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
                    SELECT IdPago, Concepto, FechaPago, Importe, Anulado, FechaCreacion,
                           FechaAnulacion, IdReserva, IdUsuarioCreacion, IdUsuarioAnulacion
                    FROM Pago
                    WHERE IdReserva = @idReserva
                    ORDER BY FechaPago DESC
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
            EjecutarAnulacion(idPago, idUsuarioAnulacion);
        }

        private int EjecutarAnulacion(int idPago, int idUsuarioAnulacion)
        {
            // Usuario por defecto hasta hacer la autenticacion.
            if (idUsuarioAnulacion <= 0)
            idUsuarioAnulacion = 1; // Usuario por defecto
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    UPDATE Pago 
                    SET Anulado = true, FechaAnulacion = @fechaAnulacion, IdUsuarioAnulacion = @idUsuarioAnulacion
                    WHERE IdPago = @idPago
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
            return res;
        }

        private Pago MapPago(MySqlDataReader reader)
        {
            return new Pago
            {
                IdPago = reader.GetInt32(nameof(Pago.IdPago)),
                Concepto = reader.GetString(nameof(Pago.Concepto)),
                FechaPago = reader.GetDateTime(nameof(Pago.FechaPago)),
                Importe = reader.GetDecimal(nameof(Pago.Importe)),
                Anulado = reader.GetBoolean(nameof(Pago.Anulado)),
                FechaCreacion = reader.GetDateTime(nameof(Pago.FechaCreacion)),
                FechaAnulacion = reader.IsDBNull(reader.GetOrdinal(nameof(Pago.FechaAnulacion))) ? (DateTime?)null : reader.GetDateTime(nameof(Pago.FechaAnulacion)),
                IdReserva = reader.GetInt32(nameof(Pago.IdReserva)),
                IdUsuarioCreacion = reader.GetInt32(nameof(Pago.IdUsuarioCreacion)),
                IdUsuarioAnulacion = reader.IsDBNull(reader.GetOrdinal(nameof(Pago.IdUsuarioAnulacion))) ? (int?)null : reader.GetInt32(nameof(Pago.IdUsuarioAnulacion))
            };
        }
    }
}