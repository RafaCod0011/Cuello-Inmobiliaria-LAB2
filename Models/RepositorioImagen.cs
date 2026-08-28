using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class RepositorioImagen : RepositorioBase, IRepositorioImagen
    {
        public RepositorioImagen(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(ImagenInmueble p)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO ImagenInmueble (IdInmueble, Ruta, Orden) 
                               VALUES (@inmuebleId, @ruta, @orden);
                               SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@inmuebleId", p.IdInmueble);
                    command.Parameters.AddWithValue("@ruta", p.Ruta);
                    command.Parameters.AddWithValue("@orden", p.Orden);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    p.IdImagen = res;
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
                string sql = $"DELETE FROM ImagenInmueble WHERE IdImagen = @id";
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

        public int Modificacion(ImagenInmueble p)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE ImagenInmueble SET Ruta = @ruta, Orden = @orden WHERE IdImagen = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", p.IdImagen);
                    command.Parameters.AddWithValue("@ruta", p.Ruta);
                    command.Parameters.AddWithValue("@orden", p.Orden);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public ImagenInmueble? ObtenerPorId(int id)
        {
            ImagenInmueble? res = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT IdImagen, IdInmueble, Ruta, Orden FROM ImagenInmueble WHERE IdImagen = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        res = new ImagenInmueble
                        {
                            IdImagen = reader.GetInt32("IdImagen"),
                            IdInmueble = reader.GetInt32("IdInmueble"),
                            Ruta = reader.GetString("Ruta"),
                            Orden = reader.GetInt32("Orden")
                        };
                    }
                    connection.Close();
                }
            }
            return res;
        }

        public IList<ImagenInmueble> ObtenerLista(int paginaNro = 1, int tamPagina = 10)
        {
            List<ImagenInmueble> res = new List<ImagenInmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = $@"SELECT IdImagen, IdInmueble, Ruta, Orden 
                                FROM ImagenInmueble 
                                ORDER BY IdImagen 
                                LIMIT {tamPagina} OFFSET {(paginaNro - 1) * tamPagina}";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(new ImagenInmueble
                        {
                            IdImagen = reader.GetInt32("IdImagen"),
                            IdInmueble = reader.GetInt32("IdInmueble"),
                            Ruta = reader.GetString("Ruta"),
                            Orden = reader.GetInt32("Orden")
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
                string sql = "SELECT COUNT(IdImagen) FROM ImagenInmueble";
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

        public IList<ImagenInmueble> BuscarPorInmueble(int inmuebleId)
        {
            List<ImagenInmueble> res = new List<ImagenInmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = "SELECT IdImagen, IdInmueble, Ruta, Orden FROM ImagenInmueble WHERE IdInmueble = @inmuebleId ORDER BY Orden";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inmuebleId", inmuebleId);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(new ImagenInmueble
                        {
                            IdImagen = reader.GetInt32("IdImagen"),
                            IdInmueble = reader.GetInt32("IdInmueble"),
                            Ruta = reader.GetString("Ruta"),
                            Orden = reader.GetInt32("Orden")
                        });
                    }
                    connection.Close();
                }
            }
            return res;
        }
        public IList<ImagenInmueble> BuscarPorInmuebleIds(IList<int> ids)
        {
            if (ids == null || !ids.Any()) return new List<ImagenInmueble>();

            var res = new List<ImagenInmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string idsString = string.Join(",", ids);
                string sql = $"SELECT IdImagen, IdInmueble, Ruta, Orden FROM ImagenInmueble WHERE IdInmueble IN ({idsString}) ORDER BY Orden";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        res.Add(new ImagenInmueble
                        {
                            IdImagen = reader.GetInt32("IdImagen"),
                            IdInmueble = reader.GetInt32("IdInmueble"),
                            Ruta = reader.GetString("Ruta"),
                            Orden = reader.GetInt32("Orden")
                        });
                    }
                    connection.Close();
                }
            }
            return res;
        }
    }
}