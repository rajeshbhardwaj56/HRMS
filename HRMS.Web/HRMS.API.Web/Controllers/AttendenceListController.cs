using HRMS.API.BusinessLayer.ITF;
<<<<<<< HEAD
using HRMS.Models;
=======
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AttendenceListController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        public AttendenceListController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;

        }
        [HttpPost]
        public IActionResult AddUpdateAttendenceList(AttendenceListModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateAttendenceList(model);
            response = Ok(result);
            return response;
        }
        [HttpPost]
        public IActionResult GetAllAttendenceList(AttendenceListInputParans model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllAttendenceList(model));
            return response;

        }
<<<<<<< HEAD
        [HttpPost]
        public IActionResult AddUpdateAttendace(Attendance model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateAttendace(model));
            return response;

        }
        [HttpPost]
        public IActionResult GetAttendenceListID(Attendance model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendenceListID(model));
            return response;

        }

        [HttpPost]
        public IActionResult DeleteAttendanceDetails(Attendance model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteAttendanceDetails(model));
            return response;

        }
        [HttpPost]
        public IActionResult GetMyAttendanceList(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetMyAttendanceList(model));
            return response;

        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetAttendance(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendance(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAttendanceForCalendar(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendanceForCalendar(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetTeamAttendanceForCalendar(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetTeamAttendanceForCalendar(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetApprovedAttendance(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetApprovedAttendance(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAttendanceForApproval(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendanceForApproval(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetAttendanceDeviceLogs(AttendanceDeviceLog model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendanceDeviceLogs(model));
            return response;
        }
=======

>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
    }
}