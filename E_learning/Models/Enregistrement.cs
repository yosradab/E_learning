namespace E_learning.Models
{
    public class Enregistrement
    {
        public string IdEnregistrement { get; set; }
        public string LienMP4 { get; set; }
        public DateTime DateCreation { get; set; }

        public string CoursId { get; set; }
        public Cours Cours { get; set; }

        public string? SessionId { get; set; }
        public SessionVisioconference Session { get; set; }
    }

}