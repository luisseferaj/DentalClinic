using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Data.Models
{
    public class Favorites
    {
        public int Client_Id { get; set; }
        public int Service_Id { get; set; }
    }
}
