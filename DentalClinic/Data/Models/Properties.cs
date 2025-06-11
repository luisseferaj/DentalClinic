using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Data.Models
{
    public class Services
    {
        [Key]
        public int Service_Id { get; set; }
        public int Owner_ID { get; set; }
        public bool Availability { get; set; }
        public string Name { get; set;}
        public string Description { get; set;}
        public string Type { get; set;}
        public string Address { get; set;}
      
        public double Overall_Rating { get; set; }
        public int Nr_Of_Ratings { get; set; }

        [ForeignKey("Service_Id")]
        public virtual ICollection<Reviews> Reviews { get; set; }
    }
}
