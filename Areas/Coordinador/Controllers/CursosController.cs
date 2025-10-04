using EX_Parcial_VilchezGuardia_JF.Data;
using EX_Parcial_VilchezGuardia_JF.Models;
using EX_Parcial_VilchezGuardia_JF.Areas.Coordinador.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EX_Parcial_VilchezGuardia_JF.Areas.Coordinador.Controllers
{
    [Area("Coordinador")]
    [Authorize(Roles = "Coordinador")]
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Coordinador/Cursos
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Matriculas)
                .ToListAsync();
            return View(cursos);
        }

        // GET: /Coordinador/Cursos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Coordinador/Cursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: /Coordinador/Cursos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound();

            return View(curso);
        }

        // POST: /Coordinador/Cursos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // POST: /Coordinador/Cursos/Desactivar/5
        [HttpPost]
        public async Task<IActionResult> Desactivar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound();

            curso.Activo = !curso.Activo;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Coordinador/Cursos/Matriculas/5
        public async Task<IActionResult> Matriculas(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
                return NotFound();

            var viewModel = new CursoConMatriculasViewModel
            {
                CursoId = curso.Id,
                Nombre = curso.Nombre,
                Activo = curso.Activo,
                CupoMaximo = curso.Creditos,
                Matriculas = curso.Matriculas.Select(m => new MatriculaInfo
                {
                    MatriculaId = m.Id,
                    UsuarioId = m.UsuarioId,
                    UsuarioEmail = m.Usuario?.Email,
                    Estado = m.Estado,
                    FechaRegistro = m.FechaRegistro
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Confirmar matrícula
        [HttpPost]
        public async Task<IActionResult> ConfirmarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
                return NotFound();

            matricula.Estado = EstadoMatricula.Confirmada;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
        }

        // POST: Cancelar matrícula
        [HttpPost]
        public async Task<IActionResult> CancelarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
                return NotFound();

            matricula.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
        }
    }
}

