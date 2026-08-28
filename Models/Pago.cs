using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cuello_Inmobiliaria_LAB2.Models
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        [StringLength(100)]
        public string Concepto { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        [DataType(DataType.Currency)]
        public decimal Importe { get; set; }

        public bool Anulado { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaAnulacion { get; set; }

        [Required]
        public int IdReserva { get; set; }

        [Required]
        public int IdUsuarioCreacion { get; set; }

        public int? IdUsuarioAnulacion { get; set; }

        [ForeignKey(nameof(IdReserva))]
        public virtual Reserva? Reserva { get; set; }

        [ForeignKey(nameof(IdUsuarioCreacion))]
        public virtual Usuario? UsuarioCreacion { get; set; }

        [ForeignKey(nameof(IdUsuarioAnulacion))]
        public virtual Usuario? UsuarioAnulacion { get; set; }
    }
}