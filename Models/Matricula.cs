using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EX_Parcial_VilchezGuardia_JF.Models
{   
    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }
    
    public class Matricula
    {
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }
        public Curso? Curso { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Required]
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;

        // Navegación al usuario (Identity)
        public IdentityUser? Usuario { get; set; }
    }
}
