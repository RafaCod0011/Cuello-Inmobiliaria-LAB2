using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    [Authorize]
    public class PagoController : Controller
    {
        private readonly IRepositorioPago repositorioPago;
        private readonly IRepositorioReserva repositorioReserva;
        private readonly ILogger<PagoController> logger;

        public PagoController(
            IRepositorioPago repositorioPago,
            IRepositorioReserva repositorioReserva,
            ILogger<PagoController> logger)
        {
            this.repositorioPago = repositorioPago;
            this.repositorioReserva = repositorioReserva;
            this.logger = logger;
        }

        // POST: Pago/Crear
        [HttpPost]
        public IActionResult Crear([FromBody] Pago pago)
        {
            try
            {
                if (pago == null)
                    return BadRequest(new { error = "Datos inválidos" });

                if (string.IsNullOrWhiteSpace(pago.Concepto))
                    return BadRequest(new { error = "El concepto es obligatorio" });

                if (pago.Importe <= 0)
                    return BadRequest(new { error = "El importe debe ser mayor a 0" });

                if (pago.IdReserva <= 0)
                    return BadRequest(new { error = "La reserva es obligatoria" });

                pago.FechaPago = pago.FechaPago == default ? DateTime.Today : pago.FechaPago;
                pago.FechaCreacion = DateTime.Now;
                pago.Anulado = false;
                pago.IdUsuarioCreacion = User.UsuarioId();

                var id = repositorioPago.Alta(pago);

                return Json(new { success = true, id = id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear pago");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: Pago/EditarConcepto
        [HttpPost]
        public IActionResult EditarConcepto([FromBody] Pago pago)
        {
            try
            {
                if (pago == null || pago.IdPago <= 0)
                    return BadRequest(new { error = "Datos inválidos" });

                if (string.IsNullOrWhiteSpace(pago.Concepto))
                    return BadRequest(new { error = "El concepto es obligatorio" });

                var existente = repositorioPago.ObtenerPorId(pago.IdPago);
                if (existente == null)
                    return NotFound(new { error = "Pago no encontrado" });

                if (existente.Anulado)
                    return BadRequest(new { error = "No se puede editar un pago anulado" });

                // Solo se puede editar el concepto
                existente.Concepto = pago.Concepto;
                repositorioPago.Modificacion(existente);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al editar pago");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: Pago/Anular/5
        [HttpPost]
        public IActionResult Anular(int id)
        {
            try
            {
                var pago = repositorioPago.ObtenerPorId(id);
                if (pago == null)
                    return NotFound(new { error = "Pago no encontrado" });

                if (pago.Anulado)
                    return BadRequest(new { error = "El pago ya está anulado" });

                repositorioPago.Anular(id, User.UsuarioId());

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al anular pago");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: Pago/ObtenerPorReserva/5
        [HttpGet]
        public IActionResult ObtenerPorReserva(int idReserva)
        {
            try
            {
                var pagos = repositorioPago.ObtenerPorReserva(idReserva);
                var resultado = pagos.Select(p => new
                {
                    p.IdPago,
                    p.Concepto,
                    FechaPago = p.FechaPago.ToString("dd/MM/yyyy"),
                    p.Importe,
                    p.Anulado,
                    FechaCreacion = p.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                    FechaAnulacion = p.FechaAnulacion?.ToString("dd/MM/yyyy HH:mm"),
                    UsuarioCreacion = p.UsuarioCreacion != null
                        ? $"{p.UsuarioCreacion.Nombre} {p.UsuarioCreacion.Apellido}"
                        : "N/A",
                    UsuarioAnulacion = p.UsuarioAnulacion != null
                        ? $"{p.UsuarioAnulacion.Nombre} {p.UsuarioAnulacion.Apellido}"
                        : null
                });
                return Json(resultado);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener pagos");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}