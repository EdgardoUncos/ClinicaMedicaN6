﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedica.Entities
{
    public class Pacientes
    {
        [Key]
        public int PacienteId { get; set; }
        [Required]
        public int PersonaId { get; set; }
        public bool ObraSocial { get; set; }

        [ForeignKey(nameof(PersonaId))]
        public Personas Persona { get; set; }

        public ICollection<Turnos> Turnos { get; set; }

        public ICollection<CitasMedicas> CitasMedicas { get; set; }
    }
}
