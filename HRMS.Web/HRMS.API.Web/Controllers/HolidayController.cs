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
        public async Task<IActionResult> AddUpdateHoliday(HolidayModel model)
        {
            IActionResult response = Unauthorized();
            Result result = await _businessLayer.AddUpdateHoliday(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllHolidays(HolidayInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllHolidays(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetHolidayList(HolidayInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetHolidayList(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllHolidayList(HolidayInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllHolidayList(model);
            response = Ok(result);
            return response;
        }
    }
}
