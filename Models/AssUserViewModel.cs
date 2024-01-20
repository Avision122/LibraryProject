using System.ComponentModel.DataAnnotations;

namespace Projekt_studia2.Models
{
    public class AddUserViewModel
    {
        [Required]
        [Display(Name = "Nazwa użytkownika")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Rola")]
        public string Role { get; set; }
    }

}
