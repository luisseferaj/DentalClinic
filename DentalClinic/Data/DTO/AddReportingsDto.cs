namespace DentalClinic.Data.DTO
{
    public class AddReportingsDto
    {
        public int Client_Id { get; set; }
        public int Service_Id { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime BookingTime { get; set; }
        public string Description { get; set; }
        public IFormFile photo { get; set; }
    }
}
