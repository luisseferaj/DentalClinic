using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using DentalClinic.Data;
using DentalClinic.Data.DTO;
using DentalClinic.Data.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Linq;
using System.Net.Mail;
using System.Net;

namespace DentalClinic.Services
{
    public interface IClienttServices
    {
        public Task<object[]> ClientAvaibleServices(GestAvaibleServicesDto dto);
        public Task<ActionResult> ServiceInfo(int id);

        public Task<object> GetClient(int id);

        public Task<object> GetDoctor(int id);

        public Task<int> ChangeEmail(int id, string email);

        public Task<int> ChangePassword(ChangePasswordDto dto);

        public Task<int> AddFavorites(int user_id,int service_id);

        public Task<int> DeleteFavorites(int user_id,int service_id);

        public Task<object[]> GetFavorites(int id);

        public Task<bool> CheckAvailability(CheckAvailabilityDto dto);

        public Task<int> AddBooking(Bookings obj);

        public Task<int> CancelBooking(BookingDto dto);

        public Task<object[]> GetBookings(int id);

        public Task<int> AddReview(AddReviewDto dto);

        public Task<int> AddReporting(AddReportingsDto dto);

        public Task<object> AddRatings(AddRatingsDto dto);

        public Task<object> RateDoctor(int id, double rating);
    }
    public class ClientServices : IClientServices
    {
        private readonly DBContext _context;

        public ClientServices(DBContext context)
        {
            _context = context;
        }

        public async Task<bool> SendEmail(string toEmailAddress, string content, string Subject)
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
        public static bool VerifyPassword(string storedHash, string providedPassword)
        {
            // Split the stored hash to get the salt and the hash components
            var parts = storedHash.Split(':', 2);
            if (parts.Length != 2)
            {
                throw new FormatException("The stored password hash is not in the expected format.");
            }

            var salt = Convert.FromBase64String(parts[0]);
            var storedSubkey = parts[1];

            // Hash the provided password using the same salt
            string hashedProvidedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            // Compare the hashes
            return storedSubkey == hashedProvidedPassword;
        }
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        public async Task<int> ChangeEmail(int id, string email)
        {
            try
            {
                var result = await _context.Users
                            .Where(u => u.User_Id == id)
                            .FirstOrDefaultAsync();

                if (result == null) { return -1; }
                var condition = await _context.Users.Where(e => e.Email == email).FirstOrDefaultAsync();
                if (condition != null)
                {
                    return -2;
                }
                result.Email = email;
                var nr=await _context.SaveChangesAsync();

                SendEmail(email, $"{result.Name} your email address has been changed this is your new email address that will be used in our app.",
                    "Email Change");

                return nr;

            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<int> ChangePassword(ChangePasswordDto dto)
        {
            try
            {
                var rezult = await _context.Users
                        .Where(u => u.User_Id == dto.Id)
                        .FirstOrDefaultAsync();
                if (rezult == null) { return -1; }
                if(VerifyPassword(rezult.Password, dto.Password))
                {
                    rezult.Password = HashPassword(dto.NewPassword);

                    var nr=_context.SaveChanges();
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

        public async Task<object> GetClient(int id)
        {
            try
            {
                var guest = await _context.Users
                    .Where(g => g.User_Id == id)
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
                    })
                    .FirstOrDefaultAsync();

                if (client == null)
                {
                    return null;
                }
                return client;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object[]> ClientAvaibleServices(ClientAvaibleServicesDto dto)
        {
            try
            {
                var services = await _context.Services
                                 .Where(p => p.Type == dto.Type &&  
                                             p.Address == dto.Address &&                                     
                                             p.Availability==true)
                                             .Select(p => new
                                             {
                                                 p.Name,
                                                 p.Price,
                                                 p.Address,
                                                 p.Service_ID,                                                                                               
                                             })   
                                             .ToArrayAsync();

                var notAvailableIds = await _context.Bookings
                    .Where(b => services.Select(serv => serv.Service_Id).Contains(b.Service_Id) &&
                           b.Status== "upcoming" && 
                           ((dto.StratDate >= b.Start_Date && dto.StratDate <= b.End_Date) ||
                            (dto.EndDate >= b.Start_Date && dto.EndDate <= b.End_Date)))
                    .Select(b => b.Service_Id)
                    .ToListAsync();

                var availableServices = services.Where(serv => !notAvailableIds.Contains(serv.Service_Id)).ToArray();


                if (availableServices.Any())
                {
                    return availableServices;
                }
                return [];
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ActionResult> ServiceInfo(int id)
        {
            try
            {
                var result= await _context.Services
                            .Where(p=> p.Service_Id == id)                            
                            .Include(p => p.Reviews)
                            .FirstOrDefaultAsync();

                if (result == null)
                {
                    return null;
                }
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                var serializedResult = JsonSerializer.Serialize(result, options);

                return new JsonResult(serializedResult);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> AddFavorites(int user_id, int service_id)
        {
            try
            {
                Favorites obj=new Favorites();
                obj.Service_Id = service_id;
                obj.Client_Id= user_id;

                await _context.Favorites.AddAsync(obj);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                throw;
            }
        }

        public async Task<int> DeleteFavorites(int user_id, int service_id)
        {
            try
            {
                var rezult= await _context.Favorites
                            .Where(f=> f.Service_Id == service_id && f.Client_Id== user_id)
                            .FirstOrDefaultAsync();
                if(rezult == null) { return -1; }
                _context.Favorites.Remove(rezult);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object[]> GetFavorites(int id)
        {
            try
            {
                var rezult= await _context.Favorites
                        .Where(f=> f.Client_Id==id)
                        .Select(f=> f.Service_Id)
                        .ToArrayAsync();

                if(rezult == null) { return []; }

                var rez= await _context.Services
                    .Where(p => rezult.Contains(p.Service_Id))
                    .Select(p => new
                    {
                        p.Name,
                        p.Price,
                        p.Address,
                        p.Service_Id,
                        p.Overall_Rating,                       
                        p.Type,                    
                    })
                    .ToArrayAsync();

                return rez;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> CheckAvailability(CheckAvailabilityDto dto)
        {
            try
            {
                var result = await _context.Bookings
                    .Where(b => b.Service_Id == dto.Service_Id &&
                                 b.Status == "upcoming" &&
                                ((dto.StartDate >= b.Start_Date && dto.StartDate <= b.End_Date) ||
                                 (dto.EndDate >= b.Start_Date && dto.EndDate <= b.End_Date)))
                    .FirstOrDefaultAsync();

                if (result != null) { return false; }

                var rez = await _context.Services
                        .Where(p => p.Service_Id == dto.Service_Id && p.Availability==true)
                        .FirstOrDefaultAsync();
                if (rez == null) { return false; }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> AddBooking(Bookings obj)
        {
            try
            {
                await _context.Bookings.AddAsync(obj);
                var result= await _context.SaveChangesAsync();

                

                var rez=await _context.Services
                    .Where(p=> p.Service_Id==obj.Service_Id)
                    .FirstOrDefaultAsync();

                if(rez != null)
                {
                    rez.Nr_Of_Bookings += 1;
                    _context.SaveChangesAsync();
                }

                await SendBookingEmail(obj.Client_Id, obj.Amount, obj.Service_Name, obj.Start_Date);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> SendBookingEmail(int id,double amount, string name, DateTime Start_Date, DateTime End_Date)
        {
            try
            {
                var result = await _context.Users.Where(u => u.User_Id == id).Select(u => new { u.Email, u.Name }).FirstOrDefaultAsync();
                var body = $"{result.Name} hello!\n" + $"You just successfully booked {name}.\n" +
                    $"Your booking is on {Start_Date.ToString("yyyy-MM-dd")}" +
                    $"for the amount of {amount} €";

                SendEmail(result.Email, body,"Booking confirmation");
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<int> CancelBooking(BookingDto dto)
        {
            try
            {
                var result= await _context.Bookings
                        .Where(b=> b.Service_Id == dto.Service_Id && b.Date == dto.Date && b.BookingTime==dto.BookingTime)
                        .FirstOrDefaultAsync();

                if (result == null) { return -1; }
                result.Status="canceled";
                var nr= await _context.SaveChangesAsync();

                await SendCancelationEmail(result.Client_Id, result.Amount, result.Service_Name, result.Date);

                return nr;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> SendCancelationEmail(int id, double amount, string name, DateTime Date)
        {
            try
            {
                var result = await _context.Users.Where(u => u.User_Id == id).Select(u => new { u.Email, u.Name }).FirstOrDefaultAsync();
                var body = $"{result.Name} hello!\n" + $"You just successfully canceld your booking with {name}.\n";

                SendEmail(result.Email, body, "Booking Cancelation");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<object[]> GetBookings(int id)
        {
            try
            {
                var result = await _context.Bookings
                    .Where(b => b.Client_Id == id)
                    .ToArrayAsync();

                if (result == null) { return [] ; }
                return result;

            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<int> AddReview(AddReviewDto dto)
        {
            try
            {
                Reviews rev= new Reviews();
                rev.Service_Id=dto.Service_Id;
                rev.Description = dto.Review;
                
                await _context.Reviews.AddAsync(rev);
                return await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<int> AddReporting(AddReportingsDto dto)
        {
            try
            {
                Reportings obj = new Reportings();
                obj.Client_Id = dto.Client_Id;
                obj.Service_Id = dto.Service_Id;
                obj.BookingTime=dto.BookingTime;
                obj.Date = dto.Date;
                obj.Reporting_User_Type = "client";
                obj.Status = "pending";
                obj.Fine = null;
                obj.Description = dto.Description;

                await _context.Reportings.AddAsync(obj);

                string photosDirectoryPath = @"C:\Users\User\Desktop\photos\reportings";

                string fileName = $"{dto.Property_Id}{dto.Date.ToString("yyyy-MM-dd")}.jpg";

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
            catch( Exception ex)
            {
                throw;
            }
        }

        public async Task<object> AddRatings(AddRatingsDto dto)
        {
            try
            {
                var rezult = await _context.Services
                        .Where(p => p.Service_Id == dto.Service_Id)
                        .FirstOrDefaultAsync();
                if(rezult == null) { return null; 
                rezult.Rating += dto.Rating;
                rezult.Nr_Of_Ratings += 1;
                var nr = rezult.Nr_Of_Ratings;
                rezult.Overall_Rating = (rezult.Rating / nr) /6;
                var x= await _context.SaveChangesAsync();
                if (x == 0) { return null; }
                return new { rezult.Rating rezult.Nr_Of_Ratings };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> RateDoctor(int id,double rating)
        {
            try
            {
                var result= await _context.Doctor.Where(h=> h.Doctor_Id == id).FirstOrDefaultAsync();

                if(result == null) { return null; }
                result.rating += rating;
                result.Nr_Of_Ratings += 1;
                var nr=await _context.SaveChangesAsync();
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

        public async Task<object> GetDoctor(int id)
        {
            try
            {
                var doctor = await _context.Users
                    .Where(g => g.User_Id == id)
                    .Select(g => new
                    {
                        g.Name,
                        g.Surname,
                        g.Email,
                        g.Phone,
                        g.Birthday,
                        g.UserType,
                        g.Nationality,
                        g.Doctor
                    })
                    .FirstOrDefaultAsync();

                if (doctor == null)
                {
                    return null;
                }
                return doctor;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
