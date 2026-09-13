using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class RepositorioInmueble : RepositorioBase, IRepositorioInmueble
    {
        public RepositorioInmueble(IConfiguration configuration) : base(configuration)
        {
        }

        // Validación Direccion
        public bool ExisteDireccion(string direccion, int? idExcluir = null)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(1) FROM Inmueble WHERE Direccion = @direccion";
                if (idExcluir.HasValue)
                    sql += " AND IdInmueble != @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@direccion", direccion);
                    if (idExcluir.HasValue)
                        command.Parameters.AddWithValue("@id", idExcluir.Value);
                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public int Alta(Inmueble i)
        {
            if (ExisteDireccion(i.Direccion))
                throw new Exception("Ya existe un inmueble con esa dirección.");

            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Inmueble 
                    (Direccion, Cupo, PrecioPorDia, PorcentajeReserva, Estado, Latitud, Longitud, IdPropietario, IdTipo) 
                    VALUES (@direccion, @cupo, @precio, @porcentaje, @estado, @latitud, @longitud, @idPropietario, @idTipo);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@direccion", i.Direccion);
                    command.Parameters.AddWithValue("@cupo", i.Cupo);
                    command.Parameters.AddWithValue("@precio", i.PrecioPorDia);
                    command.Parameters.AddWithValue("@porcentaje", i.PorcentajeReserva);
                    command.Parameters.AddWithValue("@estado", i.Estado.ToString());
                    command.Parameters.AddWithValue("@latitud", i.Latitud.HasValue ? (object)i.Latitud.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@longitud", i.Longitud.HasValue ? (object)i.Longitud.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@idPropietario", i.IdPropietario);
                    command.Parameters.AddWithValue("@idTipo", i.IdTipo);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    i.IdInmueble = res;
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
                string sql = $"DELETE FROM Inmueble WHERE {nameof(Inmueble.IdInmueble)} = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Inmueble i)
        {
            if (ExisteDireccion(i.Direccion, i.IdInmueble))
                throw new Exception("Ya existe otro inmueble con esa dirección.");

            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @$"UPDATE Inmueble 
                    SET Direccion=@direccion, Cupo=@cupo, PrecioPorDia=@precio, PorcentajeReserva=@porcentaje, 
                        Estado=@estado, Latitud=@latitud, Longitud=@longitud, 
                        IdPropietario=@idPropietario, IdTipo=@idTipo 
                    WHERE {nameof(Inmueble.IdInmueble)} = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@direccion", i.Direccion);
                    command.Parameters.AddWithValue("@cupo", i.Cupo);
                    command.Parameters.AddWithValue("@precio", i.PrecioPorDia);
                    command.Parameters.AddWithValue("@porcentaje", i.PorcentajeReserva);
                    command.Parameters.AddWithValue("@estado", i.Estado.ToString());
                    command.Parameters.AddWithValue("@latitud", i.Latitud.HasValue ? (object)i.Latitud.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@longitud", i.Longitud.HasValue ? (object)i.Longitud.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@idPropietario", i.IdPropietario);
                    command.Parameters.AddWithValue("@idTipo", i.IdTipo);
                    command.Parameters.AddWithValue("@id", i.IdInmueble);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Inmueble> ObtenerLista(int pagina = 1, int tamPagina = 10)
        {
            List<Inmueble> res = new List<Inmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = $@"SELECT 
                    IdInmueble, Direccion, Cupo, PrecioPorDia, PorcentajeReserva, Estado, 
                    Latitud, Longitud, IdPropietario, IdTipo
                    FROM Inmueble
                    LIMIT {tamPagina} OFFSET {(pagina - 1) * tamPagina}";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapInmueble(reader));
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
                string sql = "SELECT COUNT(IdInmueble) FROM Inmueble";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    connection.Close();
                }
            }
            return res;
        }

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? i = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT 
                    IdInmueble, Direccion, Cupo, PrecioPorDia, PorcentajeReserva, Estado, 
                    Latitud, Longitud, IdPropietario, IdTipo
                    FROM Inmueble
                    WHERE IdInmueble = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        i = MapInmueble(reader);
                    }
                    connection.Close();
                }
            }
            return i;
        }

        public IList<Inmueble> ObtenerPorPropietario(int idPropietario)
        {
            List<Inmueble> res = new List<Inmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdInmueble, Direccion, Cupo, PrecioPorDia, PorcentajeReserva, Estado, 
                    Latitud, Longitud, IdPropietario, IdTipo
                    FROM Inmueble
                    WHERE IdPropietario = @idPropietario";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idPropietario", idPropietario);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapInmueble(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Inmueble> ObtenerPorTipo(int idTipo)
        {
            List<Inmueble> res = new List<Inmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdInmueble, Direccion, Cupo, PrecioPorDia, PorcentajeReserva, Estado, 
                    Latitud, Longitud, IdPropietario, IdTipo
                    FROM Inmueble
                    WHERE IdTipo = @idTipo";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idTipo", idTipo);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapInmueble(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Inmueble> ObtenerDisponibles()
        {
            List<Inmueble> res = new List<Inmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdInmueble, Direccion, Cupo, PrecioPorDia, PorcentajeReserva, Estado, 
                    Latitud, Longitud, IdPropietario, IdTipo
                    FROM Inmueble
                    WHERE Estado = 'Activo'";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapInmueble(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Inmueble> BuscarPorDireccion(string parteDireccion)
        {
            List<Inmueble> res = new List<Inmueble>();
            parteDireccion = "%" + parteDireccion + "%";
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT IdInmueble, Direccion, Cupo, PrecioPorDia, PorcentajeReserva, Estado, 
                    Latitud, Longitud, IdPropietario, IdTipo
                    FROM Inmueble
                    WHERE Direccion LIKE @parte";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@parte", parteDireccion);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapInmueble(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Inmueble> BuscarDisponibles(DateTime desde, DateTime hasta, int? idTipo, int? cupoMin, int pagina, int tamano)
        {
            var res = new List<Inmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT i.IdInmueble, i.Direccion, i.Cupo, i.PrecioPorDia, i.PorcentajeReserva,
                        i.Estado, i.Latitud, i.Longitud, i.IdPropietario, i.IdTipo,
                        t.Nombre AS TipoNombre,
                        (SELECT Ruta FROM ImagenInmueble 
                            WHERE IdInmueble = i.IdInmueble 
                            ORDER BY Orden LIMIT 1) AS PortadaRuta
                    FROM Inmueble i
                    LEFT JOIN TipoInmueble t ON i.IdTipo = t.IdTipo
                    WHERE i.Estado = 'Activo'
                    AND NOT EXISTS (
                        SELECT 1 FROM Reserva r
                        WHERE r.IdInmueble = i.IdInmueble
                            AND (r.FechaTerminacionAnticipada IS NULL OR r.FechaTerminacionAnticipada >= @desde)
                            AND r.FechaInicio <= @hasta
                            AND r.FechaFin >= @desde
                    )
                ";
                if (idTipo.HasValue) sql += " AND i.IdTipo = @idTipo";
                if (cupoMin.HasValue) sql += " AND i.Cupo >= @cupoMin";
                sql += $" LIMIT {tamano} OFFSET {(pagina - 1) * tamano}";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@desde", desde);
                    command.Parameters.AddWithValue("@hasta", hasta);
                    if (idTipo.HasValue) command.Parameters.AddWithValue("@idTipo", idTipo.Value);
                    if (cupoMin.HasValue) command.Parameters.AddWithValue("@cupoMin", cupoMin.Value);

                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var inm = new Inmueble
                        {
                            IdInmueble = reader.GetInt32("IdInmueble"),
                            Direccion = reader.GetString("Direccion"),
                            Cupo = reader.GetInt32("Cupo"),
                            PrecioPorDia = reader.GetDecimal("PrecioPorDia"),
                            PorcentajeReserva = reader.GetDecimal("PorcentajeReserva"),
                            Estado = Enum.Parse<EstadoInmueble>(reader.GetString("Estado")),
                            Latitud = reader.IsDBNull(reader.GetOrdinal("Latitud")) ? null : reader.GetDecimal("Latitud"),
                            Longitud = reader.IsDBNull(reader.GetOrdinal("Longitud")) ? null : reader.GetDecimal("Longitud"),
                            IdPropietario = reader.GetInt32("IdPropietario"),
                            IdTipo = reader.GetInt32("IdTipo")
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("TipoNombre")))
                        {
                            inm.Tipo = new TipoInmueble
                            {
                                IdTipo = inm.IdTipo,
                                Nombre = reader.GetString("TipoNombre")
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("PortadaRuta")))
                        {
                            inm.Imagenes = new List<ImagenInmueble>
                            {
                                new ImagenInmueble
                                {
                                    IdInmueble = inm.IdInmueble,
                                    Ruta = reader.GetString("PortadaRuta"),
                                    Orden = 0
                                }
                            };
                        }

                        res.Add(inm);
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public int ContarDisponibles(DateTime desde, DateTime hasta, int? idTipo, int? cupoMin)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT COUNT(1)
                    FROM Inmueble i
                    WHERE i.Estado = 'Activo'
                    AND NOT EXISTS (
                        SELECT 1 FROM Reserva r
                        WHERE r.IdInmueble = i.IdInmueble
                            AND (r.FechaTerminacionAnticipada IS NULL OR r.FechaTerminacionAnticipada >= @desde)
                            AND r.FechaInicio <= @hasta
                            AND r.FechaFin >= @desde
                    )
                ";
                if (idTipo.HasValue) sql += " AND i.IdTipo = @idTipo";
                if (cupoMin.HasValue) sql += " AND i.Cupo >= @cupoMin";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@desde", desde);
                    command.Parameters.AddWithValue("@hasta", hasta);
                    if (idTipo.HasValue) command.Parameters.AddWithValue("@idTipo", idTipo.Value);
                    if (cupoMin.HasValue) command.Parameters.AddWithValue("@cupoMin", cupoMin.Value);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public IList<Inmueble> BuscarConFiltros(string? direccion, string? estado, int? idTipo, int pagina, int tamano)
        {
            var res = new List<Inmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                    SELECT IdInmueble, Direccion, Cupo, PrecioPorDia, PorcentajeReserva, 
                        Estado, Latitud, Longitud, IdPropietario, IdTipo
                    FROM Inmueble
                    WHERE 1=1
                ";

                if (!string.IsNullOrWhiteSpace(direccion))
                    sql += " AND Direccion LIKE @direccion";
                if (!string.IsNullOrWhiteSpace(estado))
                    sql += " AND Estado = @estado";
                if (idTipo.HasValue)
                    sql += " AND IdTipo = @idTipo";

                sql += $" LIMIT {tamano} OFFSET {(pagina - 1) * tamano}";

                using (var command = new MySqlCommand(sql, connection))
                {
                    if (!string.IsNullOrWhiteSpace(direccion))
                        command.Parameters.AddWithValue("@direccion", "%" + direccion + "%");
                    if (!string.IsNullOrWhiteSpace(estado))
                        command.Parameters.AddWithValue("@estado", estado);
                    if (idTipo.HasValue)
                        command.Parameters.AddWithValue("@idTipo", idTipo.Value);

                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(MapInmueble(reader));
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public int ContarConFiltros(string? direccion, string? estado, int? idTipo)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(IdInmueble) FROM Inmueble WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(direccion))
                    sql += " AND Direccion LIKE @direccion";
                if (!string.IsNullOrWhiteSpace(estado))
                    sql += " AND Estado = @estado";
                if (idTipo.HasValue)
                    sql += " AND IdTipo = @idTipo";

                using (var command = new MySqlCommand(sql, connection))
                {
                    if (!string.IsNullOrWhiteSpace(direccion))
                        command.Parameters.AddWithValue("@direccion", "%" + direccion + "%");
                    if (!string.IsNullOrWhiteSpace(estado))
                        command.Parameters.AddWithValue("@estado", estado);
                    if (idTipo.HasValue)
                        command.Parameters.AddWithValue("@idTipo", idTipo.Value);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        
        // Mapear desde IDataReader
        private Inmueble MapInmueble(MySqlDataReader reader)
        {
            return new Inmueble
            {
                IdInmueble = reader.GetInt32(nameof(Inmueble.IdInmueble)),
                Direccion = reader.GetString(nameof(Inmueble.Direccion)),
                Cupo = reader.GetInt32(nameof(Inmueble.Cupo)),
                PrecioPorDia = reader.GetDecimal(nameof(Inmueble.PrecioPorDia)),
                PorcentajeReserva = reader.GetDecimal(nameof(Inmueble.PorcentajeReserva)),
                Estado = Enum.Parse<EstadoInmueble>(reader.GetString(nameof(Inmueble.Estado))),
                Latitud = reader.IsDBNull(reader.GetOrdinal(nameof(Inmueble.Latitud))) ? (decimal?)null : reader.GetDecimal(nameof(Inmueble.Latitud)),
                Longitud = reader.IsDBNull(reader.GetOrdinal(nameof(Inmueble.Longitud))) ? (decimal?)null : reader.GetDecimal(nameof(Inmueble.Longitud)),
                IdPropietario = reader.GetInt32(nameof(Inmueble.IdPropietario)),
                IdTipo = reader.GetInt32(nameof(Inmueble.IdTipo))
            };
        }
    }
}