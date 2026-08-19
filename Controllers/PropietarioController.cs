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
        [Route("[controller]/Index")]
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
   }     
}