namespace Projekt_studia2.Models
{
    using System.ComponentModel.DataAnnotations;

    public class AddBookViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        [Required]
        [Range(0, 3000)]
        public int Year { get; set; }

        [Required]
        [Range(0, 10000)]
        public int AvailableCopies { get; set; }
    }

}
