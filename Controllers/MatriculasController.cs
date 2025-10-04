using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EX_Parcial_VilchezGuardia_JF.Data;
using EX_Parcial_VilchezGuardia_JF.Models;

namespace EX_Parcial_VilchezGuardia_JF.Controllers
{
    [Authorize] // solo usuarios autenticados pueden inscribirse
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.Activo)
            {
                TempData["Error"] = "El curso no existe o no está disponible.";
                return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
            }

            var user = await _userManager.GetUserAsync(User);

            // ❌ Validación: ya matriculado en el mismo curso
            bool yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == user.Id && m.Estado != EstadoMatricula.Cancelada);

            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás matriculado en este curso.";
                return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
            }

            // ❌ Validación: no superar cupo
            int inscritos = await _context.Matriculas
                .CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);

            if (inscritos >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ya alcanzó su cupo máximo.";
                return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
            }

            // ❌ Validación: solapamiento de horarios
            var matriculasUsuario = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == user.Id && m.Estado != EstadoMatricula.Cancelada)
                .ToListAsync();

            foreach (var m in matriculasUsuario)
            {
                if (curso.HorarioInicio < m.Curso.HorarioFin &&
                    curso.HorarioFin > m.Curso.HorarioInicio)
                {
                    TempData["Error"] = $"El curso se solapa con {m.Curso.Nombre}.";
                    return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
                }
            }

            // ✅ Crear matrícula en estado Pendiente
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = user.Id,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Te has inscrito correctamente. Estado: Pendiente.";
            return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
        }
    }
}
