using HRMS.API.BusinessLayer.ITF;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.WhatsHappeningModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HRMS.API.Web.Controllers.Holiday
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class HolidayController : ControllerBase
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public HolidayController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        public IActionResult AddUpdateHoliday(HolidayModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateHoliday(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetAllHolidays(HolidayInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllHolidays(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetHolidayList(HolidayInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetHolidayList(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAllHolidayList(HolidayInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllHolidayList(model));
            return response;
        }
        [HttpPost]
        public IActionResult AddUpdateDesignation(DesignationModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateDesignation(model);
            response = Ok(result);
            return response;
        }
        [HttpPost]
        public IActionResult GetAllDesignationList(DesignationInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllDesignationList(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteDesignation(DesignationInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteDesignation(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetDesignationDetails(DesignationInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetDesignationDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult CheckDuplicateDesignation(DesignationModel model)
        {
            bool isDuplicate = _businessLayer.CheckDuplicateDesignation(model);

            // Return lowercase "isDuplicate" to match JS
            return Ok(new
            {
                isDuplicate = isDuplicate
            });
        }
        [HttpPost]
        public IActionResult AddUpdateLOB(LOBModel model)
        {
            IActionResult response = Unauthorized();

            Result result = _businessLayer.AddUpdateLOB(model);

            response = Ok(result);

            return response;
        }

        [HttpPost]
        public IActionResult GetAllLOBList(LOBInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(_businessLayer.GetAllLOBList(model));

            return response;
        }

        [HttpPost]
        public IActionResult DeleteLOB(LOBInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(_businessLayer.DeleteLOB(model));

            return response;
        }

        [HttpPost]
        public IActionResult GetLOBDetails(LOBInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(_businessLayer.GetLOBDetails(model));

            return response;
        }

        [HttpPost]
        public IActionResult CheckDuplicateLOB(LOBModel model)
        {
            bool isDuplicate = _businessLayer.CheckDuplicateLOB(model);

            return Ok(new
            {
                isDuplicate = isDuplicate
            });
        }
        [HttpPost]
        public IActionResult AddUpdateSubDepartment(SubDepartmentModel model)
        {
            IActionResult response = Unauthorized();

            Result result = _businessLayer.AddUpdateSubDepartment(model);

            response = Ok(result);

            return response;
        }

        [HttpPost]
        public IActionResult GetAllSubDepartmentList(SubDepartmentInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(_businessLayer.GetAllSubDepartmentList(model));

            return response;
        }

        [HttpPost]
        public IActionResult DeleteSubDepartment(SubDepartmentInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(_businessLayer.DeleteSubDepartment(model));

            return response;
        }

        [HttpPost]
        public IActionResult GetSubDepartmentDetails(SubDepartmentInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(_businessLayer.GetSubDepartmentDetails(model));

            return response;
        }

        [HttpPost]
        public IActionResult CheckDuplicateSubDepartment(SubDepartmentModel model)
        {
            bool isDuplicate = _businessLayer.CheckDuplicateSubDepartment(model);

            return Ok(new
            {
                isDuplicate = isDuplicate
            });
        }
    }
}
