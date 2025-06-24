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
        public async Task<IActionResult> AddUpdateCompany(CompanyModel model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateCompany(model);
            response = Ok(result);
            return response;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GetCompaniesLogo(CompanyLoginModel model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetCompaniesLogo(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllCompanies(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllCompanies(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllCompaniesList(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllCompaniesList(model);
            response = Ok(result);
            return response;
        }
    }
}
