using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    [Authorize]
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

                string wwwPath = environment.WebRootPath;
                string path = Path.Combine(wwwPath, "Uploads", "Inmuebles", id.ToString());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var existentes = repositorio.BuscarPorInmueble(id);
                int ultimoOrden = existentes.Any() ? existentes.Max(i => i.Orden) : -1; // -1 para que la primera sea 0
                bool esPrimera = !existentes.Any();

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

                        int orden;
                        if (esPrimera)
                        {
                            orden = 0; // portada
                            esPrimera = false; // solo la primera imagen es portada
                        }
                        else
                        {
                            orden = ++ultimoOrden;
                        }

                        var imagen = new ImagenInmueble
                        {
                            IdInmueble = id,
                            Ruta = $"/Uploads/Inmuebles/{id}/{nombreArchivo}",
                            Orden = orden
                        };
                        repositorio.Alta(imagen);
                    }
                }

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

        [HttpPost]
        public IActionResult MarcarPortada(int id)
        {
            try
            {
                var imagen = repositorio.ObtenerPorId(id);
                if (imagen == null)
                    return NotFound();

                var imagenes = repositorio.BuscarPorInmueble(imagen.IdInmueble);
                
                // Buscar la portada actual (orden 0)
                var portadaActual = imagenes.FirstOrDefault(i => i.Orden == 0);
                if (portadaActual != null && portadaActual.IdImagen != id)
                {
                    // Reasignar un orden a la portada actual (la movemos al final)
                    int maxOrden = imagenes.Max(i => i.Orden);
                    portadaActual.Orden = maxOrden + 1;
                    repositorio.Modificacion(portadaActual);
                }

                // Asignar orden 0 a la imagen seleccionada
                imagen.Orden = 0;
                repositorio.Modificacion(imagen);

                // Devolver la lista actualizada de imágenes
                return Ok(repositorio.BuscarPorInmueble(imagen.IdInmueble));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al marcar portada");
                return BadRequest(ex.Message);
            }
        }
    }
}