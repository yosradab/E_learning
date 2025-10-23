using System.ComponentModel.DataAnnotations;

namespace E_learning.Models
{
  public enum TypeUtilisateur
    {
        Apprenant,
        Formateur,
        Administrateur
    }

    // Classe de base pour tous les utilisateurs, utilisant un enum pour le type.
    public class Utilisateur
    {
        [Key]
        public string IdUtilisateur { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public bool EstActif { get; set; } = true;
        public TypeUtilisateur TypeUtilisateur { get; set; }
        public List<CoursFormateur> CoursFormateurs { get; set; } = new List<CoursFormateur>();
        // Nouvelle relation : Un utilisateur peut avoir plusieurs absences (en tant qu'apprenant)
        public List<CoursApprenant> CoursApprenants { get; set; } = new List<CoursApprenant>();

        public ICollection<Absence> AbsencesApprenant { get; set; } = new List<Absence>();

        public ICollection<Absence> AbsencesEnregistrees { get; set; } = new List<Absence>();
        public List<Absence> ListeAbsencesUtilisateur { get; set; } = new List<Absence>();
    
      public string? Photo { get; set; } 
        public DateTime DateDeNaissance { get; set; }
        public string Adresse { get; set; }
        public string NumTel { get; set; }
        public string Identifiant { get; set; } // Can be used for CIN or Passport number
    }

}