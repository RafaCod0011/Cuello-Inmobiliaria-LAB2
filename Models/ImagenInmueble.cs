using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class ImagenInmueble
    {
        [Key]
        [Display(Name = "Código de imagen")]
        public int IdImagen { get; set; }

        [Required(ErrorMessage = "La ruta de la imagen es obligatoria")]
        [StringLength(500, ErrorMessage = "La ruta no puede superar los 500 caracteres")]
        [Display(Name = "Ruta de la imagen")]
        public string Ruta { get; set; } = "";

        [Display(Name = "Orden de visualización")]
        public int Orden { get; set; } = 0;

        [Required]
        [Display(Name = "Inmueble")]
        public int IdInmueble { get; set; }

        [ForeignKey(nameof(IdInmueble))]
        public virtual Inmueble? Inmueble { get; set; }
    }
}