using System.ComponentModel.DataAnnotations;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class LoginView
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        [Display(Name = "Email")]
        public string Usuario { get; set; } = "";

        [Required(ErrorMessage = "La clave es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Clave")]
        public string Clave { get; set; } = "";
    }
}