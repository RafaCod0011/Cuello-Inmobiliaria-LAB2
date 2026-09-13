using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;   
using Microsoft.Extensions.Logging;
using System;

namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    [Authorize]
    public class TipoInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble repositorio;
        private readonly ILogger<TipoInmuebleController> logger;

        public TipoInmuebleController(IRepositorioTipoInmueble repositorio, ILogger<TipoInmuebleController> logger)
        {
            this.repositorio = repositorio;
            this.logger = logger;
        }

        // GET: TipoInmueble/Index
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
                return View(lista);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Index de TipoInmueble");
                throw;
            }
        }

        // GET: TipoInmueble/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TipoInmueble/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TipoInmueble tipo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    repositorio.Alta(tipo);
                    TempData["Mensaje"] = "Tipo de inmueble creado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                return View(tipo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create POST");
                ModelState.AddModelError("", ex.Message);
                return View(tipo);
            }
        }

        // GET: TipoInmueble/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();
                return View(entidad);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Edit GET");
                throw;
            }
        }

        // POST: TipoInmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, TipoInmueble entidad)
        {
            if (!ModelState.IsValid)
                return View(entidad);

            try
            {
                var t = repositorio.ObtenerPorId(id);
                if (t == null)
                    return NotFound();

                t.Nombre = entidad.Nombre;
                repositorio.Modificacion(t);
                TempData["Mensaje"] = "Tipo actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Edit POST");
                ModelState.AddModelError("", ex.Message);
                return View(entidad);
            }
        }

        // GET: TipoInmueble/Delete/5
        [Authorize(Policy = "Administrador")]
        public ActionResult Eliminar(int id)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();
                return View(entidad);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Eliminar GET");
                throw;
            }
        }

        // POST: TipoInmueble/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public ActionResult Eliminar(int id, TipoInmueble entidad)
        {
            try
            {
                repositorio.Baja(id);
                TempData["Mensaje"] = "Tipo eliminado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Eliminar POST");
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        // GET: TipoInmueble/Buscar?term=...
        [HttpGet]
        public IActionResult Buscar(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 1)
                    return Json(new List<object>());

                var resultados = repositorio.BuscarPorNombre(term)
                    .Select(t => new
                    {
                        id = t.IdTipo,
                        text = t.Nombre
                    })
                    .ToList();

                return Json(resultados);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al buscar tipos de inmueble");
                return Json(new List<object>());
            }
        }
    }
}