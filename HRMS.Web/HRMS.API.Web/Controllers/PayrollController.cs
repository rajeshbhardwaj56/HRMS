using HRMS.API.BusinessLayer.ITF;
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
        public IActionResult AddUpdateEmployeeMonthlySalary(EmployeeMonthlySalaryModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmployeeMonthlySalary(model));
            return response;
        }

    }
}
