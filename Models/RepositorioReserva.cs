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
            if (!EstaDisponible(r.IdInmueble, r.FechaInicio, r.FechaFin))
                throw new Exception("El inmueble no está disponible en las fechas seleccionadas.");

            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    INSERT INTO Reserva 
                    (FechaInicio, FechaFin, MontoDiario, FechaCreacion, IdInmueble, IdInquilino, IdUsuarioCreacion, IdReservaOrigen)
                    VALUES (@fechaInicio, @fechaFin, @montoDiario, @fechaCreacion, @idInmueble, @idInquilino, @idUsuarioCreacion, @idReservaOrigen);
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
                    command.Parameters.AddWithValue("@idReservaOrigen", r.IdReservaOrigen);
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
            var reserva = ObtenerPorId(id);
            if (reserva == null)
                throw new Exception("La reserva no existe.");

            if (reserva.EstaVigente)
                throw new Exception("No se puede eliminar una reserva que está vigente.");

            // Verificar si esta reserva tiene extensiones
            var todas = ObtenerLista(1, int.MaxValue);
            bool tieneExtensiones = todas.Any(r => r.IdReservaOrigen == id);
            if (tieneExtensiones)
                throw new Exception("No se puede eliminar una reserva que tiene extensiones asociadas.");

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
                    SELECT r.IdReserva, r.FechaInicio, r.FechaFin, r.MontoDiario, r.FechaCreacion, 
                        r.FechaTerminacionAnticipada, r.IdInmueble, r.IdInquilino, 
                        r.IdUsuarioCreacion, r.IdUsuarioTerminacion, r.IdReservaOrigen,
                        u1.Nombre AS NombreCreador, u1.Apellido AS ApellidoCreador,
                        u2.Nombre AS NombreTerminador, u2.Apellido AS ApellidoTerminador
                    FROM Reserva r
                    LEFT JOIN Usuario u1 ON r.IdUsuarioCreacion = u1.IdUsuario
                    LEFT JOIN Usuario u2 ON r.IdUsuarioTerminacion = u2.IdUsuario
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
                    SELECT r.IdReserva, r.FechaInicio, r.FechaFin, r.MontoDiario, r.FechaCreacion,
                        r.FechaTerminacionAnticipada, r.IdInmueble, r.IdInquilino, 
                        r.IdUsuarioCreacion, r.IdUsuarioTerminacion, r.IdReservaOrigen,
                        u1.Nombre AS NombreCreador, u1.Apellido AS ApellidoCreador,
                        u2.Nombre AS NombreTerminador, u2.Apellido AS ApellidoTerminador
                    FROM Reserva r
                    LEFT JOIN Usuario u1 ON r.IdUsuarioCreacion = u1.IdUsuario
                    LEFT JOIN Usuario u2 ON r.IdUsuarioTerminacion = u2.IdUsuario
                    WHERE r.IdReserva = @id
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
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion, IdReservaOrigen
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
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion, IdReservaOrigen
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
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion, IdReservaOrigen
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
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion, IdReservaOrigen
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
                           FechaTerminacionAnticipada, IdInmueble, IdInquilino, IdUsuarioCreacion, IdUsuarioTerminacion, IdReservaOrigen
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

        public IList<Reserva> BuscarConFiltros(string? estado, DateTime? desde, DateTime? hasta, int? porTerminarDias, int pagina, int tamano)
        {
            var res = new List<Reserva>();
            using (var connection = new MySqlConnection(connectionString))
            {
                var (whereSql, parametros) = ConstruirFiltros(estado, desde, hasta, porTerminarDias);

                string sql = $@"
                    SELECT r.IdReserva, r.FechaInicio, r.FechaFin, r.MontoDiario, r.FechaCreacion, 
                        r.FechaTerminacionAnticipada, r.IdInmueble, r.IdInquilino, 
                        r.IdUsuarioCreacion, r.IdUsuarioTerminacion, r.IdReservaOrigen,
                        u1.Nombre AS NombreCreador, u1.Apellido AS ApellidoCreador,
                        u2.Nombre AS NombreTerminador, u2.Apellido AS ApellidoTerminador
                    FROM Reserva r
                    LEFT JOIN Usuario u1 ON r.IdUsuarioCreacion = u1.IdUsuario
                    LEFT JOIN Usuario u2 ON r.IdUsuarioTerminacion = u2.IdUsuario
                    WHERE 1=1 {whereSql}
                    LIMIT {tamano} OFFSET {(pagina - 1) * tamano}
                ";

                using (var command = new MySqlCommand(sql, connection))
                {
                    foreach (var p in parametros)
                        command.Parameters.AddWithValue(p.Key, p.Value);

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

        public int ContarConFiltros(string? estado, DateTime? desde, DateTime? hasta, int? porTerminarDias)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var (whereSql, parametros) = ConstruirFiltros(estado, desde, hasta, porTerminarDias);

                string sql = $"SELECT COUNT(IdReserva) FROM Reserva r WHERE 1=1 {whereSql}";

                using (var command = new MySqlCommand(sql, connection))
                {
                    foreach (var p in parametros)
                        command.Parameters.AddWithValue(p.Key, p.Value);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        // Método auxiliar WHERE dinámico
        private (string whereSql, Dictionary<string, object> parametros) ConstruirFiltros(
            string? estado, DateTime? desde, DateTime? hasta, int? porTerminarDias)
        {
            var where = "";
            var parametros = new Dictionary<string, object>();
            var hoy = DateTime.Today;

            // Filtro por estado
            if (!string.IsNullOrWhiteSpace(estado))
            {
                switch (estado)
                {
                    case "Vigente":
                        where += " AND r.FechaTerminacionAnticipada IS NULL " +
                                " AND r.FechaInicio <= @hoy AND r.FechaFin >= @hoy";
                        parametros["@hoy"] = hoy;
                        break;
                    case "Proxima":
                        where += " AND r.FechaTerminacionAnticipada IS NULL AND r.FechaInicio > @hoy";
                        parametros["@hoy"] = hoy;
                        break;
                    case "Finalizada":
                        where += " AND r.FechaTerminacionAnticipada IS NULL AND r.FechaFin < @hoy";
                        parametros["@hoy"] = hoy;
                        break;
                    case "Terminada":
                        where += " AND r.FechaTerminacionAnticipada IS NOT NULL";
                        break;
                }
            }

            // Filtro rango de fechas
            if (desde.HasValue)
            {
                where += " AND r.FechaFin >= @desde";
                parametros["@desde"] = desde.Value;
            }
            if (hasta.HasValue)
            {
                where += " AND r.FechaInicio <= @hasta";
                parametros["@hasta"] = hasta.Value;
            }

            // Filtro "por terminar en X días"
            if (porTerminarDias.HasValue && porTerminarDias.Value > 0)
            {
                where += " AND r.FechaTerminacionAnticipada IS NULL " +
                        " AND r.FechaFin BETWEEN @hoy AND @fechaLimite";
                parametros["@hoy"] = hoy;
                parametros["@fechaLimite"] = hoy.AddDays(porTerminarDias.Value);
            }

            return (where, parametros);
        }

        private Reserva MapReserva(MySqlDataReader reader)
        {
            var r = new Reserva
            {
                IdReserva = reader.GetInt32("IdReserva"),
                FechaInicio = reader.GetDateTime("FechaInicio"),
                FechaFin = reader.GetDateTime("FechaFin"),
                MontoDiario = reader.GetDecimal("MontoDiario"),
                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                FechaTerminacionAnticipada = reader.IsDBNull(reader.GetOrdinal("FechaTerminacionAnticipada"))
                    ? (DateTime?)null
                    : reader.GetDateTime("FechaTerminacionAnticipada"),
                IdInmueble = reader.GetInt32("IdInmueble"),
                IdInquilino = reader.GetInt32("IdInquilino"),
                IdUsuarioCreacion = reader.GetInt32("IdUsuarioCreacion"),
                IdUsuarioTerminacion = reader.IsDBNull(reader.GetOrdinal("IdUsuarioTerminacion"))
                    ? (int?)null
                    : reader.GetInt32("IdUsuarioTerminacion")
            };

            // Cargar IdReservaOrigen si existe en el SELECT
            if (reader.GetOrdinal("IdReservaOrigen") >= 0 && !reader.IsDBNull(reader.GetOrdinal("IdReservaOrigen")))
            {
                r.IdReservaOrigen = reader.GetInt32("IdReservaOrigen");
            }

            // Cargar el usuario que creó la reserva
            int idxNombreCreador = reader.GetOrdinal("NombreCreador");
            if (!reader.IsDBNull(idxNombreCreador))
            {
                r.UsuarioCreacion = new Usuario
                {
                    Id = r.IdUsuarioCreacion,
                    Nombre = reader.GetString("NombreCreador"),
                    Apellido = reader.GetString("ApellidoCreador")
                };
            }

            // Cargar el usuario que terminó la reserva (si existe)
            int idxNombreTerminador = reader.GetOrdinal("NombreTerminador");
            if (r.IdUsuarioTerminacion.HasValue && !reader.IsDBNull(idxNombreTerminador))
            {
                r.UsuarioTerminacion = new Usuario
                {
                    Id = r.IdUsuarioTerminacion.Value,
                    Nombre = reader.GetString("NombreTerminador"),
                    Apellido = reader.GetString("ApellidoTerminador")
                };
            }

            return r;
        }
    }
}