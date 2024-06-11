using HRMS.Models.Common;
using HRMS.Models.LeavePolicy;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Results = HRMS.Models.Common.Results;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
    public class LeavePolicyController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;

        public LeavePolicyController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        public IActionResult LeavePolicyListing()
        {
            Results results = new Results();
            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult LeavePolicyListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            LeavePolicyInputParans leavePolicyParams = new LeavePolicyInputParans();
            leavePolicyParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            var data = _businessLayer.SendPostAPIRequest(leavePolicyParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);

            return Json(new { data = results.LeavePolicy });

        }

        public IActionResult Index(string id)
        {
            LeavePolicyModel leavePolicyModel = new LeavePolicyModel();
            leavePolicyModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            if (!string.IsNullOrEmpty(id))
            {
                leavePolicyModel.LeavePolicyID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                leavePolicyModel = JsonConvert.DeserializeObject<Results>(data).leavePolicyModel;
            }

            return View(leavePolicyModel);
        }

        [HttpPost]
        public IActionResult Index(LeavePolicyModel leavePolicyModel)
        {
            // Leaves less than maximum leaves

            if (leavePolicyModel.MaximumCasualLeaveAllocationAllowed + leavePolicyModel.MaximumMedicalLeaveAllocationAllowed + leavePolicyModel.MaximumAnnualLeaveAllocationAllowed > leavePolicyModel.MaximumLeaveAllocationAllowed)
            {
                ModelState.AddModelError(string.Empty, "The Casual,Medical and Annual leaves must be equal or less than to Maximum leaves allowed.");
                //return BadRequest("The Casual,Medical and Annual leaves must be equal or less than to Maximum leaves allowed.");
            }

            if (ModelState.IsValid)
            {
                leavePolicyModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

                var data = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.AddUpdateLeavePolicy), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);

                //if (leavePolicyModel.LeavePolicyID > 0)
                //{
                //    return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.LeavePolicy, new { id = leavePolicyModel.LeavePolicyID.ToString() });
                //}
                //else
                //{
                //    return RedirectToActionPermanent(WebControllarsConstants.LeavePolicyListing, WebControllarsConstants.LeavePolicy);
                //}
                return RedirectToActionPermanent(WebControllarsConstants.LeavePolicyListing, WebControllarsConstants.LeavePolicy);
            }
            else
            {
                return View(leavePolicyModel);
            }
        }
    }
}
