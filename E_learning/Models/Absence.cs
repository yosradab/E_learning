using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_learning.Models
{
    public enum StatutAbsence
    {
        NonJustifiee,
        Justifiee
    }
    public class Absence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string IdAbsence { get; set; }
        public DateTime DateAbsence { get; set; }
        public StatutAbsence Statut { get; set; }
        public string Motif { get; set; }
        public int PeriodeAbsence { get; set; }
        public DateTime DateAjout { get; set; }
        public string UtilisateurId { get; set; }
        [ForeignKey("UtilisateurId")]
        public Utilisateur Utilisateur { get; set; }
        public string EnregistreurId { get; set; }
        [ForeignKey("EnregistreurId")]
        public Utilisateur Enregistreur { get; set; }
    }

}