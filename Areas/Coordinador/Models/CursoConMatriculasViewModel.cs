using System;
using System.Collections.Generic;
using EX_Parcial_VilchezGuardia_JF.Models;

namespace EX_Parcial_VilchezGuardia_JF.Areas.Coordinador.Models
{
    public class CursoConMatriculasViewModel
    {
        public int CursoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int CupoMaximo { get; set; }

        public List<MatriculaInfo> Matriculas { get; set; } = new();
    }

    public class MatriculaInfo
    {
        public int MatriculaId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string? UsuarioEmail { get; set; }
        public EstadoMatricula Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}

