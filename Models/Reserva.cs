using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class Reserva
    {
        [Key]
        [Display(Name = "Código")]
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [Display(Name = "Fecha de fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El monto diario es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto diario")]
        [DataType(DataType.Currency)]
        public decimal MontoDiario { get; set; }

        // Monto total calculado (días * MontoDiario)
        public decimal MontoTotal => (decimal)((FechaFin - FechaInicio).TotalDays + 1) * MontoDiario;

        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de terminación anticipada")]
        [DataType(DataType.Date)]
        public DateTime? FechaTerminacionAnticipada { get; set; }

        // Claves
        [Required]
        [Display(Name = "Inmueble")]
        public int IdInmueble { get; set; }

        [Required]
        [Display(Name = "Inquilino")]
        public int IdInquilino { get; set; }

        [Required]
        [Display(Name = "Usuario creador")]
        public int IdUsuarioCreacion { get; set; }

        [Display(Name = "Usuario que terminó")]
        public int? IdUsuarioTerminacion { get; set; }

        // Propiedades de navegación (ORM)
        [ForeignKey(nameof(IdInmueble))]
        public virtual Inmueble? Inmueble { get; set; }

        [ForeignKey(nameof(IdInquilino))]
        public virtual Inquilino? Inquilino { get; set; }

        [ForeignKey(nameof(IdUsuarioCreacion))]
        public virtual Usuario? UsuarioCreacion { get; set; }

        [ForeignKey(nameof(IdUsuarioTerminacion))]
        public virtual Usuario? UsuarioTerminacion { get; set; }

        public virtual ICollection<Pago>? Pagos { get; set; }

        // Propiedades calculadas
        public bool EstaVigente
        {
            get
            {
                if (FechaTerminacionAnticipada.HasValue)
                    return FechaTerminacionAnticipada.Value > DateTime.Today;
                return FechaFin >= DateTime.Today && FechaInicio <= DateTime.Today;
            }
        }

        public bool EstaTerminada
        {
            get { return FechaTerminacionAnticipada.HasValue; }
        }

        public bool EsProxima
        {
            get { return !EstaTerminada && FechaInicio > DateTime.Today; }
        }

        public override string ToString()
        {
            return $"Reserva #{IdReserva} - {Inmueble?.Direccion ?? "N/A"}";
        }
    }
}