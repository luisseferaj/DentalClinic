using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Data.Models
{
    public class Reportings
    {
        public int Client_Id { get; set; }
        public int Service_Id { get; set; }
        public DateTime BookingTime { get; set; }
        public DateTime Start_Date { get; set; }
        public string Reporting_User_Type { get; set; }
        public string Status { get; set; }
        public double? Fine {get; set;}
        public string Description { get; set; }

    }
}
