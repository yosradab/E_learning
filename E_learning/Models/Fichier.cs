namespace E_learning.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace E_learning.Models
    {
        public class Fichier
        {
            [Key]
            public string IdFichier { get; set; }
            public string Nom { get; set; }
            public string CheminFichier { get; set; }
            public string TypeFichier { get; set; } 
            // Clé étrangère pour le Cours (nullable)
            public string? CoursId { get; set; }
            public Cours? Cours { get; set; }

            // Clé étrangère pour l'Exercice (nullable)
            public string? ExerciceId { get; set; }
            public Exercice? Exercice { get; set; }
            // Clé étrangère pour le Résultat (nullable)
            public string? ResultatId { get; set; }
            public Resultat? Resultat { get; set; }
        }
    }

}
