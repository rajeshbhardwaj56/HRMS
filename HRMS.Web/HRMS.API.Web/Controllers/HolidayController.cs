using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models;
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
    }
}
