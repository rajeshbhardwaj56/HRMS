using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public CompanyController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        public IActionResult AddUpdateCompany(CompanyModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateCompany(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetAllCompanies(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllCompanies(model));
            return response;
        }
    }
}
