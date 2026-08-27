using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    public class ImagenesController : Controller
    {
        private readonly IRepositorioImagen repositorio;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<ImagenesController> logger;

        public ImagenesController(
            IRepositorioImagen repositorio,
            IWebHostEnvironment environment,
            ILogger<ImagenesController> logger)
        {
            this.repositorio = repositorio;
            this.environment = environment;
            this.logger = logger;
        }

        // GET: Inmueble/Imagenes/5
        public ActionResult Index(int id)
        {
            try
            {
                var imagenes = repositorio.BuscarPorInmueble(id);
                ViewBag.InmuebleId = id;
                return View(imagenes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cargar imágenes del inmueble {id}", id);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Alta(int id, List<IFormFile> imagenes)
        {
            try
            {
                if (imagenes == null || imagenes.Count == 0)
                    return BadRequest("No se recibieron archivos.");

                // Ruta donde se guardarán las imágenes
                string wwwPath = environment.WebRootPath;
                string path = Path.Combine(wwwPath, "Uploads", "Inmuebles", id.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Obtener el último orden usado para este inmueble
                var existentes = repositorio.BuscarPorInmueble(id);
                int ultimoOrden = existentes.Any() ? existentes.Max(i => i.Orden) : 0;

                foreach (var file in imagenes)
                {
                    if (file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                        var rutaArchivo = Path.Combine(path, nombreArchivo);

                        using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Crear la entidad ImagenInmueble
                        var imagen = new ImagenInmueble
                        {
                            IdInmueble = id,
                            Ruta = $"/Uploads/Inmuebles/{id}/{nombreArchivo}",
                            Orden = ++ultimoOrden
                        };
                        repositorio.Alta(imagen);
                    }
                }

                // Devolver la lista actualizada.
                return Ok(repositorio.BuscarPorInmueble(id));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al subir imágenes para inmueble {id}", id);
                return BadRequest(ex.Message);
            }
        }

        // POST: Imagenes/Eliminar/{id}
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var entidad = repositorio.ObtenerPorId(id);
                if (entidad == null)
                    return NotFound();

                // Eliminar el archivo físico
                string rutaFisica = Path.Combine(environment.WebRootPath, entidad.Ruta.TrimStart('/'));
                if (System.IO.File.Exists(rutaFisica))
                {
                    System.IO.File.Delete(rutaFisica);
                }

                // Eliminar el registro de la base de datos.
                repositorio.Baja(id);

                // Devolver la lista actualizada.
                return Ok(repositorio.BuscarPorInmueble(entidad.IdInmueble));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar imagen {id}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}