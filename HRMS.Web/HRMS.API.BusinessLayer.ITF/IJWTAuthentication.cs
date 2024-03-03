
using HRMS.Models.Common;
using Microsoft.Extensions.Configuration;

namespace HRMS.API.BusinessLayer.ITF
{
    public interface IJWTAuthentication
    {
        public IConfiguration _config { get; set; }
        public LoginUser Authenticate(LoginUser userModel);
    }
}
