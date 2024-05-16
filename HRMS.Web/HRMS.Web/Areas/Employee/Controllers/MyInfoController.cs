using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Leave;
using HRMS.Models.MyInfo;
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
            MyInfoInputParams model = new MyInfoInputParams()
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
            };
            model.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
            model.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            model.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetMyInfo), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<MyInfoResults>(data);
            results.leaveResults.leavesSummary.ForEach(x => x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.LeaveSummaryID.ToString()));
            return View(results);
        }

        [HttpPost]
        public IActionResult Index(MyInfoResults model)
        {
            model.leaveResults.leaveSummaryModel.NoOfDays = (model.leaveResults.leaveSummaryModel.EndDate - model.leaveResults.leaveSummaryModel.StartDate).Days + 1;
            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays / 2;
            }

            if (ModelState.IsValid && model.leaveResults.leaveSummaryModel.NoOfDays > 0)
            {
                model.leaveResults.leaveSummaryModel.LeaveStatusID = (int)LeaveStatus.PendingApproval;

                model.leaveResults.leaveSummaryModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
                model.leaveResults.leaveSummaryModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
                model.leaveResults.leaveSummaryModel.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
                var data = _businessLayer.SendPostAPIRequest(model.leaveResults.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(data);
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Leave applied successfully.";
                return RedirectToActionPermanent(
                  Constants.Index,
                   WebControllarsConstants.MyInfo,
                 new { id = _businessLayer.EncodeStringBase64(result.PKNo.ToString()) });
            }
            return View(model);
        }
    }
}
