using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Leave;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.Employee.Controllers
{
    [Area(Constants.ManageEmployee)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Employee))]
    public class MyInfoController : Controller
    {
        IHttpContextAccessor _context;
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        public MyInfoController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IHttpContextAccessor context)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            _context = context;
        }

        public IActionResult Index(string id)
        {
            LeaveInputParams model = new LeaveInputParams()
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(id),
            };
            model.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            model.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            model.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetlLeavesSummary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);
            return View(results);
        }

        [HttpPost]
        public IActionResult Index(LeaveResults model)
        {
            model.leaveSummayModel.NoOfDays = (model.leaveSummayModel.EndDate - model.leaveSummayModel.StartDate).Days + 1;
            if ((int)LeaveDay.HalfDay == model.leaveSummayModel.LeaveDurationTypeID)
            {
                model.leaveSummayModel.NoOfDays = model.leaveSummayModel.NoOfDays / 2;
            }

            if (ModelState.IsValid && model.leaveSummayModel.NoOfDays > 0)
            {
                model.leaveSummayModel.LeaveStatusID = (int)LeaveStatus.PendingApproval;

                model.leaveSummayModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
                model.leaveSummayModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
                model.leaveSummayModel.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
                var data = _businessLayer.SendPostAPIRequest(model.leaveSummayModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(data);
                return RedirectToActionPermanent(
                  Constants.Index,
                   WebControllarsConstants.MyInfo,
                 new { id = result.PKNo.ToString() });
            }
            return View(model);
        }
    }
}
