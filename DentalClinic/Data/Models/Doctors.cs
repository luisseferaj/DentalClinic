using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Data.Models
{
    public class Doctors
    {
        [Key, ForeignKey("User")]
        public int Doctor_Id { get; set; }
        public virtual Users User { get; set; }
        public bool aproved { get; set; }
        public bool banned { get; set; }
        public DateTime startDate { get; set; }
        public int Nr_Of_Ratings { get; set; }
        public double rating { get; set; }

    }
}
