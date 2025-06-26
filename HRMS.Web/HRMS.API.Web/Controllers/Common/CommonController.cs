using System.Threading.Tasks;
using HRMS.API.BusinessLayer;
using HRMS.API.BusinessLayer.ITF;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.FormPermission;
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
        [AllowAnonymous]
        public async Task<IActionResult> GetAllResults(long CompanyID)
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();

            var countriesResult = await _businessLayer.GetAllCountries();
            var languagesResult = await _businessLayer.GetAllCompanyLanguages(CompanyID);
            var departmentsResult = await _businessLayer.GetAllCompanyDepartments(CompanyID);
            var employmentTypesResult = await _businessLayer.GetAllCompanyEmployeeTypes(CompanyID);
            var currenciesResult = await _businessLayer.GetAllCurrencies(CompanyID);

            results.Countries = countriesResult.Countries;
            results.Languages = languagesResult.Languages;
            results.Departments = departmentsResult.Departments;
            results.EmploymentTypes = employmentTypesResult.EmploymentTypes;
            results.Currencies = currenciesResult.Currencies;

            return Ok(results);
        }

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetAllCountries()
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllCountries();
            response = Ok(result);
            return response;
        }

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetAllLanguages()
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllLanguages();
            response = Ok(result);
            return response;
        }

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetAllEmployees()
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllEmployees();
            response = Ok(result);
            return response;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.ResetPassword(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetFogotPasswordDetails(ChangePasswordModel model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetFogotPasswordDetails(model);
            response = Ok(result);
            return response;
        }

        #region Page Permission

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetAllCompanyDepartments(long CompanyID)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllCompanyDepartments(CompanyID);
            response = Ok(result);
            return response;
        }

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetAllCompanyFormsPermission(long CompanyID)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllCompanyFormsPermission(CompanyID);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> AddFormPermissions(FormPermissionViewModel objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddFormPermissions(objmodel);
            response = Ok(result);
            return response;
        }

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetFormByDepartmentID(long DepartmentId)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetFormByDepartmentID(DepartmentId);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetUserFormPermissions(FormPermissionVM objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetUserFormPermissions(objmodel);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> AddUserFormPermissions(FormPermissionVM objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUserFormPermissions(objmodel);
            response = Ok(result);
            return response;
        }


        [HttpPost]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetUserFormByDepartmentID(FormPermissionVM objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetUserFormByDepartmentID(objmodel);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> CheckUserFormPermissionByEmployeeID(FormPermissionVM objmodel)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.CheckUserFormPermissionByEmployeeID(objmodel);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        [OutputCache(Duration = 999999)]
        public async Task<IActionResult> GetJobLocationsByCompany(Joblcoations model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetJobLocationsByCompany(model);
            response = Ok(result);
            return response;
        }

        #endregion Page Permission

        #region Exception
        [AllowAnonymous]
        [HttpPost]
        public async  Task<IActionResult> InsertException(ExceptionLogModel model)
        {
           await _businessLayer.InsertException(model);
            return Ok(); 
        }
        #endregion Exception

    }
}
