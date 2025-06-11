using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using DentalClinic.Data;
using DentalClinic.Data.DTO;
using DentalClinic.Data.Models;
using System;
using System.Security.Cryptography;

namespace DentalClinic.Services
{
    public interface ISignUpServices
    {
        Task<Users> SignUpHost(SignUpDoctorDto doctorDto);
        Task<Users> SignUpGuest(SignUpClientDto userDto);
    }
    public class SignUpServices : ISignUpServices
    {
        private readonly DBContext _context;

        public SignUpServices(DBContext context)
        {
            _context = context;
        }
        private string HashPassword(string password)
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

        public async Task<Users> SignUpClient(SignUpClientDto userDto)
        {

            try
            {
                var condition = await _context.Users.Where(e => e.Email == userDto.Email).FirstOrDefaultAsync();
                if (condition != null)
                {
                    return null;
                }
                Users newUser = new Users
                {
                    Name = userDto.Name,
                    Surname = userDto.Surname,
                    Email = userDto.Email,
                    Password = HashPassword(userDto.Password),
                    Phone = userDto.Phone,
                    Birthday = userDto.Birthday,
                    UserType = "guest",
                    Two_Fa = userDto.Two_Fa,
                    Nationality = userDto.Nationality,
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                Client client = new Client
                {
                    Client_Id = newUser.User_Id,
                    rating = 5.0,
                    Nr_Of_Ratings = 1,
                    banned = false,
                };
                await _context.Client.AddAsync(guest);
                await _context.SaveChangesAsync();

                string fileName = $"{newUser.Client_Id}.jpg";
                string photosDirectoryPath = @"C:\Users\User\Desktop\photos\users";

                if (!Directory.Exists(photosDirectoryPath))
                {
                    Directory.CreateDirectory(photosDirectoryPath);
                }

                string filePath = Path.Combine(photosDirectoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await userDto.photo.CopyToAsync(stream);
                }
                return newUser;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<Users> SignUpDoctor(SignUpDoctorDto doctorDto)
        {
            try
            {
                var condition = await _context.Users.Where(e => e.Email == doctorDto.Email).FirstOrDefaultAsync();
                if (condition!=null) {
                    return null;
                }
                Users newUser = new Users
                {
                    Name = doctorDto.Name,
                    Surname = doctorDto.Surname,
                    Email = doctorDto.Email,
                    Password = HashPassword(doctorDto.Password),
                    Phone = doctorDto.Phone,
                    Birthday = doctorDto.Birthday,
                    UserType = "doctor",
                    Two_Fa = doctorDto.Two_Fa,
                    Nationality = doctorDto.Nationality,
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                Doctors doctor = new Doctors
                {
                    Doctor_Id = newUser.User_Id,
                    aproved = false,
                    banned=false,
                    startDate=DateTime.UtcNow,
                    Nr_Of_Ratings = 1,
                    rating =5.0,

            };
                await _context.Doctor.AddAsync(doctor);
                await _context.SaveChangesAsync();

                string fileName = $"{newUser.User_Id}.jpg";
                string photosDirectoryPath = @"C:\Users\User\Desktop\photos\users";

                if (!Directory.Exists(photosDirectoryPath))
                {
                    Directory.CreateDirectory(photosDirectoryPath);
                }

                string filePath = Path.Combine(photosDirectoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doctorDto.photo.CopyToAsync(stream);
                }

                return newUser;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
