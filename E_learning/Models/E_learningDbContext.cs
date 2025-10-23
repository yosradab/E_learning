using E_learning.Models;
using E_learning.Models.E_learning.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace E_learning.Models
{
    public class ElearningDbContext : DbContext
    {
        public ElearningDbContext(DbContextOptions<ElearningDbContext> options)
            : base(options)
        {
        }

        // Existing DbSets
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Cours> Cours { get; set; }
        public DbSet<SessionVisioconference> SessionsVisioconference { get; set; }
        public DbSet<Exercice> Exercices { get; set; }
        public DbSet<Enregistrement> Enregistrements { get; set; }
        public DbSet<Fichier> Fichiers { get; set; }
        public DbSet<Absence> Absences { get; set; }
        public DbSet<Resultat> Resultats { get; set; }

        // Existing join table for Courses and Trainers
        public DbSet<CoursFormateur> CoursFormateurs { get; set; }

        // NEW: The DbSet for the new Courses and Students join table
        public DbSet<CoursApprenant> CoursApprenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys (existing code)
            modelBuilder.Entity<Utilisateur>().HasKey(u => u.IdUtilisateur);
            modelBuilder.Entity<Cours>().HasKey(c => c.IdCours);
            modelBuilder.Entity<SessionVisioconference>().HasKey(s => s.IdSession);
            modelBuilder.Entity<Exercice>().HasKey(e => e.IdExercice);
            modelBuilder.Entity<Enregistrement>().HasKey(en => en.IdEnregistrement);
            modelBuilder.Entity<Fichier>().HasKey(f => f.IdFichier);

            // --- Configuration for Cours-Formateur Relationship ---
            // The composite key no longer includes IdApprenant
            modelBuilder.Entity<CoursFormateur>()
                .HasKey(cf => new { cf.IdCours, cf.IdFormateur });

            modelBuilder.Entity<CoursFormateur>()
                .HasOne(cf => cf.Cours)
                .WithMany(c => c.CoursFormateurs)
                .HasForeignKey(cf => cf.IdCours);

            modelBuilder.Entity<CoursFormateur>()
                .HasOne(cf => cf.Formateur)
                .WithMany(u => u.CoursFormateurs)
                .HasForeignKey(cf => cf.IdFormateur)
                .OnDelete(DeleteBehavior.NoAction);

            // The configuration for Apprenant is now removed from this section.

            // --- NEW: Configuration for Cours-Apprenant Relationship ---
            modelBuilder.Entity<CoursApprenant>()
                .HasKey(ca => new { ca.IdCours, ca.IdApprenant });

            modelBuilder.Entity<CoursApprenant>()
                .HasOne(ca => ca.Cours)
                .WithMany(c => c.CoursApprenants)
                .HasForeignKey(ca => ca.IdCours);

            modelBuilder.Entity<CoursApprenant>()
                .HasOne(ca => ca.Apprenant)
                .WithMany(u => u.CoursApprenants)
                .HasForeignKey(ca => ca.IdApprenant)
                .OnDelete(DeleteBehavior.NoAction);

            // --- Configuration for Cours-Fichier Relationship ---
            modelBuilder.Entity<Cours>()
                .HasMany(c => c.ListeFichiers)
                .WithOne(f => f.Cours)
                .HasForeignKey(f => f.CoursId)
                .OnDelete(DeleteBehavior.SetNull);

            // Other existing configurations... (unchanged)
            modelBuilder.Entity<Fichier>()
                .HasOne(f => f.Exercice)
                .WithMany(e => e.ListeFichiers)
                .HasForeignKey(f => f.ExerciceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Cours>()
                .HasMany(c => c.ListeExercices)
                .WithOne(e => e.Cours)
                .HasForeignKey(e => e.CoursId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cours>()
                .HasMany(c => c.ListeEnregistrements)
                .WithOne(en => en.Cours)
                .HasForeignKey(en => en.CoursId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionVisioconference>()
                .HasMany<Enregistrement>()
                .WithOne(en => en.Session)
                .HasForeignKey(en => en.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Absence>()
                .HasOne(a => a.Utilisateur)
                .WithMany(u => u.ListeAbsencesUtilisateur)
                .HasForeignKey(a => a.UtilisateurId)
                .OnDelete(DeleteBehavior.NoAction);

          

            modelBuilder.Entity<Absence>()
                .HasOne(a => a.Enregistreur)
                .WithMany(u => u.AbsencesEnregistrees)
                .HasForeignKey(a => a.EnregistreurId)
                .OnDelete(DeleteBehavior.NoAction);
            // Relation entre Resultat et Fichier (un-à-zéro-ou-un)
            modelBuilder.Entity<Resultat>()
                .HasOne(r => r.Fichier)
                .WithOne(f => f.Resultat)
                .HasForeignKey<Resultat>(r => r.FichierId)
                .IsRequired(false) // Le lien est optionnel
                .OnDelete(DeleteBehavior.SetNull);

            // Relation entre Utilisateur et Resultat (un utilisateur peut être un apprenant pour plusieurs résultats)
            modelBuilder.Entity<Resultat>()
                .HasOne(r => r.Apprenant)
                .WithMany()
                .HasForeignKey(r => r.IdApprenant)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation entre Utilisateur et Resultat (un utilisateur peut être un auteur pour plusieurs résultats)
            modelBuilder.Entity<Resultat>()
                .HasOne(r => r.Auteur)
                .WithMany()
                .HasForeignKey(r => r.IdAuteur)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation entre Cours et Resultat (un cours peut avoir plusieurs résultats)
            modelBuilder.Entity<Resultat>()
                .HasOne(r => r.Cours)
                .WithMany()
                .HasForeignKey(r => r.CoursId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
