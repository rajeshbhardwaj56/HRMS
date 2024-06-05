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
        public IActionResult GetAllActiveEmployees(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllActiveEmployees(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetlLeavesSummary(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetlLeavesSummary(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetlAllLeavesSummary(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllLeavesSummary(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdateLeave(LeaveSummaryModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateLeave(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetLeavePolicys(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeavePolicys(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetLeaveTypes(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveTypes(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetLeaveDurationTypes(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveDurationTypes(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetMyInfo(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetMyInfo(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdateEmploymentDetails(EmploymentDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmploymentDetails(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetEmploymentDetailsByEmployee(EmploymentDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmploymentDetailsByEmployee(model));
            return response;
        }
        
    }
}
