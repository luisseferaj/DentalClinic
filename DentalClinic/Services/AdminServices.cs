using Microsoft.EntityFrameworkCore;
using DentalClinic.Data;
using DentalClinic.Data.DTO.AdminDto;
using DentalClinic.Data.Models;
using System.Net.Mail;
using System.Net;
using System.Linq;

namespace DentalClinic.Services
{
    public interface IAdminServices
    {
        public Task<object> GetAdmin(int id);
        public Task<object[]> GetNotApprovedDoctors();
        public Task<int> ApproveDoctor(int id);
        public Task<int> DontApproveDoctor(int id);
        public Task<object[]> GetClientReportings();
        public Task<object[]> GetDoctorReportings();
        public Task<int> DontApproveReporting(ReportingsDto dto);
        public Task<int> ApproveClientReporting(ApproveReportingDto dto);
        public Task<int> ApproveDoctorReporting(AprvDoctorReportingDto dto);
        public Task<object[]> GetRevenue(); 
    }
    public class AdminServices : IAdminServices
    {
        private readonly DBContext _context;

        public AdminServices(DBContext context)
        {
            _context = context;
        }

        public async Task<int> ApproveDoctorReporting(ApproveReportingDto dto)
        {
            try
            {
                var rezult = await _context.Reportings
                    .Where(r => r.Service_Id == dto.Service_Id)
                    .ToArrayAsync();
                var rez=rezult.Where(r=>r.Start_Date==dto.StarDate).FirstOrDefault();
                var count = rezult.Where(r => r.Status == "approved" && r.Reporting_User_Type == "guest").Count();
                if ( rez== null ) { return -1; }
                rez.Status = "approved";
                rez.Fine = dto.Fine;
                var NR = await _context.SaveChangesAsync();
                await SideWorkApproveClientReporting(rez,count);
                return NR;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> ApproveDoctorReporting(AprvDoctorReportingDto dto)
        {
            try
            {
                var rezult = await _context.Reportings
                    .Where(r => r.Client_Id==dto.ClientId)
                    .ToArrayAsync();
                var rez = rezult.Where(r => r.Start_Date == dto.StarDate && r.Service_Id==dto.Service_id).FirstOrDefault();
                var count = rezult.Where(r => r.Status == "approved" && r.Reporting_User_Type == "doctor").Count();
                if (rez == null) { return -1; }
                rez.Status = "approved";
                rez.Fine = dto.Fine;
                var NR = await _context.SaveChangesAsync();
                await SideWorkApproveDoctorReporting(rez, count);
                return NR;
            }
            catch (Exception ex)
            {
                throw;
            }
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

        public async Task<int> SideWorkApproveDoctorReporting(Reportings obj, int count)
        {
            try
            {
                var guest=await _context.Users.Where(u=> u.User_Id==obj.Client_Id).Select(u => new { u.Client, u.Name, u.Email }).FirstOrDefaultAsync();
                await SendEmail(guest.Email, $"Hello {guest.Name}! We would like to inform you that you have been fined {obj.Fine} €. Because of the following report: {obj.Description}", "Importat information!");
                if (count >= 2)
                {
                    guest.Client.banned = true;
                    await _context.SaveChangesAsync();
                    await SendEmail(guest.Email, $"Hello {guest.Name}! We would like to inform you that you have been banned from our app.", "Importat information!");
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> SideWorkApproveClientReporting(Reportings obj, int count)
        {
            try
            {
                var doctorId = await _context.Services.Where(p => p.Service_Id == obj.Service_Id).FirstOrDefaultAsync();
                var doctor = await _context.Users
                       .Where(u => u.User_Id == doctorId.Owner_ID)
                       .Select(u => new { u.doctor, u.Name, u.Email })
                       .FirstOrDefaultAsync();
                await SendEmail(host.Email, $"Hello {doctor.Name}! We would like to inform you that you have been fined {obj.Fine} €. Because of the following report: {obj.Description}", "Importat information!");
                if (count >= 2)
                {
                    doctorId.Availability = false;
                    doctor.Doctor.banned = true;
                    await _context.SaveChangesAsync();
                    await SendEmail(host.Email, $"Hello {doctor.Name}! We would like to inform you that you have been banned from our app.","Importat information!");
                }
                return 0;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<int> ApproveDoctor(int id)
        {
            try
            {
                var rezult=await _context.Doctor
                        .Where(h=> h.Doctor_Id == id)
                        .FirstOrDefaultAsync();
                if(rezult == null) { return -1; }
                rezult.aproved=true;

                return await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<int> DontApproveReporting(ReportingsDto dto)
        {
            try
            {
                var rezult= await _context.Reportings
                    .Where(r=> r.Services_Id == dto.Service_id && r.Start_Date == dto.StarDate)
                    .FirstOrDefaultAsync();
                if(rezult == null) { return -1; }
                rezult.Status = "notapproved";
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> DontApproveDoctor(int id)
        {
            try
            {
                var rezult = await _context.Users
                        .Where(u => u.User_Id == id && u.UserType=="doctor")
                        .FirstOrDefaultAsync();
                if (rezult == null) { return -1; }
                _context.Remove(rezult);
                var rez= await _context.Doctor
                        .Where(h=>h.Doctor_Id==id)
                        .FirstOrDefaultAsync();
                _context.Remove(rez);

                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> GetAdmin(int id)
        {
                
            try
            {
                var rezult = await _context.Users
                        .Where(u => u.User_Id == id)
                        .FirstOrDefaultAsync();
                if (rezult == null) { return null; }
                return rezult;
            }
            catch (Exception ex)
            {
                throw;
            }
                
        }

        public async Task<object[]> GetClientReportings()
        {
            try
            {
                var rezult= await _context.Reportings
                        .Where(r=>r.Reporting_User_Type=="client" && r.Status=="pending")
                        .ToArrayAsync();
                if (rezult == null) { return []; }
                return rezult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object[]> GetDoctorReportings()
        {
            try
            {
                var rezult = await _context.Reportings
                        .Where(r => r.Reporting_User_Type == "doctor" && r.Status == "pending")
                        .ToArrayAsync();
                if (rezult == null) { return []; }
                return rezult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object[]> GetNotApprovedDoctors()
        {
            try
            {
                var rezult= await _context.Users
                        .Where(u=>u.UserType=="doctor" && u.Doctor.aproved==false)
                        .Select(u => new
                        {
                            u.User_Id,
                            u.Name,
                            u.Surname,
                            u.Email,
                            u.Phone,
                            u.Birthday,
                            u.Nationality,
                            u.Host
                        })
                        .ToArrayAsync();
                if(rezult == null) { return[] ; }
                return rezult;
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
                    .Where(b=>b.Status=="done")
                    .Select(b => new
                {
                    Date = b.End_Date.ToString("yyyy-mm-dd"),
                    Amount=b.Amount*0.1
                }).ToArrayAsync();
                if(result == null) { return []; }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
