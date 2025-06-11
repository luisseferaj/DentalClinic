using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using DentalClinic.Data.DTO;
using DentalClinic.Data.Models;
using DentalClinic.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DentalClinic.Controllers
{
    [Route("api/Clients")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientServices _clientServices;

        public ClientController(IClientServices clientServices)
        {
            _clientServices = clientServices;
        }

        [HttpPost("AvaibleServices")]
        public async Task<IActionResult> AvaibleServices([FromBody] GestAvaibleServicesDto dto)
        {
            try
            {
                var result = await _clientServices.ClientAvaibleServices(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetService/{id}")]
        public async Task<IActionResult> ServiceInfo(string id)
        {
            try
            {
                int ID;
                int.TryParse(id, out ID);

                var result = await _clientServices.ServiceInfo(ID);
                if (result == null) { return NotFound(); }
                return Ok(result);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("GetClient/{id}")]
        public async Task<IActionResult> GetClient(string id)
        {
            try
            {
                int ID;
                int.TryParse(id, out ID);
                var result = await _clientServices.GetClient(ID);
                if (result == null) { return NotFound(); };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPatch("ChangeEmail/{id}/{email}")]
        public async Task<IActionResult> ChangeEmailAsync(string id, string email)
        {
            try
            {
                int ID;
                int.TryParse(id, out ID);
                var rezult = await _clientServices.ChangeEmail(ID, email);
                if (rezult == -1) { return NotFound(); }
                if (rezult == -2) { return Conflict(); }
                if (rezult == 0) { return StatusCode(500, "Internal Server Error"); }
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
            var rezult = -3;
            try
            {
                Console.WriteLine(rezult);
                rezult = await _clientServices.ChangePassword(dto);
                if (rezult == -1) { return NotFound(); }
                if (rezult == -2) { return Conflict(); }
                if (rezult == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("AddFavorites/{client_id}/{service_id}")]
        public async Task<IActionResult> AddFavorites(string client_id, string service_id)
        {
            try
            {
                var result = await _clientServices.AddFavorites(int.Parse(client_id), int.Parse(service_id));
                if (result == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("DeleteFavorite/{client_id}/{service_id}")]
        public async Task<IActionResult> DeleteFavorite(string client_id, string service_id)
        {
            try
            {
                var result = await _clientServices.DeleteFavorites(int.Parse(client_id), int.Parse(service_id));
                if(result == -1) { return NotFound(); }
                if (result == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch(Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetFavorites/{id}")]
        public async Task<IActionResult> GetFavorites(string id)
        {
            try
            {
                var result= await _clientServices.GetFavorites(int.Parse(id));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CheckAvailability")]
        public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityDto dto)
        {
            try
            {
                var rezult = await _clientServices.CheckAvailability(dto);
                if (rezult) { return Ok(); }
                return Conflict();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }
        [HttpPost("AddBooking")]
        public async Task<IActionResult> AddBookings([FromBody] Bookings obj)
        {
            try
            {
                obj.Status = "upcoming";
                var result= await _clientServices.AddBooking(obj);
                if(result == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPatch("CancelBooking")]
        public async Task<IActionResult> CancelBooking(BookingDto dto)
        {
            try
            {
                var result=await _clientServices.CancelBooking(dto);
                if (result == -1) { return NotFound(); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("GetBookings/{id}")]
        public async Task<IActionResult> GetBookings(string id)
        {
            try
            {
                return Ok(await _clientServices.GetBookings(int.Parse(id)));
            }
            catch(Exception ex) 
            {
                return BadRequest();
            }
        }

        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewDto dto)
        {
            try
            {
                var result = await _clientServices.AddReview(dto);
                if (result == 0) {return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("AddReporting")]
        public async Task<IActionResult> AddReporting([FromForm] AddReportingsDto dto)
        {
            try
            {
                var result= await _clientServices.AddReporting(dto);
                if (result == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

       
        [HttpPatch("AddDoctorsRating/{id}/{rating}")]
        public async Task<IActionResult> AddDoctorRating(string id, string rating)
        {
            try
            {
                var rezult = await _doctorServices.RateDoctors(int.Parse(id),int.Parse(rating));
                if (rezult == null) { return StatusCode(500, "Internal Server Error"); }
                return Ok(rezult);
            }
            catch (Exception ex)
            {
                return BadRequest();    
            }
        }

        [HttpGet("GetDoctor/{id}")]
        public async Task<IActionResult> GetDoctor(string id)
        {
            try
            {
                int ID;
                int.TryParse(id, out ID);
                var result = await _clientServices.GetDoctor(ID);
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
