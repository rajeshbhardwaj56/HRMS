using HRMS.API.BusinessLayer.ITF;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.PayRoll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;

        public PayrollController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }
        [HttpPost]
        public IActionResult GetEmployeesMonthlySalary(SalaryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmployeesMonthlySalary(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetEmployeeSalaryMonths(EmployeeSalaryMonthRequestModel request)
        {

                var result = _businessLayer.GetEmployeeSalaryMonths(request.EmployeeID);

            return Ok(result);
            
        }

        [HttpPost]
        public IActionResult GetEmployeeSalary(EmployeeSalaryGetRequestModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmployeeSalary(model));
            return response;
        }

        [HttpPost]
        public IActionResult CalculateEmployeeSalary(EmployeeSalaryRequestModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.CalculateEmployeeSalary(model));
            return response;
        }

        [HttpPost]
        public IActionResult SaveEmployeeSalary(EmployeeSalaryRequestModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.SaveEmployeeSalary(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetPayrollPeriodDetails(PayrollPeriodRequestModel model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.GetPayrollPeriodDetails(
                    model.SalaryMonth,
                    model.SalaryYear
                )
            );

            return response;
        }
        [HttpPost]
        public IActionResult GetPayrollPeriodsForDropdown()
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.GetPayrollPeriodsForDropdown()
            );

            return response;
        }
        [HttpPost]
        public IActionResult GetSalarySlipSettings(SalarySlipSettingsInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.GetSalarySlipSettings(
                    model.CompanyID,
                    model.SalarySlipSettingID
                )
            );

            return response;
        }
        [HttpPost]
        public IActionResult AddUpdateSalarySlipSettings(
    SalarySlipSettingsModel model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.AddUpdateSalarySlipSettings(model)
            );

            return response;
        }
        [HttpPost]
        public IActionResult ExportEmployeeSalary(
            SalaryInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.GetEmployeeSalaryForExport(
                    model.Year ?? 0,
                    model.Month ?? 0
                )
            );

            return response;
        }
        [HttpPost]
        public IActionResult AutoCalculateEmployeeSalary(AutoCalculateSalaryRequestModel model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.AutoCalculateEmployeeSalary(
                    model.Month ?? 0,
                    model.Year ?? 0,
                    model.UserID ?? 0
                )
            );

            return response;
        }
        [HttpPost]
        public IActionResult VerifyEmployeeSalary(
            VerifyEmployeeSalaryRequestModel request)
        {
            IActionResult response = Unauthorized();


            response = Ok(
                _businessLayer.VerifyEmployeeSalary(
                    request.EmployeeIDs,
                    request.Month,
                    request.Year,
                    request.UserID
                   
                )
            );


            return response;
        }
    }
}
