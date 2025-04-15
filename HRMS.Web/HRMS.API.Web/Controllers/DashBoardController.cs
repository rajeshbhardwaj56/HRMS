using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.DashBoard;
<<<<<<< HEAD
using HRMS.Models.Employee;
using HRMS.Models.ImportFromExcel;
using Microsoft.AspNetCore.Authorization;
=======
using HRMS.Models.WhatsHappening;
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
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
<<<<<<< HEAD
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
        [AllowAnonymous]
        public IActionResult AddUpdateEmployeeFromExecel(ImportEmployeeDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmployeeFromExecel(model));
            return response;
        }
        [HttpPost] 
        public IActionResult  GetSubDepartmentDictionary(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetSubDepartmentDictionary(model));
            return response;
        }

=======
        public IActionResult GetDashBoardodel(DashBoardModelInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetDashBoardodel(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdateWhatsHappening(WhatsHappening model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateWhatsHappening(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetWhatsHappenings(WhatsHappening model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetWhatsHappenings(model));
            return response;
        }
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39

    }
}
