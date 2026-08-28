using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class RepositorioTipoInmueble : RepositorioBase, IRepositorioTipoInmueble
    {
        public RepositorioTipoInmueble(IConfiguration configuration) : base(configuration)
        {
        }


        // Alta
        public int Alta(TipoInmueble t)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO TipoInmueble (Nombre) VALUES (@nombre);
                            SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", t.Nombre);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    t.IdTipo = res;
                    connection.Close();
                }
            }
            return res;
        }

        // Baja
        public int Baja(int id)
        {
            if (TieneInmueblesAsociados(id))
                throw new Exception("No se puede eliminar el tipo porque tiene inmuebles asociados.");

            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "DELETE FROM TipoInmueble WHERE IdTipo = @id";
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

        // Modificación
        public int Modificacion(TipoInmueble t)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "UPDATE TipoInmueble SET Nombre = @nombre WHERE IdTipo = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", t.Nombre);
                    command.Parameters.AddWithValue("@id", t.IdTipo);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        // Verificar si un tipo tiene inmuebles asociados
        private bool TieneInmueblesAsociados(int idTipo)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(1) FROM Inmueble WHERE IdTipo = @idTipo";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idTipo", idTipo);
                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public IList<TipoInmueble> ObtenerLista()
        {
            var res = new List<TipoInmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT IdTipo, Nombre FROM TipoInmueble ORDER BY Nombre";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(new TipoInmueble
                        {
                            IdTipo = reader.GetInt32(nameof(TipoInmueble.IdTipo)),
                            Nombre = reader.GetString(nameof(TipoInmueble.Nombre))
                        });
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? t = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT IdTipo, Nombre FROM TipoInmueble WHERE IdTipo = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        t = new TipoInmueble
                        {
                            IdTipo = reader.GetInt32(nameof(TipoInmueble.IdTipo)),
                            Nombre = reader.GetString(nameof(TipoInmueble.Nombre))
                        };
                    }
                    connection.Close();
                }
            }
            return t;
        }

        public IList<TipoInmueble> ObtenerLista(int pagina = 1, int tamPagina = 10)
        {
            var res = new List<TipoInmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = $@"SELECT IdTipo, Nombre 
                                FROM TipoInmueble 
                                ORDER BY Nombre
                                LIMIT {tamPagina} OFFSET {(pagina - 1) * tamPagina}";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(new TipoInmueble
                        {
                            IdTipo = reader.GetInt32(nameof(TipoInmueble.IdTipo)),
                            Nombre = reader.GetString(nameof(TipoInmueble.Nombre))
                        });
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
                string sql = "SELECT COUNT(IdTipo) FROM TipoInmueble";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    connection.Close();
                }
            }
            return res;
        }
    }
}