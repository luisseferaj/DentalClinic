namespace DentalClinic.Data.DTO.AdminDto
{
    public class AprvDoctorReportingDto
    {
        public int ClientId { get; set; }
        public int Service_Id { get; set; }
        public DateTime StarDate { get; set; }
        public double Fine { get; set; }
    }
}
