using System.ServiceModel.Channels;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
using HRMS.Models.MyInfo;
using HRMS.Models.WhatsHappeningModel;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.Employee.Controllers
{
    [Area(Constants.ManageEmployee)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Employee + "," + RoleConstants.Manager + "," + RoleConstants.SuperAdmin))]
    public class MyInfoController : Controller
    {
        IHttpContextAccessor _context;
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        private readonly IS3Service _s3Service;
        public MyInfoController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IHttpContextAccessor context, IS3Service s3Service)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            _context = context;
            _s3Service = s3Service;
        }
        [HttpGet]
        public IActionResult Index(string id)
        {
            MyInfoInputParams model = new MyInfoInputParams()
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
            };
            model.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
            model.GenderId = Convert.ToInt32(_context.HttpContext.Session.GetString(Constants.Gender));

            model.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            model.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetMyInfo), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

            var results = JsonConvert.DeserializeObject<MyInfoResults>(data);

            results.leaveResults.leavesSummary.ForEach(x => x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.LeaveSummaryID.ToString()));
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

            var TotalApproveList = results.leaveResults.leavesSummary.Where(x => x.StartDate >= fiscalYearStart && x.LeaveStatusID == (int)LeaveStatus.Approved && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave)).OrderBy(x => x.LeaveSummaryID).ToList();

            var employeeDetails = GetEmployeeDetails(model.CompanyID, model.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(model.CompanyID, employeeDetails.LeavePolicyID ?? 0);
            if (leavePolicyModel != null)
            {
                //var joindate = employeeDetails.EmploymentDetail.Select(x => x.JoiningDate).FirstOrDefault();
                var joindate = results.employmentDetail.JoiningDate;
                if (joindate != null)
                {
                    DateTime joinDate1 = joindate.Value;
                    double accruedLeave1 = CalculateAccruedLeaveForCurrentFiscalYear(joinDate1, leavePolicyModel.Annual_MaximumLeaveAllocationAllowed);
                    var TotalApproveLists = TotalApproveList.Sum(x => x.NoOfDays);

                    double TotalApprove = Convert.ToDouble(TotalApproveLists);
                    double Totacarryforword = 0.0;
                    var Totaleavewithcarryforword = 0.0;
                    var accruedLeaves = accruedLeave1 - TotalApprove;
                    if (leavePolicyModel.Annual_IsCarryForward == true)
                    {
                        Totacarryforword = Convert.ToDouble(employeeDetails.CarryForword);
                        Totaleavewithcarryforword = Totacarryforword + accruedLeaves;
                    }
                    else
                    {
                        Totaleavewithcarryforword = accruedLeaves;
                    }
                    ViewBag.TotalLeave = TotalApprove;
                    ViewBag.TotalAnnualLeave = Totaleavewithcarryforword;
                    ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);
                }
            }
            return View(results);

        }

        [HttpGet]
        public IActionResult GetEmployeeLeaveDetails(string employeeID)
        {
            MyInfoInputParams model = new MyInfoInputParams();
            model.EmployeeID = Convert.ToInt64(employeeID);
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
                Approvals = results.leavesSummary.Where(x => x.LeaveStatusID != (int)LeaveStatus.Approved && x.LeaveStatusID != (int)LeaveStatus.NotApproved).ToList();
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
        public JsonResult ApproveRejectLeave(long leaveSummaryID, bool isApproved, string ApproveRejectComment,int leaveTypeID)
        {
            ErrorLeaveResults obj = new ErrorLeaveResults();
            MyInfoInputParams employee = new MyInfoInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);
            var rowData = results.leavesSummary.Where(x => x.LeaveSummaryID == leaveSummaryID).FirstOrDefault();
            if (rowData != null)
            {
                rowData.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
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
            if(leaveTypeID == (int) LeaveType.CompOff)
            {
                rowData.CampOff = (int)CompOff.CompOff;
            }
            if (isApproved == true)
            {
                double JoiningDays = 0;
                if (rowData.JoiningDate.HasValue)
                {
                    TimeSpan? difference = DateTime.Today - rowData.JoiningDate;

                    if (difference.HasValue)
                    {
                        JoiningDays = difference.Value.TotalDays;
                    }
                }
                var currentYearDate = GetAprilFirstDate();

                int totalDays = (rowData.EndDate - rowData.StartDate).Days + 1;
                int weekendDays = 0;
                var Holidaylist = new List<HolidayModel>();
                var leavePolicyModel = GetLeavePolicyData(rowData.CompanyID, rowData.LeavePolicyID);
                //    Annual Leavel
                if (rowData.LeaveTypeID == (int)LeaveType.AnnualLeavel)
                {
                    DateTime today = DateTime.Today;
                    DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);
                    var leaveSummaryData = GetLeaveSummaryData(rowData.EmployeeID, rowData.UserID, rowData.CompanyID);
                    var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
                    var TotalApproveList = leaveSummaryDataResult.Where(x => x.StartDate >= fiscalYearStart && x.LeaveStatusID == (int)LeaveStatus.Approved && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave)).OrderBy(x => x.LeaveSummaryID).ToList();
                    double accruedLeave1 = CalculateAccruedLeaveForCurrentFiscalYear(rowData.JoiningDate.Value, leavePolicyModel.Annual_MaximumLeaveAllocationAllowed);
                    var TotalApproveLists = TotalApproveList.Sum(x => x.NoOfDays);
                    double TotalApprove = Convert.ToDouble(TotalApproveLists);
                    var Totaleavewithcarryforword = 0.0;
                    var accruedLeaves = accruedLeave1 - TotalApprove;
                    Totaleavewithcarryforword = accruedLeaves;
                    //validation for applicable after working days
                    if (Convert.ToDouble(rowData.NoOfDays) > accruedLeaves)
                    {
                        var Message = "Leave duration exceeds the annual leave balance of " + accruedLeaves + " days";
                        obj.status = 1;
                        obj.message = Message;
                        return Json(new { data = obj });
                    }
                    
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
                
            }
            return Json(new { data = resultsLeaveResultsdata });
        }

        [HttpPost]
        //public IActionResult Index(MyInfoResults model, List<IFormFile> postedFiles)
        //{
        //    var startDate = model.leaveResults.leaveSummaryModel.StartDate;
        //    var endDate = model.leaveResults.leaveSummaryModel.EndDate;
        //    if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
        //    {
        //        endDate = startDate;
        //    }
        //    //get current year date
        //    var currentYearDate = GetAprilFirstDate();
        //    //validation for fromdate should not greater than todate
        //    if ((int)LeaveDay.HalfDay != model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
        //    {
        //        if (startDate > endDate)
        //        {
        //            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //            var message = "End date must be greater than or equal to start date.";
        //            return Json(new { isValid = false, message = message });
        //        }
        //    }

        //    // Initialize totalDays and weekendDays
        //    int totalDays = (endDate - startDate).Days + 1;
        //    int weekendDays = 0;

        //    // Continue with further processing...
        //    model.leaveResults.leaveSummaryModel.NoOfDays = totalDays;
        //    if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
        //    {
        //        model.leaveResults.leaveSummaryModel.StartDate = model.leaveResults.leaveSummaryModel.EndDate = model.leaveResults.leaveSummaryModel.StartDate;
        //        model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays / 2;
        //    }
        //    else
        //    {
        //        model.leaveResults.leaveSummaryModel.LeaveDurationTypeID = (int)LeaveDay.FullDay;
        //    }
        //    if (model.leaveResults.leaveSummaryModel.NoOfDays > 0)
        //    {
        //        model.leaveResults.leaveSummaryModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
        //        model.leaveResults.leaveSummaryModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
        //        model.leaveResults.leaveSummaryModel.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));

        //        // Get employee details   
        //        var employeeDetails = GetEmployeeDetails(model.leaveResults.leaveSummaryModel.CompanyID, model.leaveResults.leaveSummaryModel.EmployeeID);
        //        model.leaveResults.leaveSummaryModel.LeavePolicyID = employeeDetails.LeavePolicyID ?? 0;
        //        model.leaveResults.leaveSummaryModel.LeaveStatusID = (int)LeaveStatus.PendingApproval;
        //        model.leaveResults.leaveSummaryModel.EmployeeID = employeeDetails.EmployeeID;
        //        var joindate = employeeDetails.JoiningDate;

        //        // Get leave policy data  
        //        var leavePolicyModel = GetLeavePolicyData(model.leaveResults.leaveSummaryModel.CompanyID, model.leaveResults.leaveSummaryModel.LeavePolicyID);

        //        //Get leave summary data  
        //        var leaveSummaryData = GetLeaveSummaryData(model.leaveResults.leaveSummaryModel.EmployeeID, model.leaveResults.leaveSummaryModel.UserID, model.leaveResults.leaveSummaryModel.CompanyID);
        //        var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
        //        var EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
        //        var isAlreadyTakenLeave = ValidateAlreadyTakenLeaves(model, leaveSummaryData, EmployeeID);
        //        if (isAlreadyTakenLeave)
        //        {
        //            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //            var message = "You have already applied leave for the same Date. Please remove the conflicting leave before applying again.";
        //            // return RedirectToActionPermanent(
        //            //Constants.Index,
        //            // WebControllarsConstants.MyInfo);
        //            return Json(new { isValid = false, message = message });
        //        }
        //        double JoiningDays = 0;
        //        if (joindate.HasValue)
        //        {
        //            TimeSpan? difference = DateTime.Today - joindate.Value;

        //            if (difference.HasValue)
        //            {
        //                JoiningDays = difference.Value.TotalDays;
        //            }
        //        }



        //        //  Maternity Leave
        //        if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.MaternityLeave)
        //        {
        //            if (model.leaveResults.leaveSummaryModel.ExpectedDeliveryDate > endDate)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "End date must be greater than or equal to Expected Delivery Date.";

        //                return Json(new { isValid = false, message = message });

        //            }

        //            //Validation for applicable after working days
        //            if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "You can't apply leave(s) before " + leavePolicyModel.Maternity_ApplicableAfterWorkingDays + " days of joining";
        //                return Json(new { isValid = false, message = message });
        //            }



        //            var maxLeave = leavePolicyModel.Maternity_MaximumLeaveAllocationAllowed;
        //            var maxLeaveDays = maxLeave * 7;

        //            // Check if the leave duration exceeds the maximum allowed leave
        //            if (totalDays > maxLeaveDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "Leave duration exceeds the maximum allowed of " + maxLeaveDays + " days.";

        //                return Json(new { isValid = false, message = message });
        //            }




        //        }

        //        // Miscarriage Leave
        //        if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.Miscarriage)
        //        {

        //            var maxLeave = leavePolicyModel.Miscarriage_MaximumLeaveAllocationAllowed;
        //            var maxLeaveDays = maxLeave * 7; // Convert weeks to days
        //                                             // Check if the leave duration exceeds the maximum allowed leave
        //            if (totalDays > maxLeaveDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                TempData[HRMS.Models.Common.Constants.toastMessage] = "Leave duration exceeds the maximum allowed of " + maxLeaveDays + " days.";
        //                return RedirectToActionPermanent(
        //           Constants.Index,
        //            WebControllarsConstants.MyInfo);
        //            }

        //            //Validation for applicable after working days
        //            if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                TempData[HRMS.Models.Common.Constants.toastMessage] = "You can't apply leave(s) before " + leavePolicyModel.Maternity_ApplicableAfterWorkingDays + " days of joining";
        //                return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
        //            }
        //        }

        //        // Adoption Leave
        //        if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.Adoption)
        //        {
        //            // Calculate the leave duration
        //            // Get the maximum allowed leave in days
        //            var maxLeave = leavePolicyModel.Adoption_MaximumLeaveAllocationAllowed;
        //            var maxLeaveDays = maxLeave * 7;

        //            if (totalDays > maxLeaveDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "Leave duration exceeds the maximum allowed of " + maxLeaveDays + " days.";
        //                  return Json(new { isValid = false, message = message });
        //            }

        //            //Validation for applicable after working days
        //            if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "You can't apply leave(s) before " + leavePolicyModel.Maternity_ApplicableAfterWorkingDays + " days of joining";
        //                //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
        //                return Json(new { isValid = false, message = message });
        //            }
        //        }
        //        var Holidaylist = new List<HolidayModel>();
        //        if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.AnnualLeavel || model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.Paternity || model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.MedicalLeave)
        //        {
        //            //Get Holiday list
        //            var holidaydata = GetHolidayData(model.leaveResults.leaveSummaryModel.CompanyID);
        //            var holidaydataList = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(holidaydata);
        //            Holidaylist = holidaydataList.Holiday.ToList();
        //        }
        //        //Paternity Leave
        //        if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.Paternity)
        //        {
        //            var maxPaternityLeave = leavePolicyModel.Paternity_applicableAfterWorkingMonth;
        //            var maxPaternityDays = maxPaternityLeave * 30;

        //            //Validation for applicable after working days
        //            if (maxPaternityDays > JoiningDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "You can't apply leave(s) before " + maxPaternityDays + " days of joining";
        //                // return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
        //                return Json(new { isValid = false, message = message });
        //            }
        //            //Validation for Childdate must be less than or equal of child date
        //            if (model.leaveResults.leaveSummaryModel.ChildDOB.Date > model.leaveResults.leaveSummaryModel.EndDate.Date)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "Child DOB must be less than or Equal to End Date.";
        //                //     return RedirectToActionPermanent(
        //                //Constants.Index,
        //                // WebControllarsConstants.MyInfo);
        //                return Json(new { isValid = false, message = message });
        //            }

        //            //Validation for taking leave within 3 months
        //            var ChildDate = model.leaveResults.leaveSummaryModel.ChildDOB.AddMonths(3);
        //            if (ChildDate.Date <= model.leaveResults.leaveSummaryModel.StartDate.Date)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "Paternity leave must be taken within 3 months after delivery or miscarriage.";
        //                //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
        //                return Json(new { isValid = false, message = message });
        //            }



        //            // Validation for not including Weekend Days and Holidays
        //            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        //            {
        //                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        //                bool isHoliday = Holidaylist.Any(h => date >= h.FromDate.Date && date <= h.ToDate.Date);

        //                // Exclude only once if it's either a weekend or a holiday (or both)
        //                if (isWeekend || isHoliday)
        //                {
        //                    weekendDays++;
        //                }
        //            }
        //            model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays - weekendDays;
        //            if (model.leaveResults.leaveSummaryModel.NoOfDays <= 0)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "There will be a holiday on the selected day.";
        //                //    return RedirectToActionPermanent(
        //                //Constants.Index,
        //                // WebControllarsConstants.MyInfo);

        //                return Json(new { isValid = false, message = message });
        //            }
        //            // Get the maximum allowed leave in days
        //            var maxLeave = leavePolicyModel.Paternity_maximumLeaveAllocationAllowed;
        //            var totalNoDays = model.leaveResults.leaveSummaryModel.NoOfDays;
        //            if (totalNoDays > maxLeave)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "Leave duration exceeds the maximum allowed of " + maxLeave + " days.";
        //                //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
        //                return Json(new { isValid = false, message = message });
        //            }
        //        }

        //        //    Annual Leave
        //        if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.AnnualLeavel || model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.MedicalLeave)
        //        {

        //            // Validation to prevent applying for leave on April 1, 2, and 3 of any year


        //            // Validation for not including Weekend Days and Holidays
        //            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        //            {
        //                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        //                bool isHoliday = Holidaylist.Any(h => date >= h.FromDate.Date && date <= h.ToDate.Date);

        //                // Exclude only once if it's either a weekend or a holiday (or both)
        //                if (isWeekend || isHoliday)
        //                {
        //                    weekendDays++;
        //                }
        //            }

        //            model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays - weekendDays;
        //            if (model.leaveResults.leaveSummaryModel.NoOfDays <= 0)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "There will be a holiday on the selected day.";

        //                return Json(new { isValid = false, message = message });
        //            }


        //            //validation for applicable after working days
        //            if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > JoiningDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "You can't apply leave(s) before " + leavePolicyModel.Annual_ApplicableAfterWorkingDays + " days of joining";

        //                return Json(new { isValid = false, message = message });
        //            }
        //            double accruedLeave1 = CalculateAccruedLeaveForCurrentFiscalYear(joindate.Value, leavePolicyModel.Annual_MaximumLeaveAllocationAllowed);
        //            DateTime today = DateTime.Today;
        //            DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);

        //            var TotalApproveList = leaveSummaryDataResult.Where(x => x.StartDate >= fiscalYearStart && x.LeaveStatusID == (int)LeaveStatus.Approved && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave)).OrderBy(x => x.LeaveSummaryID).ToList();
        //            var TotalApproveLists = TotalApproveList.Sum(x => x.NoOfDays);
        //            double TotalApprove = Convert.ToDouble(TotalApproveLists);
        //            var accruedLeaves = accruedLeave1 - TotalApprove;
        //            //validation for annual leave balance
        //            if (Convert.ToDouble(model.leaveResults.leaveSummaryModel.NoOfDays) > accruedLeaves)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "Leave duration exceeds the annual leave balance of " + accruedLeaves + " days";


        //                return Json(new { isValid = false, message = message });
        //            }

        //        }

        //        //    Comp Off Leavel
        //        if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.CompOff)
        //        {


        //            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        //            {
        //                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        //                bool isHoliday = Holidaylist.Any(h => date >= h.FromDate.Date && date <= h.ToDate.Date);

        //                // Exclude only once if it's either a weekend or a holiday (or both)
        //                if (isWeekend || isHoliday)
        //                {
        //                    weekendDays++;
        //                }
        //            }

        //            model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays - weekendDays;
        //            if (model.leaveResults.leaveSummaryModel.NoOfDays <= 0)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "There will be a holiday on the selected day.";
        //                //    return RedirectToActionPermanent(
        //                //Constants.Index,
        //                // WebControllarsConstants.MyInfo);

        //                return Json(new { isValid = false, message = message });
        //            }
        //            if (model.CampOffLeaveCount < totalDays)
        //            {

        //                var message = "There will be a 123.";
        //                return Json(new { isValid = false, message = message });
        //            }

        //                //validation for maximum medical leave allowed
        //                var totalJoiningDays = (DateTime.Today - employeeDetails.InsertedDate).TotalDays;
        //            if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > totalJoiningDays)
        //            {
        //                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //                var message = "You can't apply leave(s) before " + leavePolicyModel.Annual_ApplicableAfterWorkingDays + " days of joining";
        //                //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
        //                return Json(new { isValid = false, message = message });
        //            }
        //        }

        //        _s3Service.ProcessFileUpload(postedFiles, model.leaveResults.leaveSummaryModel.UploadCertificate, out string newCertificateKey);

        //        if (!string.IsNullOrEmpty(newCertificateKey))
        //        {
        //            if (!string.IsNullOrEmpty(model.leaveResults.leaveSummaryModel.UploadCertificate))
        //            {
        //                _s3Service.DeleteFile(model.leaveResults.leaveSummaryModel.UploadCertificate);
        //            }
        //            model.leaveResults.leaveSummaryModel.UploadCertificate = newCertificateKey;
        //        }
        //        else
        //        {
        //            model.leaveResults.leaveSummaryModel.UploadCertificate = _s3Service.ExtractKeyFromUrl(model.leaveResults.leaveSummaryModel.UploadCertificate);
        //        }
        //        var data = _businessLayer.SendPostAPIRequest(model.leaveResults.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

        //        var result = JsonConvert.DeserializeObject<Result>(data);
        //        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
        //        var messageData = "Leave applied successfully.";
        //        return Json(new { isValid = true, message = messageData });
        //    }
        //    return View(model);
        //}

        public IActionResult Index(MyInfoResults model, List<IFormFile> postedFiles)
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
            leaveSummary.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
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
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.AnnualLeavel
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.MedicalLeave
                                           || leaveSummary.LeaveTypeID == (int)LeaveType.CompOff;

            if (needsWeekendHolidayCount)
            {
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    bool isHoliday = Holidaylist.Any(h => date >= h.FromDate.Date && date <= h.ToDate.Date);
                    if (isWeekend || isHoliday)
                        weekendDays++;
                }
                leaveSummary.NoOfDays -= weekendDays;
                if (leaveSummary.NoOfDays <= 0)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    return Json(new { isValid = false, message = "There will be a holiday on the selected day." });
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
                        return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
                    }

                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = $"You can't apply leave(s) before {leavePolicyModel.Maternity_ApplicableAfterWorkingDays} days of joining";
                        return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
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

                    if (leaveSummary.ChildDOB.Date > leaveSummary.EndDate.Date)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = "Child DOB must be less than or Equal to End Date." });
                    }

                    if (leaveSummary.ChildDOB.AddMonths(3).Date <= leaveSummary.StartDate.Date)
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
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {leavePolicyModel.Annual_ApplicableAfterWorkingDays} days of joining" });
                    }

                    double accruedLeave1 = CalculateAccruedLeaveForCurrentFiscalYear(joinDate.Value, leavePolicyModel.Annual_MaximumLeaveAllocationAllowed);
                    DateTime today = DateTime.Today;
                    DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);

                    var approvedLeaves = leaveSummaryDataResult
                        .Where(x => x.StartDate >= fiscalYearStart && x.LeaveStatusID == (int)LeaveStatus.Approved
                                    && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                        .Sum(x => x.NoOfDays);

                    decimal accruedLeaves = Convert.ToDecimal(accruedLeave1) - approvedLeaves;

                    if (leaveSummary.NoOfDays > accruedLeaves)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"Leave duration exceeds the annual leave balance of {accruedLeaves} days" });
                    }
                    break;

                case LeaveType.CompOff:
                    if (model.CampOffLeaveCount < leaveSummary.NoOfDays)
                    {
                        var Message = "Exceeds maximum Compensatory Off allowed " + model.CampOffLeaveCount + ' ' + "days leaves";
                        return Json(new { isValid = false, message = Message });
                    }
                    var totalJoiningDays = (DateTime.Today - employeeDetails.InsertedDate).TotalDays;
                    if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > totalJoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        return Json(new { isValid = false, message = $"You can't apply leave(s) before {leavePolicyModel.Annual_ApplicableAfterWorkingDays} days of joining" });
                    }
                    CampOffEligible modeldata = new CampOffEligible();
                    modeldata.EmployeeID = leaveSummary.EmployeeID;
                    modeldata.EmployeeNumber = Convert.ToString(_context.HttpContext.Session.GetString(Constants.EmployeeNumberWithoutAbbr)); ;
                    modeldata.StartDate = model.leaveResults.leaveSummaryModel.StartDate.ToString("yyyy-MM-dd");
                    modeldata.EndDate = model.leaveResults.leaveSummaryModel.EndDate.ToString("yyyy-MM-dd");
                    var campoffdata = GetValidateCompOffLeave(modeldata);
                    if(campoffdata !=null)
                    {
                        if(campoffdata.IsEligible==0)
                        {
                            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                            return Json(new { isValid = false, message = campoffdata.Message });
                        }
                        else
                        {
                            model.leaveResults.leaveSummaryModel.CampOff =(int)CompOff.CompOff;
                        }

                    }

                    
                    break;

                default:
                    // Handle other leave types if necessary or no validation
                    break;
            }

            // Process file upload
            _s3Service.ProcessFileUpload(postedFiles, leaveSummary.UploadCertificate, out string newCertificateKey);

            if (!string.IsNullOrEmpty(newCertificateKey))
            {
                if (!string.IsNullOrEmpty(leaveSummary.UploadCertificate))
                {
                    _s3Service.DeleteFile(leaveSummary.UploadCertificate);
                }
                leaveSummary.UploadCertificate = newCertificateKey;
            }

            // Finalize leave application
            var data = _businessLayer.SendPostAPIRequest(model.leaveResults.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

            var result = JsonConvert.DeserializeObject<Result>(data);
            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
            var messageData = "Leave applied successfully.";
            return Json(new { isValid = true, message = messageData });

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
                CompanyID = companyID
            };

            var leaveSummaryData = _businessLayer.SendPostAPIRequest(myInfoInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetlLeavesSummary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            return leaveSummaryData;
        }

        private string GetHolidayData(long companyID)
        {
            HolidayInputParams myInfoInputParams = new HolidayInputParams
            {
                CompanyID = companyID
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
            // Define financial year start and end based on the current date
            DateTime today = DateTime.Today;
            //DateTime today = new DateTime(2024, 6, 14);
            // DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1); // Start from April 1st of current financial year
            DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 3, 31); // Start from 20th march of current financial year
            DateTime fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-11); // March 31st of the next year

            // Annual entitlement and accrual per month

            double annualLeaveEntitlement = Annual_MaximumLeaveAllocationAllowed;
            double monthlyAccrual = annualLeaveEntitlement / 12;

            double totalAccruedLeave = 0;

            // If the join date is before the fiscal year start, adjust it to the start of the fiscal year
            if (joinDate < fiscalYearStart)
            {
                joinDate = fiscalYearStart;
            }

            // Start from the month of joining and calculate leave for completed months up to today
            DateTime current = new DateTime(joinDate.Year, joinDate.Month, 1);

            while (current <= today)
            {
                // Get the last day of the current month
                int daysInMonth = DateTime.DaysInMonth(current.Year, current.Month);
                DateTime lastDayOfMonth = new DateTime(current.Year, current.Month, daysInMonth).AddDays(-11);

                // Adjust the comparison in the current month
                if (current.Month == today.Month && current.Year == today.Year)
                {
                    // Special case: Compare the days worked in the current month up to today
                    int daysWorkedInMonth = (today - joinDate).Days + 1;

                    // Accrue leave if more than 10 days worked
                    if (daysWorkedInMonth > Convert.ToInt32(_configuration["DaysWorkedInMonth:DaysWorkedInMonth"]))
                    {
                        totalAccruedLeave += monthlyAccrual;
                    }


                }
                else
                {
                    // For past months, compare the join date with the last day of the month
                    if (joinDate <= lastDayOfMonth)
                    {
                        int daysWorkedInMonth = (lastDayOfMonth - joinDate).Days + 1;
                        if (daysWorkedInMonth > Convert.ToInt32(_configuration["DaysWorkedInMonth:DaysWorkedInMonth"]))
                        {
                            totalAccruedLeave += monthlyAccrual;
                        }
                    }
                }

                // Move to the next month
                current = current.AddMonths(1);

                // Adjust join date to the 1st of the next month after the first iteration
                if (current.Month > joinDate.Month || current.Year > joinDate.Year)
                {
                    joinDate = new DateTime(current.Year, current.Month, 1);
                }
            }

            return totalAccruedLeave;
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
                      Constants.Index,
                       WebControllarsConstants.MyInfo);
        }
        [HttpPost]
        public IActionResult UpdateLeaveStatus(string id)
        {
            UpdateLeaveStatus model = new UpdateLeaveStatus()
            {
                LeaveSummaryID = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt64(_businessLayer.DecodeStringBase64(id)),
                EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID)),
                NewLeaveStatusID =(int)LeaveStatus.Cancelled,
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.UpdateLeaveStatus), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                var results = JsonConvert.DeserializeObject<UpdateLeaveStatus>(data);

                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = results.Message;
            }
            return RedirectToActionPermanent(
                      Constants.Index,
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

        [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Manager + "," + RoleConstants.SuperAdmin))]

        public IActionResult GetTeamEmployeeList()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetTeamEmployeeList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            try
            {
                List<EmployeeDetails> employeeDetails = new List<EmployeeDetails>();
                EmployeeInputParams model = new EmployeeInputParams();
                model.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                model.RoleID = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmployeeListByManagerID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                employeeDetails = JsonConvert.DeserializeObject<List<EmployeeDetails>>(data);


                if (employeeDetails == null || employeeDetails.Count == 0)
                {
                    return Json(new { data = new List<object>(), message = "No employees found" });
                }
                else
                {
                    employeeDetails.ForEach(x =>
                    {
                        x.EmployeePhoto = string.IsNullOrEmpty(x.EmployeePhoto)
                            ? "/assets/img/No_image.png"   //Use default image if profile photo is missing
                            : _s3Service.GetFileUrl(x.EmployeePhoto);
                    });
                }
                return Json(new { data = employeeDetails });
            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred", details = ex.Message });
            }
        }




        [HttpGet]
        public IActionResult Support()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PolicyCategoryDetails()
        {
            PolicyCategoryInputParams model = new PolicyCategoryInputParams();
            model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.PolicyCategoryDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var detailsList = JsonConvert.DeserializeObject<List<LeavePolicyDetailsModel>>(data);


            if (detailsList != null)
            {
                foreach (var item in detailsList)
                {
                    if (!string.IsNullOrEmpty(item.PolicyDocument))
                    {
                        item.PolicyDocument = _s3Service.GetFileUrl(item.PolicyDocument);
                    }
                }
            }
            return View(detailsList);
        }
        [HttpGet]
        [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Manager + "," + RoleConstants.SuperAdmin))]
        public IActionResult TeamAttendenceList()
        {
            var firstName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
            var middleName = Convert.ToString(HttpContext.Session.GetString(Constants.MiddleName)); // Assuming this exists
            var lastName = Convert.ToString(HttpContext.Session.GetString(Constants.Surname)); // Assuming this exists
            ViewBag.EmployeeName = $"{firstName} {middleName} {lastName}".Trim();
            return View();
        }
        [HttpGet]
        public IActionResult TeamAttendenceCalendarList(int year, int month, int Page, int PageSize)
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
            };

            var data = _businessLayer.SendPostAPIRequest(models, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidaysVM>(data);
            model.Attendances.ForEach(x =>
            {
                x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.EmployeeId.ToString());
                x.EmployeeNumberWithoutAbbr = _businessLayer.EncodeStringBase64(x.EmployeeNumberWithoutAbbr.ToString());
            });
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
        public IActionResult ExportAttendance(int Year, int Month)
        {
            try
            {
                var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                var roleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));

                var models = new AttendanceInputParams
                {
                    Year = Year,
                    Month = Month,
                    UserId = employeeId,
                    RoleId = roleId,
                    PageSize = 100000,
                    Page = 1
                };

                var response = _businessLayer.SendPostAPIRequest(
                    models,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var model = JsonConvert.DeserializeObject<AttendanceWithHolidaysVM>(response);
                if (model == null || model.Attendances == null || model.Attendances.Count == 0)
                {
                    return NotFound("No attendance data found.");
                }

                int daysInMonth = DateTime.DaysInMonth(Year, Month);

                // Prepare day keys (e.g. "01_Thu")
                var dayKeys = new List<string>();
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(Year, Month, day);
                    var dayKey = date.ToString("dd_ddd");
                    dayKeys.Add(dayKey);
                }

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Attendance");

                    // Build header row
                    var headers = new List<string> { "EmployeNumber", "EmployeeName" };
                    headers.AddRange(dayKeys.Select(k => k.Replace("_", " ")));
                    headers.Add("TotalWorkingDays");
                    headers.Add("PresentDays");

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

                        rowIndex++;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var excelBytes = package.GetAsByteArray();

                    return File(
                        excelBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Attendance_{Year}_{Month}.xlsx"
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
    }
}
