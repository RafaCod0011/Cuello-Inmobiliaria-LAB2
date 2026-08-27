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
    }
}