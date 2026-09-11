using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    [Authorize]
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
                if (reserva.FechaInicio < DateTime.Today)
                {
                    ModelState.AddModelError("FechaInicio", "La fecha de inicio no puede ser anterior al día de hoy.");
                }
                if (reserva.FechaFin < DateTime.Today)
                {
                    ModelState.AddModelError("FechaFin", "La fecha de fin no puede ser anterior al día de hoy.");
                }
                if (reserva.FechaFin < reserva.FechaInicio)
                {
                    ModelState.AddModelError("", "La fecha de fin debe ser posterior a la fecha de inicio.");
                }

                if (!ModelState.IsValid)
                {
                    CargarListasDesplegables();
                    return View(reserva);
                }
                reserva.IdUsuarioCreacion = User.UsuarioId();
                reserva.FechaCreacion = DateTime.Now;

                repositorio.Alta(reserva);
                //Seña inicial
                RegistrarPagoInicial(reserva);
                TempData["Mensaje"] = "Reserva creada correctamente";
                return RedirectToAction(nameof(Index));
                        
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

                //Extenciones asociadas a la reserva
                var extensiones = repositorio.ObtenerLista(1, int.MaxValue)
                    .Where(r => r.IdReservaOrigen == id)
                    .ToList();
                ViewBag.Extensiones = extensiones;   

                CargarRelacion(entidad);
                CargarListasDesplegables(entidad.IdInmueble, entidad.IdInquilino);
                
                // Select2
                var inquilinoSeleccionado = repositorioInquilino.ObtenerPorId(entidad.IdInquilino);
                ViewBag.InquilinoSeleccionado = inquilinoSeleccionado?.ToString();
                ViewBag.InquilinoId = entidad.IdInquilino;
                ViewBag.Pagos = repositorioPago.ObtenerPorReserva(id);
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
            try
            {
                var reservaOriginal = repositorio.ObtenerPorId(id);
                if (reservaOriginal == null) return NotFound();

                // No permitir editar si tiene extensiones
                var tieneExtensiones = repositorio.ObtenerLista(1, int.MaxValue)
                    .Any(r => r.IdReservaOrigen == id);
                if (tieneExtensiones)
                {
                    TempData["Error"] = "No se puede editar esta reserva porque tiene extensiones asociadas. " +
                                        "Eliminá primero las extensiones si querés modificarla.";
                    return RedirectToAction(nameof(Ver), new { id });
}

                if (reservaOriginal.EstaTerminada)
                {
                    TempData["Error"] = "No se puede editar una reserva terminada.";
                    return RedirectToAction(nameof(Ver), new { id });
                }
                if (reservaOriginal.FechaFin < DateTime.Today)
                {
                    TempData["Error"] = "No se puede editar una reserva finalizada.";
                    return RedirectToAction(nameof(Ver), new { id });
                }

                var yaComenzo = reservaOriginal.FechaInicio <= DateTime.Today;
                var tienePagos = repositorioPago.ObtenerPorReserva(id).Any(p => !p.Anulado);

                if (tienePagos)
                {
                    entidad.MontoDiario = reservaOriginal.MontoDiario;
                    ModelState.Remove(nameof(entidad.MontoDiario));
                }

                // Si ya comenzo, ignorar cambios 
                if (yaComenzo)
                {
                    entidad.FechaInicio = reservaOriginal.FechaInicio;
                    entidad.FechaFin = reservaOriginal.FechaFin;
                    entidad.IdInmueble = reservaOriginal.IdInmueble;
                    entidad.IdInquilino = reservaOriginal.IdInquilino;
                }
                if (tienePagos)
                {
                    entidad.MontoDiario = reservaOriginal.MontoDiario;
                }

                // Validaciones
                if (!yaComenzo)
                {
                    if (entidad.FechaInicio < DateTime.Today)
                        ModelState.AddModelError("FechaInicio", "La fecha de inicio no puede ser anterior a hoy.");
                    if (entidad.FechaFin <= entidad.FechaInicio)
                        ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la de inicio.");
                    if (!repositorio.EstaDisponible(entidad.IdInmueble, entidad.FechaInicio, entidad.FechaFin, id))
                        ModelState.AddModelError("", "El inmueble no está disponible en las fechas seleccionadas.");
                }

                if (!ModelState.IsValid)
                {
                    CargarListasDesplegables(entidad.IdInmueble, entidad.IdInquilino);
                    ViewBag.Pagos = repositorioPago.ObtenerPorReserva(id);
                    return View(entidad);
                }

                reservaOriginal.FechaInicio = entidad.FechaInicio;
                reservaOriginal.FechaFin = entidad.FechaFin;
                reservaOriginal.MontoDiario = entidad.MontoDiario;
                reservaOriginal.IdInmueble = entidad.IdInmueble;
                reservaOriginal.IdInquilino = entidad.IdInquilino;

                repositorio.Modificacion(reservaOriginal);

                TempData["Mensaje"] = "Reserva modificada correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Edit POST");
                ModelState.AddModelError("", ex.Message);
                CargarListasDesplegables(entidad.IdInmueble, entidad.IdInquilino);
                ViewBag.Pagos = repositorioPago.ObtenerPorReserva(id);
                return View(entidad);
            }
        }

        // GET: Reserva/Delete/5
        [Authorize(Policy = "Administrador")]
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
        [Authorize(Policy = "Administrador")]
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

                var tienePagoMulta = entidad.Pagos.Any(p => 
                p.Concepto.StartsWith("Multa por terminación") && !p.Anulado);
                bool puedeTerminar = !entidad.EstaTerminada && 
                             (entidad.EstaVigente || entidad.EsProxima) && 
                             !tienePagoMulta;
                ViewBag.TienePagoMulta = tienePagoMulta;
                ViewBag.PuedeTerminar = puedeTerminar;

                ViewBag.Extensiones = repositorio.ObtenerLista(1, int.MaxValue)
                .Where(r => r.IdReservaOrigen == id)
                .ToList();

                
                if (entidad.IdReservaOrigen != null)
                {
                    ViewBag.ReservaOrigen = repositorio.ObtenerPorId(entidad.IdReservaOrigen.Value);
                }
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
                        IdReserva = reserva.IdReserva,
                        IdUsuarioCreacion = User.UsuarioId() 
                    };
                    repositorioPago.Alta(pago);
                }

                // Actualizar reserva
                repositorio.TerminarAnticipadamente(id, fechaTerminacion, User.UsuarioId()); 

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

        // GET: Reserva/Extender/5
        public ActionResult Extender(int id)
        {
            try
            {
                var reservaOriginal = repositorio.ObtenerPorId(id);
                if (reservaOriginal == null)
                    return NotFound();

                if (reservaOriginal.EstaTerminada)
                {
                    TempData["Error"] = "No se puede extender una reserva que ya fue terminada.";
                    return RedirectToAction(nameof(Ver), new { id });
                }

                CargarRelacion(reservaOriginal);
                ViewBag.ReservaOriginal = reservaOriginal;

                var nuevaReserva = new Reserva
                {
                    FechaInicio = reservaOriginal.FechaFin.AddDays(1),
                    FechaFin = reservaOriginal.FechaFin.AddDays(2),
                    MontoDiario = reservaOriginal.Inmueble?.PrecioPorDia ?? reservaOriginal.MontoDiario
                };

                return View(nuevaReserva);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Extender GET");
                throw;
            }
        }

        // POST: Reserva/Extender/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Extender(int id, Reserva entidad)
        {
            try
            {
                var reservaOriginal = repositorio.ObtenerPorId(id);
                if (reservaOriginal == null)
                    return NotFound();

                if (reservaOriginal.EstaTerminada)
                {
                    TempData["Error"] = "No se puede extender una reserva terminada.";
                    return RedirectToAction(nameof(Ver), new { id });
                }

                // Validaciones
                if (entidad.FechaInicio < DateTime.Today)
                    ModelState.AddModelError("FechaInicio", "La fecha de inicio no puede ser anterior a hoy.");
                if (entidad.FechaFin <= entidad.FechaInicio)
                    ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la de inicio.");
                if (entidad.MontoDiario <= 0)
                    ModelState.AddModelError("MontoDiario", "El monto debe ser mayor a 0.");

                var minimoInicio = reservaOriginal.FechaFin.AddDays(1);
                if (entidad.FechaInicio < minimoInicio)
                    ModelState.AddModelError("FechaInicio",
                        $"La extensión debe comenzar a partir del {minimoInicio:dd/MM/yyyy}.");

                if (!ModelState.IsValid)
                {
                    CargarRelacion(reservaOriginal);
                    ViewBag.ReservaOriginal = reservaOriginal;
                    return View(entidad);
                }

                // Verificar disponibilidad
                if (!repositorio.EstaDisponible(reservaOriginal.IdInmueble, entidad.FechaInicio, entidad.FechaFin))
                {
                    ModelState.AddModelError("", "El inmueble no está disponible en las fechas seleccionadas.");
                    CargarRelacion(reservaOriginal);
                    ViewBag.ReservaOriginal = reservaOriginal;
                    return View(entidad);
                }

                var nuevaReserva = new Reserva
                {
                    FechaInicio = entidad.FechaInicio,
                    FechaFin = entidad.FechaFin,
                    MontoDiario = entidad.MontoDiario,
                    IdInmueble = reservaOriginal.IdInmueble,
                    IdInquilino = reservaOriginal.IdInquilino,
                    IdUsuarioCreacion = User.UsuarioId(),
                    IdReservaOrigen = reservaOriginal.IdReserva,
                    FechaCreacion = DateTime.Now
                };

                repositorio.Alta(nuevaReserva);
                RegistrarPagoInicial(nuevaReserva);

                TempData["Mensaje"] = $"Reserva extendida correctamente. Nueva reserva #{nuevaReserva.IdReserva}.";
                return RedirectToAction(nameof(Ver), new { id = nuevaReserva.IdReserva });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al extender reserva");
                ModelState.AddModelError("", ex.Message);
                var reserva = repositorio.ObtenerPorId(id);
                CargarRelacion(reserva);
                ViewBag.ReservaOriginal = reserva;
                return View(new Reserva());
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