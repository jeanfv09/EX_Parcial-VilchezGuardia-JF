using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EX_Parcial_VilchezGuardia_JF.Data;
using EX_Parcial_VilchezGuardia_JF.Models;
using Microsoft.Extensions.Caching.Distributed; // 👈 agregado para usar Redis
using System.Text.Json; // 👈 serializar/deserializar

namespace EX_Parcial_VilchezGuardia_JF.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache; // 👈 agregado para Redis

        private const string CursosCacheKey = "cursos_activos"; // 👈 clave cache

        public CatalogoController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Catalogo
        public async Task<IActionResult> Index(string nombre, int? creditosMin, int? creditosMax, TimeSpan? horaInicio, TimeSpan? horaFin)
        {
            List<Curso> cursos;

            // 👇 Intentar leer de cache Redis
            var cached = await _cache.GetStringAsync(CursosCacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                cursos = JsonSerializer.Deserialize<List<Curso>>(cached)!;
            }
            else
            {
                cursos = await _context.Cursos.Where(c => c.Activo).ToListAsync();

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) // TTL 60s
                };
                await _cache.SetStringAsync(CursosCacheKey, JsonSerializer.Serialize(cursos), options);
            }

            // ✅ filtros aplicados sobre la lista
            var query = cursos.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));
                
            if (creditosMin.HasValue)
                query = query.Where(c => c.Creditos >= creditosMin.Value);

            if (creditosMax.HasValue)
                query = query.Where(c => c.Creditos <= creditosMax.Value);
                

            if (horaInicio.HasValue && horaFin.HasValue)
                query = query.Where(c => c.HorarioInicio >= horaInicio.Value && c.HorarioFin <= horaFin.Value);

            return View(query.ToList());
        }

        // GET: /Catalogo/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

            // 👇 Guardar en sesión el último curso visitado
            HttpContext.Session.SetInt32("LastCourseId", curso.Id);
            HttpContext.Session.SetString("LastCourseName", curso.Nombre);

            return View(curso);
        }

        // 👇 Nuevo método para invalidar cache cuando se cree o edite curso
        [NonAction]
        public async Task InvalidateCursosCache()
        {
            await _cache.RemoveAsync(CursosCacheKey);
        }
    }
}

