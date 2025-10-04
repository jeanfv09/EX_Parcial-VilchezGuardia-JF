using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EX_Parcial_VilchezGuardia_JF.Data;
using EX_Parcial_VilchezGuardia_JF.Models;

namespace EX_Parcial_VilchezGuardia_JF.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Catalogo
        public async Task<IActionResult> Index(string nombre, int? creditosMin, int? creditosMax, TimeSpan? horaInicio, TimeSpan? horaFin)
        {
            var query = _context.Cursos.AsQueryable();

            // Solo cursos activos
            query = query.Where(c => c.Activo);

            // ✅ Filtro por nombre (ahora insensible a mayúsculas/minúsculas y parcial)
            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => EF.Functions.Like(c.Nombre, $"%{nombre}%"));

            // ✅ Validaciones de créditos
            if (creditosMin.HasValue)
                query = query.Where(c => c.Creditos >= creditosMin.Value);

            if (creditosMax.HasValue)
                query = query.Where(c => c.Creditos <= creditosMax.Value);

            // ✅ Validación de horarios
            if (horaInicio.HasValue && horaFin.HasValue)
                query = query.Where(c => c.HorarioInicio >= horaInicio.Value && c.HorarioFin <= horaFin.Value);

            var cursos = await query.ToListAsync();
            return View(cursos);
        }

        // GET: /Catalogo/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

            return View(curso);
        }
    }
}

