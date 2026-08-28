using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class RepositorioReserva : RepositorioBase, IRepositorioReserva
    {
        public RepositorioReserva(IConfiguration configuration) : base(configuration)
        {
        }

        public bool EstaDisponible(int idInmueble, DateTime fechaInicio, DateTime fechaFin, int? idReservaExcluir = null)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT COUNT(1) FROM Reserva 
                    WHERE IdInmueble = @idInmueble
                    AND (FechaTerminacionAnticipada IS NULL OR FechaTerminacionAnticipada >= @fechaInicio)
                    AND FechaFin >= @fechaInicio
                    AND FechaInicio <= @fechaFin
                ";
                if (idReservaExcluir.HasValue)
                    sql += " AND IdReserva != @idExcluir";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", idInmueble);
                    command.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    command.Parameters.AddWithValue("@fechaFin", fechaFin);
                    if (idReservaExcluir.HasValue)
                        command.Parameters.AddWithValue("@idExcluir", idReservaExcluir.Value);

                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count == 0;
                }
            }
        }

        public int Alta(Reserva r)
        {
            // Valores por defecto al no tener autenticacion implementada todavia.
            if (r.IdUsuarioCreacion <= 0)
                r.IdUsuarioCreacion = 1;
            if (!EstaDisponible(r.IdInmueble, r.FechaInicio, r.FechaFin))
                throw new Exception("El inmueble no está disponible en las fechas seleccionadas.");

            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    INSERT INTO Reserva 
                    (FechaInicio, FechaFin, MontoDiario, FechaCreacion, IdInmueble, IdInquilino, IdUsuarioCreacion)
                    VALUES (@fechaInicio, @fechaFin, @montoDiario, @fechaCreacion, @idInmueble, @idInquilino, @idUsuarioCreacion);
                    SELECT LAST_INSERT_ID();
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@fechaInicio", r.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFin", r.FechaFin);
                    command.Parameters.AddWithValue("@montoDiario", r.MontoDiario);
                    command.Parameters.AddWithValue("@fechaCreacion", r.FechaCreacion);
                    command.Parameters.AddWithValue("@idInmueble", r.IdInmueble);
                    command.Parameters.AddWithValue("@idInquilino", r.IdInquilino);
                    command.Parameters.AddWithValue("@idUsuarioCreacion", r.IdUsuarioCreacion);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    r.IdReserva = res;
                    connection.Close();
                }
            }
            return res;
        }

        public int Baja(int id)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "DELETE FROM Reserva WHERE IdReserva = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Reserva r)
        {
            if (!EstaDisponible(r.IdInmueble, r.FechaInicio, r.FechaFin, r.IdReserva))
                throw new Exception("El inmueble no está disponible en las fechas seleccionadas.");

            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    UPDATE Reserva 
                    SET FechaInicio = @fechaInicio, FechaFin = @fechaFin, MontoDiario = @montoDiario,
                        IdInmueble = @idInmueble, IdInquilino = @idInquilino
                    WHERE IdReserva = @id
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@fechaInicio", r.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFin", r.FechaFin);
                    command.Parameters.AddWithValue("@montoDiario", r.MontoDiario);
                    command.Parameters.AddWithValue("@idInmueble", r.IdInmueble);
                    command.Parameters.AddWithValue("@idInquilino", r.IdInquilino);
                    command.Parameters.AddWithValue("@id", r.IdReserva);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Reserva> ObtenerLista(int pagina = 1, int tamPagina = 10)
        {
            List<Reserva> res = new List<Reserva>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = $@"
                    SELECT IdReserva, FechaInicio, FechaFin, MontoDiario, FechaCreacion, 
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion
                    FROM Reserva
                    LIMIT {tamPagina} OFFSET {(pagina - 1) * tamPagina}
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapReserva(reader));
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
                string sql = "SELECT COUNT(IdReserva) FROM Reserva";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    connection.Close();
                }
            }
            return res;
        }

        public Reserva? ObtenerPorId(int id)
        {
            Reserva? r = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT IdReserva, FechaInicio, FechaFin, MontoDiario, FechaCreacion,
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion
                    FROM Reserva
                    WHERE IdReserva = @id
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        r = MapReserva(reader);
                    }
                    connection.Close();
                }
            }
            return r;
        }

        public IList<Reserva> ObtenerPorInmueble(int idInmueble)
        {
            List<Reserva> res = new List<Reserva>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT IdReserva, FechaInicio, FechaFin, MontoDiario, FechaCreacion,
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion
                    FROM Reserva
                    WHERE IdInmueble = @idInmueble
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", idInmueble);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapReserva(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Reserva> ObtenerPorInquilino(int idInquilino)
        {
            List<Reserva> res = new List<Reserva>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT IdReserva, FechaInicio, FechaFin, MontoDiario, FechaCreacion,
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion
                    FROM Reserva
                    WHERE IdInquilino = @idInquilino
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInquilino", idInquilino);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapReserva(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Reserva> ObtenerVigentes()
        {
            DateTime hoy = DateTime.Today;
            List<Reserva> res = new List<Reserva>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT IdReserva, FechaInicio, FechaFin, MontoDiario, FechaCreacion,
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion
                    FROM Reserva
                    WHERE (FechaTerminacionAnticipada IS NULL OR FechaTerminacionAnticipada >= @hoy)
                    AND FechaFin >= @hoy
                    AND FechaInicio <= @hoy
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@hoy", hoy);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapReserva(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Reserva> ObtenerPorTerminarEn(int dias)
        {
            DateTime hoy = DateTime.Today;
            DateTime fechaLimite = hoy.AddDays(dias);
            List<Reserva> res = new List<Reserva>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT IdReserva, FechaInicio, FechaFin, MontoDiario, FechaCreacion,
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion
                    FROM Reserva
                    WHERE (FechaTerminacionAnticipada IS NULL OR FechaTerminacionAnticipada >= @hoy)
                    AND FechaFin >= @hoy
                    AND FechaFin <= @fechaLimite
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@hoy", hoy);
                    command.Parameters.AddWithValue("@fechaLimite", fechaLimite);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapReserva(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Reserva> BuscarPorFechas(DateTime desde, DateTime hasta)
        {
            List<Reserva> res = new List<Reserva>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT IdReserva, FechaInicio, FechaFin, MontoDiario, FechaCreacion,
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion
                    FROM Reserva
                    WHERE (FechaInicio >= @desde AND FechaInicio <= @hasta)
                       OR (FechaFin >= @desde AND FechaFin <= @hasta)
                       OR (FechaInicio <= @desde AND FechaFin >= @hasta)
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@desde", desde);
                    command.Parameters.AddWithValue("@hasta", hasta);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapReserva(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public void TerminarAnticipadamente(int idReserva, DateTime fechaTerminacion, int idUsuarioTerminacion)
        {       
            // Valores por defecto al no tener autenticacion implementada todavia.
            if (idUsuarioTerminacion <= 0)
                idUsuarioTerminacion = 1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    UPDATE Reserva 
                    SET FechaTerminacionAnticipada = @fechaTerminacion, IdUsuarioTerminacion = @idUsuarioTerminacion
                    WHERE IdReserva = @id
                ";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@fechaTerminacion", fechaTerminacion);
                    command.Parameters.AddWithValue("@idUsuarioTerminacion", idUsuarioTerminacion);
                    command.Parameters.AddWithValue("@id", idReserva);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private Reserva MapReserva(MySqlDataReader reader)
        {
            return new Reserva
            {
                IdReserva = reader.GetInt32(nameof(Reserva.IdReserva)),
                FechaInicio = reader.GetDateTime(nameof(Reserva.FechaInicio)),
                FechaFin = reader.GetDateTime(nameof(Reserva.FechaFin)),
                MontoDiario = reader.GetDecimal(nameof(Reserva.MontoDiario)),
                FechaCreacion = reader.GetDateTime(nameof(Reserva.FechaCreacion)),
                FechaTerminacionAnticipada = reader.IsDBNull(reader.GetOrdinal(nameof(Reserva.FechaTerminacionAnticipada))) ? (DateTime?)null : reader.GetDateTime(nameof(Reserva.FechaTerminacionAnticipada)),
                IdInmueble = reader.GetInt32(nameof(Reserva.IdInmueble)),
                IdInquilino = reader.GetInt32(nameof(Reserva.IdInquilino)),
                IdUsuarioCreacion = reader.GetInt32(nameof(Reserva.IdUsuarioCreacion)),
                IdUsuarioTerminacion = reader.IsDBNull(reader.GetOrdinal(nameof(Reserva.IdUsuarioTerminacion))) ? (int?)null : reader.GetInt32(nameof(Reserva.IdUsuarioTerminacion))
            };
        }
    }
}