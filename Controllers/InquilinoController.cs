using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    public class InquilinoController : Controller
    {
        private readonly IRepositorioInquilino repositorio;
        private readonly IConfiguration config;
        private readonly ILogger<InquilinoController> logger;

        public InquilinoController(IRepositorioInquilino repo, IConfiguration config, ILogger<InquilinoController> logger)
        {
            this.repositorio = repo;
            this.config = config;
            this.logger = logger;
        }

        // Poner los breakpoints en los metodos para hacer la depuración
        // GET: Inquilino/Index
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
                logger.LogError(ex, "Error en Index de Inquilino");
                throw;
            }
        }

        // GET: Inquilino/Create
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create GET");
                throw;
            }
        }

        // POST: Inquilino/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Inquilino inquilino)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    repositorio.Alta(inquilino);
                    TempData["Mensaje"] = "Inquilino creado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                    return View(inquilino);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create POST");
                throw;
            }
        }

        // GET: Inquilino/Edit/5
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

        // POST: Inquilino/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Inquilino entidad)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var i = repositorio.ObtenerPorId(id);
                    if (i == null)
                        return NotFound();

                    i.Nombre = entidad.Nombre;
                    i.Apellido = entidad.Apellido;
                    i.Dni = entidad.Dni;
                    i.Telefono = entidad.Telefono;
                    i.Email = entidad.Email;

                    repositorio.Modificacion(i);
                    TempData["Mensaje"] = "Datos guardados correctamente";
                    return RedirectToAction(nameof(Index));
                }
                return View(entidad);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Edit POST");
                throw;
            }
        }

        // GET: Inquilino/Delete/5
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

        // POST: Inquilino/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, Inquilino entidad)
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
                throw;
            }
        }
    }
}