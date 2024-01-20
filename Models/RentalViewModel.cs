namespace Projekt_studia2.Models
{
    public class RentalViewModel
    {
        public int RentalId { get; set; }
        public string BookTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TimeRemaining => (EndDate - DateTime.Today).Days;
    }
}
