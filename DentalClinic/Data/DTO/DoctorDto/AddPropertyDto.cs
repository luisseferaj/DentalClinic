namespace NestQuest.Data.DTO.HostDTO
{
    public class AddPropertyDto
    {
        public int Owner_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public double Daily_Price { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Nr_Rooms { get; set; }
        public int Max_Nr_Of_Guests { get; set; }
        public bool Pets { get; set; }
        public int Nr_Of_Baths { get; set; }
        public string Checkin_Time { get; set; }
        public string Checkout_Time { get; set; }
        public bool Parties { get; set; }
        public bool Smoking { get; set; }
        public int Nr_Of_Parking_Spots { get; set; }

        public List<BedDto> Beds { get; set; }
        public List<UtilitiesDto> Utilities { get; set; }

        //Property Images
        public List<IFormFile> Images { get; set; }
    }
}
