﻿using ClinicaMedica.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Xml;

namespace ClinicaMedica.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Personas> Personas { get; set; }
        public DbSet<Pacientes> Pacientes { get; set; }
        public DbSet<Especialidades> Especialidades { get; set; }
        public DbSet<Medicos> Medicos { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Servicios> Servicios { get; set; }
        public DbSet<MediosPago> MedioPagos { get; set; }
        public DbSet<Pagos> Pagos { get; set; }
        public DbSet<CitasMedicas> CitasMedicas { get; set; }
        public DbSet<DetalleCitas> DetalleCitas { get; set; }
        public DbSet<Horarios> Horarios { get; set; }
        public DbSet<Turnos> Turnos { get; set; }
        public DbSet<Paquetes> Paquetes { get; set; }
        public DbSet<PaquetesServicios> PaquetesServicios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Turnos>(entity =>
            {
                entity.HasKey(t => t.TurnoId);

                entity.Property(t => t.Fecha)
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(t => t.Asistencia)
                    .IsRequired();

                entity.Property(t => t.Estado)
                      .HasMaxLength(20)
                      .HasDefaultValue("Disponible");

                // Configurar el tipo de columna para Estado
                entity.Property(t => t.Estado)
                      .HasMaxLength(20) // Tamaño máximo de VARCHAR(20)
                      .HasDefaultValue("Disponible"); // Valor por defecto en la base de datos

                // Configurar PacienteId como clave foránea nullable
                entity.HasOne(t => t.Paciente)
                  .WithMany(p => p.Turnos) // Usa la colección de Turnos en Paciente
                  .HasForeignKey(t => t.PacienteId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false); // Permite valores NULL

                // Relación con Horarios (corrigiendo WithMany)
                entity.HasOne(t => t.Horario)
                    .WithMany(h => h.Turnos) // Ahora usando la colección de Turnos en Horarios
                    .HasForeignKey(t => t.HorarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Médicos (corrigiendo WithMany)
                entity.HasOne(t => t.Medico)
                    .WithMany(m => m.Turnos) // Ahora usando la colección de Turnos en Médicos
                    .HasForeignKey(t => t.MedicoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Agregar restricción UNIQUE en HorarioId, MedicoId y Fecha
                entity.HasIndex(t => new { t.HorarioId, t.MedicoId, t.Fecha })
                    .IsUnique()
                    .HasDatabaseName("UQ_Turnos_Horario_Medico_Fecha");

            });
            //DetalleCitas

            modelBuilder.Entity<DetalleCitas>()
            .HasKey(e => new { e.CitaMedicaId, e.ServicioId });

            modelBuilder.Entity<DetalleCitas>().Property(dc => dc.Cantidad)
                 .IsRequired()
                 .HasDefaultValue(1);
            // Relación entre DetalleCitas y CitasMedicas
            modelBuilder.Entity<DetalleCitas>()
                .HasOne(d => d.CitaMedica) // Propiedad de navegación
                .WithMany(c => c.DetalleCitas) // Relación inversa
                .HasForeignKey(d => d.CitaMedicaId) // Clave foránea
                .OnDelete(DeleteBehavior.Restrict);

            // Relación entre DetalleCitas y Servicios
            modelBuilder.Entity<DetalleCitas>()
                .HasOne(d => d.Servicio) // Propiedad de navegación
                .WithMany(s => s.DetalleCitas) // Relación inversa
                .HasForeignKey(d => d.ServicioId) // Clave foránea
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CitasMedicas>(entity =>
            {
                entity.HasKey(c => c.CitaMedicaId);

                // Relación con Pacientes (NO ACTION en DELETE)
                entity.HasOne(c => c.Paciente)
                      .WithMany(p => p.CitasMedicas) // Asegúrate de que `Pacientes` tiene `ICollection<CitasMedicas>`
                      .HasForeignKey(c => c.PacienteId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Relación con Médicos (NO ACTION en DELETE)
                entity.HasOne(c => c.Medico)
                      .WithMany(m => m.CitasMedicas) // Asegúrate de que `Medicos` tiene `ICollection<CitasMedicas>`
                      .HasForeignKey(c => c.MedicoId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Relación con DetalleCitas
                entity.HasMany(c => c.DetalleCitas)
                      .WithOne(d => d.CitaMedica)
                      .HasForeignKey(d => d.CitaMedicaId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Medicos>(entity =>
            {
                entity.HasKey(m => m.MedicoId);

                entity.Property(m => m.Sueldo)
                    .HasColumnType("decimal(18,2)") // Se recomienda usar decimal en lugar de float para valores monetarios
                    .IsRequired();

                entity.HasOne(m => m.Persona)
                    .WithMany() // Si `Personas` no tiene una lista de médicos, se deja vacío
                    .HasForeignKey(m => m.PersonaId)
                    .OnDelete(DeleteBehavior.Restrict); // Evita eliminación en cascada accidental

                entity.HasOne(m => m.Especialidad)
                    .WithMany(e => e.Medicos) // Se vincula con la propiedad de navegación en Especialidades
                    .HasForeignKey(m => m.EspecialidadId)
                    .OnDelete(DeleteBehavior.Restrict); // Evita eliminaciones accidentales

                entity.HasMany(m => m.Turnos)
                    .WithOne(t => t.Medico) // Asegúrate de que `Turnos` tenga una propiedad `Medico`
                    .HasForeignKey(t => t.MedicoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(m => m.CitasMedicas)
                    .WithOne(c => c.Medico) // Asegúrate de que `CitasMedicas` tenga una propiedad `Medico`
                    .HasForeignKey(c => c.MedicoId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Especialidades>(entity =>
            {
                entity.HasKey(e => e.EspecialidadId);

                entity.Property(e => e.Detalle)
                    .HasMaxLength(300)
                    .IsRequired();

                entity.HasMany(e => e.Medicos)
                    .WithOne(m => m.Especialidad) // Debes asegurarte de que la clase Medicos tiene una propiedad de navegación llamada Especialidad
                    .HasForeignKey(m => m.EspecialidadId)
                    .OnDelete(DeleteBehavior.NoAction); // Esto define el comportamiento en cascada al eliminar una especialidad
            });

            modelBuilder.Entity<Paquetes>(entity =>
            {
                entity.HasKey(p => p.PaqueteId); // Define la clave primaria

                entity.Property(p => p.Nombre)
                    .IsRequired() // Requerido
                    .HasMaxLength(100); // Longitud máxima de 100 caracteres

                entity.Property(p => p.Precio)
                    .IsRequired() // Requerido
                    .HasColumnType("decimal(10,2)"); // Define el tipo de dato en la BD

                entity.HasMany(p => p.PaquetesServicios) // Relación con PaquetesServicios
                    .WithOne(ps => ps.Paquete)
                    .HasForeignKey(ps => ps.PaqueteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de la tabla Servicios
            modelBuilder.Entity<Servicios>(entity =>
            {
                entity.HasKey(s => s.ServicioId); // Define la clave primaria

                entity.Property(s => s.Nombre)
                    .IsRequired() // Campo obligatorio
                    .HasMaxLength(100); // Longitud máxima de 100 caracteres

                entity.Property(s => s.Descripcion)
                    .HasMaxLength(300); // Longitud máxima de 300 caracteres

                entity.Property(s => s.Precio)
                    .IsRequired() // Campo obligatorio
                    .HasColumnType("decimal(10,2)") // Define el tipo de dato en la BD
                    .HasDefaultValue(0); // Valor predeterminado

                entity.HasMany(s => s.DetalleCitas) // Relación con DetalleCitas
                    .WithOne(d => d.Servicio)
                    .HasForeignKey(d => d.ServicioId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(s => s.PaquetesServicios) // Relación con PaquetesServicios
                    .WithOne(ps => ps.Servicio)
                    .HasForeignKey(ps => ps.ServicioId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


        // Configuración de la clave primaria compuesta en PaquetesServicios
        modelBuilder.Entity<PaquetesServicios>()
                .HasKey(ps => new { ps.PaqueteId, ps.ServicioId });

            // Configuración de las relaciones
            modelBuilder.Entity<PaquetesServicios>()
                .HasOne(ps => ps.Paquete)
                .WithMany(p => p.PaquetesServicios)
                .HasForeignKey(ps => ps.PaqueteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaquetesServicios>()
                .HasOne(ps => ps.Servicio)
                .WithMany(s => s.PaquetesServicios)
                .HasForeignKey(ps => ps.ServicioId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

    }
}
