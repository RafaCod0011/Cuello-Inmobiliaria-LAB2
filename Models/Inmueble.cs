using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public enum EstadoInmueble
    {
        Activo,
        Suspendido
    }

    public class Inmueble
    {
        [Key]
        [Display(Name = "Código")]
        public int IdInmueble { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(255, ErrorMessage = "La dirección no puede superar los 255 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = "";

        [Required(ErrorMessage = "El cupo es obligatorio")]
        [Range(1, 50, ErrorMessage = "El cupo debe ser entre 1 y 50 personas")]
        [Display(Name = "Cupo (personas)")]
        public int Cupo { get; set; }

        [Required(ErrorMessage = "El precio por día es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio por día")]
        [DataType(DataType.Currency)]
        public decimal PrecioPorDia { get; set; }

        [Required(ErrorMessage = "El porcentaje de reserva es obligatorio")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        [Display(Name = "Porcentaje de reserva (%)")]
        public decimal PorcentajeReserva { get; set; } = 30m;

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Display(Name = "Estado")]
        public EstadoInmueble Estado { get; set; } = EstadoInmueble.Activo;

        [Display(Name = "Latitud")]
        [Range(-90, 90, ErrorMessage = "Latitud fuera de rango (-90 a 90)")]
        public decimal? Latitud { get; set; }

        [Display(Name = "Longitud")]
        [Range(-180, 180, ErrorMessage = "Longitud fuera de rango (-180 a 180)")]
        public decimal? Longitud { get; set; }

        // Claves
        [Required(ErrorMessage = "El propietario es obligatorio")]
        [Display(Name = "Propietario")]
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "El tipo de inmueble es obligatorio")]
        [Display(Name = "Tipo")]
        public int IdTipo { get; set; }

        // Propiedades de navegación (ORM)
        [ForeignKey(nameof(IdPropietario))]
        public virtual Propietario? Propietario { get; set; }

        [ForeignKey(nameof(IdTipo))]
        public virtual TipoInmueble? Tipo { get; set; }

        public virtual ICollection<ImagenInmueble>? Imagenes { get; set; }

        public override string ToString()
        {
            return $"{Direccion} - {Tipo?.Nombre ?? "Sin tipo"}";
        }
    }
}