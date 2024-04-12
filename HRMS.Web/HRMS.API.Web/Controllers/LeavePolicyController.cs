using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.LeavePolicy;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
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
            response = Ok(_businessLayer.GetAllLeavePolicys(model));
            return response;
        }
    }
}
