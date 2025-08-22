using System;
using System.ServiceModel.Channels;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using HRMS.Models;
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.FormPermission;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
using HRMS.Models.MyInfo;
using HRMS.Models.WhatsHappeningModel;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.Employee.Controllers
{
    [Authorize]
    [Area(Constants.ManageEmployee)]
    //  [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Employee + "," + RoleConstants.Manager + "," + RoleConstants.SuperAdmin))]
    public class MyInfoController : Controller
    {
        IHttpContextAccessor _context;
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private readonly IS3Service _s3Service;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;
        public MyInfoController(ICheckUserFormPermission CheckUserFormPermission, IConfiguration configuration, IBusinessLayer businessLayer, IHttpContextAccessor context, IS3Service s3Service)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
            _context = context;
            _s3Service = s3Service;
            _CheckUserFormPermission = CheckUserFormPermission;
        }

        [HttpGet]
        public IActionResult Index(string id)
        {
           
            var model = new MyInfoInputParams
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
                EmployeeID = GetSessionLong(Constants.EmployeeID),
                GenderId = GetSessionInt(Constants.Gender),
                JobLocationTypeID = GetSessionInt(Constants.JobLocationID),
                UserID = GetSessionLong(Constants.UserID),
                CompanyID = GetSessionLong(Constants.CompanyID)
            };
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(model.EmployeeID, (int)PageName.MyInfo);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Call API
            var jsonData = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetMyInfo),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            var results = JsonConvert.DeserializeObject<MyInfoResults>(jsonData);         
            return View(results);
        }


      
        private long GetSessionLong(string key)
        {
            return long.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }

        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }

        [HttpGet]
        public IActionResult GetEmployeeLeaveDetails(string employeeID,long leaveSummaryID)
        {
            MyInfoInputParams model = new MyInfoInputParams();
            model.EmployeeID = Convert.ToInt64(employeeID);
            model.LeaveSummaryID = Convert.ToInt64(leaveSummaryID);
            model.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            model.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
            var employeeDetails = GetEmployeeDetails(model.CompanyID, model.EmployeeID);
            model.GenderId = Convert.ToInt32(employeeDetails.Gender);
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetMyInfo), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<MyInfoResults>(data);
            if (results?.leaveResults?.leavesSummary != null)
            {
                foreach (var leave in results.leaveResults.leavesSummary)
                {
                    if (!string.IsNullOrEmpty(leave.UploadCertificate))
                    {
                        leave.UploadCertificate = _s3Service.GetFileUrl(leave.UploadCertificate);
                    }
                }
            }
            DateTime today = DateTime.Today;
            DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);
            var leavePolicyModel = GetLeavePolicyData(model.CompanyID, employeeDetails.LeavePolicyID ?? 0);
            ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);
            return Json(results);
        }


        [HttpPost]
        public JsonResult GetLeaveForApprovals(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            MyInfoInputParams employee = new MyInfoInputParams();
            var Approvals = new List<LeaveSummaryModel>();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            employee.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);
            var employeeDetails = GetEmployeeDetails(employee.CompanyID, employee.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(employee.CompanyID, employeeDetails.LeavePolicyID ?? 0);
            if (leavePolicyModel != null)
            {
                Approvals = results.leavesSummary.Where(x => x.LeaveStatusID == (int)LeaveStatus.PendingApproval).ToList();
                ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);
                if (leavePolicyModel.Paternity_medicalDocument == true)
                {
                    ViewBag.Paternity_medicalDocument = true;
                }
            }
            return Json(new { data = Approvals });
        }


        [HttpPost]
        public JsonResult GetLeaveForApproved(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            MyInfoInputParams employee = new MyInfoInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            employee.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);
            var Approvals = results.leavesSummary.Where(x => x.LeaveStatusID == (int)LeaveStatus.Approved).ToList();
            var employeeDetails = GetEmployeeDetails(employee.CompanyID, employee.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(employee.CompanyID, employeeDetails.LeavePolicyID ?? 0);
            ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);
            return Json(new { data = Approvals });
        }


        [HttpPost]
        public JsonResult GetLeaveForUserCancelled(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            MyInfoInputParams employee = new MyInfoInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            employee.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);
            var Approvals = results.leavesSummary.Where(x => x.LeaveStatusID == (int)LeaveStatus.Cancelled).ToList();
            var employeeDetails = GetEmployeeDetails(employee.CompanyID, employee.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(employee.CompanyID, employeeDetails.LeavePolicyID ?? 0);
            if (leavePolicyModel != null)
            {
                ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);
            }
            return Json(new { data = Approvals });
        }

        [HttpPost]
        public JsonResult GetLeaveForReject(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            MyInfoInputParams employee = new MyInfoInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            employee.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);

            var Approvals = results.leavesSummary.Where(x => x.LeaveStatusID == (int)LeaveStatus.NotApproved).ToList();

            var employeeDetails = GetEmployeeDetails(employee.CompanyID, employee.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(employee.CompanyID, employeeDetails.LeavePolicyID ?? 0);

            ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);

            return Json(new { data = Approvals });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ApproveRejectLeave(long leaveSummaryID, bool isApproved, string approveRejectComment, int leaveTypeID)
        {
            var response = new ErrorLeaveResults();

      
            long companyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            long employeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            long userID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
            long RoleID = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));

           
            var inputParams = new MyInfoInputParams
            {
                CompanyID = companyID,
                EmployeeID = employeeID,
                RoleId = RoleID 
            };

            var jsonData = _businessLayer.SendPostAPIRequest(inputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals),
                HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result?.ToString();

            var leaveResults = JsonConvert.DeserializeObject<LeaveResults>(jsonData);
            var leaveRecord = leaveResults?.leavesSummary?.FirstOrDefault(x => x.LeaveSummaryID == leaveSummaryID);

            if (leaveRecord == null)
            {
                response.status = 1;
                response.message = "Leave record not found.";
                return Json(new { data = response });
            }

            // Update leave record with approval/rejection info
            leaveRecord.UserID = userID;
            leaveRecord.ApproveRejectComment = approveRejectComment;
            leaveRecord.LeaveStatusID = isApproved ? (int)LeaveStatus.Approved : (int)LeaveStatus.NotApproved;

            if (leaveTypeID == (int)LeaveType.CompOff)
            {
                leaveRecord.CampOff = (int)CompOff.CompOff;
            }

            if (isApproved && leaveRecord.LeaveTypeID == (int)LeaveType.AnnualLeavel)
            {
                // Get fiscal year start (April 1st)
                DateTime fiscalYearStart = DateTime.Today.Month >= 4
                    ? new DateTime(DateTime.Today.Year, 4, 1)
                    : new DateTime(DateTime.Today.Year - 1, 4, 1);

                // Fetch leave summary for this employee & company
                var leaveSummaryDataJson = GetLeaveSummaryData(leaveRecord.EmployeeID, leaveRecord.UserID, leaveRecord.CompanyID);
                var leaveSummaryData = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryDataJson)?.leavesSummary;

                // Filter approved annual/medical leaves in current fiscal year
                var approvedLeaves = leaveSummaryData?.Where(x =>
                    x.StartDate >= fiscalYearStart &&
                    x.LeaveStatusID == (int)LeaveStatus.Approved &&
                    (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave)).ToList();

                double approvedLeaveTotal = (double?)approvedLeaves?.Sum(x => x.NoOfDays) ?? 0;
                double maxAnnualLeaveLimit = 30;

                // Load leave policy & calculate accrued leaves
                var leavePolicy = GetLeavePolicyData(leaveRecord.CompanyID, leaveRecord.LeavePolicyID);
                double accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(leaveRecord.JoiningDate ?? DateTime.Today, leavePolicy.Annual_MaximumLeaveAllocationAllowed);

                // Calculate maximum available leave left to accrue
                double maxAvailable = maxAnnualLeaveLimit - approvedLeaveTotal;

                // Cap accrued leave so it doesn’t exceed max available
                accruedLeave = Math.Min(accruedLeave, maxAvailable);

                // Add carry forward if policy allows, but ensure capped
                if (leavePolicy.Annual_IsCarryForward)
                {
                    double carryForward = Convert.ToDouble(GetEmployeeDetails(leaveRecord.CompanyID, leaveRecord.EmployeeID)?.CarryForword ?? 0);
                    accruedLeave = Math.Min(accruedLeave + carryForward, maxAvailable);
                }

                double totalLeaveWithCarryForward = approvedLeaveTotal + accruedLeave;

                // Final cap for safety
                totalLeaveWithCarryForward = Math.Min(totalLeaveWithCarryForward, maxAnnualLeaveLimit);

                // Validation: Requested days should not exceed balance
                if ((double)leaveRecord.NoOfDays > totalLeaveWithCarryForward - approvedLeaveTotal)
                {
                    response.status = 1;
                    response.message = $"Leave duration exceeds the privilege leave balance of {totalLeaveWithCarryForward - approvedLeaveTotal} days.";
                    return Json(new { data = response });
                }
            }

            // Send update leave request
            var updateResponseJson = _businessLayer.SendPostAPIRequest(leaveRecord,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave),
                HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result?.ToString();

            var updateResult = JsonConvert.DeserializeObject<Result>(updateResponseJson);
            string message = "";
            if (updateResult != null && updateResult.PKNo > 0)
            {
                string actionText = isApproved ? "approved" : "rejected";

                if (!string.IsNullOrEmpty(leaveRecord.OfficialEmailID) && leaveRecord.OfficialEmailID.Contains("@"))
                {

                    var emailProps = new sendEmailProperties
                    {
                        emailSubject = $"Leave {actionText} Notification",
                        emailBody = $@"
<p>Hi {leaveRecord.EmployeeFirstName},</p>

<p>
    Your <strong>{leaveRecord.LeaveTypeName}</strong>  
    (from <strong>{leaveRecord.StartDate:MMMM dd, yyyy}</strong> to <strong>{leaveRecord.EndDate:MMMM dd, yyyy}</strong>)
    has been <strong>{actionText}</strong>.
</p>

<br/>

   <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>  
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>"
                    };

                    emailProps.EmailToList.Add(leaveRecord.OfficialEmailID);
                    var responses = EmailSender.SendEmail(emailProps);

                    if (responses.responseCode == "200")
                    {
                        message = $"Leave {actionText} successfully and email sent successfully.";
                    }
                }


            }


            return Json(new { data = updateResult, message = message });
        }
        private EmployeeModel GetEmployeeDetails(long companyId, long employeeId)
        {
            var employeeDetailsJson = _businessLayer.SendPostAPIRequest(new EmployeeInputParams { CompanyID = companyId, EmployeeID = employeeId }, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var employeeDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(employeeDetailsJson).employeeModel;
            return employeeDetails;
        }


        private CompOffValidationResult GetValidateCompOffLeave(CampOffEligible model)
        {
            var CompOffDataJson = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetValidateCompOffLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var CompOffData = JsonConvert.DeserializeObject<CompOffValidationResult>(CompOffDataJson);
            return CompOffData;
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
                CompanyID = companyID,
                JobLocationTypeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.JobLocationID))
            };

            var leaveSummaryData = _businessLayer.SendPostAPIRequest(myInfoInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetlLeavesSummary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            return leaveSummaryData;
        }
        private string GetHolidayData(long companyID)
        {
            HolidayInputParams myInfoInputParams = new HolidayInputParams
            {
                CompanyID = companyID,
                EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID))
            };

            var holidayData = _businessLayer.SendPostAPIRequest(myInfoInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidayList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            return holidayData;
        }

        private string GetAgentHolidayData(long companyID,long employeeID)
        {
            HolidayInputParams myInfoInputParams = new HolidayInputParams
            {
                CompanyID = companyID,
                EmployeeID = employeeID
            };

            var holidayData = _businessLayer.SendPostAPIRequest(myInfoInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidayList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            return holidayData;
        }
        public DateTime GetAprilFirstDate()
        {
            DateTime now = DateTime.Now;
            int year = now.Month < 4 ? now.Year - 1 : now.Year;
            return new DateTime(year, 4, 1);
        }
        private bool ValidateAlreadyTakenLeaves(MyInfoResults model, string leaveSummaryData, long employeeId)
        {
            var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
            var isAlreadyTakenLeave = false;
            var id = model.leaveResults.leaveSummaryModel.LeaveSummaryID;
            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                isAlreadyTakenLeave = leaveSummaryDataResult?.Any(x => x.StartDate.Date == model.leaveResults.leaveSummaryModel.StartDate.Date && x.EmployeeID == employeeId && x.LeaveSummaryID != id) ?? false;
            }
            else
            {
                // Additional validation logic for full-day leaves can be added here.
                // Check if full-day leave has already been taken
                isAlreadyTakenLeave = leaveSummaryDataResult?.Any(x =>
                    x.StartDate.Date <= model.leaveResults.leaveSummaryModel.EndDate.Date && // Existing leave starts before or on the new leave's end date
                    x.EndDate.Date >= model.leaveResults.leaveSummaryModel.StartDate.Date && x.EmployeeID == employeeId
                    && x.LeaveSummaryID != id// Existing leave ends after or on the new leave's start date
                ) ?? false;

            }

            return isAlreadyTakenLeave;
        }



        private double CalculateAccruedLeaveForCurrentFiscalYear(DateTime joinDate, int Annual_MaximumLeaveAllocationAllowed)
        {
            DateTime today = DateTime.Today;

            // Fiscal year starts from March 21st of current or previous year
            //DateTime fiscalYearStart = new DateTime(today.Month > 3 || (today.Month == 3 && today.Day >= 21)
            //                                        ? today.Year : today.Year - 1, 3, 21);
            //DateTime fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-1); // Ends on March 20th next year


            DateTime fiscalYearStart;
            DateTime fiscalYearEnd;

            if (today.Year == 2025)
            {
                // ✅ Special case: 2025 fiscal year is from 21 May 2025 to 20 March 2026
                fiscalYearStart = new DateTime(2025, 5, 21);
                fiscalYearEnd = new DateTime(2026, 3, 20);
            }
            else
            {
                // ✅ Default logic: Fiscal year from 21 March current/previous year to 20 March next year
                fiscalYearStart = new DateTime(today.Month > 3 || (today.Month == 3 && today.Day >= 21)
                                               ? today.Year : today.Year - 1, 3, 21);
                fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-1); // Ends on 20 March next year
            }


            double annualLeaveEntitlement = Annual_MaximumLeaveAllocationAllowed;
            double monthlyAccrual = annualLeaveEntitlement / 12;
            double totalAccruedLeave = 0;

            // If join date is before fiscal year, adjust to fiscal start
            if (joinDate < fiscalYearStart)
                joinDate = fiscalYearStart;

            // Start from the accrual period containing the join date
            DateTime accrualPeriodStart = GetAccrualPeriodStart(joinDate);
            DateTime accrualPeriodEnd = accrualPeriodStart.AddMonths(1).AddDays(-1); // 20th of next month

            while (accrualPeriodStart <= today && accrualPeriodStart <= fiscalYearEnd)
            {
                // Adjust for join date or current date
                DateTime effectiveStart = joinDate > accrualPeriodStart ? joinDate : accrualPeriodStart;
                DateTime effectiveEnd = accrualPeriodEnd < today ? accrualPeriodEnd : today;

                int daysWorked = (effectiveEnd - effectiveStart).Days + 1;

                if (daysWorked > Convert.ToInt32(_configuration["DaysWorkedInMonth:DaysWorkedInMonth"]))
                {
                    totalAccruedLeave += monthlyAccrual;
                }

                // Move to next accrual period
                accrualPeriodStart = accrualPeriodStart.AddMonths(1);
                accrualPeriodEnd = accrualPeriodStart.AddMonths(1).AddDays(-1);
            }

            return totalAccruedLeave;
        }
       
        private DateTime GetAccrualPeriodStart(DateTime date)
        {
            if (date.Day >= 21)
                return new DateTime(date.Year, date.Month, 21);
            else
            {
                DateTime prevMonth = date.AddMonths(-1);
                return new DateTime(prevMonth.Year, prevMonth.Month, 21);
            }
        }


        [HttpPost]
        public IActionResult DeleteLeavesSummary(string id)
        {
            MyInfoInputParams model = new MyInfoInputParams()
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteLeavesSummary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(
                      Constants.ApplyLeave,
                       WebControllarsConstants.MyInfo);
        }
        [HttpPost]
        public IActionResult UpdateLeaveStatus(string id)
        {
            UpdateLeaveStatus model = new UpdateLeaveStatus()
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
                EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID)),
                NewLeaveStatusID = (int)LeaveStatus.Cancelled,
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.UpdateLeaveStatus), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                var results = JsonConvert.DeserializeObject<UpdateLeaveStatus>(data);

                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = results.Message;
            }
            return RedirectToActionPermanent(
                      Constants.ApplyLeave,
                       WebControllarsConstants.MyInfo);
        }
        [HttpPost]
        public IActionResult UpdateAgentLeaveStatus(string id,string empId)
        {
            UpdateLeaveStatus model = new UpdateLeaveStatus()
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
                EmployeeID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(empId)),
                NewLeaveStatusID = (int)LeaveStatus.Cancelled,
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.UpdateLeaveStatus), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                var results = JsonConvert.DeserializeObject<UpdateLeaveStatus>(data);

                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = results.Message;
            }
            return RedirectToActionPermanent(
                      Constants.ApplyAgentLeave,
                       WebControllarsConstants.MyInfo);
        }
        [HttpGet]
        public IActionResult PolicyDetails()
        {
            LeavePolicyDetailsModel leavePolicyModel = new LeavePolicyDetailsModel();
            leavePolicyModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));


            var data = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicyDetailsByCompanyId), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            leavePolicyModel = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).LeavePolicyDetailsModel;

            return View(leavePolicyModel);
        }


        public IActionResult GetTeamEmployeeList()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            var RoleId = GetSessionInt(Constants.RoleID);
            var EmployeeID = GetSessionInt(Constants.EmployeeID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.MyTeam);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View(results);
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetTeamEmployeeList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, string subDeptFilter,
    string empTypeFilter,
    string locationFilter,string managerFilter)
        {
            try
            {
                EmployeeDashboardResponse employeeDetails = new EmployeeDashboardResponse();
                EmployeeInputParams model = new EmployeeInputParams();
                model.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                model.RoleID = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
                model.SubDepartmentID = string.IsNullOrEmpty(subDeptFilter) ? 0 : Convert.ToInt64(subDeptFilter);
                model.EmployeeTypeID = string.IsNullOrEmpty(empTypeFilter) ? 0 : Convert.ToInt64(empTypeFilter);
                model.LocationID = string.IsNullOrEmpty(locationFilter) ? 0 : Convert.ToInt64(locationFilter);
                model.ManagerID = string.IsNullOrEmpty(managerFilter) ? 0 : Convert.ToInt64(managerFilter);
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmployeeListByManagerID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                employeeDetails = JsonConvert.DeserializeObject<EmployeeDashboardResponse>(data);


                if (employeeDetails == null || employeeDetails.Employees == null || employeeDetails.Employees.Count == 0)
                {
                    return Json(new { data = new List<object>(), message = "No employees found" });
                }
                else
                {
                    employeeDetails.Employees.ForEach(x =>
                    {
                        x.EmployeePhoto = string.IsNullOrEmpty(x.EmployeePhoto)
                            ? "/assets/img/No_image.png"   
                            : _s3Service.GetFileUrl(x.EmployeePhoto);
                    });
                }
                return Json(new { data = employeeDetails.Employees,
                    subDepartments = employeeDetails.SubDepartmentList,
                    locations = employeeDetails.LocationList,
                    employeeTypes = employeeDetails.EmploymentTypesList,
                    managers = employeeDetails.ManagerList
                });

            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred", details = ex.Message });
            }
        }




        [HttpGet]
        public IActionResult Support()
        {
            var RoleId = GetSessionInt(Constants.RoleID);
            var EmployeeID = GetSessionInt(Constants.EmployeeID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.Support);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
        }


        [HttpGet]
        public IActionResult ViewPolicyDocument(string id)
        {
            
            if (string.IsNullOrEmpty(id))
                return NotFound();
            var stream = _s3Service.GetFileStream(id); 
            if (stream == null)
                return NotFound();
            var extension = Path.GetExtension(id).ToLower();
            var mimeType = "application/octet-stream";

            switch (extension)
            {
                case ".pdf":
                    mimeType = "application/pdf";
                    break;
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".webp":
                    mimeType = "image/webp";
                    break;
            }

            return File(stream, mimeType);
        }


        [HttpGet]
        public IActionResult PolicyCategoryDetails()
        {
            PolicyCategoryInputParams model = new PolicyCategoryInputParams();
            model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            model.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.PolicyCategoryDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var detailsList = JsonConvert.DeserializeObject<List<LeavePolicyDetailsModel>>(data);
            if (detailsList != null)
            {
                foreach (var item in detailsList)
                {
                    if (!string.IsNullOrEmpty(item.PolicyDocument))
                    {
                        item.PolicyDocument = Url.Action("ViewPolicyDocument", "MyInfo", new { id = item.PolicyDocument });

                    }
                }
            }
            return View(detailsList);
        }

        [HttpPost]
        public JsonResult AcknowledgePolicy(long policyId)
        {

            var employeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            AcknowledgePolicyModel model = new AcknowledgePolicyModel { Id = policyId, EmployeeId = employeeID };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddAcknowledgePolicy), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var result = JsonConvert.DeserializeObject<Result>(data);
            if (result.ErrorCode == "ALREADY_ACK")
            {
                return Json(new { success = false, alreadyAcknowledged = true, message = result.Message });
            }
            else if (!string.IsNullOrEmpty(result.ErrorCode))
            {
                return Json(new { success = false, message = result.Message });
            }
            else
            {
                return Json(new { success = true, message = result.Message, pkNo = result.PKNo });
            }
        }

        [HttpGet]
        public IActionResult TeamAttendenceList()
        {
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);       
            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.TeamAttendenceList);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            var firstName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
            var middleName = Convert.ToString(HttpContext.Session.GetString(Constants.MiddleName)); // Assuming this exists
            var lastName = Convert.ToString(HttpContext.Session.GetString(Constants.Surname)); // Assuming this exists
            ViewBag.EmployeeName = $"{firstName} {middleName} {lastName}".Trim();
            return View();
        }
        [HttpGet]
        public IActionResult TeamAttendenceCalendarList(int year, int month, int Page, int PageSize, string SearchTerm, int jobLocationId)
        {
            var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var RoleID = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));

            AttendanceInputParams models = new AttendanceInputParams
            {
                Year = year,
                Month = month,
                UserId = employeeId,
                RoleId = RoleID,
                PageSize = PageSize,
                Page = Page,
                SearchTerm = SearchTerm,
                JobLocationID = jobLocationId
            };
            AttendanceWithHolidaysVM model = new AttendanceWithHolidaysVM();

            var data = _businessLayer.SendPostAPIRequest(models, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            model = JsonConvert.DeserializeObject<AttendanceWithHolidaysVM>(data);
            model.Attendances.ForEach(x =>
            {
                x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.EmployeeId.ToString());
                x.EmployeeNumberWithoutAbbr = _businessLayer.EncodeStringBase64(x.EmployeeNumberWithoutAbbr.ToString());
            });
            var CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            Joblcoations modeldata = new Joblcoations();
            modeldata.CompanyId = CompanyID;
            var objdata = _businessLayer.SendPostAPIRequest(modeldata, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.GetJobLocationsByCompany), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            model.JoblocationList = JsonConvert.DeserializeObject<List<Joblcoations>>(objdata);
            return Json(new { data = model });
        }


        [HttpGet]
        public async Task<IActionResult> GetAttendanceDetails(string date, string employeeId, string employeeNo)
        {
            AttendanceDetailsInputParams objmodel = new AttendanceDetailsInputParams();
            if (string.IsNullOrEmpty(date))
            {
                return BadRequest("Invalid parameters.");
            }
            if (employeeId == null)
                                                                                                 {
                objmodel.EmployeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                objmodel.EmployeeNumber = Convert.ToString(HttpContext.Session.GetString(Constants.EmployeeNumberWithoutAbbr));
            }
            else
            {
                objmodel.EmployeeNumber = _businessLayer.DecodeStringBase64(employeeNo);
                objmodel.EmployeeId = Convert.ToInt64(_businessLayer.DecodeStringBase64(employeeId));
            }

            objmodel.SelectedDate = Convert.ToDateTime(date);

            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format.");
            }

            var data = _businessLayer.SendPostAPIRequest(objmodel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.FetchAttendanceHolidayAndLeaveInfo), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceDetailsVM>(data);

            // Return JSON (you can return a PartialView if you want HTML)
            return Json(model);
        }


        [HttpGet]
        public IActionResult ExportAttendance(DateTime FromDate, DateTime ToDate, int jobLocationId)
        {
            try
            {
                var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                var roleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));

                var models = new AttendanceInputParams
                {
                    FromDate = FromDate,
                    ToDate = ToDate,
                    UserId = employeeId,
                    RoleId = roleId,
                    PageSize = 0,
                    Page = 1,
                    JobLocationID = jobLocationId
                };

                var response = _businessLayer.SendPostAPIRequest(
                    models,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetExportAttendanceForCalendar),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var model = JsonConvert.DeserializeObject<AttendanceWithHolidaysVM>(response);
                if (model == null || model.Attendances == null || model.Attendances.Count == 0)
                {
                    return NotFound("No attendance data found.");
                }

                

                // Prepare day keys (e.g. "01_Thu")
                var dayKeys = new List<string>();
                for (DateTime date = FromDate; date <= ToDate; date = date.AddDays(1))
                {
                    dayKeys.Add(date.ToString("dd_ddd"));
                }

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Attendance");

                    // Build header row
                    var headers = new List<string> { "EmployeNumber", "EmployeeName" };
                    headers.AddRange(dayKeys.Select(k => k.Replace("_", " ")));
                    headers.Add("TotalWorkingDays");
                    headers.Add("PresentDays");
                    headers.Add("TotalLeaves");

                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Column(i + 1).AutoFit();
                    }

                    // Build data rows
                    int rowIndex = 2;
                    foreach (var record in model.Attendances)
                    {
                        worksheet.Cells[rowIndex, 1].Value = record.EmployeNumber;
                        worksheet.Cells[rowIndex, 2].Value = record.EmployeeName;

                        for (int colIndex = 0; colIndex < dayKeys.Count; colIndex++)
                        {
                            var key = dayKeys[colIndex];
                            string val = record.AttendanceByDay != null && record.AttendanceByDay.ContainsKey(key)
                                ? record.AttendanceByDay[key]
                                : "";
                            worksheet.Cells[rowIndex, 3 + colIndex].Value = val;
                        }

                        worksheet.Cells[rowIndex, 3 + dayKeys.Count].Value = record.TotalWorkingDays.ToString() ?? "-";
                        worksheet.Cells[rowIndex, 4 + dayKeys.Count].Value = record.PresentDays.ToString() ?? "-";
                        worksheet.Cells[rowIndex, 5 + dayKeys.Count].Value = record.TotalLeaves.ToString() ?? "-";

                        rowIndex++;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var excelBytes = package.GetAsByteArray();
                    string fileName = $"Attendance_{FromDate:yyyyMMdd}_to_{ToDate:yyyyMMdd}.xlsx";
                    return File(
                        excelBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }

            }
            catch (Exception ex)
            {
                // Optionally log ex.Message
                return StatusCode(500, "An error occurred while exporting attendance.");
            }
        }

        public IActionResult Whatshappening()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult Whatshappening(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            WhatsHappeningModelParans WhatsHappeningModelParams = new WhatsHappeningModelParans();
            WhatsHappeningModelParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(WhatsHappeningModelParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllWhatsHappeningDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            results.WhatsHappeningList.ForEach(x => x.IconImage = _s3Service.GetFileUrl(x.IconImage));
            return Json(new { data = results.WhatsHappeningList });

        }

        #region Apply Leave
        public IActionResult ApplyLeave(string id)
        {
          
            var model = new MyInfoInputParams
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
                EmployeeID = GetSessionLong(Constants.EmployeeID),
                GenderId = GetSessionInt(Constants.Gender),
                JobLocationTypeID = GetSessionInt(Constants.JobLocationID),
                UserID = GetSessionLong(Constants.UserID),
                CompanyID = GetSessionLong(Constants.CompanyID)
            };
            var RoleId = GetSessionInt(Constants.RoleID);

            //var FormPermission = _CheckUserFormPermission.GetFormPermission(model.EmployeeID, (int)PageName.MyInfo);
            //if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            //{
            //    HttpContext.Session.Clear();
            //    HttpContext.SignOutAsync();
            //    return RedirectToAction("Index", "Home", new { area = "" });
            //}

            // Call API
            var jsonData = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetMyInfo),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            var results = JsonConvert.DeserializeObject<MyInfoResults>(jsonData);

            // Encrypt and fetch certificates
            if (results?.leaveResults?.leavesSummary != null)
            {
                foreach (var leave in results.leaveResults.leavesSummary)
                {
                    leave.EncryptedIdentity = _businessLayer.EncodeStringBase64(leave.LeaveSummaryID.ToString());

                    if (!string.IsNullOrEmpty(leave.UploadCertificate))
                        leave.UploadCertificate = _s3Service.GetFileUrl(leave.UploadCertificate);
                }
            }
            var employeeDetails = GetEmployeeDetails(model.CompanyID, model.EmployeeID);
            var leavePolicy = GetLeavePolicyData(model.CompanyID, employeeDetails.LeavePolicyID ?? 0);

            // Fiscal year: March 21
            DateTime today = DateTime.Today;
            DateTime fiscalYearStart = (today.Month > 3 || (today.Month == 3 && today.Day >= 21))
                ? new DateTime(today.Year, 3, 21)
                : new DateTime(today.Year - 1, 3, 21);

            // Approved leaves: Annual + Medical only
            var approvedLeaves = results?.leaveResults?.leavesSummary?
                .Where(x => x.StartDate >= fiscalYearStart
                            && x.LeaveStatusID == (int)LeaveStatus.Approved
                            && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                .ToList();

            if (leavePolicy != null && results?.employmentDetail?.JoiningDate != null)
            {
                decimal approvedLeaveDays = approvedLeaves?.Sum(x => x.NoOfDays) ?? 0.0m;
                double approvedLeaveTotal = (double)approvedLeaveDays;

                double maxAnnualLeaveLimit = 30;
                double totalLeaveWithCarryForward = 0;

                if (approvedLeaveTotal >= maxAnnualLeaveLimit)
                {
                    // Already reached or exceeded limit, do not accrue further
                    totalLeaveWithCarryForward = maxAnnualLeaveLimit;
                }
                else
                {
                    // Calculate accrual
                    DateTime joinDate = results.employmentDetail.JoiningDate.Value;
                    double accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(joinDate, leavePolicy.Annual_MaximumLeaveAllocationAllowed);

                    // Cap accruedLeave so total does not exceed 30 minus approved leaves
                    double maxAvailable = maxAnnualLeaveLimit - approvedLeaveTotal;
                    accruedLeave = Math.Min(accruedLeave, maxAvailable);

                    // Add carry forward if applicable
                    if (leavePolicy.Annual_IsCarryForward == true)
                    {
                        double carryForward = employeeDetails.CarryForword ?? 0.0;
                        // Add carry forward but ensure total stays capped
                        accruedLeave = Math.Min(accruedLeave + carryForward, maxAvailable);
                    }

                    totalLeaveWithCarryForward = accruedLeave- approvedLeaveTotal ;

                    // Final safety cap (optional)
                    totalLeaveWithCarryForward = Math.Min(totalLeaveWithCarryForward, maxAnnualLeaveLimit);
                }

                // ViewBag assignments
                ViewBag.TotalLeave = approvedLeaveDays;
                ViewBag.TotalAnnualLeave = totalLeaveWithCarryForward;
                ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicy.Annual_MaximumConsecutiveLeavesAllowed);
            }

            return View(results);
        }

        [HttpPost]

        public IActionResult ApplyLeave(MyInfoResults model, List<IFormFile> postedFiles)
        {
            var leaveSummary = model.leaveResults.leaveSummaryModel;
            var startDate = leaveSummary.StartDate;
            var endDate = leaveSummary.EndDate;

            if ((int)LeaveDay.HalfDay == leaveSummary.LeaveDurationTypeID)
            {
                endDate = startDate;
            }

            var currentYearDate = GetAprilFirstDate();

            if ((int)LeaveDay.HalfDay != leaveSummary.LeaveDurationTypeID && startDate > endDate)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                return Json(new { isValid = false, message = "End date must be greater than or equal to start date." });
            }

            int totalDays = (endDate - startDate).Days + 1;
            int weekendDays = 0;

            leaveSummary.NoOfDays = totalDays;

            if ((int)LeaveDay.HalfDay == leaveSummary.LeaveDurationTypeID)
            {
                leaveSummary.StartDate = leaveSummary.EndDate = startDate;
                leaveSummary.NoOfDays = leaveSummary.NoOfDays / 2;
            }
            else
            {
                leaveSummary.LeaveDurationTypeID = (int)LeaveDay.FullDay;
            }

            if (leaveSummary.NoOfDays <= 0)
            {
                return View(model);
            }

            leaveSummary.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
            leaveSummary.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
            leaveSummary.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));

            var employeeDetails = GetEmployeeDetails(leaveSummary.CompanyID, leaveSummary.EmployeeID);
            leaveSummary.LeavePolicyID = employeeDetails.LeavePolicyID ?? 0;
            leaveSummary.LeaveStatusID = (int)LeaveStatus.PendingApproval;
            leaveSummary.EmployeeID = employeeDetails.EmployeeID;
            var joinDate = employeeDetails.JoiningDate;

            var leavePolicyModel = GetLeavePolicyData(leaveSummary.CompanyID, leaveSummary.LeavePolicyID);
            var leaveSummaryData = GetLeaveSummaryData(leaveSummary.EmployeeID, leaveSummary.UserID, leaveSummary.CompanyID);
            var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
            var EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var isAlreadyTakenLeave = ValidateAlreadyTakenLeaves(model, leaveSummaryData, EmployeeID);

            if (isAlreadyTakenLeave)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                return Json(new { isValid = false, message = "You have already applied leave for the same Date. Please remove the conflicting leave before applying again." });
            }

            double JoiningDays = 0;
            if (joinDate.HasValue)
            {
                JoiningDays = (DateTime.Today - joinDate.Value).TotalDays;
            }

            // Prepare Holiday list if needed
            List<HolidayModel> Holidaylist = new();
            if (leaveSummary.LeaveTypeID == (int)LeaveType.AnnualLeavel
                || leaveSummary.LeaveTypeID == (int)LeaveType.Paternity
                || leaveSummary.LeaveTypeID == (int)LeaveType.MedicalLeave)
            {
                var holidaydata = GetHolidayData(leaveSummary.CompanyID);
                var holidaydataList = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(holidaydata);
                Holidaylist = holidaydataList.Holiday.ToList();
            }

            // Count weekends and holidays only once for leave types that need it
            bool needsWeekendHolidayCount = leaveSummary.LeaveTypeID == (int)LeaveType.Paternity
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.AnnualLeavel //Privilage Leave
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.MedicalLeave
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.CompOff
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.LeaveWithOutPay;

            if (needsWeekendHolidayCount)
            {
                var weekOffInput = new LeaveWeekOfInputParams
                {
                    EmployeeID = leaveSummary.EmployeeID,
                    FromDate = leaveSummary.StartDate,
                    ToDate = leaveSummary.EndDate
                };

                // Call API to fetch week-offs
                var weekOffApiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveWeekOffDates);
                var weekOffResponse = _businessLayer.SendPostAPIRequest(weekOffInput, weekOffApiUrl, HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result?.ToString();
                List<DateTime> weekOffDates = new();
                if (!string.IsNullOrEmpty(weekOffResponse))
                {
                    weekOffDates = JsonConvert.DeserializeObject<List<DateTime>>(weekOffResponse) ?? new List<DateTime>();
                }
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {

                    bool isWeekend = weekOffDates.Contains(date.Date);

                    bool isHoliday = Holidaylist.Any(h => date >= h.FromDate.Date && date <= h.ToDate.Date);
                    if (isWeekend || isHoliday)
                        weekendDays++;
                }
                leaveSummary.NoOfDays -= weekendDays;
                if (leaveSummary.NoOfDays <= 0)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    return Json(new { isValid = false, message = "There will be a holiday or  weekeoff on the selected day ." });
                }
            }

            switch ((LeaveType)leaveSummary.LeaveTypeID)
            {
                case LeaveType.MaternityLeave:
                    if (leaveSummary.ExpectedDeliveryDate > endDate)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = "End date must be greater than or equal to Expected Delivery Date." });
                    }

                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {leavePolicyModel.Maternity_ApplicableAfterWorkingDays} days of joining" });
                    }

                    var maxMaternityLeaveDays = leavePolicyModel.Maternity_MaximumLeaveAllocationAllowed * 7;
                    if (totalDays > maxMaternityLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"Leave duration exceeds the maximum allowed of {maxMaternityLeaveDays} days." });
                    }
                    break;

                case LeaveType.Miscarriage:
                    var maxMiscarriageLeaveDays = leavePolicyModel.Miscarriage_MaximumLeaveAllocationAllowed * 7;
                    if (totalDays > maxMiscarriageLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = $"Leave duration exceeds the maximum allowed of {maxMiscarriageLeaveDays} days.";
                        return RedirectToActionPermanent(Constants.ApplyLeave, WebControllarsConstants.MyInfo);
                    }

                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = $"You can't apply leave(s) before {leavePolicyModel.Maternity_ApplicableAfterWorkingDays} days of joining";
                        return RedirectToActionPermanent(Constants.ApplyLeave, WebControllarsConstants.MyInfo);
                    }
                    break;

                case LeaveType.Adoption:
                    var maxAdoptionLeaveDays = leavePolicyModel.Adoption_MaximumLeaveAllocationAllowed * 7;
                    if (totalDays > maxAdoptionLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"Leave duration exceeds the maximum allowed of {maxAdoptionLeaveDays} days." });
                    }

                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {leavePolicyModel.Maternity_ApplicableAfterWorkingDays} days of joining" });
                    }
                    break;

                case LeaveType.Paternity:
                    var maxPaternityDays = leavePolicyModel.Paternity_applicableAfterWorkingMonth * 30;

                    if (maxPaternityDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {maxPaternityDays} days of joining" });
                    }

                    if (leaveSummary.ChildDOB.Value.Date > leaveSummary.EndDate.Date)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = "Child DOB must be less than or Equal to End Date." });
                    }

                    if (leaveSummary.ChildDOB.Value.AddMonths(3).Date <= leaveSummary.StartDate.Date)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = "Paternity leave must be taken within 3 months after delivery or miscarriage." });
                    }

                    if (leaveSummary.NoOfDays > leavePolicyModel.Paternity_maximumLeaveAllocationAllowed)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"Leave duration exceeds the maximum allowed of {leavePolicyModel.Paternity_maximumLeaveAllocationAllowed} days." });
                    }
                    break;

                case LeaveType.AnnualLeavel:
                case LeaveType.MedicalLeave:
                    if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new
                        {
                            isValid = false,
                            message = $"You can't apply leave(s) before {leavePolicyModel.Annual_ApplicableAfterWorkingDays} days of joining"
                        });
                    }

                    DateTime today = DateTime.Today;
                    DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);


                    decimal approvedLeaves = leaveSummaryDataResult
                        .Where(x => x.StartDate >= fiscalYearStart
                            && x.LeaveStatusID == (int)LeaveStatus.Approved
                            && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                        .Sum(x => x.NoOfDays);


                    double accruedLeaves = 0;
                    if (approvedLeaves < 30)
                    {
                        double accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(joinDate.Value, leavePolicyModel.Annual_MaximumLeaveAllocationAllowed);

                        if (leavePolicyModel.Annual_IsCarryForward == true)
                        {
                            double carryForward = Convert.ToDouble(employeeDetails.CarryForword);
                            accruedLeave += carryForward;
                        }

                        // Respect 30-leave annual cap
                        double maxAvailable = 30 - (double)approvedLeaves;
                        accruedLeaves = Math.Min(accruedLeave, maxAvailable);
                    }
                    else
                    {
                        accruedLeaves = 0; // Block both accrual and carry forward
                    }

                    // Step 3: Final validation
                    if (leaveSummary.NoOfDays > (decimal)accruedLeaves)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new
                        {
                            isValid = false,
                            message = $"Leave duration exceeds the privilege leave balance of {accruedLeaves} day(s)"
                        });
                    }

                    break;


                case LeaveType.CompOff:
                    if (model.CampOffLeaveCount < leaveSummary.NoOfDays)
                    {
                        var Message = "Exceeds maximum Compensatory Off allowed " + model.CampOffLeaveCount + ' ' + "days leaves";
                        return Json(new { isValid = false, message = Message });
                    }
                    //var totalJoiningDays = (DateTime.Today - employeeDetails.JoiningDate).TotalDays;
                    if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {leavePolicyModel.Annual_ApplicableAfterWorkingDays} days of joining" });
                    }
                    CampOffEligible modeldata = new CampOffEligible();
                    modeldata.JobLocationTypeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.JobLocationID));
                    modeldata.EmployeeID = leaveSummary.EmployeeID;
                    modeldata.EmployeeNumber = Convert.ToString(_context.HttpContext.Session.GetString(Constants.EmployeeNumberWithoutAbbr)); ;
                    modeldata.StartDate = model.leaveResults.leaveSummaryModel.StartDate.ToString("yyyy-MM-dd");
                    modeldata.EndDate = model.leaveResults.leaveSummaryModel.EndDate.ToString("yyyy-MM-dd");
                    modeldata.RequestedLeaveDays = leaveSummary.NoOfDays;
                    var campoffdata = GetValidateCompOffLeave(modeldata);
                    if (campoffdata != null)
                    {
                        if (campoffdata.IsEligible == 0)
                        {
                            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                            return Json(new { isValid = false, message = campoffdata.Message });
                        }
                        else
                        {
                            model.leaveResults.leaveSummaryModel.CampOff = (int)CompOff.OtherLeaves;
                        }

                    }


                    break;

                default:

                    break;
            }


            _s3Service.ProcessFileUpload(postedFiles, leaveSummary.UploadCertificate, out string newCertificateKey);

            if (!string.IsNullOrEmpty(newCertificateKey))
            {
                if (!string.IsNullOrEmpty(leaveSummary.UploadCertificate))
                {
                    _s3Service.DeleteFile(leaveSummary.UploadCertificate);
                }
                leaveSummary.UploadCertificate = newCertificateKey;
            }
            else
            {
                leaveSummary.UploadCertificate = _s3Service.ExtractKeyFromUrl(leaveSummary.UploadCertificate);
            }

            var data = _businessLayer.SendPostAPIRequest(model.leaveResults.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            string messageData = "";
            var result = JsonConvert.DeserializeObject<Result>(data);
            if (!string.IsNullOrEmpty(result.Message) &&
       result.Message.Contains("Leave status Added/Modified successfully", StringComparison.OrdinalIgnoreCase))
            {

                EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
                employeeobj.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
                var employeeApiResponse = _businessLayer.SendPostAPIRequest(
            employeeobj,
            _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
            HttpContext.Session.GetString(Constants.SessionBearerToken),
            true
        ).Result.ToString();
                var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);

                var Manager1Email = HttpContext.Session.GetString(Constants.Manager1Email).ToString();
                if (!string.IsNullOrEmpty(Manager1Email) && Manager1Email.Contains("@"))
                {
                    var Name = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));

                    sendEmailProperties sendEmailProperties = new sendEmailProperties
                    {

                        emailSubject = "Send a request for leave approval",
                        emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
           {Name} has sent a request for leave approval.<br/><br/>

            <table style='width: 100%; max-width: 600px; border-collapse: collapse; border: 1px solid #000;'>
                <thead style='background-color: #f2f2f2;'>
                    <tr>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>UserId </th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Name</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Email</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Department</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style='border: 1px solid #000; padding: 8px;'>{employeeResult.EmployeNumber}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{employeeResult.EmployeeName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{employeeResult.PersonalEmailAddress}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{employeeResult.DepartmentName}</td>
                    </tr>
                </tbody>
            </table><br/>

            

            <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>
        </div>"
                    };
                    sendEmailProperties.EmailToList.Add(Manager1Email);
                    emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);
                }



                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                messageData = "Leave applied successfully.";
            }
            else
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                messageData = "Some thing went wrong";
            }

            return Json(new { isValid = true, message = messageData });

        }




        #endregion Apply Leave
        #region Agent Leave
        public IActionResult ApplyAgentLeave(string id, string jobLocationId, string genderId, string employeeId)
        {

            int genID = 0;
            long jobID = 0;
            if (!string.IsNullOrEmpty(employeeId))
            {
         
                int.TryParse(genderId, out genID);
                long.TryParse(jobLocationId, out jobID);

            }
            var RoleId = GetSessionInt(Constants.RoleID);
            var formEmployeeID = GetSessionLong(Constants.EmployeeID);
            var FormPermission = _CheckUserFormPermission.GetFormPermission(formEmployeeID, (int)PageName.ApplyAgentLeave);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }


            var model = new MyInfoInputParams
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
                EmployeeID = string.IsNullOrEmpty(employeeId)? GetSessionLong(Constants.EmployeeID): Convert.ToInt64(_businessLayer.DecodeStringBase64(employeeId)),
                GenderId = string.IsNullOrEmpty(genderId)? GetSessionInt(Constants.Gender) :genID,
                JobLocationTypeID = string.IsNullOrEmpty(jobLocationId)? GetSessionInt(Constants.JobLocationID) : jobID,
                UserID = GetSessionLong(Constants.UserID) ,
                CompanyID = GetSessionLong(Constants.CompanyID)
            };
           



            var jsonData = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetMyAgentInfo),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            var results = JsonConvert.DeserializeObject<MyInfoResults>(jsonData);

            if (results?.leaveResults?.leavesSummary != null)
            {
                foreach (var leave in results.leaveResults.leavesSummary)
                {
                    leave.EncryptedIdentity = _businessLayer.EncodeStringBase64(leave.LeaveSummaryID.ToString());
                    leave.Encrypted = _businessLayer.EncodeStringBase64(leave.EmployeeID.ToString());
                    //leave.EncryptedGender = _businessLayer.EncodeStringBase64(leave.Gender.ToString());
                    leave.EncryptedLocation= _businessLayer.EncodeStringBase64(leave.JobLocationID.ToString());

                    if (!string.IsNullOrEmpty(leave.UploadCertificate))
                        leave.UploadCertificate = _s3Service.GetFileUrl(leave.UploadCertificate);
                }
            }
            var employeeDetails = GetEmployeeDetails(model.CompanyID, model.EmployeeID);
            var leavePolicy = GetLeavePolicyData(model.CompanyID, employeeDetails.LeavePolicyID ?? 0);

            // Fiscal year: March 21
            DateTime today = DateTime.Today;
            DateTime fiscalYearStart = (today.Month > 3 || (today.Month == 3 && today.Day >= 21))
                ? new DateTime(today.Year, 3, 21)
                : new DateTime(today.Year - 1, 3, 21);

            // Approved leaves: Annual + Medical only
            var approvedLeaves = results?.leaveResults?.leavesSummary?
                .Where(x => x.StartDate >= fiscalYearStart
                            && x.LeaveStatusID == (int)LeaveStatus.Approved
                            && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                .ToList();

            if (leavePolicy != null && results?.employmentDetail?.JoiningDate != null)
            {
                decimal approvedLeaveDays = approvedLeaves?.Sum(x => x.NoOfDays) ?? 0.0m;
                double approvedLeaveTotal = (double)approvedLeaveDays;

                double maxAnnualLeaveLimit = 30;
                double totalLeaveWithCarryForward = 0;

                if (approvedLeaveTotal >= maxAnnualLeaveLimit)
                {

                    totalLeaveWithCarryForward = maxAnnualLeaveLimit;
                }
                else
                {
                    // Calculate accrual
                    DateTime joinDate = results.employmentDetail.JoiningDate.Value;
                    double accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(joinDate, leavePolicy.Annual_MaximumLeaveAllocationAllowed);

                    // Cap accruedLeave so total does not exceed 30 minus approved leaves
                    double maxAvailable = maxAnnualLeaveLimit - approvedLeaveTotal;
                    accruedLeave = Math.Min(accruedLeave, maxAvailable);

                    // Add carry forward if applicable
                    if (leavePolicy.Annual_IsCarryForward == true)
                    {
                        double carryForward = employeeDetails.CarryForword ?? 0.0;
                        // Add carry forward but ensure total stays capped
                        accruedLeave = Math.Min(accruedLeave + carryForward, maxAvailable);
                    }

                    totalLeaveWithCarryForward = accruedLeave - approvedLeaveTotal;

                    // Final safety cap (optional)
                    totalLeaveWithCarryForward = Math.Min(totalLeaveWithCarryForward, maxAnnualLeaveLimit);
                }

                // ViewBag assignments
                ViewBag.TotalLeave = approvedLeaveDays;
                ViewBag.TotalAnnualLeave = totalLeaveWithCarryForward;
                ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicy.Annual_MaximumConsecutiveLeavesAllowed);
            }

            return View(results);
        }

        [HttpPost]

        public IActionResult ApplyAgentLeave(MyInfoResults model, List<IFormFile> postedFiles)
        {
           
            var leaveSummary = model.leaveResults.leaveSummaryModel;
            var startDate = leaveSummary.StartDate;
            var endDate = leaveSummary.EndDate;
            if ((int)LeaveDay.HalfDay == leaveSummary.LeaveDurationTypeID)
            {
                endDate = startDate;
            }
            var currentYearDate = GetAprilFirstDate();

            if ((int)LeaveDay.HalfDay != leaveSummary.LeaveDurationTypeID && startDate > endDate)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                return Json(new { isValid = false, message = "End date must be greater than or equal to start date." });
            }

            int totalDays = (endDate - startDate).Days + 1;
            int weekendDays = 0;

            leaveSummary.NoOfDays = totalDays;

            if ((int)LeaveDay.HalfDay == leaveSummary.LeaveDurationTypeID)
            {
                leaveSummary.StartDate = leaveSummary.EndDate = startDate;
                leaveSummary.NoOfDays = leaveSummary.NoOfDays / 2;
            }
            else
            {
                leaveSummary.LeaveDurationTypeID = (int)LeaveDay.FullDay;
            }

            if (leaveSummary.NoOfDays <= 0)
            {
                return View(model);
            }

            //leaveSummary.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
            leaveSummary.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
            leaveSummary.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));

            var employeeDetails = GetEmployeeDetails(leaveSummary.CompanyID, leaveSummary.EmployeeID);
            leaveSummary.LeavePolicyID = employeeDetails.LeavePolicyID ?? 0;
            leaveSummary.LeaveStatusID = (int)LeaveStatus.PendingApproval;
            leaveSummary.EmployeeID = employeeDetails.EmployeeID;
            var joinDate = employeeDetails.JoiningDate;

            var leavePolicyModel = GetLeavePolicyData(leaveSummary.CompanyID, leaveSummary.LeavePolicyID);
            var leaveSummaryData = GetLeaveSummaryData(leaveSummary.EmployeeID, leaveSummary.UserID, leaveSummary.CompanyID);
            var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
            var EmployeeID = employeeDetails.EmployeeID;
            var isAlreadyTakenLeave = ValidateAlreadyTakenLeaves(model, leaveSummaryData, EmployeeID);

            if (isAlreadyTakenLeave)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                return Json(new { isValid = false, message = "You have already applied leave for the same Date. Please remove the conflicting leave before applying again." });
            }

            double JoiningDays = 0;
            if (joinDate.HasValue)
            {
                JoiningDays = (DateTime.Today - joinDate.Value).TotalDays;
            }

            // Prepare Holiday list if needed
            List<HolidayModel> Holidaylist = new();
            if (leaveSummary.LeaveTypeID == (int)LeaveType.AnnualLeavel
                || leaveSummary.LeaveTypeID == (int)LeaveType.Paternity
                || leaveSummary.LeaveTypeID == (int)LeaveType.MedicalLeave)
            {
                var holidaydata = GetAgentHolidayData(leaveSummary.CompanyID, leaveSummary.EmployeeID);
                var holidaydataList = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(holidaydata);
                Holidaylist = holidaydataList.Holiday.ToList();
            }

            // Count weekends and holidays only once for leave types that need it
            bool needsWeekendHolidayCount = leaveSummary.LeaveTypeID == (int)LeaveType.Paternity
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.AnnualLeavel //Privilage Leave
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.MedicalLeave
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.CompOff
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.LeaveWithOutPay;

            if (needsWeekendHolidayCount)
            {
                var weekOffInput = new LeaveWeekOfInputParams
                {
                    EmployeeID = leaveSummary.EmployeeID,
                    FromDate = leaveSummary.StartDate,
                    ToDate = leaveSummary.EndDate
                };

                // Call API to fetch week-offs
                var weekOffApiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveWeekOffDates);
                var weekOffResponse = _businessLayer.SendPostAPIRequest(weekOffInput, weekOffApiUrl, HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result?.ToString();
                List<DateTime> weekOffDates = new();
                if (!string.IsNullOrEmpty(weekOffResponse))
                {
                    weekOffDates = JsonConvert.DeserializeObject<List<DateTime>>(weekOffResponse) ?? new List<DateTime>();
                }
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {

                    bool isWeekend = weekOffDates.Contains(date.Date);

                    bool isHoliday = Holidaylist.Any(h => date >= h.FromDate.Date && date <= h.ToDate.Date);
                    if (isWeekend || isHoliday)
                        weekendDays++;
                }
                leaveSummary.NoOfDays -= weekendDays;
                if (leaveSummary.NoOfDays <= 0)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    return Json(new { isValid = false, message = "There will be a holiday or  weekeoff on the selected day ." });
                }
            }

            switch ((LeaveType)leaveSummary.LeaveTypeID)
            {
                case LeaveType.MaternityLeave:
                    if (leaveSummary.ExpectedDeliveryDate > endDate)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = "End date must be greater than or equal to Expected Delivery Date." });
                    }

                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {leavePolicyModel.Maternity_ApplicableAfterWorkingDays} days of joining" });
                    }

                    var maxMaternityLeaveDays = leavePolicyModel.Maternity_MaximumLeaveAllocationAllowed * 7;
                    if (totalDays > maxMaternityLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"Leave duration exceeds the maximum allowed of {maxMaternityLeaveDays} days." });
                    }
                    break;

                case LeaveType.Miscarriage:
                    var maxMiscarriageLeaveDays = leavePolicyModel.Miscarriage_MaximumLeaveAllocationAllowed * 7;
                    if (totalDays > maxMiscarriageLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = $"Leave duration exceeds the maximum allowed of {maxMiscarriageLeaveDays} days.";
                        return RedirectToActionPermanent(Constants.ApplyAgentLeave, WebControllarsConstants.MyInfo);
                    }

                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = $"You can't apply leave(s) before {leavePolicyModel.Maternity_ApplicableAfterWorkingDays} days of joining";
                        return RedirectToActionPermanent(Constants.ApplyAgentLeave, WebControllarsConstants.MyInfo);
                    }
                    break;

                case LeaveType.Adoption:
                    var maxAdoptionLeaveDays = leavePolicyModel.Adoption_MaximumLeaveAllocationAllowed * 7;
                    if (totalDays > maxAdoptionLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"Leave duration exceeds the maximum allowed of {maxAdoptionLeaveDays} days." });
                    }

                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {leavePolicyModel.Maternity_ApplicableAfterWorkingDays} days of joining" });
                    }
                    break;

                case LeaveType.Paternity:
                    var maxPaternityDays = leavePolicyModel.Paternity_applicableAfterWorkingMonth * 30;

                    if (maxPaternityDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {maxPaternityDays} days of joining" });
                    }

                    if (leaveSummary.ChildDOB.Value.Date > leaveSummary.EndDate.Date)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = "Child DOB must be less than or Equal to End Date." });
                    }

                    if (leaveSummary.ChildDOB.Value.AddMonths(3).Date <= leaveSummary.StartDate.Date)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = "Paternity leave must be taken within 3 months after delivery or miscarriage." });
                    }

                    if (leaveSummary.NoOfDays > leavePolicyModel.Paternity_maximumLeaveAllocationAllowed)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"Leave duration exceeds the maximum allowed of {leavePolicyModel.Paternity_maximumLeaveAllocationAllowed} days." });
                    }
                    break;

                case LeaveType.AnnualLeavel:
                case LeaveType.MedicalLeave:
                    if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new
                        {
                            isValid = false,
                            message = $"You can't apply leave(s) before {leavePolicyModel.Annual_ApplicableAfterWorkingDays} days of joining"
                        });
                    }

                    DateTime today = DateTime.Today;
                    DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);


                    decimal approvedLeaves = leaveSummaryDataResult
                        .Where(x => x.StartDate >= fiscalYearStart
                            && x.LeaveStatusID == (int)LeaveStatus.Approved
                            && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                        .Sum(x => x.NoOfDays);


                    double accruedLeaves = 0;
                    if (approvedLeaves < 30)
                    {
                        double accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(joinDate.Value, leavePolicyModel.Annual_MaximumLeaveAllocationAllowed);

                        if (leavePolicyModel.Annual_IsCarryForward == true)
                        {
                            double carryForward = Convert.ToDouble(employeeDetails.CarryForword);
                            accruedLeave += carryForward;
                        }

                        // Respect 30-leave annual cap
                        double maxAvailable = 30 - (double)approvedLeaves;
                        accruedLeaves = Math.Min(accruedLeave, maxAvailable);
                    }
                    else
                    {
                        accruedLeaves = 0; // Block both accrual and carry forward
                    }

                    // Step 3: Final validation
                    if (leaveSummary.NoOfDays > (decimal)accruedLeaves)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new
                        {
                            isValid = false,
                            message = $"Leave duration exceeds the privilege leave balance of {accruedLeaves} day(s)"
                        });
                    }

                    break;


                case LeaveType.CompOff:
                    if (model.CampOffLeaveCount < leaveSummary.NoOfDays)
                    {
                        var Message = "Exceeds maximum Compensatory Off allowed " + model.CampOffLeaveCount + ' ' + "days leaves";
                        return Json(new { isValid = false, message = Message });
                    }
                    //var totalJoiningDays = (DateTime.Today - employeeDetails.JoiningDate).TotalDays;
                    if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {leavePolicyModel.Annual_ApplicableAfterWorkingDays} days of joining" });
                    }
                    CampOffEligible modeldata = new CampOffEligible();
                    modeldata.JobLocationTypeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.JobLocationID));
                    modeldata.EmployeeID = leaveSummary.EmployeeID;
                    modeldata.EmployeeNumber = Convert.ToString(_context.HttpContext.Session.GetString(Constants.EmployeeNumberWithoutAbbr)); ;
                    modeldata.StartDate = model.leaveResults.leaveSummaryModel.StartDate.ToString("yyyy-MM-dd");
                    modeldata.EndDate = model.leaveResults.leaveSummaryModel.EndDate.ToString("yyyy-MM-dd");
                    modeldata.RequestedLeaveDays = leaveSummary.NoOfDays;
                    var campoffdata = GetValidateCompOffLeave(modeldata);
                    if (campoffdata != null)
                    {
                        if (campoffdata.IsEligible == 0)
                        {
                            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                            return Json(new { isValid = false, message = campoffdata.Message });
                        }
                        else
                        {
                            model.leaveResults.leaveSummaryModel.CampOff = (int)CompOff.OtherLeaves;
                        }

                    }


                    break;

                default:

                    break;
            }


            _s3Service.ProcessFileUpload(postedFiles, leaveSummary.UploadCertificate, out string newCertificateKey);

            if (!string.IsNullOrEmpty(newCertificateKey))
            {
                if (!string.IsNullOrEmpty(leaveSummary.UploadCertificate))
                {
                    _s3Service.DeleteFile(leaveSummary.UploadCertificate);
                }
                leaveSummary.UploadCertificate = newCertificateKey;
            }
            else
            {
                leaveSummary.UploadCertificate = _s3Service.ExtractKeyFromUrl(leaveSummary.UploadCertificate);
            }

            var data = _businessLayer.SendPostAPIRequest(model.leaveResults.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            string messageData = "";
            var result = JsonConvert.DeserializeObject<Result>(data);
            if (!string.IsNullOrEmpty(result.Message) &&
       result.Message.Contains("Leave status Added/Modified successfully", StringComparison.OrdinalIgnoreCase))
            {

                EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
                employeeobj.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
                var employeeApiResponse = _businessLayer.SendPostAPIRequest(
            employeeobj,
            _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
            HttpContext.Session.GetString(Constants.SessionBearerToken),
            true
        ).Result.ToString();
                var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);

                var Manager1Email = HttpContext.Session.GetString(Constants.Manager1Email).ToString();
                if (!string.IsNullOrEmpty(Manager1Email) && Manager1Email.Contains("@"))
                {
                    var Name = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));

                    sendEmailProperties sendEmailProperties = new sendEmailProperties
                    {

                        emailSubject = "Send a request for leave approval",
                        emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
           {Name} has sent a request for leave approval.<br/><br/>

            <table style='width: 100%; max-width: 600px; border-collapse: collapse; border: 1px solid #000;'>
                <thead style='background-color: #f2f2f2;'>
                    <tr>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>UserId </th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Name</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Email</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Department</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style='border: 1px solid #000; padding: 8px;'>{employeeResult.EmployeNumber}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{employeeResult.EmployeeName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{employeeResult.PersonalEmailAddress}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{employeeResult.DepartmentName}</td>
                    </tr>
                </tbody>
            </table><br/>

            

            <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>
        </div>"
                    };
                    sendEmailProperties.EmailToList.Add(Manager1Email);
                    emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);
                }



                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                messageData = "Leave applied successfully.";
            }
            else
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                messageData = "Some thing went wrong";
            }

            return Json(new { isValid = true, message = messageData });

        }


        [HttpPost]
        public IActionResult DeleteAgentLeavesSummary(string id)
        {
            MyInfoInputParams model = new MyInfoInputParams()
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteLeavesSummary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(
                      Constants.ApplyAgentLeave,
                       WebControllarsConstants.MyInfo);
        }


        [HttpPost]
        public JsonResult GetLastLevelEmployeeDropdown()
        {
            var employeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            LastLevelEmployeeDropdownParams lastLevelParams = new LastLevelEmployeeDropdownParams();
            lastLevelParams.EmployeeID = employeeID;
            var data = _businessLayer.SendPostAPIRequest(lastLevelParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLastLevelEmployeeDropdown), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<LastLevelEmployeeDropdown>>(data);
            return Json(new { data = model });

        }

        [HttpPost]
        public JsonResult GetApplyLeaveData(string employeeId, string jobLocationId, string genderId)
        {
            long empID = 0;
            int genID = 0;
            long jobID = 0;
            if (!string.IsNullOrEmpty(employeeId))
            {
                long.TryParse(employeeId, out empID);
                int.TryParse(genderId, out genID);
                long.TryParse(jobLocationId, out jobID);

            }
            var companyId = GetSessionLong(Constants.CompanyID);
            var model = new MyInfoInputParams
            {
                LeaveSummaryID = 0,
                EmployeeID = empID,
                GenderId = genID,
                JobLocationTypeID = jobID,
                UserID = GetSessionLong(Constants.UserID),
                CompanyID = companyId
            };

            var jsonData = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetMyInfo),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            var results = JsonConvert.DeserializeObject<MyInfoResults>(jsonData);

            var employeeDetails = GetEmployeeDetails(companyId, empID);
            var leavePolicy = GetLeavePolicyData(companyId, employeeDetails.LeavePolicyID ?? 0);

            // Fiscal year logic
            DateTime today = DateTime.Today;
            DateTime fiscalYearStart = (today.Month > 3 || (today.Month == 3 && today.Day >= 21))
                ? new DateTime(today.Year, 3, 21)
                : new DateTime(today.Year - 1, 3, 21);

            var approvedLeaves = results?.leaveResults?.leavesSummary?
                .Where(x => x.StartDate >= fiscalYearStart
                    && x.LeaveStatusID == (int)LeaveStatus.Approved
                    && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                .ToList();

            decimal approvedLeaveDays = approvedLeaves?.Sum(x => x.NoOfDays) ?? 0.0m;
            double approvedLeaveTotal = (double)approvedLeaveDays;
            double totalLeaveWithCarryForward = 0;
            double maxAnnualLeaveLimit = 30;

            if (leavePolicy != null && results?.employmentDetail?.JoiningDate != null)
            {
                if (approvedLeaveTotal >= maxAnnualLeaveLimit)
                {
                    totalLeaveWithCarryForward = maxAnnualLeaveLimit;
                }
                else
                {
                    DateTime joinDate = results.employmentDetail.JoiningDate.Value;
                    double accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(joinDate, leavePolicy.Annual_MaximumLeaveAllocationAllowed);

                    double maxAvailable = maxAnnualLeaveLimit - approvedLeaveTotal;
                    accruedLeave = Math.Min(accruedLeave, maxAvailable);

                    if (leavePolicy.Annual_IsCarryForward == true)
                    {
                        double carryForward = employeeDetails.CarryForword ?? 0.0;
                        accruedLeave = Math.Min(accruedLeave + carryForward, maxAvailable);
                    }

                    totalLeaveWithCarryForward = accruedLeave - approvedLeaveTotal;
                    totalLeaveWithCarryForward = Math.Min(totalLeaveWithCarryForward, maxAnnualLeaveLimit);
                }
            }
            var filteredLeaveTypes = results?.leaveResults?.leaveTypes;
            if (results?.CampOffLeaveCount <= 0)
            {
                filteredLeaveTypes = filteredLeaveTypes?
                    .Where(lt => !lt.Text.Equals("Compensatory Off", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            return Json(new
            {
                leaveTypes = filteredLeaveTypes,
                totalApprovedLeave = approvedLeaveTotal,
                totalAnnualLeave = totalLeaveWithCarryForward,
                consecutiveDaysAllowed = leavePolicy?.Annual_MaximumConsecutiveLeavesAllowed ?? 0
            });
        }
        #endregion Agent Leave


       

        #region Approve Leave
        public IActionResult ApproveLeave(string id)
        {
          
            var model = new MyInfoInputParams
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
                EmployeeID = GetSessionLong(Constants.EmployeeID),
                GenderId = GetSessionInt(Constants.Gender),
                JobLocationTypeID = GetSessionInt(Constants.JobLocationID),
                UserID = GetSessionLong(Constants.UserID),
                CompanyID = GetSessionLong(Constants.CompanyID)
            };
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(model.EmployeeID, (int)PageName.ApproveLeave);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Call API
            var jsonData = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetMyInfo),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            var results = JsonConvert.DeserializeObject<MyInfoResults>(jsonData);

            // Encrypt and fetch certificates
            if (results?.leaveResults?.leavesSummary != null)
            {
                foreach (var leave in results.leaveResults.leavesSummary)
                {
                    leave.EncryptedIdentity = _businessLayer.EncodeStringBase64(leave.LeaveSummaryID.ToString());

                    if (!string.IsNullOrEmpty(leave.UploadCertificate))
                        leave.UploadCertificate = _s3Service.GetFileUrl(leave.UploadCertificate);
                }
            }
            var employeeDetails = GetEmployeeDetails(model.CompanyID, model.EmployeeID);
            var leavePolicy = GetLeavePolicyData(model.CompanyID, employeeDetails.LeavePolicyID ?? 0);

            // Fiscal year: March 21
            DateTime today = DateTime.Today;
            DateTime fiscalYearStart = (today.Month > 3 || (today.Month == 3 && today.Day >= 21))
                ? new DateTime(today.Year, 3, 21)
                : new DateTime(today.Year - 1, 3, 21);

            // Approved leaves: Annual + Medical only
            var approvedLeaves = results?.leaveResults?.leavesSummary?
                .Where(x => x.StartDate >= fiscalYearStart
                            && x.LeaveStatusID == (int)LeaveStatus.Approved
                            && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                .ToList();

            if (leavePolicy != null && results?.employmentDetail?.JoiningDate != null)
            {
                decimal approvedLeaveDays = approvedLeaves?.Sum(x => x.NoOfDays) ?? 0.0m;
                double approvedLeaveTotal = (double)approvedLeaveDays;

                double maxAnnualLeaveLimit = 30;
                double totalLeaveWithCarryForward = 0;

                if (approvedLeaveTotal >= maxAnnualLeaveLimit)
                {
                    // Already reached or exceeded limit, do not accrue further
                    totalLeaveWithCarryForward = maxAnnualLeaveLimit;
                }
                else
                {
                    // Calculate accrual
                    DateTime joinDate = results.employmentDetail.JoiningDate.Value;
                    double accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(joinDate, leavePolicy.Annual_MaximumLeaveAllocationAllowed);

                    // Cap accruedLeave so total does not exceed 30 minus approved leaves
                    double maxAvailable = maxAnnualLeaveLimit - approvedLeaveTotal;
                    accruedLeave = Math.Min(accruedLeave, maxAvailable);

                    // Add carry forward if applicable
                    if (leavePolicy.Annual_IsCarryForward == true)
                    {
                        double carryForward = employeeDetails.CarryForword ?? 0.0;
                        // Add carry forward but ensure total stays capped
                        accruedLeave = Math.Min(accruedLeave + carryForward, maxAvailable);
                    }

                    totalLeaveWithCarryForward = approvedLeaveTotal + accruedLeave;

                    // Final safety cap (optional)
                    totalLeaveWithCarryForward = Math.Min(totalLeaveWithCarryForward, maxAnnualLeaveLimit);
                }

                // ViewBag assignments
                ViewBag.TotalLeave = approvedLeaveDays;
                ViewBag.TotalAnnualLeave = totalLeaveWithCarryForward;
                ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicy.Annual_MaximumConsecutiveLeavesAllowed);
            }

            return View(results);
        }

      

        #endregion Approve Leave
    }
}
