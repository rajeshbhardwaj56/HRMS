using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using HRMS.Models.MyInfo;
using HRMS.Models.ShiftType;
using HRMS.Models.User;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Linq;

namespace HRMS.Web.Areas.Employee.Controllers
{
    [Authorize]
    [Area(Constants.ManageEmployee)]
    //  [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Employee + "," + RoleConstants.Manager + "," + RoleConstants.SuperAdmin))]
    public class AttendanceController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;

        public AttendanceController(ICheckUserFormPermission CheckUserFormPermission, IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
            _CheckUserFormPermission = CheckUserFormPermission;
        }

        public IActionResult AttendenceListing()
        {

            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetAllAttendenceList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            AttendenceListInputParans attendenceListParams = new AttendenceListInputParans();
            attendenceListParams.ID = Convert.ToInt64(HttpContext.Session.GetString(Constants.ID));

            var data = _businessLayer.SendPostAPIRequest(attendenceListParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAllAttendenceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);

            return Json(new { data = results.AttendenceList });

        }
        public IActionResult Index(string id)
        {

            Attendance model = new Attendance();
            if (!string.IsNullOrEmpty(id))
            {
                model.ID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendenceListID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).AttendanceModel;
            }
            HRMS.Models.Common.Results results = GetAllEmployees(Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)));
            model.Employeelist = results.Employee;
            return View(model);
        }

        public HRMS.Models.Common.Results GetAllEmployees(long EmployeeID)
        {
            HRMS.Models.Common.Results result = null;
            var data = "";
            if (HttpContext.Session.GetString(Constants.ResultsData) != null)
            {
                data = HttpContext.Session.GetString(Constants.ResultsData);
            }
            else
            {
                data = _businessLayer.SendGetAPIRequest("Common/GetAllEmployees?EmployeeID=" + EmployeeID, HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            }
            HttpContext.Session.SetString(Constants.ResultsData, data);
            result = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            return result;
        }

        [HttpPost]
        public IActionResult Index(AttendenceListModel AttendenceListModel)
        {
            if (ModelState.IsValid)
            {

                var data = _businessLayer.SendPostAPIRequest(AttendenceListModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendenceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);

                if (AttendenceListModel.ID > 0)
                {

                    return RedirectToAction("AttendenceListing");/*Constants.Index, WebControllarsConstants.AttendenceList, new { id = AttendenceListModel.ID.ToString() });*/

                }
                else
                {
                    return RedirectToActionPermanent(WebControllarsConstants.AttendenceListing, WebControllarsConstants.Attendance);

                }

            }
            else
            {
                return View(AttendenceListModel);
            }
        }

        [HttpGet]
        public IActionResult AttendenceList()
        {

            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.AttendenceList);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
        }
        [HttpPost]
        public IActionResult AttendenceCalendarList(AttendanceInputParams objmodel)
        {
            var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var employeeName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
            var employeeMiddleName = Convert.ToString(HttpContext.Session.GetString(Constants.MiddleName));
            var employeeLastName = Convert.ToString(HttpContext.Session.GetString(Constants.Surname));
            var EmployeeNumber = Convert.ToString(HttpContext.Session.GetString(Constants.EmployeeNumberWithoutAbbr));
            
            var employeeFullName = string.Join(separator: " ", new[] { employeeName, employeeMiddleName, employeeLastName }.Where(name => !string.IsNullOrWhiteSpace(name)));
            ViewBag.employeeFullName = employeeFullName;
            AttendanceInputParams models = new AttendanceInputParams();
            models.Year = objmodel.Year;
            models.Month = objmodel.Month;
            models.UserId = Convert.ToInt64(EmployeeNumber);

            var data = _businessLayer.SendPostAPIRequest(models, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendanceForMonthlyViewCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<MonthlyViewAttendance>(data);
            return Json(new { data = model, employeeFullName = employeeFullName });
        }
        [HttpGet]
        public IActionResult MyAttendanceList()
        {

            var ManagerL1 = HttpContext.Session.GetString(Constants.Manager1Name).ToString();
            var ManagerL2 = HttpContext.Session.GetString(Constants.Manager2Name).ToString();
            ViewBag.ManagerL1 = ManagerL1;
            ViewBag.ManagerL2 = ManagerL2;
            return View();
        }
        [HttpPost]
        public JsonResult GetMyAttendenceList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            var ManagerL1 = HttpContext.Session.GetString(Constants.Manager1Name).ToString();
            var ManagerL2 = HttpContext.Session.GetString(Constants.Manager2Name).ToString();
            ViewBag.ManagerL1 = ManagerL1;
            ViewBag.ManagerL2 = ManagerL2;
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.Month = DateTime.Now.Month;
            attendenceListParams.Year = DateTime.Now.Year;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(attendenceListParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetMyAttendanceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

            var model = JsonConvert.DeserializeObject<MyAttendanceList>(data);
            model.Attendances.ForEach(x => x.EncodedId = _businessLayer.EncodeStringBase64(x.ID.ToString()));

            return Json(new { data = model });
        }
        [HttpGet]
        public IActionResult MyAttendance(string id)
        {
            Attendance model = new Attendance();
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                model.ID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendenceListID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).AttendanceModel;
            }
            //HRMS.Models.Common.Results results = GetAllEmployees(Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)));
            //model.Employeelist = results.Employee;
            return View(model);

        }
        [HttpPost]
        public IActionResult MyAttendance(Attendance AttendenceListModel)
        {

            AttendenceListModel.WorkDate = AttendenceListModel.FirstLogDate;
            var UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            AttendenceListModel.UserId = UserId;
            AttendenceListModel.CreatedDate = DateTime.Now;
            AttendenceListModel.ModifiedBy = UserId;
            AttendenceListModel.CreatedBy = UserId;
            AttendenceListModel.IsDeleted = false;
            AttendenceListModel.IsManual = true;
            AttendenceListModel.AttendanceStatus = AttendanceStatus.Submitted.ToString();
            AttendenceListModel.AttendanceStatusId = (int)AttendanceStatusId.Pending;
            AttendenceListModel.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
            employeeobj.EmployeeID = AttendenceListModel.UserId ?? 0;
            var data = _businessLayer.SendPostAPIRequest(
                AttendenceListModel,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var result = JsonConvert.DeserializeObject<Result>(data);
            if (result != null && result.Message.Contains("Record for this user with the same date already exists!", StringComparison.OrdinalIgnoreCase))
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
            }
            else
            {
                var employeeApiResponse = _businessLayer.SendPostAPIRequest(
            employeeobj,
            _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
            HttpContext.Session.GetString(Constants.SessionBearerToken),
            true
        ).Result.ToString();
                var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);

                var Manager1Email = HttpContext.Session.GetString(Constants.Manager1Email).ToString();
                if (!string.IsNullOrEmpty(Manager1Email))
                {
                    var Name = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
                    sendEmailProperties sendEmailProperties = new sendEmailProperties
                    {

                        emailSubject = "Send a request for Attendance approval",
                        emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
           {Name} has sent a request for attendance approval.<br/><br/>

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
                TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
            }

            return RedirectToActionPermanent(WebControllarsConstants.MyAttendanceList, WebControllarsConstants.Attendance);
        }




        //public IActionResult MyAttendance(Attendance AttendenceListModel)
        //{
        //    AttendenceListModel.WorkDate = AttendenceListModel.FirstLogDate;
        //    var UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
        //    AttendenceListModel.UserId = UserId.ToString();
        //    AttendenceListModel.CreatedDate = DateTime.Now;
        //    AttendenceListModel.ModifiedBy = UserId;
        //    AttendenceListModel.CreatedBy = UserId;
        //    AttendenceListModel.IsDeleted = false;
        //    AttendenceListModel.IsManual = true;
        //    AttendenceListModel.AttendanceStatus = AttendanceStatus.Submitted.ToString();
        //    AttendenceListModel.AttendanceStatusId = (int)AttendanceStatusId.Pending;
        //    var data = _businessLayer.SendPostAPIRequest(AttendenceListModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
        //    var result = JsonConvert.DeserializeObject<Result>(data);
        //    if (result != null && result.Message.Contains("Record for this user with the same date already exists!", StringComparison.OrdinalIgnoreCase))
        //    {
        //        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
        //        TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
        //    }
        //    else
        //    {
        //        var Manager1Email = HttpContext.Session.GetString(Constants.Manager1Email).ToString();
        //        if (Manager1Email != null && Manager1Email != "")
        //        {
        //            var ManagerName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
        //            sendEmailProperties sendEmailProperties = new sendEmailProperties();
        //            sendEmailProperties.emailSubject = "Send a request for attendance approval";
        //            sendEmailProperties.emailBody = "Hi, " + ManagerName + " has sent a request for attendance approval.";
        //            sendEmailProperties.EmailToList.Add(Manager1Email); 
        //            emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);
        //            if (responses.responseCode == "200")
        //            {
        //            }
        //            else
        //            {
        //            }
        //        }
        //        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
        //        TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
        //    }
        //    return RedirectToActionPermanent(WebControllarsConstants.MyAttendanceList, WebControllarsConstants.Attendance);

        //}
        [HttpGet]
        public IActionResult DeleteAttendanceDetails(string id)
        {
            id = _businessLayer.DecodeStringBase64(id);
            Attendance model = new Attendance()
            {
                ID = Convert.ToInt32(id),
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.DeleteAttendanceDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(WebControllarsConstants.MyAttendanceList, WebControllarsConstants.Attendance);
        }
        public IActionResult TeamAttendenceList()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }


        [HttpPost]
        public JsonResult ApproveRejectAttendance(long attendanceId, long employeeId, string status, string approveRejectComment, DateTime startDate, DateTime endDate, DateTime workDate, int attendanceStatusId, string actionText)
        {
            // Get session values
            string approvesStatus = "";
            var modifiedBy = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var managerFirstName = HttpContext.Session.GetString(Constants.FirstName);
            string manager2Email = HttpContext.Session.GetString(Constants.Manager1Email) ?? string.Empty;
            string bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
            EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
            employeeobj.EmployeeID = employeeId;
            // Get employee details
            var employeeApiResponse = _businessLayer.SendPostAPIRequest(
                employeeobj,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
                bearerToken,
                true
            ).Result.ToString();

            var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);
            var attendanceModel = new Attendance
            {
                ID = attendanceId,
                WorkDate = workDate,
                UserId = employeeId,
                AttendanceStatus = status,
                FirstLogDate = startDate,
                LastLogDate = endDate,
                Comments = approveRejectComment,
                ModifiedBy = modifiedBy,
                ModifiedDate = DateTime.Now,
                AttendanceStatusId = attendanceStatusId,
                RoleId = RoleId
            };
            if (RoleId == (int)Roles.Admin || RoleId == (int)Roles.SuperAdmin)
            {
                attendanceModel.AttendanceStatusId = (int)AttendanceStatusId.AdminApproved;
            }
            else
            {
                // Determine new status based on action and current status
                if (actionText.Equals("approve", StringComparison.OrdinalIgnoreCase))
                {
                    attendanceModel.AttendanceStatusId = status == AttendanceStatusId.Pending.ToString()
                        ? (int)AttendanceStatusId.L1Approved
                        : (int)AttendanceStatusId.L2Approved;
                }
                else
                {
                    attendanceModel.AttendanceStatusId = status == AttendanceStatusId.Pending.ToString()
                        ? (int)AttendanceStatusId.L1Rejected
                        : (int)AttendanceStatusId.L2Rejected;
                }
            }

            // Submit updated attendance
            var updateApiResponse = _businessLayer.SendPostAPIRequest(
                attendanceModel,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace),
                bearerToken,
                true
            ).Result.ToString();

            var result = JsonConvert.DeserializeObject<Result>(updateApiResponse);
            if (result != null && result.Message.Contains("Record for this user with the same date already exists!", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = result.Message });
            }

            // Email notifications
            bool isEmailSent = false;
            string emailMessage = string.Empty;

            var emailList = new List<string>();
            string subject = string.Empty;
            string body = string.Empty;
            var actions = actionText.Equals("approve", StringComparison.OrdinalIgnoreCase) ? "approved" : "rejected";
            switch (attendanceModel.AttendanceStatusId)
            {
                case (int)AttendanceStatusId.L1Approved:
                    if (!string.IsNullOrEmpty(manager2Email))
                    {
                        // Email to manager 2
                        var managerEmailProps = new sendEmailProperties
                        {
                            emailSubject = "Send a request for attendance approval",
                            emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
           {employeeResult.EmployeeName} has sent a request for attendance approval.<br/><br/>

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
        </div>",
                            EmailToList = new List<string> { manager2Email }
                        };
                        EmailSender.SendEmail(managerEmailProps);
                    }

                    // Email to employee
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress) &&
      employeeResult.PersonalEmailAddress.Contains("@"))
                    {
                        approvesStatus = "L1 manager";
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
   Hi, {employeeResult.EmployeeName}, your attendance has been  {actions} by your {approvesStatus}.
  <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>";
                    }
                    break;

                case (int)AttendanceStatusId.L2Rejected:
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress) &&
     employeeResult.PersonalEmailAddress.Contains("@"))
                    {
                        approvesStatus = "L2 manager";
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
    Hi {employeeResult.EmployeeName}, your attendance   has been {actions}  by your {approvesStatus}.
    <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>";
                    }
                    break;

                default:
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress) &&
     employeeResult.PersonalEmailAddress.Contains("@"))
                    {
                        if (RoleId == (int)Roles.Admin || RoleId == (int)Roles.SuperAdmin)
                        {
                            approvesStatus = "Admin";
                        }
                        else
                        {
                            approvesStatus = "L1 manager";
                        }

                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
Hi, {employeeResult.EmployeeName}, your attendance has been  {actions} by your {approvesStatus}.
    <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>";
                    }
                    break;
            }

            if (emailList.Any())
            {
                var emailProps = new sendEmailProperties
                {
                    emailSubject = subject,
                    emailBody = body,
                    EmailToList = emailList
                };
                var emailResponse = EmailSender.SendEmail(emailProps);
                isEmailSent = emailResponse.responseCode == "200";
                emailMessage = isEmailSent ? " and email sent." : ", but email sending failed.";
            }

            var actionVerb = actionText.Equals("approve", StringComparison.OrdinalIgnoreCase) ? "approved" : "rejected";

            return Json(new
            {
                success = true,
                message = $"Attendance {actionVerb} successfully{emailMessage}"
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetTeamAttendenceList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.Month = DateTime.Now.Month;
            //attendenceListParams.Month =1;
            attendenceListParams.Year = DateTime.Now.Year;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            attendenceListParams.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var data = _businessLayer.SendPostAPIRequest(attendenceListParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidays>(data);
            return Json(new { data = model });
        }

        public IActionResult ApprovedAttendance()
        {
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.ApprovedAttendance);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
        }

        [HttpPost]
        public JsonResult GetApprovedAttendance([FromBody] AttendanceStatusRequest request)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.AttendanceStatusId = request.AttendanceStatus;
            attendenceListParams.Year = request.Year;
            attendenceListParams.Month = request.Month;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            attendenceListParams.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var data = _businessLayer.SendPostAPIRequest(
                attendenceListParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetApprovedAttendance), HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<Attendance>>(data).ToList();
            return Json(new { data = model });
        }
        [HttpPost]
        public JsonResult GetManagerApprovedAttendance([FromBody] AttendanceStatusRequest request)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            //if (!Enum.IsDefined(typeof(AttendanceStatusId), attendanceStatus))
            //{
            //    attendanceStatus = (int)AttendanceStatusId.Pending;
            //}
            attendenceListParams.AttendanceStatusId = request.AttendanceStatus;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(
                attendenceListParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetManagerApprovedAttendance), HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<Attendance>>(data).ToList();
            return Json(new { data = model });
        }

        [HttpGet]
        public IActionResult GetEmployeeAttendanceShiftDetails(int employeeID, int Id)
        {
            ShiftTypeModel shiftTypeModel = new ShiftTypeModel();
            EmployeeAttendance EmployeeAttendanceModel = new EmployeeAttendance();
            var CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var employeeDetails = GetEmployeeDetails(CompanyID, employeeID);
            shiftTypeModel.ShiftTypeID = employeeDetails.ShiftTypeID;
            shiftTypeModel.CompanyID = CompanyID;
            var data = _businessLayer.SendPostAPIRequest(shiftTypeModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.ShiftType, APIApiActionConstants.GetAllShiftTypes), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            shiftTypeModel = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).shiftTypeModel;
            EmployeeAttendanceModel.FullName = employeeDetails.FirstName + ' ' + employeeDetails.MiddleName + ' ' + employeeDetails.Surname;
            EmployeeAttendanceModel.EmployeeNumber = employeeDetails.EmployeeNumber;
            EmployeeAttendanceModel.EmployeeJoiningdate = employeeDetails.JoiningDate.Value.ToString("dd/MM/yyyy");
            //EmployeeAttendanceModel.EmployeeDesignation = employeeDetails.DesignationName;
            EmployeeAttendanceModel.EmployeeDepartment = employeeDetails.DepartmentName;
            EmployeeAttendanceModel.ManagerName = employeeDetails.ManagerName;
            //EmployeeAttendanceModel.Employeeemail = employeeDetails.OfficialEmailID;
            EmployeeAttendanceModel.ShiftStartDate = shiftTypeModel.StartTime;
            EmployeeAttendanceModel.ShiftEndDate = shiftTypeModel.EndTime;
            return Json(EmployeeAttendanceModel);
        }


        public IActionResult TeamAttendanceLogs()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }
        [HttpPost]
        public JsonResult GetTeamAttendanceLogs(string EmployeeId)
        {
            AttendanceDeviceLog attendenceDeviceLogs = new AttendanceDeviceLog();
            attendenceDeviceLogs.EmployeeId = EmployeeId;
            attendenceDeviceLogs.CreatedBy = HttpContext.Session.GetString(Constants.EmployeeID);
            var data = _businessLayer.SendPostAPIRequest(attendenceDeviceLogs, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendanceDeviceLogs), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceLogResponse>(data);
            return Json(new { data = model.AttendanceLogs });
        }
        [HttpPost]
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
                ;
                return Json(new { data = employeeDetails });
            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred", details = ex.Message });
            }
        }

        #region CompOff
        [HttpGet]
        public IActionResult CompOffApplication()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetCompOffAttendanceLogs(long attendanceStatus)
        {
            CompOffAttendanceInputParams inputParams = new CompOffAttendanceInputParams
            {
                EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)),
                JobLocationTypeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.JobLocationID)),
                AttendanceStatus = attendanceStatus,
            };

            var data = _businessLayer.SendPostAPIRequest(
                inputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetCompOffAttendanceList),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var model = JsonConvert.DeserializeObject<List<CompOffAttendanceRequestModel>>(data);

            return Json(new { data = model });
        }

        [HttpPost]
        public IActionResult SubmitCompOffRequest([FromBody] CompOffLogSubmission submission)
        {
            var result = new Result();
            EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
            try
            {
                var userId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                foreach (var attendance in submission.Logs)
                {
                    var compOff = new CompOffAttendanceRequestModel
                    {
                        AttendanceId = attendance.AttendanceId,
                        EmployeeId = attendance.EmployeeId,
                        WorkDate = attendance.AttendanceDate,
                        FirstLogDate = attendance.FirstLog,
                        LastLogDate = attendance.LastLog,
                        HoursWorked = attendance.HoursWorked,
                        Comments = submission.Comment,
                        ModifiedBy = userId,
                        CreatedBy = attendance.CreatedBy ?? userId,
                        IsDeleted = false,
                        AttendanceStatusId = (int)AttendanceStatusId.Pending,


                    };
                    employeeobj.EmployeeID = attendance.EmployeeId;
                    var responseString = _businessLayer.SendPostAPIRequest(compOff, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateCompOffAttendace),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result?.ToString();

                    if (!string.IsNullOrWhiteSpace(responseString))
                    {

                        var employeeApiResponse = _businessLayer.SendPostAPIRequest(
            employeeobj,
            _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
            HttpContext.Session.GetString(Constants.SessionBearerToken),
            true
        ).Result.ToString();

                        var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);

                        result = JsonConvert.DeserializeObject<Result>(responseString);
                        if (!string.IsNullOrEmpty(result?.Message) && result.Message.ToLower().Contains("error"))
                            break;
                        var managerEmail = HttpContext.Session.GetString(Constants.Manager1Email);
                        if (!string.IsNullOrEmpty(managerEmail))
                        {
                            var Name = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
                            sendEmailProperties sendEmailProperties = new sendEmailProperties
                            {

                                emailSubject = "Send a request for CompOff Attendance approval",
                                emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
           {Name} has sent a request for attendance approval.<br/><br/>

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
                            sendEmailProperties.EmailToList.Add(managerEmail);
                            emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);

                        }
                    }
                    else
                    {
                        result.Message = "No response from API.";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = "Error while submitting comp-off requests: " + ex.Message;
            }

            return Json(new { message = result?.Message ?? "Comp-Off requests submitted successfully." });
        }

        [HttpGet]
        public IActionResult ApproveCompOff()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetManagerApprovedCompOff([FromBody] AttendanceStatusRequest request)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.AttendanceStatusId = request.AttendanceStatus;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(
                attendenceListParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetManagerApprovedAttendance), HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<Attendance>>(data).ToList();
            return Json(new { data = model });
        }

        [HttpPost]
        public JsonResult GetApprovedCompOff([FromBody] AttendanceStatusRequest request)
        {
            CompOffInputParams attendenceListParams = new CompOffInputParams();
            attendenceListParams.AttendanceStatusId = request.CompOffStatus;
            attendenceListParams.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(
                attendenceListParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetApprovedCompOff), HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<CompOffAttendanceRequestModel>>(data).ToList();
            return Json(new { data = model });
        }


        [HttpPost]
        public JsonResult ApproveRejectCompOff(long compOffId, long attendanceId, long employeeId, string status, string approveRejectComment, DateTime startDate, DateTime endDate, DateTime workDate, int attendanceStatusId, string actionText)
        {
            string approvesStatus = "";
            var modifiedBy = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var managerFirstName = HttpContext.Session.GetString(Constants.FirstName);
            string manager2Email = HttpContext.Session.GetString(Constants.Manager1Email) ?? string.Empty;
            string bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
            EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
            employeeobj.EmployeeID = employeeId;
            // Get employee details
            var employeeApiResponse = _businessLayer.SendPostAPIRequest(
                employeeobj,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
                bearerToken,
                true
            ).Result.ToString();
            var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);

            // Prepare attendance model
            var attendanceModel = new CompOffAttendanceRequestModel
            {
                AttendanceId = attendanceId,
                ID = compOffId,
                WorkDate = workDate,
                UserId = employeeId,
                AttendanceStatus = status,
                FirstLogDate = startDate,
                LastLogDate = endDate,
                Comments = approveRejectComment,
                ModifiedBy = modifiedBy,
                EmployeeId = employeeId,
                AttendanceStatusId = attendanceStatusId,
                RoleId = RoleId
            };

            // Determine new status based on action and current status
            if (RoleId == (int)Roles.Admin || RoleId == (int)Roles.SuperAdmin)
            {
                attendanceModel.AttendanceStatusId = (int)AttendanceStatusId.AdminApproved;
            }
            else
            {
                if (actionText.Equals("approve", StringComparison.OrdinalIgnoreCase))
                {
                    attendanceModel.AttendanceStatusId = status == AttendanceStatusId.Pending.ToString()
                        ? (int)AttendanceStatusId.L1Approved
                        : (int)AttendanceStatusId.L2Approved;
                }
                else
                {
                    attendanceModel.AttendanceStatusId = status == AttendanceStatusId.Pending.ToString()
                        ? (int)AttendanceStatusId.L1Rejected
                        : (int)AttendanceStatusId.L2Rejected;
                }
            }



            // Submit updated attendance
            var updateApiResponse = _businessLayer.SendPostAPIRequest(
                attendanceModel,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateCompOffAttendace),
                bearerToken,
                true
            ).Result.ToString();

            var result = JsonConvert.DeserializeObject<Result>(updateApiResponse);


            // Email notifications
            bool isEmailSent = false;
            string emailMessage = string.Empty;

            var emailList = new List<string>();
            string subject = string.Empty;
            string body = string.Empty;
            var actions = actionText.Equals("approve", StringComparison.OrdinalIgnoreCase) ? "approved" : "rejected";
            switch (attendanceModel.AttendanceStatusId)
            {
                case (int)AttendanceStatusId.L1Approved:
                    if (!string.IsNullOrEmpty(manager2Email))
                    {
                        // Email to manager 2
                        var managerEmailProps = new sendEmailProperties
                        {
                            emailSubject = "Send a request for CompOff attendance approval",
                            emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
           {employeeResult.EmployeeName} has sent a request for attendance approval.<br/><br/>

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
        </div>",

                            EmailToList = new List<string> { manager2Email }
                        };
                        EmailSender.SendEmail(managerEmailProps);
                    }

                    // Email to employee
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress) &&
     employeeResult.PersonalEmailAddress.Contains("@"))
                    {
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
 Hi, {employeeResult.EmployeeName}, your CompOff has been  {actions} by your L1 manager.
   <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>";
                    }
                    break;

                case (int)AttendanceStatusId.L2Rejected:
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress) &&
     employeeResult.PersonalEmailAddress.Contains("@"))
                    {
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
 Hi, {employeeResult.EmployeeName}, your CompOff has been  {actions} by your L2 manager.
   <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>";
                    }
                    break;

                default:
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress) &&
    employeeResult.PersonalEmailAddress.Contains("@"))
                    {

                        if (RoleId == (int)Roles.Admin || RoleId == (int)Roles.SuperAdmin)
                        {
                            approvesStatus = "Admin";
                        }
                        else
                        {
                            approvesStatus = "L1 manager";
                        }

                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";

                        body = $@"
 Hi, {employeeResult.EmployeeName}, your CompOff has been  {actions} by your {approvesStatus}.
   <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>  
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>";

                    }
                    break;
            }

            if (emailList.Any())
            {
                var emailProps = new sendEmailProperties
                {
                    emailSubject = subject,
                    emailBody = body,
                    EmailToList = emailList
                };
                var emailResponse = EmailSender.SendEmail(emailProps);
                isEmailSent = emailResponse.responseCode == "200";
                emailMessage = isEmailSent ? " and email sent." : ", but email sending failed.";
                //emailMessage = ", but email sending failed.";
            }

            var actionVerb = actionText.Equals("approve", StringComparison.OrdinalIgnoreCase) ? "approved" : "rejected";

            return Json(new
            {
                success = true,
                message = $"Attendance {actionVerb} successfully{emailMessage}"
            });
        }
        #endregion CompOff
        private EmployeeModel GetEmployeeDetails(long companyId, long employeeId)
        {
            var employeeDetailsJson = _businessLayer.SendPostAPIRequest(new EmployeeInputParams { CompanyID = companyId, EmployeeID = employeeId }, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var employeeDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(employeeDetailsJson).employeeModel;
            return employeeDetails;
        }

        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }
        #region Attendance Approval
        public IActionResult TeamAttendenceApprovalList()
        {
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);
            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.AttendanceApproval);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            var firstName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
            var middleName = Convert.ToString(HttpContext.Session.GetString(Constants.MiddleName)); 
            var lastName = Convert.ToString(HttpContext.Session.GetString(Constants.Surname)); // Assuming this exists
            ViewBag.EmployeeName = $"{firstName} {middleName} {lastName}".Trim();
            return View();
        }
        [HttpGet]
        public IActionResult TeamAttendenceForApprovalList(int year, int month, int Page, int PageSize, string SearchTerm, int jobLocationId)
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

            var data = _businessLayer.SendPostAPIRequest(models, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForApproval), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
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

        [HttpPost]
        public IActionResult GetAttendanceApprovalList(
    string sEcho,
    int iDisplayStart,
    int iDisplayLength,
    string sSearch,
    string sortCol,
    string sortDir)
        {
            long reportingToId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            int roleId = Convert.ToInt32(HttpContext.Session.GetString(Constants.RoleID));

            var columnMapping = new Dictionary<string, string>
    {
        {"employeeID", "EmployeeID"},
        {"workDate", "WorkDate"},
        {"employeNumber", "EmployeNumber"},
        {"attendanceStatus", "AttendanceStatus"},
        {"remarks", "Remarks"}
    };



            var model = new AttendanceInputParams
            {
                UserId = reportingToId,
                RoleId = roleId,
                SortCol = columnMapping.ContainsKey(sortCol) ? columnMapping[sortCol] : "EmployeeID",
                SortDir = string.IsNullOrEmpty(sortDir) ? "DESC" : sortDir.ToUpper(),
                DisplayStart = iDisplayStart,
                DisplayLength = iDisplayLength,
                SearchTerm = string.IsNullOrEmpty(sSearch) ? null : sSearch
            };

            var apiResponse = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.AttendenceList,
                    APIApiActionConstants.GetTeamAttendanceForApproval),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result;

            var attendanceData = JsonConvert.DeserializeObject<AttendanceWithHolidaysVM>(apiResponse.ToString());

            return Json(new
            {
                draw = sEcho,
                recordsTotal = attendanceData.TotalRecords,
                recordsFiltered = attendanceData.TotalRecords,
                data = attendanceData.Attendances.Select(a => new
                {
                    employeeId = a.EmployeeId,
                    employeNumber = a.EmployeNumber,
                    employeeName=a.EmployeeName ,
                    workDate = a.WorkDate,
                    attendanceStatus = a.Status,
                    remarks = a.Remarks,
                    id = a.ID 
                })
            });
        }
        [HttpPost]
        public IActionResult SaveAttendanceStatus(string EmployeeId, string Status, string Remarks, DateTime WorkDate)
        {
          
            var employeeId = Convert.ToInt64(_businessLayer.DecodeStringBase64(EmployeeId)); 
            var updatedByUserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var managerId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)); 
            SaveTeamAttendanceStatus model = new SaveTeamAttendanceStatus
            {
                EmployeeId = employeeId, 
                ManagerId = managerId, 
                UserID = updatedByUserId,
                WorkDate = WorkDate,
                AttendanceStatus = Status,
                ApprovedByAdmin = false,
                Remarks = string.IsNullOrEmpty(Remarks) ? "" : Remarks
            };

            var apiUrl = _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.AttendenceList,
                APIApiActionConstants.SaveOrUpdateAttendanceStatus
            );

            var response = _businessLayer.SendPostAPIRequest(
                model, apiUrl, HttpContext.Session.GetString(Constants.SessionBearerToken), true
            ).Result.ToString();

            var dataModel = JsonConvert.DeserializeObject<Result>(response);

            return Json(new { data = dataModel });
        }






        [HttpPost]
        public  IActionResult SaveBulkAttendanceStatus([FromBody] List<AttendanceRecordDto> records)
        {
            var currentEmployeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

            try
            {
                foreach (var record in records)
                {
                    
                    if (record.EmployeeId == null || record.WorkDate == null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "EmployeeId and WorkDate are required fields"
                        });
                    }

                    var attendanceRequest = new SaveTeamAttendanceStatus
                    {
                        EmployeeId = record.EmployeeId,
                        ID =record.ID,
                        UserID = currentEmployeeId,
                        WorkDate = record.WorkDate.Value,
                        AttendanceStatus = record.AttendanceStatus ,
                        Remarks = record.Remarks, 
                        ApprovedByAdmin = true,
                        ApprovedStatus =record.ApprovedStatus 

                    };
                    var apiUrl = _businessLayer.GetFormattedAPIUrl(
                            APIControllarsConstants.AttendenceList,
                            APIApiActionConstants.SaveOrUpdateAttendanceStatus
                        );
                    var apiResponse =  _businessLayer.SendPostAPIRequest(
                        attendanceRequest,
                        apiUrl,
                        HttpContext.Session.GetString(Constants.SessionBearerToken), true
            ).Result.ToString();

                    var responseModel = JsonConvert.DeserializeObject<Result>(apiResponse);

                    if (responseModel == null || responseModel.PKNo <= 0)
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = responseModel?.Message ?? "Error processing attendance record"
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = $"{records.Count} attendance records processed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"An error occurred: {ex.Message}"
                });
            }
        }



        #endregion Attendance Approval


        #region AgentCompOff
        [HttpGet]
        public IActionResult AgentCompOffApplication()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetAgentCompOffAttendanceLogs(long attendanceStatus, string employeeId, string jobLocationId)
        {
            long empID = 0;
            long jobID = 0;
            if (!string.IsNullOrEmpty(employeeId))
            {
                long.TryParse(employeeId, out empID);
                long.TryParse(jobLocationId, out jobID);
            }
            CompOffAttendanceInputParams inputParams = new CompOffAttendanceInputParams
            {
                EmployeeID = empID,
                JobLocationTypeID = jobID,
                AttendanceStatus = attendanceStatus,
            };

            var data = _businessLayer.SendPostAPIRequest(
                inputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetCompOffAttendanceList),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var model = JsonConvert.DeserializeObject<List<CompOffAttendanceRequestModel>>(data);

            return Json(new { data = model });
        }

        [HttpPost]
        public IActionResult SubmitAgentCompOffRequest([FromBody] CompOffLogSubmission submission)
        {
            var result = new Result();
            EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
            try
            {
                var userId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                foreach (var attendance in submission.Logs)
                {
                    var compOff = new CompOffAttendanceRequestModel
                    {
                        AttendanceId = attendance.AttendanceId,
                        EmployeeId = attendance.EmployeeId,
                        WorkDate = attendance.AttendanceDate,
                        FirstLogDate = attendance.FirstLog,
                        LastLogDate = attendance.LastLog,
                        HoursWorked = attendance.HoursWorked,
                        Comments = submission.Comment,
                        ModifiedBy = userId,
                        CreatedBy = attendance.CreatedBy ?? userId,
                        IsDeleted = false,
                        AttendanceStatusId = (int)AttendanceStatusId.Pending,


                    };
                    employeeobj.EmployeeID = attendance.EmployeeId;
                    var responseString = _businessLayer.SendPostAPIRequest(compOff, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateCompOffAttendace),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result?.ToString();

                    if (!string.IsNullOrWhiteSpace(responseString))
                    {

                        var employeeApiResponse = _businessLayer.SendPostAPIRequest(
            employeeobj,
            _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
            HttpContext.Session.GetString(Constants.SessionBearerToken),
            true
        ).Result.ToString();

                        var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);

                        result = JsonConvert.DeserializeObject<Result>(responseString);
                        if (!string.IsNullOrEmpty(result?.Message) && result.Message.ToLower().Contains("error"))
                            break;
                        var managerEmail = HttpContext.Session.GetString(Constants.Manager1Email);
                        if (!string.IsNullOrEmpty(managerEmail))
                        {
                            var Name = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
                            sendEmailProperties sendEmailProperties = new sendEmailProperties
                            {

                                emailSubject = "Send a request for CompOff Attendance approval",
                                emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
           {Name} has sent a request for attendance approval.<br/><br/>

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
                            sendEmailProperties.EmailToList.Add(managerEmail);
                            emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);

                        }
                    }
                    else
                    {
                        result.Message = "No response from API.";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = "Error while submitting comp-off requests: " + ex.Message;
            }

            return Json(new { message = result?.Message ?? "Comp-Off requests submitted successfully." });
        }

       
        #endregion Agent CompOff



    }

}
