using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;

namespace HRMS.API.Web.Controllers.Common
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        IConfiguration _configuration;
        IJWTAuthentication _jWTAuthentication;
        public LoginController(IConfiguration configuration, IJWTAuthentication jWTAuthentication)
        {
            _configuration = configuration;
            jWTAuthentication._config = _configuration;
            _jWTAuthentication = jWTAuthentication;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginUser login)
        {
            var result = await _jWTAuthentication.Authenticate(login);
            if (result == null)
            {
                return Unauthorized();
            }
            return Ok(result);
        }


    }
}
