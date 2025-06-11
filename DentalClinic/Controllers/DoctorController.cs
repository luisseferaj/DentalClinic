using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using DentalClinic.Data.DTO;
using DentalClinic.Data.DTO.HostDTO;
using DentalClinic.Data.Models;
using DentalClinic.Services;
using System.Globalization;

namespace DentalClinic.Controllers
{
    [Route("api/Doctors")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDctorServices _doctorServices;

        public DoctorController(IDoctorServices DoctorServices)
        {
            _doctorServices = doctorServices;
        }

        [HttpPatch("ChangeEmail/{id}/{email}")]
        public async Task<IActionResult> ChangeEmailAsync(string id, string email)
        {
            try
            {
                int ID;
                int.TryParse(id, out ID);
                var result = await _doctorServices.ChangeEmail(ID, email);
                if (result == -1) { return NotFound(); }
                if (result == -2) { return Conflict(); }
                if (result == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var result = -3;
            try
            {
                Console.WriteLine(result);
                result = await _doctorServices.ChangePassword(dto);
                if (result == -1) { return NotFound(); }
                if (result == -2) { return Conflict(); }
                if (result == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("AddService")]
        public async Task<IActionResult> AddService([FromForm] AddServiceDto obj)
        {
            try
            {
                var result = await _doctorServices.AddService(obj);
                if (result == 0) { return NotFound(); }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("ListServices/{doctorId}")]
        public async Task<IActionResult> ListDoctorServices(string doctorId)
        {
            try
            {
                if (!int.TryParse(doctorId, out int ID))
                {
                    return BadRequest("Invalid ID format. ID must be an integer.");
                }
                var result = await _doctorServices.ListDoctorServices(ID);
                return Ok(result);
            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        [HttpGet("Getservice/{serviceId}")]
        public async Task<IActionResult> GetService(string serviceId)
        {
            try
            {
                if (!int.TryParse(serviceId, out int ID))
                {
                    return BadRequest("Invalid ID format. ID must be an integer.");
                }
                var result = await _dcotorServices.ServiceInfo(ID);
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPatch("ChangeAvailability")]
        public async Task<IActionResult> SetAvailability(SetAvailabilityDto dto)
        {
            try
            {
                var result = await _doctorServices.SetServiceAvailability(dto);
                if (result == -1) return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

       

        [HttpGet("ListReviews/{propertyId}")]
        public async Task<IActionResult> GetReviews(string propertyId)
        {
            try
            {
                if (!int.TryParse(propertyId, out int ID))
                {
                    return BadRequest("Invalid ID format. ID must be an integer.");
                }
                var result = await _hostServices.GetPropertyReviews(ID);
                return Ok(result);
            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        [HttpPatch("ConfirmBooking")]
        public async Task<IActionResult> ConfirmBooking(BookingDto dto)
        {
            try
            {
                var result = await _doctorServices.ConfirmBooking(dto);
                if (result == false) return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPatch("RejectBooking")]
        public async Task<IActionResult> RejectBooking(BookingDto dto)
        {
            try
            {
                var result = await _doctorServices.RejectBooking(dto);
                if (result == false) return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetBookings/{serviceId}")]
        public async Task<IActionResult> GetBookings(string serviceId)
        {
            try
            {
                if (!int.TryParse(serviceId, out int ID))
                {
                    return BadRequest("Invalid ID format. ID must be an integer.");
                }
                var result = await _doctorServices.ViewBookings(ID);
                if(result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        [HttpPost("AddReporting")]
        public async Task<IActionResult> AddReporting([FromForm] AddReportingsDto dto)
        {
            try
            {
                var result = await _doctorServices.ReportGuest(dto);
                if (result == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPatch("AddClientRating/{id}/{rating}")]
        public async Task<IActionResult> AddClientRating(string id, string rating)
        {
            try
            {
                var result = await _doctorServices.RateGuest(int.Parse(id), int.Parse(rating));
                if (result == null) { return StatusCode(500, "Internal Server Error"); }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetRevenue")]
        public async Task<IActionResult> GetRevenue()
        {
            try
            {
                return Ok(await _doctorServices.GetRevenue());
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetClient/{startDate}/{bookingTime}/{serviceId}")]
        public async Task<IActionResult> GetClient(string startDate, string bookingTime, string serviceId)
        {
            try
            {
                if (!int.TryParse(serviceId, out int ID))
                {
                    return BadRequest("Invalid ID format. ID must be an integer.");
                }
                DateTime date;

                bool success = DateTime.TryParseExact(startDate, "yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                if (success)
                {
                    Console.WriteLine(date);
                }
                else
                {
                    Console.WriteLine("Failed to parse the date string.");
                }
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime BookingTime = DateTime.Parse(bookingTime);
                var result = await _doctorServices.GetClientDetailsByBooking(StartDate, BookingTime, ID);
                if (result == null) { return NotFound(); };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

    }
}
