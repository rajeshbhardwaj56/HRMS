using HRMS.API.BusinessLayer.ITF;
using HRMS.Models;
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AttendenceListController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        public AttendenceListController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;

        }
        [HttpPost]
        public IActionResult AddUpdateAttendenceList(AttendenceListModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateAttendenceList(model);
            response = Ok(result);
            return response;
        }
        [HttpPost]
        public IActionResult GetAllAttendenceList(AttendenceListInputParans model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllAttendenceList(model));
            return response;

        }
        [HttpPost]
        public IActionResult AddUpdateAttendace(Attandance model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateAttendace(model));
            return response;

        }
        [HttpPost]
        public IActionResult GetAttendenceListID(Attandance model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAttendenceListID(model));
            return response;

        }

        [HttpPost]
        public IActionResult DeleteAttendanceDetails(Attandance model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteAttendanceDetails(model));
            return response;

        }

    }
}