using HRMS.API.BusinessLayer.ITF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HRMS.API.Web.Controllers.Common
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CommonController : ControllerBase
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public CommonController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public IActionResult GetAllResults(long CompanyID)
        {
            IActionResult response = Unauthorized();
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            results.Countries = _businessLayer.GetAllCountries().Countries;
            results.Langueges = _businessLayer.GetAllCompanyLangueges(CompanyID).Langueges;
            results.Departments = _businessLayer.GetAllCompanyDepartments(CompanyID).Departments;
            results.EmploymentTypes = _businessLayer.GetAllCompanyEmployeeTypes(CompanyID).EmploymentTypes;
            response = Ok(results);
            return response;
        }


        [HttpGet]
        [OutputCache(Duration = 999999)]
        public IActionResult GetAllCountries()
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllCountries());
            return response;
        }


        [HttpGet]
        [OutputCache(Duration = 999999)]
        public IActionResult GetAllLangueges()
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllLangueges());
            return response;
        }

    }
}
