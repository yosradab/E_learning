using E_learning.Models.E_learning.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_learning.Models
{
    public class Cours
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string IdCours { get; set; }
        public string Niveau { get; set; } 
        public string Titre { get; set; }
        public string Description { get; set; }
        public List<Exercice> ListeExercices { get; set; } = new List<Exercice>();
        public List<Enregistrement> ListeEnregistrements { get; set; } = new List<Enregistrement>();
        public List<CoursFormateur> CoursFormateurs { get; set; } = new List<CoursFormateur>();
        public List<Fichier> ListeFichiers { get; set; } = new List<Fichier>();
        public List<Absence> ListeAbsences { get; set; } = new List<Absence>();
        public List<CoursApprenant> CoursApprenants { get; set; } = new List<CoursApprenant>();



    }
}
