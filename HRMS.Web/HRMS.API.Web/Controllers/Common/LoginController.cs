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
        public IActionResult Login([FromBody] LoginUser login)
        {
            IActionResult response = Unauthorized();
            response = Ok(_jWTAuthentication.Authenticate(login));
            return response;
        }
        
    }
}
