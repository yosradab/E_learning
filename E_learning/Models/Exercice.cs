using E_learning.Models.E_learning.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace E_learning.Models
{
    public enum TypeExercice
    {
        QCM,
        Audio,
        Texte,
        Autre
    }

    public class Exercice
    {
        [Key]
        public string IdExercice { get; set; }
        public TypeExercice Type { get; set; } 
        public string Contenu { get; set; }
        // La réponse est maintenant facultative
        public string? ReponseCorrecte { get; set; }

        // La date limite est maintenant facultative
        public DateTime? DateLimite { get; set; }


        public string? CoursId { get; set; }
        [JsonIgnore]

        public Cours? Cours { get; set; }
        public List<Fichier> ListeFichiers { get; set; } = new List<Fichier>();

    }
}