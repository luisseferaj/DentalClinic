using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DentalClinic.Data.DTO.AdminDto;
using DentalClinic.Services;

namespace DentalClinic.Controllers
{
    [Route("api/Admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminServices _adminServices;

        public AdminController(IAdminServices adminServices)
        {
            _adminServices = adminServices;
        }

        [HttpGet("GetAdmin/{id}")]
        public async Task<IActionResult> GetAdmin(string id)
        {
            try
            {
                var rezult = await _adminServices.GetAdmin(int.Parse(id));
                if (rezult == null) { return NotFound(); }
                return Ok(rezult);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetNotApprovedDoctors")]
        public async Task<IActionResult> GetNotApprovedDoctors()
        {
            try
            {
                var rezult = await _adminServices.GetNotApprovedDoctors();
                return Ok(rezult);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPatch("ApproveDoctor/{id}")]
        public async Task<IActionResult> ApproveDoctor(string id)
        {
            try
            {
                var result= await _adminServices.ApproveDoctor(int.Parse(id));
                if (result == -1) { return NotFound();}
                else if (result == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex) 
            {
                return BadRequest();
            }
        }

        [HttpDelete("DontApproveDoctor/{id}")]
        public async Task<IActionResult> DontApproveDoctor(string id)
        {
            try
            {
                var rezult= await _adminServices.DontApproveDoctor(int.Parse(id));
                if(rezult == -1) { return NotFound(); } 
                if (rezult == 0) { return StatusCode(500, "Internal Server Error");}
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetClientReportings")]
        public async Task<IActionResult> GetClientReportings()
        {
            try
            {
                return Ok(await _adminServices.GetClientReportings());
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("GetHostReportings")]
        public async Task<IActionResult> GetClientReportings()
        {
            try
            {
                return Ok(await _adminServices.GetClientReportings());
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPatch("DontApproveReporting")]
        public async Task<IActionResult> DontApproveReporting([FromBody]ReportingsDto dto)
        {
            try
            {
                var rezult= await _adminServices.DontApproveReporting(dto);
                if(rezult == -1) { return NotFound(dto); }
                if (rezult == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPatch("ApproveClientReporting")]
        public async Task<IActionResult> ApproveClientReporting([FromBody] ApproveReportingDto dto)
        {
            try
            {
                var rezult = await _adminServices.ApproveClientReporting(dto);
                if(rezult == -1) { return NotFound(); }
                if (rezult == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPatch("ApproveDoctorReporting")]
        public async Task<IActionResult> ApproveDoctorReporting([FromBody] AprvDoctorReportingDto dto)
        {
            try
            {
                var rezult = await _adminServices.ApproveDoctorReporting(dto);
                if (rezult == -1) { return NotFound(); }
                if (rezult == 0) { return StatusCode(500, "Internal Server Error"); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("GetRevenue")]
        public async Task<IActionResult> GetRevenue()
        {
            try
            {
                return Ok(await _adminServices.GetRevenue());
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
