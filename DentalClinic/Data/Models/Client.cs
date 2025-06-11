using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Data.Models
{
    public class Client
    {
        // Foreign key and primary key
        [Key, ForeignKey("User")]
        public int Client_Id { get; set; }
        // Navigation property
        public virtual Users User { get; set; }
        public double rating { get; set; }
        public int Nr_Of_Ratings { get; set; }
        public bool banned { get; set; }
    }
}
    
