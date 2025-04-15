using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using HRMS.Models;
using HRMS.Models.Common;
<<<<<<< HEAD
using HRMS.Models.DashBoard;
=======
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
using HRMS.Models.MyInfo;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
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
        public MyInfoController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IHttpContextAccessor context)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            _context = context;
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
<<<<<<< HEAD
            DateTime today = DateTime.Today;

            DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);

            var TotalApproveList = results.leaveResults.leavesSummary.Where(x => x.StartDate >= fiscalYearStart && x.LeaveStatusID == (int)LeaveStatus.Approved && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave)).OrderBy(x => x.LeaveSummaryID).ToList();

            var employeeDetails = GetEmployeeDetails(model.CompanyID, model.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(model.CompanyID, employeeDetails.LeavePolicyID ?? 0);
            //var joindate = employeeDetails.EmploymentDetail.Select(x => x.JoiningDate).FirstOrDefault();
            var joindate = results.employmentDetail.JoiningDate;
            DateTime joinDate1 = joindate.Value;
            //results.JoiningDate = joinDate1;
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
=======
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
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
            DateTime today = DateTime.Today;
            DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);
            var leavePolicyModel = GetLeavePolicyData(model.CompanyID, employeeDetails.LeavePolicyID ?? 0);
            ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);
            return Json(results);
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
            var employeeDetails = GetEmployeeDetails(employee.CompanyID, employee.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(employee.CompanyID, employeeDetails.LeavePolicyID ?? 0);

            var Approvals = results.leavesSummary.Where(x => x.LeaveStatusID != (int)LeaveStatus.Approved && x.LeaveStatusID != (int)LeaveStatus.NotApproved).ToList();
            ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);
            if (leavePolicyModel.Paternity_medicalDocument == true)
            {
                ViewBag.Paternity_medicalDocument = true;
            }

            return Json(new { data = Approvals });
        }




        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetLeaveForApproved(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            MyInfoInputParams employee = new MyInfoInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);
            var Approvals = results.leavesSummary.Where(x => x.LeaveStatusID == (int)LeaveStatus.Approved).ToList();
            var employeeDetails = GetEmployeeDetails(employee.CompanyID, employee.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(employee.CompanyID, employeeDetails.LeavePolicyID ?? 0);

            ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);

            return Json(new { data = Approvals });
        }

        [HttpPost]
        [AllowAnonymous]
<<<<<<< HEAD
        public JsonResult GetLeaveForReject(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            MyInfoInputParams employee = new MyInfoInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLeaveForApprovals), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<LeaveResults>(data);

            var Approvals = results.leavesSummary.Where(x => x.LeaveStatusID == (int)LeaveStatus.NotApproved).ToList();

            var employeeDetails = GetEmployeeDetails(employee.CompanyID, employee.EmployeeID);
            var leavePolicyModel = GetLeavePolicyData(employee.CompanyID, employeeDetails.LeavePolicyID ?? 0);

            ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed);

            return Json(new { data = Approvals });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ApproveRejectLeave(long leaveSummaryID, bool isApproved, string ApproveRejectComment)
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
                //Paternity Leave
                //if (rowData.LeaveTypeID == (int)LeaveType.Paternity)
                //{
                //    if (leavePolicyModel.Paternity_medicalDocument == true)
                //    {
                //        if (rowData.UploadCertificate == "")
                //        {
                //            var Message = "Please Provide Paternity Medical Document ";
                //            obj.status = 1;
                //            obj.message = Message;
                //            return Json(new { data = obj });
                //        }
                //    }

                //}
                //Adoption Leave
                //if (rowData.LeaveTypeID == (int)LeaveType.Adoption)
                //{
                //    if (leavePolicyModel.Adoption_MedicalDocument == true)
                //    {
                //        if (rowData.UploadCertificate == "")
                //        {
                //            var Message = "Please Provide Adoption Medical Document ";
                //            obj.status = 1;
                //            obj.message = Message;
                //            return Json(new { data = obj });
                //        }
                //    }

                //}
                //Adoption Leave
                //if (rowData.LeaveTypeID == (int)LeaveType.Miscarriage)
                //{
                //    if (leavePolicyModel.Miscarriage_MedicalDocument == true)
                //    {
                //        if (rowData.UploadCertificate == "")
                //        {
                //            var Message = "Please Provide Miscarriage Medical Document ";
                //            obj.status = 1;
                //            obj.message = Message;
                //            return Json(new { data = obj });
                //        }
                //    }

                //}


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
                    // validation for maximum consecutive leave
                    //if (rowData.NoOfDays > leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed)
                    //{
                    //    var Message = "You can't approve leave(s) more than " + leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed + " consecutive leaves";
                    //    obj.status = 1;
                    //    obj.message = Message;
                    //    return Json(new { data = obj });
                    //}



                }



                //  Maternity Leave
                if (rowData.LeaveTypeID == (int)LeaveType.MaternityLeave)
                {

                    // Expected Delivery Date must be within a valid range(weeks before expected delivery)
                    //var weeksBefore = leavePolicyModel.Maternity_ApplyBeforeHowManyDays;
                    //var daysBefore = weeksBefore * 7; // Convert weeks to days
                    //var expectedDeliveryDate = rowData.ExpectedDeliveryDate;
                    //int totalDeliveryDays = (expectedDeliveryDate - rowData.StartDate).Days + 1;
                    //// Check if the start date is too early
                    //if (totalDeliveryDays > daysBefore)
                    //{
                    //    var Message =
                    //        "Maternity leave cannot start more than " + daysBefore + " days before the expected delivery date.";
                    //    obj.status = 1;
                    //    obj.message = Message;
                    //    return Json(new { data = obj });
                    //}







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
                //  emailSendResponse response = EmailSender.SendEmail(sendEmailProperties);


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
        public IActionResult Index(MyInfoResults model, List<IFormFile> postedFiles)
        {


            var startDate = model.leaveResults.leaveSummaryModel.StartDate;

            var endDate = model.leaveResults.leaveSummaryModel.EndDate;
            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                endDate = startDate;
            }


            //get current year date
            var currentYearDate = GetAprilFirstDate();
            //validation for fromdate should not greater than todate
            if ((int)LeaveDay.HalfDay != model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                if (startDate > endDate)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    var message = "End date must be greater than or equal to start date.";
                    //return RedirectToActionPermanent(
                    //   Constants.Index,
                    //    WebControllarsConstants.MyInfo);

                    return Json(new { isValid = false, message = message });
                }
            }

            // Initialize totalDays and weekendDays
            int totalDays = (endDate - startDate).Days + 1;
            int weekendDays = 0;

            // Continue with further processing...
            model.leaveResults.leaveSummaryModel.NoOfDays = totalDays;
            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                model.leaveResults.leaveSummaryModel.StartDate = model.leaveResults.leaveSummaryModel.EndDate = model.leaveResults.leaveSummaryModel.StartDate;
                model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays / 2;
            }
            else
            {
                model.leaveResults.leaveSummaryModel.LeaveDurationTypeID = (int)LeaveDay.FullDay;
            }
=======
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
        public JsonResult ApproveRejectLeave(long leaveSummaryID, bool isApproved, string ApproveRejectComment = "")
        {
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
            var LeaveResultsdata = _businessLayer.SendPostAPIRequest(rowData, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var resultsLeaveResultsdata = JsonConvert.DeserializeObject<Result>(LeaveResultsdata);
            if (resultsLeaveResultsdata != null && resultsLeaveResultsdata.PKNo > 0)
            {
                sendEmailProperties sendEmailProperties = new sendEmailProperties();
               
                if (isApproved)
                {
                    sendEmailProperties.emailSubject = "Leave Approve Nofification";
                    sendEmailProperties.emailBody = ("Hi " + rowData.EmployeeFirstName + ", <br/><br/> Your " + rowData.LeaveTypeName + " leave ( from " + rowData.StartDate.ToString("MMMM dd, yyyy") + " to " + rowData.EndDate.ToString("MMMM dd, yyyy") + ") have been approved." + "<br/><br/>");
                }
                else
                {
                    sendEmailProperties.emailSubject = "Leave Reject Nofification";
                    sendEmailProperties.emailBody = ("Hi " + rowData.EmployeeFirstName + ", <br/><br/> Your " + rowData.LeaveTypeName + " leave ( from " + rowData.StartDate.ToString("MMMM dd, yyyy") + " to " + rowData.EndDate.ToString("MMMM dd, yyyy") + ") have been Rejected." + "<br/><br/>");
                }
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

>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
            if (model.leaveResults.leaveSummaryModel.NoOfDays > 0)
            {
                model.leaveResults.leaveSummaryModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.EmployeeID));
                model.leaveResults.leaveSummaryModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
                model.leaveResults.leaveSummaryModel.CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));

<<<<<<< HEAD
                // Get employee details   
                var employeeDetails = GetEmployeeDetails(model.leaveResults.leaveSummaryModel.CompanyID, model.leaveResults.leaveSummaryModel.EmployeeID);
                model.leaveResults.leaveSummaryModel.LeavePolicyID = employeeDetails.LeavePolicyID ?? 0;
                model.leaveResults.leaveSummaryModel.LeaveStatusID = (int)LeaveStatus.PendingApproval;
                model.leaveResults.leaveSummaryModel.EmployeeID = employeeDetails.EmployeeID;
                var joindate = employeeDetails.JoiningDate;



                // Get leave policy data  
                var leavePolicyModel = GetLeavePolicyData(model.leaveResults.leaveSummaryModel.CompanyID, model.leaveResults.leaveSummaryModel.LeavePolicyID);

                //Get leave summary data  
                var leaveSummaryData = GetLeaveSummaryData(model.leaveResults.leaveSummaryModel.EmployeeID, model.leaveResults.leaveSummaryModel.UserID, model.leaveResults.leaveSummaryModel.CompanyID);
                var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
                var EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                var isAlreadyTakenLeave = ValidateAlreadyTakenLeaves(model, leaveSummaryData, EmployeeID);
                if (isAlreadyTakenLeave)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    var message = "You have already applied leave for the same Date. Please remove the conflicting leave before applying again.";
                    // return RedirectToActionPermanent(
                    //Constants.Index,
                    // WebControllarsConstants.MyInfo);
                    return Json(new { isValid = false, message = message });
                }


                double JoiningDays = 0;
                if (joindate.HasValue)
                {
                    TimeSpan? difference = DateTime.Today - joindate.Value;

                    if (difference.HasValue)
                    {
                        JoiningDays = difference.Value.TotalDays;
                    }
                }





                //  Maternity Leave
                if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.MaternityLeave)
                {
                    if (model.leaveResults.leaveSummaryModel.ExpectedDeliveryDate > endDate)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "End date must be greater than or equal to Expected Delivery Date.";

                        return Json(new { isValid = false, message = message });

                    }

                    //Validation for applicable after working days
                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "You can't apply leave(s) before " + leavePolicyModel.Maternity_ApplicableAfterWorkingDays + " days of joining";
                        return Json(new { isValid = false, message = message });
                    }



                    var maxLeave = leavePolicyModel.Maternity_MaximumLeaveAllocationAllowed;
                    var maxLeaveDays = maxLeave * 7;

                    // Check if the leave duration exceeds the maximum allowed leave
                    if (totalDays > maxLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "Leave duration exceeds the maximum allowed of " + maxLeaveDays + " days.";
                        //    return RedirectToActionPermanent(
                        //Constants.Index,
                        // WebControllarsConstants.MyInfo);

                        return Json(new { isValid = false, message = message });
                    }




                }

                // Miscarriage Leave
                if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.Miscarriage)
                {

                    // Calculate the leave duration
                    // Get the maximum allowed leave in days
                    var maxLeave = leavePolicyModel.Miscarriage_MaximumLeaveAllocationAllowed;
                    var maxLeaveDays = maxLeave * 7; // Convert weeks to days
                                                     // Check if the leave duration exceeds the maximum allowed leave
                    if (totalDays > maxLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = "Leave duration exceeds the maximum allowed of " + maxLeaveDays + " days.";
                        return RedirectToActionPermanent(
                   Constants.Index,
                    WebControllarsConstants.MyInfo);
                    }

                    //Validation for applicable after working days
                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = "You can't apply leave(s) before " + leavePolicyModel.Maternity_ApplicableAfterWorkingDays + " days of joining";
                        return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
                    }
                }

                // Adoption Leave
                if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.Adoption)
                {
                    // Calculate the leave duration
                    // Get the maximum allowed leave in days
                    var maxLeave = leavePolicyModel.Adoption_MaximumLeaveAllocationAllowed;
                    var maxLeaveDays = maxLeave * 7;

                    if (totalDays > maxLeaveDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "Leave duration exceeds the maximum allowed of " + maxLeaveDays + " days.";
                        //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);

                        return Json(new { isValid = false, message = message });
                    }

                    //Validation for applicable after working days
                    if (leavePolicyModel.Maternity_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "You can't apply leave(s) before " + leavePolicyModel.Maternity_ApplicableAfterWorkingDays + " days of joining";
                        //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
                        return Json(new { isValid = false, message = message });
                    }
                }


                // Get Holiday list for Annual & Paternity
                var Holidaylist = new List<HolidayModel>();
                if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.AnnualLeavel || model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.Paternity || model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.MedicalLeave)
                {
                    //Get Holiday list
                    var holidaydata = GetHolidayData(model.leaveResults.leaveSummaryModel.CompanyID);
                    var holidaydataList = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(holidaydata);
                    Holidaylist = holidaydataList.Holiday.ToList();
                }


                //Paternity Leave
                if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.Paternity)
                {
                    var maxPaternityLeave = leavePolicyModel.Paternity_applicableAfterWorkingMonth;
                    var maxPaternityDays = maxPaternityLeave * 30;

                    //Validation for applicable after working days
                    if (maxPaternityDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "You can't apply leave(s) before " + maxPaternityDays + " days of joining";
                        // return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
                        return Json(new { isValid = false, message = message });
                    }
                    //Validation for Childdate must be less than or equal of child date
                    if (model.leaveResults.leaveSummaryModel.ChildDOB.Date > model.leaveResults.leaveSummaryModel.EndDate.Date)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "Child DOB must be less than or Equal to End Date.";
                        //     return RedirectToActionPermanent(
                        //Constants.Index,
                        // WebControllarsConstants.MyInfo);
                        return Json(new { isValid = false, message = message });
                    }

                    //Validation for taking leave within 3 months
                    var ChildDate = model.leaveResults.leaveSummaryModel.ChildDOB.AddMonths(3);
                    if (ChildDate.Date <= model.leaveResults.leaveSummaryModel.StartDate.Date)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "Paternity leave must be taken within 3 months after delivery or miscarriage.";
                        //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
                        return Json(new { isValid = false, message = message });
                    }



                    // Validation for not including Weekend Days and Holidays
                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                        bool isHoliday = Holidaylist.Any(h => date >= h.FromDate.Date && date <= h.ToDate.Date);

                        // Exclude only once if it's either a weekend or a holiday (or both)
                        if (isWeekend || isHoliday)
                        {
                            weekendDays++;
                        }
                    }
                    model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays - weekendDays;
                    if (model.leaveResults.leaveSummaryModel.NoOfDays <= 0)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "There will be a holiday on the selected day.";
                        //    return RedirectToActionPermanent(
                        //Constants.Index,
                        // WebControllarsConstants.MyInfo);

                        return Json(new { isValid = false, message = message });
                    }
                    // Get the maximum allowed leave in days
                    var maxLeave = leavePolicyModel.Paternity_maximumLeaveAllocationAllowed;
                    var totalNoDays = model.leaveResults.leaveSummaryModel.NoOfDays;
                    if (totalNoDays > maxLeave)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "Leave duration exceeds the maximum allowed of " + maxLeave + " days.";
                        //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
                        return Json(new { isValid = false, message = message });
                    }
                }

                //    Annual Leave
                if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.AnnualLeavel || model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.MedicalLeave)
                {

                    // Validation to prevent applying for leave on April 1, 2, and 3 of any year
                    if ((startDate.Month == 4 && (startDate.Day == 1 || startDate.Day == 2 || startDate.Day == 3)) ||
                        (endDate.Month == 4 && (endDate.Day == 1 || endDate.Day == 2 || endDate.Day == 3)))
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "You cannot apply for leave on April 1, 2, or 3.";
                        return Json(new { isValid = false, message = message });
                    }

                    // Validation for not including Weekend Days and Holidays
                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                        bool isHoliday = Holidaylist.Any(h => date >= h.FromDate.Date && date <= h.ToDate.Date);

                        // Exclude only once if it's either a weekend or a holiday (or both)
                        if (isWeekend || isHoliday)
                        {
                            weekendDays++;
                        }
                    }

                    model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays - weekendDays;
                    if (model.leaveResults.leaveSummaryModel.NoOfDays <= 0)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "There will be a holiday on the selected day.";
                        //    return RedirectToActionPermanent(
                        //Constants.Index,
                        // WebControllarsConstants.MyInfo);

                        return Json(new { isValid = false, message = message });
                    }


                    //validation for applicable after working days
                    if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > JoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "You can't apply leave(s) before " + leavePolicyModel.Annual_ApplicableAfterWorkingDays + " days of joining";

                        return Json(new { isValid = false, message = message });
                    }
                    double accruedLeave1 = CalculateAccruedLeaveForCurrentFiscalYear(joindate.Value, leavePolicyModel.Annual_MaximumLeaveAllocationAllowed);
                    DateTime today = DateTime.Today;
                    DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);

                    var TotalApproveList = leaveSummaryDataResult.Where(x => x.StartDate >= fiscalYearStart && x.LeaveStatusID == (int)LeaveStatus.Approved && (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave)).OrderBy(x => x.LeaveSummaryID).ToList();
                    var TotalApproveLists = TotalApproveList.Sum(x => x.NoOfDays);
                    double TotalApprove = Convert.ToDouble(TotalApproveLists);
                    var accruedLeaves = accruedLeave1 - TotalApprove;
                    //validation for annual leave balance
                    if (Convert.ToDouble(model.leaveResults.leaveSummaryModel.NoOfDays) > accruedLeaves)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "Leave duration exceeds the annual leave balance of " + accruedLeaves + " days";


                        return Json(new { isValid = false, message = message });
                    }






                }
                //   Medical Leave
                if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.MedicalLeave)
                {
                    ////validation for maximum medical leave allowed
                    //var medicalLeavesCount = leaveSummaryDataResult?.Where(x => x.LeaveStatusID == (int)LeaveStatus.Approved && x.StartDate > currentYearDate && x.LeaveTypeID == (int)LeaveType.MedicalLeave)?.Sum(x => x.NoOfDays);
                    //if (leavePolicyModel.Annual_MaximumMedicalLeaveAllocationAllowed <= medicalLeavesCount)
                    //{
                    //    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    //    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have taken all medical leaves for this year";
                    //    return View(model);
                    //}
                }
                //    Comp Off Leavel
                if (model.leaveResults.leaveSummaryModel.LeaveTypeID == (int)LeaveType.CompOff)
                {
                    //validation for maximum medical leave allowed
                    var totalJoiningDays = (DateTime.Today - employeeDetails.InsertedDate).TotalDays;
                    if (leavePolicyModel.Annual_ApplicableAfterWorkingDays > totalJoiningDays)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        var message = "You can't apply leave(s) before " + leavePolicyModel.Annual_ApplicableAfterWorkingDays + " days of joining";
                        //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.MyInfo);
                        return Json(new { isValid = false, message = message });
                    }
                }


                //validation for  Check employee service eligibility (minimum 80 days of service)

                ////// Validate for already taken leaves
                //var isAlreadyTakenLeave = ValidateAlreadyTakenLeaves(model, leaveSummaryData);
                //if (isAlreadyTakenLeave)
                //{
                //   TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                //    TempData[HRMS.Models.Common.Constants.toastMessage] = "You have already applied leave for the same Date";
                //   return View();
                //}
                // Validation if required documents are uploaded
                //var certificate = model.leaveResults.leaveSummaryModel.UploadCertificate;

                //// Check if the medical document is not uploaded
                //if (isSpecialLeaveType)
                //{
                //    if (string.IsNullOrEmpty(certificate))
                //    {
                //        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                //        TempData[HRMS.Models.Common.Constants.toastMessage] =
                //            "Please upload the required medical document to proceed with the maternity leave.";
                //        return View();
                //    }
                //}
                string fileName = null;

                if (postedFiles.Count > 0)
                {
                    string wwwPath = Environment.WebRootPath;
                    string contentPath = this.Environment.ContentRootPath;

                    foreach (IFormFile postedFile in postedFiles)
                    {
                        fileName = postedFile.FileName.Replace(" ", "");
                    }
                    if (fileName != null)
                    {
                        model.leaveResults.leaveSummaryModel.UploadCertificate = fileName;
                    }
                    else
                    {
                        model.leaveResults.leaveSummaryModel.UploadCertificate = "";

                    }
                }
                if (model.leaveResults.leaveSummaryModel.UploadCertificate == null)
                {
                    model.leaveResults.leaveSummaryModel.UploadCertificate = "";
                }
                var data = _businessLayer.SendPostAPIRequest(model.leaveResults.leaveSummaryModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLeave), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(data);

                if (postedFiles.Count > 0)
                {
                    string path = Path.Combine(this.Environment.WebRootPath, Constants.UploadCertificate + result.PKNo.ToString());

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    foreach (IFormFile postedFile in postedFiles)
                    {
                        using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            postedFile.CopyTo(stream);
                        }
                    }
                }





                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                var messageData = "Leave applied successfully.";
                //return RedirectToActionPermanent(
                //  Constants.Index,
                //   WebControllarsConstants.MyInfo);
                return Json(new { isValid = true, message = messageData });



            }
            return View(model);
        }




=======
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
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
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
<<<<<<< HEAD

        private string GetHolidayData(long companyID)
        {
            HolidayInputParams myInfoInputParams = new HolidayInputParams
            {
                CompanyID = companyID
            };

            var holidayData = _businessLayer.SendPostAPIRequest(myInfoInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidayList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            return holidayData;
        }


=======
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
        public DateTime GetAprilFirstDate()
        {
            DateTime now = DateTime.Now;
            int year = now.Month < 4 ? now.Year - 1 : now.Year;
            return new DateTime(year, 4, 1);
        }
<<<<<<< HEAD
        private bool ValidateAlreadyTakenLeaves(MyInfoResults model, string leaveSummaryData, long employeeId)
        {
            var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
            var isAlreadyTakenLeave = false;
            var id = model.leaveResults.leaveSummaryModel.LeaveSummaryID;
            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                isAlreadyTakenLeave = leaveSummaryDataResult?.Any(x => x.StartDate.Date == model.leaveResults.leaveSummaryModel.StartDate.Date && x.EmployeeID == employeeId && x.LeaveSummaryID != id) ?? false;
=======
        private bool ValidateAlreadyTakenLeaves(MyInfoResults model, string leaveSummaryData)
        {
            var leaveSummaryDataResult = JsonConvert.DeserializeObject<LeaveResults>(leaveSummaryData)?.leavesSummary;
            var isAlreadyTakenLeave = false;

            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                isAlreadyTakenLeave = leaveSummaryDataResult?.Any(x => x.StartDate.Date == model.leaveResults.leaveSummaryModel.HalfDayDate.Date) ?? false;
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
            }
            else
            {
                // Additional validation logic for full-day leaves can be added here.
<<<<<<< HEAD
                // Check if full-day leave has already been taken
                isAlreadyTakenLeave = leaveSummaryDataResult?.Any(x =>
                    x.StartDate.Date <= model.leaveResults.leaveSummaryModel.EndDate.Date && // Existing leave starts before or on the new leave's end date
                    x.EndDate.Date >= model.leaveResults.leaveSummaryModel.StartDate.Date && x.EmployeeID == employeeId
                    && x.LeaveSummaryID != id// Existing leave ends after or on the new leave's start date
                ) ?? false;

=======
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
            }

            return isAlreadyTakenLeave;
        }

<<<<<<< HEAD

        private double CalculateAccruedLeaveForCurrentFiscalYear(DateTime joinDate, int Annual_MaximumLeaveAllocationAllowed)
        {
            // Define financial year start and end based on the current date
            DateTime today = DateTime.Today;
            //DateTime today = new DateTime(2024, 6, 14);
            DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1); // Start from April 1st of current financial year
            DateTime fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-1); // March 31st of the next year

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
                DateTime lastDayOfMonth = new DateTime(current.Year, current.Month, daysInMonth);

                // Adjust the comparison in the current month
                if (current.Month == today.Month && current.Year == today.Year)
                {
                    // Special case: Compare the days worked in the current month up to today
                    int daysWorkedInMonth = (today - joinDate).Days + 1;

                    // Accrue leave if more than 15 days worked
                    if (daysWorkedInMonth > 15)
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
                        if (daysWorkedInMonth > 15)
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
                };
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
            List<LeavePolicyDetailsModel>  Details = new List<LeavePolicyDetailsModel>();
            PolicyCategoryInputParams model = new PolicyCategoryInputParams();
            model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.PolicyCategoryDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            Details = JsonConvert.DeserializeObject<List<LeavePolicyDetailsModel>>(data);

            return View(Details);
        }
        [HttpGet]
        public IActionResult TeamAttendenceList()
        {
            var firstName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
            var middleName = Convert.ToString(HttpContext.Session.GetString(Constants.MiddleName)); // Assuming this exists
            var lastName = Convert.ToString(HttpContext.Session.GetString(Constants.Surname)); // Assuming this exists
            ViewBag.EmployeeName = $"{firstName} {middleName} {lastName}".Trim();
            return View();
        }
        [HttpGet]
        public IActionResult TeamAttendenceCalendarList(int year, int month)
        {
            var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
           
            AttendanceInputParams models = new AttendanceInputParams
            {
                Year = year,
                Month = month,
                UserId = employeeId,
            };

            var data = _businessLayer.SendPostAPIRequest(models, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidays>(data);

            return Json(new { data = model });
        }

=======
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
    }
}
