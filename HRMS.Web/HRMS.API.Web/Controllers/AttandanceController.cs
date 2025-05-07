using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.LeavePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttandanceController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;

        public AttandanceController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }
        //[HttpPost]
        //public IActionResult GetAttandance(AttandanceInputParams model)
        //{
        //    IActionResult response = Unauthorized();
        //    response = Ok(_businessLayer.GetAttendance(model));
        //    return response;
        //}
    }
}
