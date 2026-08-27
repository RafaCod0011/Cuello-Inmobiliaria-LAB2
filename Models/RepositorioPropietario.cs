using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace Cuello_Inmobiliaria_LAB2.Models
{
	public class RepositorioPropietario : RepositorioBase, IRepositorioPropietario
	{
		public RepositorioPropietario(IConfiguration configuration) : base(configuration)
		{
			//https://www.nuget.org/packages/MySql.Data/
			//https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql/
		}

		public int Alta(Propietario p)
		{
			if (ExisteDni(p.Dni))
			throw new Exception("Ya existe un propietario con ese DNI.");
			if (ExisteEmail(p.Email))
			throw new Exception("Ya existe un propietario con ese Email.");
			int res = -1;
			using (var connection = new MySqlConnection(connectionString))
			{
				string sql = @"INSERT INTO Propietario 
					(Nombre, Apellido, Dni, Telefono, Email) 
					VALUES (@nombre, @apellido, @dni, @telefono, @email);
					SELECT LAST_INSERT_ID();";//devuelve el id insertado (SCOPE_IDENTITY para sql)
				using (var command = new MySqlCommand(sql, connection))
				{
					command.CommandType = CommandType.Text;
					command.Parameters.AddWithValue("@nombre", p.Nombre);
					command.Parameters.AddWithValue("@apellido", p.Apellido);
					command.Parameters.AddWithValue("@dni", p.Dni);
					command.Parameters.AddWithValue("@telefono", p.Telefono ?? (object)DBNull.Value);
					command.Parameters.AddWithValue("@email", p.Email);
					connection.Open();
					res = Convert.ToInt32(command.ExecuteScalar());
					p.IdPropietario = res;
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
				string sql = @$"DELETE FROM Propietario WHERE {nameof(Propietario.IdPropietario)} = @id";
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
		public int Modificacion(Propietario p)
		{
			if (ExisteDni(p.Dni, p.IdPropietario))
			throw new Exception("Ya existe otro propietario con ese DNI.");
			if (ExisteEmail(p.Email, p.IdPropietario))
			throw new Exception("Ya existe otro propietario con ese Email.");
			int res = -1;
			using (var connection = new MySqlConnection(connectionString))
			{
				string sql = @$"UPDATE Propietario 
					SET Nombre=@nombre, Apellido=@apellido, Dni=@dni, Telefono=@telefono, Email=@email
					WHERE {nameof(Propietario.IdPropietario)} = @id";
				using (var command = new MySqlCommand(sql, connection))
				{
					command.CommandType = CommandType.Text;
					command.Parameters.AddWithValue("@nombre", p.Nombre);
					command.Parameters.AddWithValue("@apellido", p.Apellido);
					command.Parameters.AddWithValue("@dni", p.Dni);
					command.Parameters.AddWithValue("@telefono", p.Telefono ?? (object)DBNull.Value);
					command.Parameters.AddWithValue("@email", p.Email);
					command.Parameters.AddWithValue("@id", p.IdPropietario);
					connection.Open();
					res = command.ExecuteNonQuery();
					connection.Close();
				}
			}
			return res;
		}

		public IList<Propietario> ObtenerLista(int pagina = 1, int tamPagina = 10)
		{
			IList<Propietario> res = new List<Propietario>();
			using (var connection = new MySqlConnection(connectionString))
			{
				string sql = $@"SELECT 
					IdPropietario, Nombre, Apellido, Dni, Telefono, Email
					FROM Propietario
					LIMIT {tamPagina} OFFSET {(pagina - 1) * tamPagina}
				";
				using (var command = new MySqlCommand(sql, connection))
				{
					command.CommandType = CommandType.Text;
					connection.Open();
					var reader = command.ExecuteReader();
					while (reader.Read())
					{
						Propietario p = new Propietario
						{
							IdPropietario = reader.GetInt32(nameof(Propietario.IdPropietario)),
							Nombre = reader.GetString(nameof(Propietario.Nombre)),
							Apellido = reader.GetString(nameof(Propietario.Apellido)),
							Dni = reader.GetString(nameof(Propietario.Dni)),
							Telefono = reader.IsDBNull(reader.GetOrdinal(nameof(Propietario.Telefono))) ? null : reader.GetString(nameof(Propietario.Telefono)),
							Email = reader.GetString(nameof(Propietario.Email)),
						};
						res.Add(p);
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
				string sql = @$"
					SELECT COUNT(IdPropietario)
					FROM Propietario
				";
				using (var command = new MySqlCommand(sql, connection))
				{
					command.CommandType = CommandType.Text;
					connection.Open();
					var reader = command.ExecuteReader();
					if (reader.Read())
					{
						res = reader.GetInt32(0);
					}
					connection.Close();
				}
			}
			return res;
		}

		virtual public Propietario ObtenerPorId(int id)
		{
			Propietario? p = null;
			using (var connection = new MySqlConnection(connectionString))
			{
				string sql = @"SELECT 
					IdPropietario, Nombre, Apellido, Dni, Telefono, Email 
					FROM Propietario
					WHERE IdPropietario=@id";
				using (var command = new MySqlCommand(sql, connection))
				{
					command.Parameters.Add("@id", DbType.Int32).Value = id;
					command.CommandType = CommandType.Text;
					connection.Open();
					var reader = command.ExecuteReader();
					if (reader.Read())
					{
						p = new Propietario
						{
							IdPropietario = reader.GetInt32(nameof(Propietario.IdPropietario)),
							Nombre = reader.GetString("Nombre"),
							Apellido = reader.GetString("Apellido"),
							Dni = reader.GetString("Dni"),
							Telefono = reader.IsDBNull(reader.GetOrdinal(nameof(Propietario.Telefono))) ? null : reader.GetString("Telefono"),
							Email = reader.GetString("Email"),
						};
					}
					connection.Close();
				}
			}
			return p;
		}

		public Propietario ObtenerPorEmail(string email)
		{
			Propietario? p = null;
			using (var connection = new MySqlConnection(connectionString))
			{
				string sql = @$"SELECT 
					{nameof(Propietario.IdPropietario)}, Nombre, Apellido, Dni, Telefono, Email
					FROM Propietario
					WHERE Email=@email";
				using (var command = new MySqlCommand(sql, connection))
				{
					command.CommandType = CommandType.Text;
					command.Parameters.Add("@email", DbType.String).Value = email;
					connection.Open();
					var reader = command.ExecuteReader();
					if (reader.Read())
					{
						p = new Propietario
						{
							IdPropietario = reader.GetInt32(nameof(Propietario.IdPropietario)),//más seguro
							Nombre = reader.GetString("Nombre"),
							Apellido = reader.GetString("Apellido"),
							Dni = reader.GetString("Dni"),
							Telefono = reader.IsDBNull(reader.GetOrdinal(nameof(Propietario.Telefono))) ? null : reader.GetString("Telefono"),
							Email = reader.GetString("Email"),
						};
					}
					connection.Close();
				}
			}
			return p;
		}

		public IList<Propietario> BuscarPorNombre(string nombre)
		{
			List<Propietario> res = new List<Propietario>();
			Propietario? p = null;
			nombre = "%" + nombre + "%";
			using (var connection = new MySqlConnection(connectionString))
			{
				string sql = @"SELECT
					IdPropietario, Nombre, Apellido, Dni, Telefono, Email
					FROM Propietario
					WHERE Nombre LIKE @nombre OR Apellido LIKE @nombre";
				using (var command = new MySqlCommand(sql, connection))
				{
					command.Parameters.Add("@nombre", DbType.String).Value = nombre;
					command.CommandType = CommandType.Text;
					connection.Open();
					var reader = command.ExecuteReader();
					while (reader.Read())
					{
						p = new Propietario
						{
							IdPropietario = reader.GetInt32(nameof(Propietario.IdPropietario)),
							Nombre = reader.GetString("Nombre"),
							Apellido = reader.GetString("Apellido"),
							Dni = reader.GetString("Dni"),
							Telefono = reader.IsDBNull(reader.GetOrdinal(nameof(Propietario.Telefono))) ? null : reader.GetString("Telefono"),
							Email = reader.GetString("Email"),
						};
						res.Add(p);
					}
					connection.Close();
				}
			}
			return res;
		}

		public bool ExisteDni(string dni, int? idExcluir = null)
		{
			using (var connection = new MySqlConnection(connectionString))
			{
				string sql = "SELECT COUNT(1) FROM Propietario WHERE Dni = @dni";
				if (idExcluir.HasValue)
					sql += " AND IdPropietario != @id";
				using (var command = new MySqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@dni", dni);
					if (idExcluir.HasValue)
						command.Parameters.AddWithValue("@id", idExcluir.Value);
					connection.Open();
					int count = Convert.ToInt32(command.ExecuteScalar());
					return count > 0;
				}
			}
		}

		public bool ExisteEmail(string email, int? idExcluir = null)
		{
			using (var connection = new MySqlConnection(connectionString))
			{
				string sql = "SELECT COUNT(1) FROM Propietario WHERE Email = @email";
				if (idExcluir.HasValue)
					sql += " AND IdPropietario != @id";
				using (var command = new MySqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@email", email);
					if (idExcluir.HasValue)
						command.Parameters.AddWithValue("@id", idExcluir.Value);
					connection.Open();
					int count = Convert.ToInt32(command.ExecuteScalar());
					return count > 0;
				}
			}
		}
	}
}
