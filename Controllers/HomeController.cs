using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cuello_Inmobiliaria_LAB2.Models;

namespace Cuello_Inmobiliaria_LAB2.Controllers

{
    public class HomeController : Controller
    {
        private readonly IRepositorioPropietario propietarios;
        private readonly IConfiguration config;

        public HomeController(IRepositorioPropietario propietarios, IConfiguration config, ILogger<HomeController> logger)
        {
            this.propietarios = propietarios;
            this.config = config;
            logger.LogInformation("Estoy en el constructor de HomeController");
        }
        
        
        public IActionResult Index()
        {   
            ViewBag.Titulo= "Pagina de Incicio";
            List<string> clientes = propietarios.ObtenerLista().Select(x => x.Nombre + " " + x.Apellido).ToList();
            return View(clientes);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}