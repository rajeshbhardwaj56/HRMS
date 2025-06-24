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
        public async Task<IActionResult> AddUpdateLeavePolicy(LeavePolicyModel model)
        {
            IActionResult response = Unauthorized();
            Result result = await _businessLayer.AddUpdateLeavePolicy(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllLeavePolicies(LeavePolicyInputParans model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllLeavePolicies(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateLeavePolicyDetails(LeavePolicyDetailsModel model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateLeavePolicyDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetLeavePolicyList(LeavePolicyDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetLeavePolicyList(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllLeavePolicyDetails(LeavePolicyDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllLeavePolicyDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLeavePolicyDetails(LeavePolicyDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteLeavePolicyDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllLeavePolicyDetailsByCompanyId(LeavePolicyDetailsInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllLeavePolicyDetailsByCompanyId(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateWhatsHappeningDetails(WhatsHappeningModels model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateWhatsHappeningDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllWhatsHappeningDetails(WhatsHappeningModelParans model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllWhatsHappeningDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWhatsHappening(WhatsHappeningModelParans model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteWhatsHappening(model);
            response = Ok(result);
            return response;
        }


    }
}
