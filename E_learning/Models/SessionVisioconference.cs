namespace E_learning.Models
{
    public class SessionVisioconference
    {
        public string IdSession { get; set; }
        public DateTime Date { get; set; }
        public string LienDailyCo { get; set; }
        public string EnregistrementMP4 { get; set; }
        public string NomSession { get; internal set; }
        public string UrlSession { get; internal set; }
        public DateTime DateHeureDebut { get; internal set; }
        public string CoursId { get; internal set; }
    }
}
