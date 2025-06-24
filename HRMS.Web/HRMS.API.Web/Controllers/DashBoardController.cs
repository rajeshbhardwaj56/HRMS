using System.Data;
using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.ImportFromExcel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class DashBoardController : ControllerBase
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public DashBoardController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        public async Task<IActionResult> GetDashBoardModel(DashBoardModelInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetDashBoardModel(model);
            response = Ok(result);
            return response;
        }

        [HttpGet]
        public async Task<IActionResult> GetCountryDictionary()
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetCountryDictionary();
            response = Ok(result);
            return response;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompaniesDictionary()
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetCompaniesDictionary();
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetEmploymentDetailsDictionaries(EmploymentDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetEmploymentDetailsDictionaries(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateEmployeeFromExecel(ImportEmployeeDetail model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateEmployeeFromExecel(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateEmployeeFromExecelBulk(BulkEmployeeImportModel employeeList)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateEmployeeFromExecelBulk(employeeList);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetSubDepartmentDictionary(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetSubDepartmentDictionary(model);
            response = Ok(result);
            return response;
        }

    }
}
