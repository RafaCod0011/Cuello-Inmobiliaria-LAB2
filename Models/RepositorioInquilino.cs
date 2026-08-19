using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class RepositorioInquilino : RepositorioBase, IRepositorioInquilino
    {
        public RepositorioInquilino(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Inquilino i)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Inquilino 
                    (Nombre, Apellido, Dni, Telefono, Email) 
                    VALUES (@nombre, @apellido, @dni, @telefono, @email);
                    SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@telefono", i.Telefono ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@email", i.Email);
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    i.IdInquilino = res;
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
                string sql = @$"DELETE FROM Inquilino WHERE {nameof(Inquilino.IdInquilino)} = @id";
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

        public int Modificacion(Inquilino i)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @$"UPDATE Inquilino 
                    SET Nombre=@nombre, Apellido=@apellido, Dni=@dni, Telefono=@telefono, Email=@email 
                    WHERE {nameof(Inquilino.IdInquilino)} = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@telefono", i.Telefono ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@email", i.Email);
                    command.Parameters.AddWithValue("@id", i.IdInquilino);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Inquilino> ObtenerLista(int pagina = 1, int tamPagina = 10)
        {
            IList<Inquilino> res = new List<Inquilino>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = $@"SELECT 
                    IdInquilino, Nombre, Apellido, Dni, Telefono, Email
                    FROM Inquilino
                    LIMIT {tamPagina} OFFSET {(pagina - 1) * tamPagina}";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Inquilino i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32(nameof(Inquilino.IdInquilino)),
                            Nombre = reader.GetString(nameof(Inquilino.Nombre)),
                            Apellido = reader.GetString(nameof(Inquilino.Apellido)),
                            Dni = reader.GetString(nameof(Inquilino.Dni)),
                            Telefono = reader.IsDBNull(reader.GetOrdinal(nameof(Inquilino.Telefono))) ? null : reader.GetString(nameof(Inquilino.Telefono)),
                            Email = reader.GetString(nameof(Inquilino.Email))
                        };
                        res.Add(i);
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
                string sql = @"SELECT COUNT(IdInquilino) FROM Inquilino";
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

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? i = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT 
                    IdInquilino, Nombre, Apellido, Dni, Telefono, Email 
                    FROM Inquilino
                    WHERE IdInquilino = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32(nameof(Inquilino.IdInquilino)),
                            Nombre = reader.GetString(nameof(Inquilino.Nombre)),
                            Apellido = reader.GetString(nameof(Inquilino.Apellido)),
                            Dni = reader.GetString(nameof(Inquilino.Dni)),
                            Telefono = reader.IsDBNull(reader.GetOrdinal(nameof(Inquilino.Telefono))) ? null : reader.GetString(nameof(Inquilino.Telefono)),
                            Email = reader.GetString(nameof(Inquilino.Email))
                        };
                    }
                    connection.Close();
                }
            }
            return i;
        }

        public Inquilino? ObtenerPorEmail(string email)
        {
            Inquilino? i = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT 
                    IdInquilino, Nombre, Apellido, Dni, Telefono, Email 
                    FROM Inquilino
                    WHERE Email = @email";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@email", DbType.String).Value = email;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32(nameof(Inquilino.IdInquilino)),
                            Nombre = reader.GetString(nameof(Inquilino.Nombre)),
                            Apellido = reader.GetString(nameof(Inquilino.Apellido)),
                            Dni = reader.GetString(nameof(Inquilino.Dni)),
                            Telefono = reader.IsDBNull(reader.GetOrdinal(nameof(Inquilino.Telefono))) ? null : reader.GetString(nameof(Inquilino.Telefono)),
                            Email = reader.GetString(nameof(Inquilino.Email))
                        };
                    }
                    connection.Close();
                }
            }
            return i;
        }

        public IList<Inquilino> BuscarPorNombre(string nombre)
        {
            List<Inquilino> res = new List<Inquilino>();
            nombre = "%" + nombre + "%";
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                    IdInquilino, Nombre, Apellido, Dni, Telefono, Email 
                    FROM Inquilino
                    WHERE Nombre LIKE @nombre OR Apellido LIKE @nombre";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@nombre", DbType.String).Value = nombre;
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Inquilino i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32(nameof(Inquilino.IdInquilino)),
                            Nombre = reader.GetString(nameof(Inquilino.Nombre)),
                            Apellido = reader.GetString(nameof(Inquilino.Apellido)),
                            Dni = reader.GetString(nameof(Inquilino.Dni)),
                            Telefono = reader.IsDBNull(reader.GetOrdinal(nameof(Inquilino.Telefono))) ? null : reader.GetString(nameof(Inquilino.Telefono)),
                            Email = reader.GetString(nameof(Inquilino.Email))
                        };
                        res.Add(i);
                    }
                    connection.Close();
                }
            }
            return res;
        }
    }
}