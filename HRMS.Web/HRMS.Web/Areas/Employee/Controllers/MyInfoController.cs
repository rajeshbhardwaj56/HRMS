using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Employee;
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
            ////validation for holidays
            //var holidayInputParams = new HolidayInputParams();
            //holidayInputParams.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
            //var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidays);
            //var bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
            //var data = _businessLayer.SendPostAPIRequest(holidayInputParams, apiUrl, bearerToken, true).Result.ToString();
            //var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);

            //if (results != null && results.Holiday.Count > 0)
            //{
            //    var currentYear = DateTime.Now.Year;
            //    var holidaysInCurrentYear = results.Holiday
            //        .Where(h => h.FromDate.Year == currentYear && h.ToDate.Year == currentYear)
            //        .ToList();
            //    foreach (var item in holidaysInCurrentYear)
            //    {
            //        if ((int)LeaveDay.HalfDay == model.leaveSummaryModel.LeaveDurationTypeID)
            //        {

            //        }
            //        else
            //        {

            //        }
            //    }

            //}


            


            model.leaveSummaryModel.NoOfDays = (model.leaveSummaryModel.EndDate - model.leaveSummaryModel.StartDate).Days + 1;
            if ((int)LeaveDay.HalfDay == model.leaveSummaryModel.LeaveDurationTypeID)
            {
                model.leaveSummaryModel.NoOfDays /= 2;
                model.leaveSummaryModel.EndDate = model.leaveSummaryModel.StartDate = model.leaveSummaryModel.HalfDayDate;
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

                var employeeDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(_businessLayer.SendPostAPIRequest(new EmployeeInputParams { CompanyID = model.leaveSummaryModel.CompanyID, EmployeeID = model.leaveSummaryModel.EmployeeID }, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString()).employeeModel;
                model.leaveSummaryModel.LeavePolicyID = employeeDetails.LeavePolicyID ?? 0;
                bool isJoiningdayMoreThan90 = (DateTime.Today - employeeDetails.InsertedDate).TotalDays > 90;
                model.leaveSummaryModel.LeaveStatusID = (int)CheckLeaveStatus(myInfoInputParams, model, isJoiningdayMoreThan90);

                //get data
                var leaveSummaryData = _businessLayer.SendPostAPIRequest(myInfoInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetlLeavesSummary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;

                //validation for Already taken leaves
                var isAlreadyTakenLeave = false;
                if ((int)LeaveDay.HalfDay == model.leaveSummaryModel.LeaveDurationTypeID)
                {
                    isAlreadyTakenLeave = leaveSummaryDataResult?.Any(x => x.StartDate.Date == model.leaveSummaryModel.HalfDayDate.Date) ?? false;
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have already applied leave for same Date";
                    return View();
                }
                else
                {
                    //List<DateTime> dates = GetDatesBetween(model.leaveSummaryModel.StartDate, model.leaveSummaryModel.EndDate);
                    //// Find common dates
                    //var commonDates = dates.Intersect(leaveSummaryDataResult.Select(x => x.StartDate.Date)).ToList();

                    //// Count the common dates
                    //int totalNumberOfCommonDates = commonDates.Count; 
                }

                //validation for Maximum Consecutive Leave
                LeavePolicyModel leavePolicyModel = new LeavePolicyModel();
                leavePolicyModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                leavePolicyModel.LeavePolicyID = Convert.ToInt64(model.leaveSummaryModel.LeavePolicyID);
                var leavePolicydata = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                leavePolicyModel = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(leavePolicydata).leavePolicyModel;
                if (model.leaveSummaryModel.NoOfDays > leavePolicyModel.MaximumConsecutiveLeavesAllowed)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You can't apply leave(s) more than " + leavePolicyModel.MaximumConsecutiveLeavesAllowed + " consecutive leaves";
                    return View();
                }

                //validation for Applicable after working days
                var totalJoiningDays = (DateTime.Today - employeeDetails.InsertedDate).TotalDays;
                if (leavePolicyModel.ApplicableAfterWorkingDays > totalJoiningDays)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You can't apply leave(s) before " + leavePolicyModel.ApplicableAfterWorkingDays + " days of joining";
                    return View();
                }

                //validation for Maximum Leave Allocation Allowed
                var currentYearDate = GetAprilFirstDate();
                var totalLeavesInAYear = leaveSummaryDataResult?.Where(x => x.LeaveStatusID == 1 && x.StartDate > currentYearDate)?.Sum(x => x.NoOfDays);
                if (leavePolicyModel.MaximumLeaveAllocationAllowed <= totalLeavesInAYear)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have taken all leaves for this year";
                    return View();
                }

                //validation for Maximum Casual Leave Allowed
                var casualLeavesCount = leaveSummaryDataResult?.Where(x => x.LeaveStatusID == 1 && x.StartDate > currentYearDate && x.LeaveTypeID == 1)?.Sum(x => x.NoOfDays);
                if (leavePolicyModel.MaximumCasualLeaveAllocationAllowed <= casualLeavesCount)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have taken all casual leaves for this year";
                    return View();
                }

                //validation for Maximum Medical Leave Allowed
                var medicalLeavesCount = leaveSummaryDataResult?.Where(x => x.LeaveStatusID == 1 && x.StartDate > currentYearDate && x.LeaveTypeID == 2)?.Sum(x => x.NoOfDays);
                if (leavePolicyModel.MaximumMedicalLeaveAllocationAllowed <= medicalLeavesCount)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have taken all medical leaves for this year";
                    return View();
                }

                //validation for Maximum Medical Leave Allowed
                var annualLeavesCount = leaveSummaryDataResult?.Where(x => x.LeaveStatusID == 1 && x.StartDate > currentYearDate && x.LeaveTypeID == 3)?.Sum(x => x.NoOfDays);
                if (leavePolicyModel.MaximumAnnualLeaveAllocationAllowed <= annualLeavesCount)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have taken all annual leaves for this year";
                    return View();
                }





                var data = _businessLayer.SendPostAPIRequest(model.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);

                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Leave applied successfully.";

                return Json(new { success = true, redirectUrl = Url.Action("Index", "MyInfo", new { id = _businessLayer.EncodeStringBase64(result.PKNo.ToString()) }) });
            }

            return PartialView("_LeaveFormPartial", model);
        }

        private LeaveStatus CheckLeaveStatus(MyInfoInputParams myInfoInputParams, LeaveResults leaveResults, bool isJoiningdayMoreThan90)
        {
            var leaveStatus = LeaveStatus.PendingApproval;

            var leavePolicy = GetLeavePolicy(leaveResults.leaveSummaryModel.LeavePolicyID ?? 0);
            if (leavePolicy == null)
                return leaveStatus;

            var employeeLeavesSummary = GetEmployeeLeavesSummary(myInfoInputParams);
            if (employeeLeavesSummary == null || employeeLeavesSummary.leavesSummary == null)
                return leaveStatus;

            var requestedLeaveDays = (int)leaveResults.leaveSummaryModel.NoOfDays;
            var totalApprovedLeaveDays = GetTotalApprovedLeaveDays(employeeLeavesSummary.leavesSummary, leaveResults.leaveSummaryModel.LeavePolicyID ?? 0);


            var leaveType = GetLeaveType(leaveResults.leaveSummaryModel.LeaveTypeName);
            if (leaveType != null)
            {
                if (isJoiningdayMoreThan90 && requestedLeaveDays <= leavePolicy.MaximumConsecutiveLeavesAllowed &&
    (requestedLeaveDays + totalApprovedLeaveDays > leavePolicy.MaximumLeaveAllocationAllowed &&
    (leavePolicy.IsCarryForward || leavePolicy.IsAllowOverAllocation || leavePolicy.IsAllowNegativeBalance)))
                {
                    leaveStatus = LeaveStatus.Approved;
                }
                else
                {
                    leaveStatus = LeaveStatus.NotApproved;
                }
            }

            if (leavePolicy.IsOptionalLeave || leavePolicy.IsPartiallyPaidLeave)
            {
                leaveStatus = LeaveStatus.PendingApproval;
            }


            return leaveStatus;
        }

        private LeavePolicyModel GetLeavePolicy(long leavePolicyID)
        {
            var leavePolicyInputParams = new LeavePolicyInputParans { LeavePolicyID = leavePolicyID };

            var leavePolicyResponse = _businessLayer.SendPostAPIRequest(
                leavePolicyInputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var leavePolicyDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(leavePolicyResponse);

            return leavePolicyDetails?.leavePolicyModel;
        }

        private LeaveSummaryResults GetEmployeeLeavesSummary(MyInfoInputParams myInfoInputParams)
        {
            var employeeLeavesSummaryResponse = _businessLayer.SendPostAPIRequest(
                myInfoInputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetlAllLeavesSummary),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            return JsonConvert.DeserializeObject<LeaveSummaryResults>(employeeLeavesSummaryResponse);
        }

        private decimal GetTotalApprovedLeaveDays(List<LeaveSummaryModel> leavesSummary, long leavePolicyID)
        {
            var periodStart = new DateTime(DateTime.Now.Year - 1, 4, 1);
            var periodEnd = new DateTime(DateTime.Now.Year, 3, 30);

            return leavesSummary?
                .Where(x => x.StartDate >= periodStart && x.EndDate <= periodEnd && x.LeaveStatusID == (int)LeaveStatus.Approved && x.LeavePolicyID == leavePolicyID)
                .Sum(leave => (decimal?)leave.NoOfDays) ?? 0;
        }

        private LeaveTypeName? GetLeaveType(string leaveTypeString)
        {
            if (Enum.TryParse<LeaveTypeName>(leaveTypeString, out var leaveType))
                return leaveType;
            return null;
        }

        public DateTime GetAprilFirstDate()
        {
            DateTime now = DateTime.Now;
            int year = now.Month < 4 ? now.Year - 1 : now.Year;
            return new DateTime(year, 4, 1);
        }
        public static List<DateTime> GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dates = new List<DateTime>();
            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                dates.Add(date);
            }
            return dates;
        }

    }
}
