using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
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
            //model.leaveResults.leaveSummaryModel.NoOfDays = (model.leaveResults.leaveSummaryModel.EndDate - model.leaveResults.leaveSummaryModel.StartDate).Days + 1;
            //if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            //{
            //    model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays / 2;
            //}

            //if (ModelState.IsValid && model.leaveResults.leaveSummaryModel.NoOfDays > 0)
            //{
            //    model.leaveResults.leaveSummaryModel.LeaveStatusID = (int)LeaveStatus.PendingApproval;

            //    model.leaveResults.leaveSummaryModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
            //    model.leaveResults.leaveSummaryModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            //    model.leaveResults.leaveSummaryModel.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
            //    var data = _businessLayer.SendPostAPIRequest(model.leaveResults.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

            //    var result = JsonConvert.DeserializeObject<Result>(data);
            //    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
            //    TempData[HRMS.Models.Common.Constants.toastMessage] = "Leave applied successfully.";
            //    return RedirectToActionPermanent(
            //      Constants.Index,
            //       WebControllarsConstants.MyInfo,
            //     new { id = _businessLayer.EncodeStringBase64(result.PKNo.ToString()) });
            //}
            return View(model);
        }

        public IActionResult GetHolidayList()
        {
            try
            {
                var holidayInputParams = new HolidayInputParams();
                holidayInputParams.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
                var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidays);
                var bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
                var data = _businessLayer.SendPostAPIRequest(holidayInputParams, apiUrl, bearerToken, true).Result.ToString();
                var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);

                if (results != null && results.Holiday.Count > 0)
                {
                    var currentYear = DateTime.Now.Year;
                    var holidaysInCurrentYear = results.Holiday
                        .Where(h => h.FromDate.Year == currentYear && h.ToDate.Year == currentYear)
                        .ToList();

                    return PartialView("_HolidayListPartial", holidaysInCurrentYear);
                }

                return PartialView("_HolidayListPartial", new List<HolidayModel>());

            }
            catch (Exception ex)
            {
                return PartialView("_HolidayListPartial", new List<HolidayModel>());
            }
        }


        [HttpPost]
        public ActionResult SubmitLeaveForm(LeaveResults model)
        {

            model.leaveSummaryModel.NoOfDays = (model.leaveSummaryModel.EndDate - model.leaveSummaryModel.StartDate).Days + 1;
            if ((int)LeaveDay.HalfDay == model.leaveSummaryModel.LeaveDurationTypeID)
            {
                model.leaveSummaryModel.NoOfDays /= 2;
            }
            if (ModelState.IsValid && model.leaveSummaryModel.NoOfDays > 0)
            {
                MyInfoInputParams myInfoInputParams = new MyInfoInputParams();

                model.leaveSummaryModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
                model.leaveSummaryModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
                model.leaveSummaryModel.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
                myInfoInputParams.EmployeeID = model.leaveSummaryModel.EmployeeID;
                myInfoInputParams.UserID = model.leaveSummaryModel.UserID;
                myInfoInputParams.CompanyID = model.leaveSummaryModel.CompanyID;

                model.leaveSummaryModel.LeaveStatusID = (int)CheckLeaveStatus(myInfoInputParams, model);

                var data = _businessLayer.SendPostAPIRequest(model.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);

                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Leave applied successfully.";

                return Json(new { success = true, redirectUrl = Url.Action("Index", "MyInfo", new { id = _businessLayer.EncodeStringBase64(result.PKNo.ToString()) }) });
            }

            return PartialView("_LeaveFormPartial", model);
        }

        private LeaveStatus CheckLeaveStatus(MyInfoInputParams myInfoInputParams, LeaveResults leaveResults)
        {
            var leavePolicyInputParams = new LeavePolicyInputParans
            {
                CompanyID = myInfoInputParams.CompanyID,
                LeavePolicyID = leaveResults.leaveSummaryModel.LeavePolicyID
            };
            var employeeLeavesSummaryResponse = _businessLayer.SendPostAPIRequest(
                myInfoInputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetlAllLeavesSummary),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var employeeLeavesSummary = JsonConvert.DeserializeObject<LeaveSummaryResults>(employeeLeavesSummaryResponse);
            var leavePolicyResponse = _businessLayer.SendPostAPIRequest(
                leavePolicyInputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var leavePolicyDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(leavePolicyResponse);
            var leavePolicy = leavePolicyDetails.leavePolicyModel;
            int requestedLeaveDays = (int)leaveResults.leaveSummaryModel.NoOfDays;
            DateTime leaveStartDate = leaveResults.leaveSummaryModel.StartDate;
            DateTime leaveEndDate = leaveResults.leaveSummaryModel.EndDate;
            DateTime periodStart = new DateTime(DateTime.Now.Year - 1, 4, 1);
            DateTime periodEnd = new DateTime(DateTime.Now.Year, 3, 30);
            var leavesWithinPeriod = employeeLeavesSummary.leavesSummary
                .Where(x => x.StartDate >= periodStart && x.EndDate <= periodEnd && x.LeaveStatusID == (int)LeaveStatus.Approved)
                .ToList();

            decimal totalApprovedLeaveDays = leavesWithinPeriod.Sum(leave => leave.NoOfDays);

            if (leavePolicy != null)
            {
                if (requestedLeaveDays + totalApprovedLeaveDays > leavePolicy.MaximumLeaveAllocationAllowed)
                {
                    return LeaveStatus.NotApproved;
                }

                if (requestedLeaveDays > leavePolicy.MaximumConsecutiveLeavesAllowed)
                {
                    return LeaveStatus.NotApproved;
                }
            }

            return LeaveStatus.PendingApproval;
        }

    }
}
