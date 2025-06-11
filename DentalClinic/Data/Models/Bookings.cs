using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Data.Models
{
    public class Bookings
    {
        public int Client_Id { get; set;}
        public int Service_Id { get; set;}
        public string Service_Name { get; set;}
        public DateTime BookingTime { get; set; }
        public DateTime Date { get; set;}
        public double Amount { get; set;}
        public string Status { get; set;}
    }
}
