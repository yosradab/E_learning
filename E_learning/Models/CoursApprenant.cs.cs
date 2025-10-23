using System.ComponentModel.DataAnnotations.Schema;

namespace E_learning.Models
{
    // A new join model for the many-to-many relationship between a Course and a Student.
    public class CoursApprenant
    {
        // Foreign key for the Course
        public string IdCours { get; set; }
        public Cours Cours { get; set; }

        // Foreign key for the Student (Apprenant)
        public string IdApprenant { get; set; }
        public Utilisateur Apprenant { get; set; }
    }
}
