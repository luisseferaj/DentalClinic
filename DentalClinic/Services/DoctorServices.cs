using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalClinicNestQuest.Data;
using DentalClinic.Data.DTO;
using DentalClinic.Data.Models;
using DentalClinic.Data.DTO.HostDTO;
using System.Net.Mail;
using System.Net;
using System.Diagnostics.Metrics;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;

namespace DentalClinic.Services
{
    public interface IDoctorServices
    {
        public Task<int> ChangeEmail(int hostId, string newEmail);
        public Task<int> ChangePassword(ChangePasswordDto dto);
        public Task<int> AddService(AddServiceDto obj);
        public Task<object> ServiceInfo(int ServiceId);
        public Task<object[]> ListDoctorServices(int hostId);
        public Task<int> SetServiceAvailability(SetAvailabilityDto dto);
        public Task<bool> ConfirmBooking(BookingDto dto);
        public Task<bool> RejectBooking(BookingDto dto);
        public Task<object[]> ViewBookings(int serviceId);
        public Task<int> ReportClient(AddReportingsDto dto);
        public Task<object> RateClient(int doctorId, double rating);
        public Task<object[]> GetRevenue();
        //add photos when adding a property and fix list all properties function

        public Task<object> GetClientDetailsByBooking(DateTime startDate, DateTime bookingTime, int ServiceId);



    }
    public class DoctorServices : IDoctorServices
    {
        private readonly DBContext _context;

        public DoctorServices(DBContext context)
        {
            _context = context;
        }

        public static async Task<bool> SendEmail(string toEmailAddress, string content, string Subject)
        {
            var fromAddress = new MailAddress("dentalclinic2@gmail.com", "Dental Clinic");
            var toAddress = new MailAddress(toEmailAddress);
            const string fromPassword = "rtbt zmpo lngl uajx";
            string subject = Subject;
            string body = content;

            try
            {
                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    await smtp.SendMailAsync(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<int> ChangeEmail(int doctorId, string email)
        {
            try
            {
                var result = await _context.Users
                            .Where(u => u.User_Id == doctorId)
                            .FirstOrDefaultAsync();

                if (result == null) { return -1; }
                var condition = await _context.Users.Where(e => e.Email == email).FirstOrDefaultAsync();
                if (condition != null)
                {
                    return -2;
                }
                result.Email = email;
                var nr = await _context.SaveChangesAsync();

                await SendEmail(email, $"{result.Name} your email address has been changed this is your new email address that will be used in our app.",
                    "Email Change");

                return nr;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> ChangePassword(ChangePasswordDto dto)
        {
            try
            {
                var result = await _context.Clients
                        .Where(u => u.User_Id == dto.Id)
                        .FirstOrDefaultAsync();
                if (result == null) { return -1; }
                if (ClientServices.VerifyPassword(result.Password, dto.Password))
                {
                    result.Password = ClientServices.HashPassword(dto.NewPassword);

                    var nr = _context.SaveChanges();
                    return nr;
                }
                else
                {
                    return -2;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> AddService(AddServiceDto serviceDto)
        {
            try
            {
                var service = new Services
                {
                    Owner_ID = serviceDto.Owner_ID,
                    Availability = true,
                    Name = serviceDto.Name,
                    Description = serviceDto.Description,
                    Type = serviceDto.Type,
                    Address = serviceDto.Address,

                    Nr_Of_Bookings = 0,
                };

            }
        }

        public async Task<object> ServiceInfo(int serviceId
        {
            try
            {
                var Service = await _context.Service.Where(p => p.Service_ID == serviceId)
                                                        .Include(p => p.Utilities)
                                                        .Include(p => p.Reviews)
                                                        .FirstOrDefaultAsync();
                if (service == null)
                    return null;

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                var serializedResult = JsonSerializer.Serialize(service, options);

                return new JsonResult(serializedResult);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<object[]> ListDoctorServices(int DoctorId)
        {
            try
            {
                var service = await _context.Services
                .Where(p => p.Owner_ID == doctorId)
                .ToArrayAsync();

                if (Services.Any())
                    return Services;
                return [];
            }
            catch (Exception)
            {
                throw;
            }

        }


        public async Task<int> SetServicesAvailability(SetAvailabilityDto dto)
        {
            var result = await _context.Services.Where(p => p.Services_ID == dto.Services_ID)
                                                  .FirstOrDefaultAsync();
            if (result == null) { return -1; }

            result.Availability = dto.Availability;
            var nr = _context.SaveChanges();

            return nr;
        }

        


        public async Task<bool> ConfirmBooking(BookingDto dto)
        {
            try
            {
                var booking = await _context.Bookings
                                            .FirstOrDefaultAsync(u => u.BookingTime == dto.BookingTime
                                                                 && u.Service_Id == dto.Service_Id
                                                                 && u.Date == dto.Date);
                if (booking == null) return false;
                booking.Status = "accepted";
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RejectBooking(BookingDto dto)
        {
            try
            {
                var booking = await _context.Bookings
                                            .FirstOrDefaultAsync(u => u.BookingTime == dto.BookingTime
                                                                 && u.Service_Id == dto.Service_Id
                                                                 && u.Date == dto.Date);
                if (booking == null || booking.Status != "upcoming") return false;
                booking.Status = "canceled";
                await _context.SaveChangesAsync();

                var client = await _context.Users.Where(g => g.User_Id == booking.Client_Id)
                                                .FirstOrDefaultAsync();

                if(client == null) return false;
                await SendEmail(client.Email, $" Hello {client.Name}, your booking has been rejected by the doctor.", "Rejected Booking");


                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<object[]> ViewBookings(int serviceId)
        {
            var booking = await _context.Bookings
                                        .Where(u => u.Service_Id == serviceId).ToArrayAsync();

            return booking;
        }

        public async Task<int> ReportGuest(AddReportingsDto dto)
        {
            try
            {
                var report = new Reportings()
                {
                    Service_Id = dto.Service_Id,
                    Date = dto.Date,
                    BookingTime = dto.BookingTime,
                    Client_Id = dto.Client_Id,
                    Reporting_User_Type = "doctor",
                    Status = "pending",
                    Fine = null,
                    Description = dto.Description
                };

                await _context.Reportings.AddAsync(report);

                string photosDirectoryPath = @"C:\Users\user\Desktop\photos\reportings";

                string fileName = $"{dto.Service_Id}{dto.Date.ToString("yyyy-MM-dd")}.jpg";

                if (!Directory.Exists(photosDirectoryPath))
                {
                    Directory.CreateDirectory(photosDirectoryPath);
                }

                string filePath = Path.Combine(photosDirectoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.photo.CopyToAsync(stream);
                }
                return await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<object> RateClient(int clientId, double rating)
        {
            try
            {
                var result = await _context.Client.Where(h => h.Client_id == clientId).FirstOrDefaultAsync();

                if (result == null) { return null; }
                result.rating += rating;
                result.Nr_Of_Ratings += 1;
                var nr = await _context.SaveChangesAsync();
                if (nr == 0)
                {
                    return null;
                }
                return new { result.rating, result.Nr_Of_Ratings };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object[]> GetRevenue()
        {
            try
            {
                var result = await _context.Bookings
                    .Where(b => b.Status == "done")
                    .Select(b => new
                    {
                        Date = b.End_Date.ToString("yyyy-mm-dd"),
                        Amount = b.Amount * 0.9
                    }).ToArrayAsync();
                if (result == null) { return []; }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> GetClientDetailsByBooking(DateTime Date, DateTime bookingTime, int serviceId)
        {
            try
            {
                var booking = await _context.Bookings.Where(b => b.Services_Id == servieId
                                                        && b.Date == Date
                                                        && b.BookingTime == bookingTime)
                                                        .FirstOrDefaultAsync();
                if (booking == null) { return null; }
                int ClientId = booking.Client_Id;

                var client = await _context.Users.Where(g => g.User_Id == ClientId)
                                          .Select(g => new
                                          {
                                              g.Name,
                                              g.Surname,
                                              g.Email,
                                              g.Phone,
                                              g.Birthday,
                                              g.UserType,
                                              g.Nationality,
                                              g.Client
                                          }).FirstOrDefaultAsync();
                if(client == null) { return null; }

                return client;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
