using HRMS.API.BusinessLayer.ITF;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.LeavePolicy;
using HRMS.Models.WhatsHappeningModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class LeavePolicyController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;

        public LeavePolicyController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        public IActionResult AddUpdateLeavePolicy(LeavePolicyModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateLeavePolicy(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetAllLeavePolicies(LeavePolicyInputParans model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllLeavePolicies(model));
            return response;
        }
         
        [HttpPost]
        public IActionResult AddUpdateLeavePolicyDetails(LeavePolicyDetailsModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateLeavePolicyDetails(model));
            return response;
        }
        
        [HttpPost]
        public IActionResult GetLeavePolicyList(LeavePolicyDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeavePolicyList(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAllLeavePolicyDetails(LeavePolicyDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllLeavePolicyDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteLeavePolicyDetails(LeavePolicyDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteLeavePolicyDetails(model));
            return response;
        }
        
        [HttpPost]
        public IActionResult GetAllLeavePolicyDetailsByCompanyId(LeavePolicyDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllLeavePolicyDetailsByCompanyId(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdateWhatsHappeningDetails(WhatsHappeningModels model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateWhatsHappeningDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAllWhatsHappeningDetails(WhatsHappeningModelParans model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllWhatsHappeningDetails(model));
            return response;
        }
        
        [HttpPost]
        public IActionResult DeleteWhatsHappening(WhatsHappeningModelParans model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteWhatsHappening(model));
            return response;
        }




    }
}
