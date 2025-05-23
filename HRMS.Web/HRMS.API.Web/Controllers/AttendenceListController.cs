using HRMS.API.BusinessLayer.ITF;
using HRMS.Models;
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using HRMS.Models.Employee;
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
        public IActionResult GetAttendanceForMonthlyViewCalendar(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendanceForMonthlyViewCalendar(model));
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
        public IActionResult GetManagerApprovedAttendance(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetManagerApprovedAttendance(model));
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
        [HttpPost]
        public IActionResult GetEmployeeDetails(EmployeePersonalDetailsById objmodel)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmployeeDetails(objmodel));
            return response;
        }
        
        [HttpPost]
        public IActionResult FetchAttendanceHolidayAndLeaveInfo(AttendanceDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.FetchAttendanceHolidayAndLeaveInfo(model));
            return response;
        }
    }
}