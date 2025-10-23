namespace E_learning.Models
{
    public class CoursFormateur
    {
        public string IdCours { get; set; }
        public Cours Cours { get; set; }

        public string IdFormateur { get; set; }
        public Utilisateur Formateur { get; set; }
    
    }
}
