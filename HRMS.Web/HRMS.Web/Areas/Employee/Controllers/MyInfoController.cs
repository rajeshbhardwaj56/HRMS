using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
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
        [AllowAnonymous]
        public JsonResult GetLeaveForApprovals(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            MyInfoInputParams employee = new MyInfoInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);
            results.leavesSummary.ForEach(x => x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.EmployeeID.ToString()));
            return Json(new { data = results.leavesSummary });
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult ApproveRejectLeave(long leaveSummaryID, bool isApproved, string ApproveRejectComment)
        {
            MyInfoInputParams employee = new MyInfoInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);
            var rowData = results.leavesSummary.Where(x => x.LeaveSummaryID == leaveSummaryID).FirstOrDefault();
            if (rowData != null)
            {
                rowData.ApproveRejectComment = ApproveRejectComment;
                if (isApproved)
                {
                    rowData.LeaveStatusID = (int)LeaveStatus.Approved;
                }
                else
                {
                    rowData.LeaveStatusID = (int)LeaveStatus.NotApproved;
                }
            }
            var LeaveResultsdata = _businessLayer.SendPostAPIRequest(rowData, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var resultsLeaveResultsdata = JsonConvert.DeserializeObject<Result>(LeaveResultsdata);
            if (resultsLeaveResultsdata != null && resultsLeaveResultsdata.PKNo > 0)
            {
                sendEmailProperties sendEmailProperties = new sendEmailProperties();
                sendEmailProperties.emailSubject = "Leave Approve Nofification";
                sendEmailProperties.emailBody = ("Hi " + rowData.EmployeeFirstName + ", <br/><br/> Your " + rowData.LeaveTypeName + " leave ( from " + rowData.StartDate.ToString("MMMM dd, yyyy") + " to " + rowData.EndDate.ToString("MMMM dd, yyyy") + ") have been approved." + "<br/><br/>");
                sendEmailProperties.EmailToList.Add(rowData.OfficialEmailID);
                sendEmailProperties.EmailCCList.Add(rowData.ManagerOfficialEmailID);
                emailSendResponse response = EmailSender.SendEmail(sendEmailProperties);
                //if (response.responseCode == "200")
                //{
                //    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                //    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email have been sent, Please reset password for Login.";
                //}
                //else
                //{
                //    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                //    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email sending failed, Please try again later.";
                //}
            }
            return Json(new { data = resultsLeaveResultsdata });
        }


        [HttpPost]
        public IActionResult Index(MyInfoResults model)
        {
            var startDate = model.leaveResults.leaveSummaryModel.StartDate;
            var endDate = model.leaveResults.leaveSummaryModel.EndDate;

            //validation for fromdate should not greater than todate
            if (startDate > endDate)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "End date must be greater than start date.";

                //return RedirectToActionPermanent(
                //   Constants.Index,
                //    WebControllarsConstants.MyInfo);
                return View(model);
            }

            //validation for not include Weekend Days
            int totalDays = (endDate - startDate).Days + 1;
            int weekendDays = 0;
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekendDays++;
                }
            }
            int workingDays = totalDays - weekendDays;
            if (workingDays <= 0)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Please include weekdays also to apply leave.";
                return RedirectToActionPermanent(
               Constants.Index,
                WebControllarsConstants.MyInfo);
            }

            model.leaveResults.leaveSummaryModel.NoOfDays = totalDays;
            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                model.leaveResults.leaveSummaryModel.StartDate = model.leaveResults.leaveSummaryModel.EndDate = model.leaveResults.leaveSummaryModel.HalfDayDate;
                model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays / 2;
            }

            if (model.leaveResults.leaveSummaryModel.NoOfDays > 0)
            {
                model.leaveResults.leaveSummaryModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
                model.leaveResults.leaveSummaryModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
                model.leaveResults.leaveSummaryModel.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));

                // Get employee details
                var employeeDetails = GetEmployeeDetails(model.leaveResults.leaveSummaryModel.CompanyID, model.leaveResults.leaveSummaryModel.EmployeeID);
                model.leaveResults.leaveSummaryModel.LeavePolicyID = employeeDetails.LeavePolicyID ?? 0;
                model.leaveResults.leaveSummaryModel.LeaveStatusID = (int)LeaveStatus.PendingApproval;

                // Get leave policy data
                var leavePolicyModel = GetLeavePolicyData(model.leaveResults.leaveSummaryModel.CompanyID, model.leaveResults.leaveSummaryModel.LeavePolicyID);

                //Get leave summary data
                var leaveSummaryData = GetLeaveSummaryData(model.leaveResults.leaveSummaryModel.EmployeeID, model.leaveResults.leaveSummaryModel.UserID, model.leaveResults.leaveSummaryModel.CompanyID);
                var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;

                //get current year date
                var currentYearDate = GetAprilFirstDate();

                //validation for applicable after working days
                var totalJoiningDays = (DateTime.Today - employeeDetails.InsertedDate).TotalDays;
                if (leavePolicyModel.ApplicableAfterWorkingDays > totalJoiningDays)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You can't apply leave(s) before " + leavePolicyModel.ApplicableAfterWorkingDays + " days of joining";
                    return View();
                }

                //validation for maximum consecutive leave
                if (model.leaveResults.leaveSummaryModel.NoOfDays > leavePolicyModel.MaximumConsecutiveLeavesAllowed)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You can't apply leave(s) more than " + leavePolicyModel.MaximumConsecutiveLeavesAllowed + " consecutive leaves";
                    return View();
                }

                //validation for maximum medical leave allowed
                var medicalLeavesCount = leaveSummaryDataResult?.Where(x => x.LeaveStatusID == (int)LeaveStatus.Approved && x.StartDate > currentYearDate && x.LeaveTypeID == (int)LeaveType.Medical)?.Sum(x => x.NoOfDays);
                if (leavePolicyModel.MaximumMedicalLeaveAllocationAllowed <= medicalLeavesCount)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have taken all medical leaves for this year";
                    return View();
                }

                //validation for maximum leave allocation allowed
                var totalLeavesInAYear = leaveSummaryDataResult?.Where(x => x.LeaveStatusID == (int)LeaveStatus.Approved && x.StartDate > currentYearDate)?.Sum(x => x.NoOfDays);
                if (leavePolicyModel.MaximumLeaveAllocationAllowed <= totalLeavesInAYear)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have taken all leaves for this year";
                    return View();
                }

                //// Validate for already taken leaves
                //var isAlreadyTakenLeave = ValidateAlreadyTakenLeaves(model, leaveSummaryData);
                //if (isAlreadyTakenLeave)
                //{
                //    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                //    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have already applied leave for the same Date";
                //    return View();
                //}

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
        private EmployeeModel GetEmployeeDetails(long companyId, long employeeId)
        {
            var employeeDetailsJson = _businessLayer.SendPostAPIRequest(new EmployeeInputParams { CompanyID = companyId, EmployeeID = employeeId }, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var employeeDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(employeeDetailsJson).employeeModel;
            return employeeDetails;
        }
        private LeavePolicyModel GetLeavePolicyData(long companyId, long leavePolicyId)
        {
            var leavePolicyModel = new LeavePolicyModel { CompanyID = companyId, LeavePolicyID = leavePolicyId };
            var leavePolicyDataJson = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var leavePolicyModelResult = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(leavePolicyDataJson).leavePolicyModel;
            return leavePolicyModelResult;
        }
        private string GetLeaveSummaryData(long employeeID, long userID, long companyID)
        {
            MyInfoInputParams myInfoInputParams = new MyInfoInputParams
            {
                EmployeeID = employeeID,
                UserID = userID,
                CompanyID = companyID
            };

            var leaveSummaryData = _businessLayer.SendPostAPIRequest(myInfoInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetlLeavesSummary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            return leaveSummaryData;
        }
        public DateTime GetAprilFirstDate()
        {
            DateTime now = DateTime.Now;
            int year = now.Month < 4 ? now.Year - 1 : now.Year;
            return new DateTime(year, 4, 1);
        }
        private bool ValidateAlreadyTakenLeaves(MyInfoResults model, string leaveSummaryData)
        {
            var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
            var isAlreadyTakenLeave = false;

            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                isAlreadyTakenLeave = leaveSummaryDataResult?.Any(x => x.StartDate.Date == model.leaveResults.leaveSummaryModel.HalfDayDate.Date) ?? false;
            }
            else
            {
                // Additional validation logic for full-day leaves can be added here.
            }

            return isAlreadyTakenLeave;
        }

    }
}
