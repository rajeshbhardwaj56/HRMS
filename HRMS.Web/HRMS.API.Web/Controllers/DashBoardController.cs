using System.Data;
using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.ExportEmployeeExcel;
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
        public IActionResult GetDashBoardModel(DashBoardModelInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetDashBoardModel(model));
            return response;
        }
        [HttpGet]
        public IActionResult GetCountryDictionary()
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetCountryDictionary());
            return response;
        }
        [HttpGet]
        public IActionResult  GetCompaniesDictionary()
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetCompaniesDictionary());
            return response;
        }
        [HttpPost]
        public IActionResult  GetEmploymentDetailsDictionaries(EmploymentDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmploymentDetailsDictionaries(model));
            return response;
        }

        [HttpPost]

        public IActionResult AddUpdateEmployeeFromExecel(ImportEmployeeDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmployeeFromExecel(model));
            return response;
        }

        [HttpPost]

        public IActionResult AddUpdateEmployeeFromExecelBulk(BulkEmployeeImportModel employeeList)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmployeeFromExecelBulk(employeeList));
            return response;
        }
        [HttpPost] 
        public IActionResult  GetSubDepartmentDictionary(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetSubDepartmentDictionary(model));
            return response;
        }

        
       

    }
}
