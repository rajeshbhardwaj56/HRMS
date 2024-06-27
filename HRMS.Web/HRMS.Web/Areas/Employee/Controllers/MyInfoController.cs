using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Employee;
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
            model.leaveResults.leaveSummaryModel.NoOfDays = (model.leaveResults.leaveSummaryModel.EndDate - model.leaveResults.leaveSummaryModel.StartDate).Days + 1;
            if ((int)LeaveDay.HalfDay == model.leaveResults.leaveSummaryModel.LeaveDurationTypeID)
            {
                model.leaveResults.leaveSummaryModel.NoOfDays = model.leaveResults.leaveSummaryModel.NoOfDays / 2;
            }

            if (model.leaveResults.leaveSummaryModel.NoOfDays > 0)
            {

                //var employeeDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(_businessLayer.SendPostAPIRequest(new EmployeeInputParams { CompanyID = model.leaveResults.leaveSummaryModel.CompanyID, EmployeeID = model.leaveResults.leaveSummaryModel.EmployeeID }, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString()).employeeModel;
                //model.leaveResults.leaveSummaryModel.LeavePolicyID = employeeDetails.LeavePolicyID ?? 0;

                model.leaveResults.leaveSummaryModel.LeavePolicyID = 1;

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
