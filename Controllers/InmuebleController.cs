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
    public class InmuebleController : Controller
    {
        private readonly IRepositorioInmueble repositorio;
        private readonly IRepositorioPropietario repositorioPropietario;
        private readonly IRepositorioTipoInmueble repositorioTipo;
        private readonly IRepositorioImagen repositorioImagen;
        private readonly IConfiguration config;
        private readonly ILogger<InmuebleController> logger;

        public InmuebleController(
            IRepositorioInmueble repo,
            IRepositorioPropietario repoPropietario,
            IRepositorioTipoInmueble repoTipo,
            IRepositorioImagen repoImagen,
            IConfiguration config,
            ILogger<InmuebleController> logger)
        {
            this.repositorio = repo;
            this.repositorioPropietario = repoPropietario;
            this.repositorioTipo = repoTipo;
            this.repositorioImagen = repoImagen;
            this.config = config;
            this.logger = logger;
        }

        //GET: Inmuebles
        public ActionResult Index(int pagina = 1, string? busqueda = null, string? estado = null, int? idTipo = null)
        {
            try
            {
                var tamaño = 5;
                pagina = Math.Max(pagina, 1);

                // Guardar filtros para la vista
                ViewBag.Busqueda = busqueda;
                ViewBag.Estado = estado;
                ViewBag.IdTipo = idTipo;

                IList<Inmueble> lista;
                int total;

                bool sinFiltros = string.IsNullOrWhiteSpace(busqueda)
                                && string.IsNullOrWhiteSpace(estado)
                                && !idTipo.HasValue;

                if (sinFiltros)
                {
                    // Camino original
                    lista = repositorio.ObtenerLista(pagina, tamaño);
                    total = repositorio.ObtenerCantidad();
                }
                else
                {
                    // Camino con filtros
                    lista = repositorio.BuscarConFiltros(busqueda, estado, idTipo, pagina, tamaño);
                    total = repositorio.ContarConFiltros(busqueda, estado, idTipo);
                }

                ViewBag.Pagina = pagina;
                ViewBag.TotalPaginas = total % tamaño == 0 ? total / tamaño : total / tamaño + 1;
                ViewBag.TotalRegistros = total;

                if (TempData.ContainsKey("Mensaje"))
                    ViewBag.Mensaje = TempData["Mensaje"];

                // Cargar relaciones (propietario, tipo)
                CargarRelaciones(lista);
                CargarPortadas(lista);

                // Lista de tipos para el select de filtro
                ViewBag.Tipos = repositorioTipo?.ObtenerLista(1, int.MaxValue) ?? new List<TipoInmueble>();

                return View(lista);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Index de Inmueble");
                throw;
            }
        }

        // GET: Inmueble/Create
        public ActionResult Create()
        {
            try
            {
                CargarListasDesplegables();
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create GET");
                throw;
            }
        }

        // POST: Inmueble/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Inmueble inmueble)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    repositorio.Alta(inmueble);
                    TempData["Mensaje"] = "Inmueble creado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    CargarListasDesplegables();
                    return View(inmueble);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create POST");
                ModelState.AddModelError("", ex.Message);
                CargarListasDesplegables();
                return View(inmueble);
            }
        }

        // GET: Inmueble/Ver/5
        public ActionResult Ver(int id)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();

                // Cargar relaciones (Propietario, Tipo)
                CargarRelacion(entidad);

                return View(entidad);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Ver GET");
                throw;
            }
        }

        // GET: Inmueble/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();

                CargarRelacion(entidad);
                CargarListasDesplegables(entidad.IdPropietario, entidad.IdTipo);

                // Select2 Propietario
                var propietarioSeleccionado = repositorioPropietario.ObtenerPorId(entidad.IdPropietario);
                ViewBag.PropietarioId = entidad.IdPropietario;
                ViewBag.PropietarioSeleccionado = propietarioSeleccionado?.ToString();

                // Select2 Tipo
                var tipoSeleccionado = repositorioTipo.ObtenerPorId(entidad.IdTipo);
                ViewBag.TipoId = entidad.IdTipo;
                ViewBag.TipoSeleccionado = tipoSeleccionado?.Nombre;

                return View(entidad);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Edit GET");
                throw;
            }
        }

        // POST: Inmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Inmueble entidad)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    CargarListasDesplegables(entidad.IdPropietario, entidad.IdTipo);
                    return View(entidad);
                }

                var i = repositorio.ObtenerPorId(id);
                if (i == null)
                    return NotFound();
                i.Direccion = entidad.Direccion;
                i.Cupo = entidad.Cupo;
                i.PrecioPorDia = entidad.PrecioPorDia;
                i.PorcentajeReserva = entidad.PorcentajeReserva;
                i.Estado = entidad.Estado;
                i.Latitud = entidad.Latitud;
                i.Longitud = entidad.Longitud;
                i.IdPropietario = entidad.IdPropietario;
                i.IdTipo = entidad.IdTipo;

                repositorio.Modificacion(i);
                TempData["Mensaje"] = "Datos guardados correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Edit POST");
                ModelState.AddModelError("", ex.Message);
                CargarListasDesplegables(entidad.IdPropietario, entidad.IdTipo);
                return View(entidad);
            }
        }

        [Authorize(Policy = "Administrador")]
        // GET: Inmueble/Eliminar/5
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
        
        [Authorize(Policy = "Administrador")]
        // POST: Inmueble/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, Inmueble entidad)
        {
            try
            {
                repositorio.Baja(id);
                TempData["Mensaje"] = "Eliminación realizada correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Eliminar POST");
                if (ex is MySqlConnector.MySqlException mysqlEx && mysqlEx.ErrorCode == MySqlConnector.MySqlErrorCode.RowIsReferenced)
                {
                    TempData["Error"] = "No se puede eliminar el inmueble porque tiene reservas asociadas.";
                }
                else if (ex.Message.Contains("foreign key constraint fails") || ex.Message.Contains("Cannot delete or update a parent row"))
                {
                    TempData["Error"] = "No se puede eliminar el inmueble porque tiene reservas asociadas.";
                }
                else
                {
                    TempData["Error"] = ex.Message;
                }

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Inmueble/Buscar?term=...
        [HttpGet]
        public IActionResult Buscar(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                    return Json(new List<object>());

                var resultados = repositorio.BuscarPorDireccion(term)
                    .Select(i => new
                    {
                        id = i.IdInmueble,
                        text = i.Direccion,
                        precio = i.PrecioPorDia
                    })
                    .ToList();

                return Json(resultados);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al buscar inmuebles");
                return Json(new List<object>());
            }
        }
        // Cargar listas desplegables para Propietarios y Tipos de Inmueble.

        private void CargarListasDesplegables(int? propietarioSeleccionado = null, int? tipoSeleccionado = null)
        {
            try
            {
                var propietarios = repositorioPropietario.ObtenerLista(1, int.MaxValue);
                ViewBag.Propietarios = propietarios;

                var tipos = repositorioTipo.ObtenerLista();
                ViewBag.Tipos = tipos;

                if (propietarioSeleccionado.HasValue)
                    ViewBag.PropietarioId = propietarioSeleccionado.Value;
                if (tipoSeleccionado.HasValue)
                    ViewBag.TipoId = tipoSeleccionado.Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cargar listas desplegables");
            }
        }

        private void CargarRelacion(Inmueble inmueble)
        {
            if (inmueble == null) return;

            if (inmueble.IdPropietario > 0 && inmueble.Propietario == null)
                inmueble.Propietario = repositorioPropietario.ObtenerPorId(inmueble.IdPropietario);

            if (inmueble.IdTipo > 0 && inmueble.Tipo == null)
                inmueble.Tipo = repositorioTipo.ObtenerPorId(inmueble.IdTipo);
        }

        private void CargarRelaciones(IList<Inmueble> inmuebles)
        {
            if (inmuebles == null || !inmuebles.Any()) return;

            var idsPropietarios = inmuebles.Select(i => i.IdPropietario).Distinct().ToList();
            var propietarios = repositorioPropietario.ObtenerLista(1, int.MaxValue)
                .Where(p => idsPropietarios.Contains(p.IdPropietario))
                .ToDictionary(p => p.IdPropietario);

            var idsTipos = inmuebles.Select(i => i.IdTipo).Distinct().ToList();
            var tipos = repositorioTipo.ObtenerLista()
                .Where(t => idsTipos.Contains(t.IdTipo))
                .ToDictionary(t => t.IdTipo);

            foreach (var i in inmuebles)
            {
                if (propietarios.TryGetValue(i.IdPropietario, out var prop))
                    i.Propietario = prop;
                if (tipos.TryGetValue(i.IdTipo, out var tipo))
                    i.Tipo = tipo;
            }
        }
        public ActionResult Imagenes(int id, [FromServices] IRepositorioImagen repoImagen)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();

                // Cargar las imágenes del inmueble
                entidad.Imagenes = repoImagen.BuscarPorInmueble(id);

                // Cargar propietario y tipo para mostrar en la vista
                CargarRelacion(entidad);

                return View(entidad); // "Imagenes.cshtml"
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cargar imágenes del inmueble {id}", id);
                throw;
            }
        }
        private void CargarPortadas(IList<Inmueble> inmuebles)
        {
            if (inmuebles == null || !inmuebles.Any()) return;

            var ids = inmuebles.Select(i => i.IdInmueble).Distinct().ToList();
            // Obtener todas las imágenes de estos inmuebles (orden 0 = portada)
            var imagenes = repositorioImagen.BuscarPorInmuebleIds(ids); 
            foreach (var i in inmuebles)
            {
                var portada = imagenes.FirstOrDefault(img => img.IdInmueble == i.IdInmueble && img.Orden == 0);
                if (portada != null)
                    i.Imagenes = new List<ImagenInmueble> { portada }; 
                else
                    i.Imagenes = new List<ImagenInmueble>(); 
            }
        }
    }
}