using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Data.Models
{
    public class Reviews
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }

        [ForeignKey("Services")]
        public int Service_Id { get; set; }

        public virtual Services Services { get; set; }
    }
}
