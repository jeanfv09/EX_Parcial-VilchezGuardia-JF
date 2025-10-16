using System.ComponentModel.DataAnnotations;

namespace EX_Parcial_VilchezGuardia_JF.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        public string Codigo { get; set; } = null!; // único definido en OnModelCreating

        [Required]
        public string Nombre { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Créditos debe ser > 0")]
        public int Creditos { get; set; }

        [Range(1, int.MaxValue)]
        public int CupoMaximo { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan HorarioInicio { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; } = true;

    // Navegación a matrículas
    public List<Matricula> Matriculas { get; set; } = new();

        // 🔹 Validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HorarioInicio >= HorarioFin)
            {
                yield return new ValidationResult(
                    "El HorarioInicio debe ser menor que el HorarioFin",
                    new[] { nameof(HorarioInicio), nameof(HorarioFin) }
                );
            }
        }
    }
}
