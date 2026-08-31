using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    public class ReservaController : Controller
    {
        private readonly IRepositorioReserva repositorio;
        private readonly IRepositorioInmueble repositorioInmueble;
        private readonly IRepositorioInquilino repositorioInquilino;
        private readonly IRepositorioPago repositorioPago;
        private readonly IConfiguration config;
        private readonly ILogger<ReservaController> logger;

        public ReservaController(
            IRepositorioReserva repositorio,
            IRepositorioInmueble repositorioInmueble,
            IRepositorioInquilino repositorioInquilino,
            IRepositorioPago repositorioPago,
            IConfiguration config,
            ILogger<ReservaController> logger)
        {
            this.repositorio = repositorio;
            this.repositorioInmueble = repositorioInmueble;
            this.repositorioInquilino = repositorioInquilino;
            this.repositorioPago = repositorioPago;
            this.config = config;
            this.logger = logger;
        }

        // GET: Reserva/Index
        public ActionResult Index(int pagina = 1)
        {
            try
            {
                var tamaño = 5;
                var lista = repositorio.ObtenerLista(Math.Max(pagina, 1), tamaño);
                ViewBag.Pagina = pagina;
                var total = repositorio.ObtenerCantidad();
                ViewBag.TotalPaginas = total % tamaño == 0 ? total / tamaño : total / tamaño + 1;
                if (TempData.ContainsKey("Mensaje"))
                    ViewBag.Mensaje = TempData["Mensaje"];

                CargarRelaciones(lista);
                return View(lista);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Index de Reserva");
                throw;
            }
        }

        // GET: Reserva/Create
        public ActionResult Create()
        {
            try
            {
                CargarListasDesplegables();
                return View(new Reserva
                {
                    FechaInicio = DateTime.Today,
                    FechaFin = DateTime.Today.AddDays(1)
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create GET");
                throw;
            }
        }

        // POST: Reserva/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Reserva reserva)
        {
            try
            {
                if (reserva.FechaFin < reserva.FechaInicio)
                {
                    ModelState.AddModelError("", "La fecha de fin debe ser posterior a la fecha de inicio.");
                    CargarListasDesplegables();
                    return View(reserva);
                }

                if (ModelState.IsValid)
                {
                    reserva.FechaCreacion = DateTime.Now;

                    repositorio.Alta(reserva);
                    TempData["Mensaje"] = "Reserva creada correctamente";

                    // Registrar pago inicial (seña)
                    RegistrarPagoInicial(reserva);

                    return RedirectToAction(nameof(Index));
                }
                CargarListasDesplegables();
                return View(reserva);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create POST");
                ModelState.AddModelError("", ex.Message);
                CargarListasDesplegables();
                return View(reserva);
            }
        }

        // GET: Reserva/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();

                CargarRelacion(entidad);
                CargarListasDesplegables(entidad.IdInmueble, entidad.IdInquilino);
                
                // Select2
                var inquilinoSeleccionado = repositorioInquilino.ObtenerPorId(entidad.IdInquilino);
                ViewBag.InquilinoSeleccionado = inquilinoSeleccionado?.ToString();
                ViewBag.InquilinoId = entidad.IdInquilino;
                return View(entidad);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Edit GET");
                throw;
            }
        }

        // POST: Reserva/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Reserva entidad)
        {
            if (entidad.FechaFin < entidad.FechaInicio)
            {
                ModelState.AddModelError("", "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (!ModelState.IsValid)
            {
                CargarListasDesplegables(entidad.IdInmueble, entidad.IdInquilino);
                return View(entidad);
            }

            try
            {
                var r = repositorio.ObtenerPorId(id);
                if (r == null)
                    return NotFound();

                r.FechaInicio = entidad.FechaInicio;
                r.FechaFin = entidad.FechaFin;
                r.MontoDiario = entidad.MontoDiario;
                r.IdInmueble = entidad.IdInmueble;
                r.IdInquilino = entidad.IdInquilino;

                repositorio.Modificacion(r);
                TempData["Mensaje"] = "Reserva modificada correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Edit POST");
                ModelState.AddModelError("", ex.Message);
                CargarListasDesplegables(entidad.IdInmueble, entidad.IdInquilino);
                return View(entidad);
            }
        }

        // GET: Reserva/Delete/5
        public ActionResult Eliminar(int id)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();
                CargarRelacion(entidad);
                return View(entidad);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Eliminar GET");
                throw;
            }
        }

        // POST: Reserva/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, Reserva entidad)
        {
            try
            {
                repositorio.Baja(id);
                TempData["Mensaje"] = "Reserva eliminada correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Eliminar POST");
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Reserva/Ver/5
        public ActionResult Ver(int id)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();

                CargarRelacion(entidad);
                entidad.Pagos = repositorioPago.ObtenerPorReserva(id);

                return View(entidad);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Ver");
                throw;
            }
        }

        // POST: Reserva/Terminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Terminar(int id, DateTime fechaTerminacion)
        {
            try
            {
                var reserva = repositorio.ObtenerPorId(id);
                if (reserva == null)
                    return NotFound();

                if (fechaTerminacion < reserva.FechaInicio)
                {
                    TempData["Error"] = "La fecha de terminación debe ser posterior a la fecha de inicio.";
                    return RedirectToAction(nameof(Ver), new { id });
                }

                // Calcular multa
                var montoMulta = CalcularMulta(reserva, fechaTerminacion);

                if (montoMulta > 0)
                {
                    var pago = new Pago
                    {
                        Concepto = "Multa por terminación anticipada",
                        FechaPago = DateTime.Today,
                        Importe = montoMulta,
                        IdReserva = reserva.IdReserva
                    };
                    repositorioPago.Alta(pago);
                }

                // Actualizar reserva
                reserva.FechaTerminacionAnticipada = fechaTerminacion;
                reserva.IdUsuarioTerminacion = 1; // Usuario autenticado
                repositorio.Modificacion(reserva);

                TempData["Mensaje"] = $"Reserva terminada anticipadamente. Multa: {montoMulta:C}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al terminar reserva");
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Ver), new { id });
            }
        }

        // GET: Reserva/Extender
        public ActionResult Extender(int id, int dias, decimal nuevoPrecioDiario)
        {
            try
            {
                var reservaOriginal = repositorio.ObtenerPorId(id);
                if (reservaOriginal == null)
                    return NotFound();

                var nuevaReserva = new Reserva
                {
                    FechaInicio = reservaOriginal.FechaFin.AddDays(1),
                    FechaFin = reservaOriginal.FechaFin.AddDays(dias),
                    MontoDiario = nuevoPrecioDiario,
                    IdInmueble = reservaOriginal.IdInmueble,
                    IdInquilino = reservaOriginal.IdInquilino,
                    IdUsuarioCreacion = 1 // Usuario autenticado
                };

                repositorio.Alta(nuevaReserva);
                TempData["Mensaje"] = $"Reserva extendida. Nueva reserva #{nuevaReserva.IdReserva} creada.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al extender reserva");
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Ver), new { id });
            }
        }

        // Metodos auxiliares.
        private void CargarListasDesplegables(int? inmuebleSeleccionado = null, int? inquilinoSeleccionado = null)
        {
            try
            {
                var inmuebles = repositorioInmueble.ObtenerLista(1, int.MaxValue);
                ViewBag.Inmuebles = inmuebles;

                var inquilinos = repositorioInquilino.ObtenerLista(1, int.MaxValue);
                ViewBag.Inquilinos = inquilinos;

                if (inmuebleSeleccionado.HasValue)
                    ViewBag.InmuebleId = inmuebleSeleccionado.Value;
                if (inquilinoSeleccionado.HasValue)
                    ViewBag.InquilinoId = inquilinoSeleccionado.Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cargar listas desplegables");
            }
        }

        private void CargarRelacion(Reserva reserva)
        {
            if (reserva == null) return;

            if (reserva.IdInmueble > 0 && reserva.Inmueble == null)
                reserva.Inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);

            if (reserva.IdInquilino > 0 && reserva.Inquilino == null)
                reserva.Inquilino = repositorioInquilino.ObtenerPorId(reserva.IdInquilino);
        }

        private void CargarRelaciones(IList<Reserva> reservas)
        {
            if (reservas == null || !reservas.Any()) return;

            var idsInmuebles = reservas.Select(r => r.IdInmueble).Distinct().ToList();
            var inmuebles = repositorioInmueble.ObtenerLista(1, int.MaxValue)
                .Where(i => idsInmuebles.Contains(i.IdInmueble))
                .ToDictionary(i => i.IdInmueble);

            var idsInquilinos = reservas.Select(r => r.IdInquilino).Distinct().ToList();
            var inquilinos = repositorioInquilino.ObtenerLista(1, int.MaxValue)
                .Where(i => idsInquilinos.Contains(i.IdInquilino))
                .ToDictionary(i => i.IdInquilino);

            foreach (var r in reservas)
            {
                if (inmuebles.TryGetValue(r.IdInmueble, out var inm))
                    r.Inmueble = inm;
                if (inquilinos.TryGetValue(r.IdInquilino, out var inq))
                    r.Inquilino = inq;
            }
        }

        private void RegistrarPagoInicial(Reserva reserva)
        {
            var inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
            if (inmueble != null)
            {
                decimal porcentaje = inmueble.PorcentajeReserva;
                decimal importePago = reserva.MontoTotal * (porcentaje / 100);

                if (importePago > 0)
                {
                    var pago = new Pago
                    {
                        Concepto = $"Seña ({porcentaje}%)",
                        FechaPago = DateTime.Today,
                        Importe = importePago,
                        IdReserva = reserva.IdReserva,
                        IdUsuarioCreacion = reserva.IdUsuarioCreacion
                    };
                    repositorioPago.Alta(pago);
                }
            }
        }

        private decimal CalcularMulta(Reserva reserva, DateTime fechaTerminacion)
        {
            int diasOriginales = (int)((reserva.FechaFin - reserva.FechaInicio).TotalDays + 1);
            if (diasOriginales <= 0) return 0;

            int diasTranscurridos = (int)((fechaTerminacion - reserva.FechaInicio).TotalDays + 1);
            if (diasTranscurridos <= 0) return 0;

            int diasRestantes = diasOriginales - diasTranscurridos;
            if (diasRestantes <= 0) return 0;

            decimal montoRestante = diasRestantes * reserva.MontoDiario;
            decimal porcentajeMulta = (diasTranscurridos < diasOriginales / 2) ? 0.50m : 0.25m;

            return montoRestante * porcentajeMulta;
        }
    }
}