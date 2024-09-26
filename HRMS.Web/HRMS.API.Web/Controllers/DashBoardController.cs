using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.DashBoard;
using HRMS.Models.WhatsHappening;
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

    }
}
