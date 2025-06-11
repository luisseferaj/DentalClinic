using DentalClinic.Data.DTO;
using DentalClinic.Data.Models;
using DentalClinic.Data;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Caching.Memory;



using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DentalClinic.Services
{
    public interface ILogInService
    {
        Task<object> LogIn(LoginCredencialsDto dto);
        Task<object> Confirm_Fa(FaCredencialsDto dto);
        Task<bool> Resend_Fa(string email);
    }
    public class LogInService: ILogInService
    {
        private readonly DBContext _context;
        private readonly IMemoryCache _memoryCache;

        public LogInService(DBContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public string GenerateRandomCode(int length = 6)
        {
            var random = new Random();
            string code = "";
            for (int i = 0; i < length; i++)
                code = System.String.Concat(code, random.Next(0, 10).ToString());
            return code;
        }

        public async Task<bool> Send2FAEmail(string toEmailAddress, string twoFACode)
        {
            var fromAddress = new MailAddress("dentalclinic2@gmail.com", "Dental Clinic");
            var toAddress = new MailAddress(toEmailAddress);
            const string fromPassword = "rtbt zmpo lngl uajx";
            const string subject = "Your 2FA Code";
            string body = $"Your two-factor authentication code is: {twoFACode}";

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

        private bool VerifyPassword(string storedHash, string providedPassword)
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

        public async Task<object> LogIn(LoginCredencialsDto dto)
        {
            try
            {
                var result= await _context.Users
                                 .Where(e => e.Email == dto.Email)
                                 .Include(e=> e.Doctor)
                                 .Include(e=>e.Client)
                                 .FirstOrDefaultAsync();
                bool banned=false;
                bool approved=true;
                string type = "admin";
 
                if (result == null)
                {
                    return null;
                }
                else if (VerifyPassword(result.Password, dto.password)==false)
                {
                    return null;
                }

                if (result.UserType == "client")
                {
                    banned = result.Client.banned;
                    type = "client";
                }
                else if (result.UserType == "doctor")
                {
                    banned = result.Doctor.banned;
                    approved = result.Doctor.aproved;
                    type = "doctor";
                }
                
                if (banned == true)
                {
                    return new { Banned = true, Email = result.Email, Type = type, Two_Fa = false, Approved = approved};
                }
                else if (approved == false)
                {
                    return new { Banned = false, Email = result.Email, Type = type, Two_Fa = false, Approved = approved};
                }
                else if (result.Two_Fa == true)
                {
                    var code=GenerateRandomCode();
                    await Send2FAEmail(result.Email, code);
                    _memoryCache.Set(result.Email, code, TimeSpan.FromMinutes(1));
                    return new { Banned = false, Email = result.Email, Type = type, Two_Fa = true, Approved= approved};

                }
                else { 
                    return new
                    {
                        Id=result.User_Id,
                        Name=result.Name,
                        Surname=result.Surname,
                        Two_fa =false,
                        Type=type,
                        Approved=approved,
                        Banned=banned,
                    };
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> Confirm_Fa(FaCredencialsDto dto)
        {
            try
            {
                _memoryCache.TryGetValue(dto.Email, out var code);
                var result = code as string;
                if (result == null) { return null; }
                else if (result != dto.code) { return null;}
                else
                {
                    var rez=await _context.Users
                                   .Where(e=>e.Email == dto.Email) 
                                   .FirstOrDefaultAsync();

                    if (rez == null) { return null; }
                    else
                    {
                        return new
                        {
                            Id = rez.User_Id,
                            Name = rez.Name,
                            Surname = rez.Surname,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Resend_Fa(string email)
        {
            try
            {
                var code = GenerateRandomCode();
                var x=await Send2FAEmail(email, code);
                _memoryCache.Set(email, code, TimeSpan.FromMinutes(1));
                return x;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
