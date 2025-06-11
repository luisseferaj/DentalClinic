using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NestQuest.Data.DTO;
using NestQuest.Services;

namespace NestQuest.Controllers
{
    [Route("api/Signup")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly ISignUpServices _signUpService;

        public SignUpController(ISignUpServices signUpService)
        {
            _signUpService = signUpService;
        }
        [HttpPost("SignUpClient")]
        public async Task<ActionResult> SignUpClient([FromForm] SignUpClientDto userDto)
        {
            try
            {
                var user = await _signUpService.SignUpClient(userDto);
                if (user == null) { return Conflict(); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }

        [HttpPost("SignUpDoctor")]
        public async Task<ActionResult> SignUpDoctor([FromForm] SignUpDoctorDto Dto)
        {
            try
            {
                var user = await _signUpService.SignUpDoctor(Dto);
                if (user == null) { return Conflict(); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }
    }
}
