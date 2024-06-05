using HRMS.API.BusinessLayer;
using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Leave;
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
            results.Languages = _businessLayer.GetAllCompanyLanguages(CompanyID).Languages;
            results.Departments = _businessLayer.GetAllCompanyDepartments(CompanyID).Departments;
            results.EmploymentTypes = _businessLayer.GetAllCompanyEmployeeTypes(CompanyID).EmploymentTypes;
            results.LeavePolicies = _businessLayer.GetLeavePolicys(new MyInfoInputParams { CompanyID=CompanyID}).leavePolicys;
            results.Currencies = _businessLayer.GetAllCurrencies(CompanyID).Currencies;
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
        public IActionResult GetAllLanguages()
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllLanguages());
            return response;
        }

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public IActionResult GetAllEmployees()
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllEmployees());
            return response;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.ResetPassword(model));
            return response;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetFogotPasswordDetails(ChangePasswordModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetFogotPasswordDetails(model));
            return response;
        }

    }
}
