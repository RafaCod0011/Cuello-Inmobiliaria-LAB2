using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cuello_Inmobiliaria_LAB2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace Cuello_Inmobiliaria_LAB2.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IRepositorioUsuario repositorio;
        private readonly ILogger<UsuarioController> logger;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;

        public UsuarioController(IRepositorioUsuario repositorio, ILogger<UsuarioController> logger, IConfiguration config, IWebHostEnvironment env)
        {
            this.repositorio = repositorio;
            this.logger = logger;
            this.config = config;
            this.env = env;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView login, string returnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: login.Clave,
                        salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"] ?? ""),
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 1000,
                        numBytesRequested: 256 / 8));

                    var usuario = repositorio.ObtenerPorEmail(login.Usuario);
                    if (usuario == null || usuario.Clave != hashed)
                    {
                        ModelState.AddModelError("", "El email o la clave no son correctos");
                        return View(login);
                    }

                    // Crear claims y autenticar
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Email),
                        new Claim("FullName", usuario.Nombre + " " + usuario.Apellido),
                        new Claim(ClaimTypes.Role, usuario.RolNombre),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return Redirect(returnUrl ?? "/Home/Index");
                }
                return View(login);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(login);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Perfil()
        {
            var id = User.UsuarioId();
            return RedirectToAction("Ver", new { id });
        }

        [Authorize]
        public IActionResult Ver(int id)
        {
            var usuario = repositorio.ObtenerPorId(id);
            if (usuario == null)
                return NotFound();

            if (!User.IsInRole("Administrador") && User.UsuarioId() != id)
                return RedirectToAction("AccessDenied", "Home");

            return View(usuario);
        }

        // GET: Usuario/Index
        [Authorize(Policy = "Administrador")]
        public IActionResult Index()
        {
            return View();
        }

        // GET: Usuario/ObtenerTodos
        [Authorize(Policy = "Administrador")]
        [HttpGet]
        public IActionResult ObtenerTodos(int pagina = 1, int tamano = 5)
        {
            try
            {
                pagina = Math.Max(pagina, 1);
                tamano = tamano > 0 ? tamano : 5;
                var usuarios = repositorio.ObtenerLista(pagina, tamano);
                var total= repositorio.ObtenerCantidad();
                var totalPaginas = total % tamano == 0 ? total / tamano : total / tamano + 1;
                var datos = usuarios.Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Email,
                    u.Rol,
                    RolNombre = u.RolNombre,
                    Avatar = u.Avatar ?? ""
                });
                return Json(new
                {
                    datos=datos,
                    paginaActual=pagina,
                    totalPaginas=totalPaginas,
                    totalRegistros=total
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { error = "Error al cargar usuarios" });
            }
        }

        // GET: Usuario/ObtenerPorId/5
        [HttpGet]
        public IActionResult ObtenerPorId(int id)
        {
            try
            {
                var u = repositorio.ObtenerPorId(id);
                if (u == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                return Json(new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Email,
                    u.Rol,
                    u.Avatar,
                    Clave = ""
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuario {Id}", id);
                return StatusCode(500, new { error = "Error al cargar el usuario" });
            }
        }

        // POST: Usuario/Create
        [HttpPost]
        [Authorize(Policy = "Administrador")]
        public IActionResult Create(Usuario usuario, IFormFile? avatarFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existente = repositorio.ObtenerPorEmail(usuario.Email);
                if (existente != null)
                    return BadRequest(new { error = "Ya existe un usuario con ese email" });

                usuario.Clave = HashearClave(usuario.Clave);

                // Guardar avatar si se subió
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    string wwwPath = env.WebRootPath;
                    string path = Path.Combine(wwwPath, "Uploads", "Avatars");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    // Alta para obtener el ID
                    int id = repositorio.Alta(usuario);

                    string extension = Path.GetExtension(avatarFile.FileName);
                    string fileName = $"avatar_{id}{extension}";
                    string rutaCompleta = Path.Combine(path, fileName);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        avatarFile.CopyTo(stream);
                    }

                    usuario.Avatar = $"/Uploads/Avatars/{fileName}";
                    repositorio.Modificacion(usuario);
                }
                else
                {
                    int id = repositorio.Alta(usuario);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: Usuario/Editar 
        [HttpPost]
        public IActionResult Editar([FromBody] Usuario usuario)
        {
            try
            {
                if (usuario == null)
                    return BadRequest(new { error = "Datos inválidos" });

                var existente = repositorio.ObtenerPorId(usuario.Id);
                if (existente == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                if (string.IsNullOrWhiteSpace(usuario.Nombre) ||
                    string.IsNullOrWhiteSpace(usuario.Apellido) ||
                    string.IsNullOrWhiteSpace(usuario.Email))
                    return BadRequest(new { error = "Complete nombre, apellido y email" });

                var otro = repositorio.ObtenerPorEmail(usuario.Email);
                if (otro != null && otro.Id != usuario.Id)
                    return BadRequest(new { error = "Ya existe otro usuario con ese email" });

                
                if (!User.IsInRole("Administrador"))
                usuario.Rol = existente.Rol;    

                // Mantener clave si no se envía nueva
                if (string.IsNullOrWhiteSpace(usuario.Clave))
                    usuario.Clave = existente.Clave;
                else
                    usuario.Clave = HashearClave(usuario.Clave);

                repositorio.Modificacion(usuario);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al editar usuario {Id}", usuario.Id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: Usuario/Eliminar
        [Authorize(Policy = "Administrador")]
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try
            {
                if (User.UsuarioId() == id)
                    return BadRequest(new { error = "No puedes eliminar tu propio usuario" });

                repositorio.Baja(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar usuario {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: Usuario/CambiarClave
        [HttpPost]
        public IActionResult CambiarClave([FromBody] CambioClaveView modelo)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuario = repositorio.ObtenerPorId(modelo.Id);
                if (usuario == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                // Verificar clave actual
                if (!string.IsNullOrEmpty(modelo.ClaveVieja))
                {
                    var claveActualHash = HashearClave(modelo.ClaveVieja);
                    if (usuario.Clave != claveActualHash)
                        return BadRequest(new { error = "La contraseña actual es incorrecta" });
                }

                // Hashear
                usuario.Clave = HashearClave(modelo.ClaveNueva);
                repositorio.Modificacion(usuario);
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar clave");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: Usuario/CambiarAvatar
        [HttpPost]
        public async Task<IActionResult> CambiarAvatar(int id, IFormFile avatar)
        {
            try
            {
                if (avatar == null || avatar.Length == 0)
                    return BadRequest(new { error = "Debe seleccionar una imagen" });

                var usuario = repositorio.ObtenerPorId(id);
                if (usuario == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                // Guardar archivo
                string wwwPath = env.WebRootPath;
                string path = Path.Combine(wwwPath, "Uploads", "Avatars");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                // Eliminar avatar anterior si existe
                if (!string.IsNullOrEmpty(usuario.Avatar))
                {
                    string rutaAnterior = Path.Combine(wwwPath, usuario.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(rutaAnterior))
                        System.IO.File.Delete(rutaAnterior);
                }

                string extension = Path.GetExtension(avatar.FileName);
                string fileName = $"avatar_{usuario.Id}{extension}";
                string rutaCompleta = Path.Combine(path, fileName);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                usuario.Avatar = $"/Uploads/Avatars/{fileName}";
                repositorio.Modificacion(usuario);

                return Json(new { success = true, avatarUrl = usuario.Avatar });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar avatar");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string HashearClave(string clave)
        {
            string salt = config["Salt"] ?? "";
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: clave,
                salt: Encoding.ASCII.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));
        }
    }

    // Extensión para obtener el ID del usuario logueado
    public static class UserExtensions
    {
        public static int UsuarioId(this System.Security.Claims.ClaimsPrincipal user)
        {
            var claim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}