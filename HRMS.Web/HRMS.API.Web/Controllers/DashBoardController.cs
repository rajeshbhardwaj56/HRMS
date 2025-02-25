using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.DashBoard;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class DashBoardController : ControllerBase
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public DashBoardController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        public IActionResult GetDashBoardodel(DashBoardModelInputParams model)
        {
            IActionResult response = Unauthorized();            
            response = Ok(_businessLayer.GetDashBoardodel(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAttandance(AttandanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendance(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAttendanceForCalendar(AttandanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendanceForCalendar(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetTeamAttendanceForCalendar(AttandanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetTeamAttendanceForCalendar(model));
            return response;
        }
    }
}
