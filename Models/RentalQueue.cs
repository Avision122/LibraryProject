namespace Projekt_studia2.Models
{
    public class RentalQueue
    {
        public int ID { get; set; }
        public int BookID { get; set; }
        public string BookTitle { get; set; }
        public string UserLogin { get; set; }

        public virtual Book Book { get; set; }
    }
}
