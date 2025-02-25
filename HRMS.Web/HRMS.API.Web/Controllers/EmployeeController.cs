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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public IActionResult GetLeaveForApprovals(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveForApprovals(model));
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
        [HttpPost]
        public IActionResult GetFilterEmploymentDetailsByEmployee(EmploymentDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetFilterEmploymentDetailsByEmployee(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteLeavesSummary(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteLeavesSummary(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetEmployeeListByManagerID(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmployeeListByManagerID(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdatePolicyCategory(PolicyCategoryModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdatePolicyCategory(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAllPolicyCategory(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllPolicyCategory(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetPolicyCategoryList(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetPolicyCategoryList(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeletePolicyCategory(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeletePolicyCategory(model));
            return response;
        }
        [HttpPost]
        public IActionResult PolicyCategoryDetails(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.PolicyCategoryDetails(model));
            return response;
        }


    }
}
