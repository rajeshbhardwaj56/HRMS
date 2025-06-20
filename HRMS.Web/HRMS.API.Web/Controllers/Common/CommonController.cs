using HRMS.API.BusinessLayer;
using HRMS.API.BusinessLayer.ITF;
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
        public IActionResult GetAllResults(long CompanyID)
        {
            IActionResult response = Unauthorized();
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            results.Countries = _businessLayer.GetAllCountries().Countries;
            results.Languages = _businessLayer.GetAllCompanyLanguages(CompanyID).Languages;
            results.Departments = _businessLayer.GetAllCompanyDepartments(CompanyID).Departments;
            results.EmploymentTypes = _businessLayer.GetAllCompanyEmployeeTypes(CompanyID).EmploymentTypes;
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

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetFogotPasswordDetails(ChangePasswordModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetFogotPasswordDetails(model));
            return response;
        }

        #region Page Permission

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public IActionResult GetAllCompanyDepartments(long CompanyID)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllCompanyDepartments(CompanyID));
            return response;
        }

        [HttpGet]
        [OutputCache(Duration = 999999)]
        public IActionResult GetAllCompanyFormsPermission(long CompanyID)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllCompanyFormsPermission(CompanyID));
            return response;
        }

        [HttpPost]
        [OutputCache(Duration = 999999)]
        public IActionResult AddFormPermissions(FormPermissionViewModel objmodel)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddFormPermissions(objmodel));
            return response;
        }
        [HttpGet]
        [OutputCache(Duration = 999999)]
        public IActionResult GetFormByDepartmentID(long DepartmentId)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetFormByDepartmentID(DepartmentId));
            return response;
        }
        
        [HttpPost]
        [OutputCache(Duration = 999999)]
        public IActionResult GetUserFormPermissions(FormPermissionVM objmodel)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetUserFormPermissions(objmodel));
            return response;
        }
        
        [HttpPost]
        [OutputCache(Duration = 999999)]
        public IActionResult AddUserFormPermissions(FormPermissionVM objmodel)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUserFormPermissions(objmodel));
            return response;
        }
        

        [HttpPost]
        [OutputCache(Duration = 999999)]
        public IActionResult GetUserFormByDepartmentID(FormPermissionVM objmodel)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetUserFormByDepartmentID(objmodel));
            return response;
        }
        [HttpPost]
        [OutputCache(Duration = 999999)]
        public IActionResult   CheckUserFormPermissionByEmployeeID(FormPermissionVM objmodel)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.CheckUserFormPermissionByEmployeeID(objmodel));
            return response;
        }
        [HttpPost]
        [OutputCache(Duration = 999999)]
        public IActionResult  GetJobLocationsByCompany(Joblcoations model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetJobLocationsByCompany(model));
            return response;
        }

        #endregion Page Permission



    }
}
