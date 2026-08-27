using System.ComponentModel.DataAnnotations;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class TipoInmueble
    {
        [Key]
        [Display(Name = "Código de Tipo")]
        public int IdTipo { get; set; }

        [Required(ErrorMessage = "El nombre del tipo es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [Display(Name = "Tipo de Inmueble")]
        public string Nombre { get; set; } = "";

        public override string ToString() => Nombre;
    }
}