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
            {//poner breakpoints para detectar errores
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
                    // Reemplazo de clave plana por clave con hash
                    propietario.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: propietario.Clave,
                            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                            prf: KeyDerivationPrf.HMACSHA1,
                            iterationCount: 1000,
                            numBytesRequested: 256 / 8));
                    repositorio.Alta(propietario);
					TempData["Mensaje"] = "Propietario creado correctamente";
                    TempData["Id"] = propietario.IdPropietario;
                    return RedirectToAction(nameof(Index));
                }
                else
                    return View(propietario);
            }
            catch (Exception ex)
            {//poner breakpoints para detectar errores
                logger.LogError(ex, "Error en Create");
                throw;
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
			{//poner breakpoints para detectar errores
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
			{//poner breakpoints para detectar errores
				logger.LogError(ex, "Error en Eliminar");
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
			{//poner breakpoints para detectar errores
				logger.LogError(ex, "Error en Edit");
				throw;
			}
		}

		// POST: Propietarios/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		//public ActionResult Edit(int id, IFormCollection collection)
		public ActionResult Edit(int id, Propietario entidad)
		{
			// Si en lugar de IFormCollection ponemos Propietario, el enlace de datos lo hace el sistema
			try
			{
				var p = repositorio.ObtenerPorId(id);
				if (p == null)
					return NotFound();
				// En caso de ser necesario usar: 
				//
				//Convert.ToInt32(collection["CAMPO"]);
				//Convert.ToDecimal(collection["CAMPO"]);
				//Convert.ToDateTime(collection["CAMPO"]);
				//int.Parse(collection["CAMPO"]);
				//decimal.Parse(collection["CAMPO"]);
				//DateTime.Parse(collection["CAMPO"]);
				////////////////////////////////////////
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
			{//poner breakpoints para detectar errores
				logger.LogError(ex, "Error en Edit");
				throw;
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
			{//poner breakpoints para detectar errores
				return BadRequest(ex.Message);
			}
		}

		// POST: Propietarios/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CambiarPass(int id, CambioClaveView cambio)
		{
			Propietario? propietario = null;
			try
			{
				// recuperar propietario original
				propietario = repositorio.ObtenerPorId(id);
				// verificar clave antigüa
				var pass = Convert.ToBase64String(KeyDerivation.Pbkdf2(
					password: cambio.ClaveVieja ?? "",
					salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"] ?? ""),
					prf: KeyDerivationPrf.HMACSHA1,
					iterationCount: 1000,
					numBytesRequested: 256 / 8));
				if (propietario?.Clave != pass)
				{
					TempData["Error"] = "Clave incorrecta";
					// se rederige porque no hay vista de cambio de pass, está compartida con Edit
					return RedirectToAction("Edit", new { id = id });
				}
				if (ModelState.IsValid)
				{
					propietario.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
							password: cambio.ClaveNueva,
							salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"] ?? ""),
							prf: KeyDerivationPrf.HMACSHA1,
							iterationCount: 1000,
							numBytesRequested: 256 / 8));
					repositorio.Modificacion(propietario);
					TempData["Mensaje"] = "Contraseña actualizada correctamente";
					return RedirectToAction(nameof(Index));
				}
				else//estado inválido
				{//pasaje de los errores del modelstate a un string en tempData
					foreach (ModelStateEntry modelState in ViewData.ModelState.Values)
					{
						foreach (ModelError error in modelState.Errors)
						{
							TempData["Error"] += error.ErrorMessage + "\n";
						}
					}
					return RedirectToAction("Edit", new { id = id });
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error en CambiarPass");
				TempData["Error"] = ex.Message;
				TempData["StackTrace"] = ex.StackTrace;
				return RedirectToAction("Edit", new { id = id });
			}
		}
   }     
}