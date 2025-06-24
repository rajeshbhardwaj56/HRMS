using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.ShiftType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ShiftTypeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;

        public ShiftTypeController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateShiftType(ShiftTypeModel shiftTypeModel)
        {
            IActionResult response = Unauthorized();
            Result result = await _businessLayer.AddUpdateShiftType(shiftTypeModel);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllShiftTypes(ShiftTypeInputParans model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllShiftTypes(model);
            response = Ok(result);
            return response;
        }
    }
}