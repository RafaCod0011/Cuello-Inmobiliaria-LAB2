using System.ComponentModel.DataAnnotations;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class Propietario
	{
		[Key]
		[Display(Name = "Código Int.")]
		public int IdPropietario { get; set; }
		[Required]
		public string Nombre { get; set; } = "";
		[Required]
		public string Apellido { get; set; } = "";
		[Required]
		public string Dni { get; set; } = "";
		[Display(Name = "Teléfono")]
		public string Telefono { get; set; } = "";
		[Required, EmailAddress]
		public string Email { get; set; } = "";
		[Required(ErrorMessage = "La clave es obligatoria"), DataType(DataType.Password)]
		public string Clave { get; set; } = "";

		public override string ToString()
		{
			//return $"{Apellido}, {Nombre}";
			//return $"{Nombre} {Apellido}";
			var res = $"{Nombre} {Apellido}";
			if(!String.IsNullOrEmpty(Dni)) {
				res += $" ({Dni})";
			}
			return res;
		}
	}
}