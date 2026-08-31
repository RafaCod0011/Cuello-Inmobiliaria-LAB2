using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    public class PropietarioController : Controller
    {
        private readonly IRepositorioPropietario repositorio;
        private readonly IConfiguration config;
		private readonly ILogger<PropietarioController> logger;
        
        public PropietarioController(IRepositorioPropietario repo, IConfiguration config, ILogger<PropietarioController> logger)
		{
			this.repositorio = repo;
			this.config = config;
			this.logger = logger;
		
		}
		// Poner los breakpoints en los metodos para hacer la depuración
        //GET: Propietarios
        public ActionResult Index(int pagina=1)
		{
			try
			{
				var tamaño = 5;
				var lista = repositorio.ObtenerLista(Math.Max(pagina, 1), tamaño);
				ViewBag.Pagina = pagina;
				var total = repositorio.ObtenerCantidad();
				ViewBag.TotalPaginas = total % tamaño == 0 ? total / tamaño : total / tamaño + 1;
				ViewBag.Id = TempData["Id"];
				if (TempData.ContainsKey("Mensaje"))
					ViewBag.Mensaje = TempData["Mensaje"];
				return View(lista);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error en Index");
				throw;
			}
		}

		// GET: Propietario/Create
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create");
                throw;
            }
        }
 
        // POST: Propietario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Propietario propietario)
        {
            try
            {
                if (ModelState.IsValid)// Pregunta si el modelo es válido
                {
                    repositorio.Alta(propietario);
					TempData["Mensaje"] = "Propietario creado correctamente";
                    TempData["Id"] = propietario.IdPropietario;
                    return RedirectToAction(nameof(Index));
                }
                else
                    return View(propietario);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Create");
                ModelState.AddModelError("", ex.Message);  // Muestra el error en la vista
        		return View(propietario);
            }
        }

        // GET: Propietarios/Delete/5
		public ActionResult Eliminar(int id)
		{
			try
			{
				var entidad = repositorio.ObtenerPorId(id);
				return View(entidad);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error en Eliminar");
				throw;
			}
		}

		// POST: Propietarios/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Eliminar(int id, Propietario entidad)
		{
			try
			{
				repositorio.Baja(id);
				TempData["Mensaje"] = "Eliminación realizada correctamente";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error en Eliminar");
				throw;
			}
		}

		// GET: Propietarios/Ver/5
		public ActionResult Ver(int id)
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
				logger.LogError(ex, "Error en Ver");
				throw;
			}
		}

		// GET: Propietarios/Edit/5
		public ActionResult Edit(int id)
		{
			try
			{
				var entidad = repositorio.ObtenerPorId(id);
				return View(entidad);//pasa el modelo a la vista
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error en Edit");
				throw;
			}
		}

		// POST: Propietarios/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(int id, Propietario entidad)
		{	
			ModelState.Remove("Clave");
			if (!ModelState.IsValid)
        	return View(entidad);
			
			try
			{
				var p = repositorio.ObtenerPorId(id);
				if (p == null)
					return NotFound();
				p.Nombre = entidad.Nombre;
				p.Apellido = entidad.Apellido;
				p.Dni = entidad.Dni;
				p.Email = entidad.Email;
				p.Telefono = entidad.Telefono;
				repositorio.Modificacion(p);
				TempData["Mensaje"] = "Datos guardados correctamente";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error en Edit");
				ModelState.AddModelError("", ex.Message);
        		return View(entidad);
			}
		}

		// POST: Propietarios/Guardar/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Guardar(int id, Propietario entidad)
		{
			try
			{
				if(!ModelState.IsValid)
					return BadRequest(ModelState);
				if(id == 0) 
				{
					id = repositorio.Alta(entidad);
				}
				else
				{
					var p = repositorio.ObtenerPorId(id);
					p.Nombre = entidad.Nombre;
					p.Apellido = entidad.Apellido;
					p.Dni = entidad.Dni;
					p.Email = entidad.Email;
					p.Telefono = entidad.Telefono;
					repositorio.Modificacion(p);
				}
				var res = repositorio.ObtenerPorId(id);
				return Ok(res);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		public IActionResult Buscar(string term)
		{
    		var resultados = repositorio.BuscarPorNombre(term ?? "")
        	.Select(p => new { idPropietario = p.IdPropietario, nombre = p.Nombre, apellido = p.Apellido });
    		return Json(resultados);
		}
   }     
}