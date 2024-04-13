using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HRMS.API.Web.Controllers
{

    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public EmployeeController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        public IActionResult AddUpdateEmployee(EmployeeModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateEmployee(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetAllEmployees(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllEmployees(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetlLeavesSummary(LeaveInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetlLeavesSummary(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdateLeave(LeaveSummayModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateLeave(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetLeaveTypes(LeaveInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveTypes(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetLeaveDurationTypes(LeaveInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveDurationTypes(model));
            return response;
        }
    }
}
