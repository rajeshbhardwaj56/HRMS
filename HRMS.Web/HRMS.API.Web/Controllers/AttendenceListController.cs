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
        public async Task<IActionResult> AddUpdateAttendenceList(AttendenceListModel model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateAttendenceList(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllAttendenceList(AttendenceListInputParans model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllAttendenceList(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateAttendace(Attendance model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateAttendace(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAttendenceListID(Attendance model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAttendenceListID(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttendanceDetails(Attendance model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteAttendanceDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetMyAttendanceList(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetMyAttendanceList(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetAttendance(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAttendance(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAttendanceForMonthlyViewCalendar(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAttendanceForMonthlyViewCalendar(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetTeamAttendanceForCalendar(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetTeamAttendanceForCalendar(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetApprovedAttendance(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetApprovedAttendance(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetManagerApprovedAttendance(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetManagerApprovedAttendance(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAttendanceForApproval(AttendanceInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAttendanceForApproval(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAttendanceDeviceLogs(AttendanceDeviceLog model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAttendanceDeviceLogs(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeeDetails(EmployeePersonalDetailsById objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetEmployeeDetails(objmodel);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> FetchAttendanceHolidayAndLeaveInfo(AttendanceDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.FetchAttendanceHolidayAndLeaveInfo(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetCompOffAttendanceList(CompOffAttendanceInputParams objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetCompOffAttendanceList(objmodel);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetApprovedCompOff(CompOffInputParams objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetApprovedCompOff(objmodel);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateCompOffAttendace(CompOffAttendanceRequestModel objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateCompOffAttendace(objmodel);
            response = Ok(result);
            return response;
        }

    }
}