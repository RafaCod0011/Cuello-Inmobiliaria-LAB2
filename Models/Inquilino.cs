using System.ComponentModel.DataAnnotations;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class Inquilino
	{
		[Key]
		[Display(Name = "Código")]
		public int IdInquilino { get; set; }

		[Required(ErrorMessage = "El nombre es obligatorio")]
		[RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü'\-\s]{2,50}$", ErrorMessage = "El nombre solo puede contener letras")]
		public string Nombre { get; set; } = "";

		[Required(ErrorMessage = "El apellido es obligatorio")]
		[RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü'\-\s]{2,50}$", ErrorMessage = "El apellido solo puede contener letras")]
		public string Apellido { get; set; } = "";

		[Required(ErrorMessage = "El DNI es obligatorio")]
		[RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe tener entre 7 y 8 dígitos, solo números, sin puntos ni letras")]
		public string Dni { get; set; } = "";

		[Display(Name = "Teléfono")]
		[RegularExpression(@"^(\+?[0-9()\-\s]{6,20})?$", ErrorMessage = "Ingresá un teléfono válido (solo números, espacios, guiones, paréntesis y opcionalmente +)")]
		public string Telefono { get; set; } = "";

		[Required, EmailAddress(ErrorMessage = "Ingresá un email con formato válido")]
		public string Email { get; set; } = "";
	}
}