using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Projekt_studia2.Models
{
    public class ReturnQueue
    {
        [Key]
        public int Id { get; set; }

        public string UserLogin { get; set; }

        [ForeignKey("Book")]
        public int BookId { get; set; }
        public virtual Book Book { get; set; }

        public string BookTitle { get; set; }

        public DateTime RequestDate { get; set; }
    }
}
