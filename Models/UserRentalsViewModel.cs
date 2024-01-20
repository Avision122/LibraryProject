using Microsoft.AspNetCore.Mvc.Rendering;

namespace Projekt_studia2.Models
{
    public class UserRentalsViewModel
    {
        public string SelectedUsername { get; set; }
        public List<SelectListItem> Users { get; set; }
        public IEnumerable<RentalDetails> Rentals { get; set; }
    }

    public class RentalDetails
    {
        public string BookTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RemainingDays => (EndDate - DateTime.Today).Days;
    }

}
