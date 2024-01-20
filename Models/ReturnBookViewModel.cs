using Microsoft.AspNetCore.Mvc.Rendering;

namespace Projekt_studia2.Models
{
    public class ReturnBookViewModel
    {
        public int SelectedRentalId { get; set; }
        public List<SelectListItem> Rentals { get; set; }
    }

}
