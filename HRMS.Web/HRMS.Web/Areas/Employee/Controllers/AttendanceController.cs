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
using System;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<JsonResult> GetAllAttendenceList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            AttendenceListInputParans attendenceListParams = new AttendenceListInputParans();
            attendenceListParams.ID = Convert.ToInt64(HttpContext.Session.GetString(Constants.ID));

            var data = _businessLayer.SendPostAPIRequest(attendenceListParams,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAllAttendenceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);

            return Json(new { data = results.AttendenceList });

        }
        public async Task<IActionResult> Index(string id)
        {
            Attendance model = new Attendance();
            if (!string.IsNullOrEmpty(id))
            {
                model.ID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(model,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendenceListID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
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
        public async Task<IActionResult> Index(AttendenceListModel AttendenceListModel)
        {
            if (ModelState.IsValid)
            {

                var data = _businessLayer.SendPostAPIRequest(AttendenceListModel,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendenceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
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
        public async Task<IActionResult> AttendenceList()
        {
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission =await _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.AttendenceList);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AttendenceCalendarList(AttendanceInputParams objmodel)
        {
            var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var employeeName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
            var employeeMiddleName = Convert.ToString(HttpContext.Session.GetString(Constants.MiddleName));
            var employeeLastName = Convert.ToString(HttpContext.Session.GetString(Constants.Surname));
            var EmployeeNumber = Convert.ToString(HttpContext.Session.GetString(Constants.EmployeeNumberWithoutAbbr));
            // Concatenate full name
            var employeeFullName = string.Join(" ", new[] { employeeName, employeeMiddleName, employeeLastName }.Where(name => !string.IsNullOrWhiteSpace(name)));
            ViewBag.employeeFullName = employeeFullName;
            AttendanceInputParams models = new AttendanceInputParams();
            models.Year = objmodel.Year;
            models.Month = objmodel.Month;
            models.UserId = Convert.ToInt64(EmployeeNumber);

            var data = _businessLayer.SendPostAPIRequest(models,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendanceForMonthlyViewCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
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
        public async Task<JsonResult> GetMyAttendenceList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            var ManagerL1 = HttpContext.Session.GetString(Constants.Manager1Name).ToString();
            var ManagerL2 = HttpContext.Session.GetString(Constants.Manager2Name).ToString();
            ViewBag.ManagerL1 = ManagerL1;
            ViewBag.ManagerL2 = ManagerL2;
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.Month = DateTime.Now.Month;
            attendenceListParams.Year = DateTime.Now.Year;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(attendenceListParams,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetMyAttendanceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();

            var model = JsonConvert.DeserializeObject<MyAttendanceList>(data);
            model.Attendances.ForEach(x => x.EncodedId = _businessLayer.EncodeStringBase64(x.ID.ToString()));

            return Json(new { data = model });
        }
        [HttpGet]
        public async Task<IActionResult> MyAttendance(string id)
        {
            Attendance model = new Attendance();
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                model.ID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(model,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendenceListID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).AttendanceModel;
            }
            //HRMS.Models.Common.Results results = GetAllEmployees(Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)));
            //model.Employeelist = results.Employee;
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> MyAttendance(Attendance AttendenceListModel)
        {

            if (AttendenceListModel.FirstLogDate.HasValue)
            {
                var selectedDate = AttendenceListModel.FirstLogDate.Value;
                if (selectedDate.DayOfWeek == DayOfWeek.Saturday || selectedDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Attendance cannot be submitted for weekends (Saturday or Sunday).";
                    return RedirectToActionPermanent(WebControllarsConstants.MyAttendanceList, WebControllarsConstants.Attendance);
                }
            }

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
            await    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace),
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
            await  _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
            HttpContext.Session.GetString(Constants.SessionBearerToken),
            true
        ).Result.ToString();
                var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);

                var Manager1Email = HttpContext.Session.GetString(Constants.Manager1Email).ToString();
                if (!string.IsNullOrEmpty(Manager1Email))
                {
                    var Name = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
                    var ManagerName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
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
                To no longer receive messages from Eternity Logistics, please click to <strong><a href='http://unsubscribe.eternitylogistics.co/'> Unsubscribe </a></strong>.<br/><br/>

                If you are happy with our services or want to share any feedback, do email us at 
                <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

                All email correspondence is sent only through our official domain: 
                <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

                <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
                If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
                Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
                This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
                Your cooperation is greatly appreciated.
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
        public async Task<IActionResult> DeleteAttendanceDetails(string id)
        {
            id = _businessLayer.DecodeStringBase64(id);
            Attendance model = new Attendance()
            {
                ID = Convert.ToInt32(id),
            };
            var data = _businessLayer.SendPostAPIRequest(model,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.DeleteAttendanceDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
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
        public async Task<JsonResult> ApproveRejectAttendance(long attendanceId, long employeeId, string status, string approveRejectComment, DateTime startDate, DateTime endDate, DateTime workDate, int attendanceStatusId, string actionText)
        {
            // Get session values
            var modifiedBy = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var managerFirstName = HttpContext.Session.GetString(Constants.FirstName);
            string manager2Email = HttpContext.Session.GetString(Constants.Manager2Email) ?? string.Empty;
            string bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
            EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
            employeeobj.EmployeeID = employeeId;
            // Get employee details
            var employeeApiResponse = _businessLayer.SendPostAPIRequest(
                employeeobj,
           await     _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
                bearerToken,
                true
            ).Result.ToString();

            var employeeResult = JsonConvert.DeserializeObject<EmployeePersonalDetails>(employeeApiResponse);

            // Convert string status to enum 
            // Prepare attendance model
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

            // Submit updated attendance
            var updateApiResponse = _businessLayer.SendPostAPIRequest(
                attendanceModel,
             await   _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace),
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
                To no longer receive messages from Eternity Logistics, please click to <strong><a href='http://unsubscribe.eternitylogistics.co/'> Unsubscribe </a></strong>.<br/><br/>

                If you are happy with our services or want to share any feedback, do email us at 
                <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

                All email correspondence is sent only through our official domain: 
                <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

                <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
                If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
                Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
                This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
                Your cooperation is greatly appreciated.
            </p>
        </div>",
                            EmailToList = new List<string> { manager2Email }
                        };
                        EmailSender.SendEmail(managerEmailProps);
                    }

                    // Email to employee
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress))
                    {
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
   Hi, {employeeResult.EmployeeName}, your CompOff has been approved.
    <p style='color: #000; font-size: 13px;'>
        To no longer receive messages from Eternity Logistics, please click to 
        <strong><a href='http://unsubscribe.eternitylogistics.co/'>Unsubscribe</a></strong>.<br/><br/>

        If you are happy with our services or want to share any feedback, do email us at 
        <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

        All email correspondence is sent only through our official domain: 
        <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

        <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
        If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
        Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
        This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
        Your cooperation is greatly appreciated.
    </p>";
                    }
                    break;

                case (int)AttendanceStatusId.L2Rejected:
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress))
                    {
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
    Hi {employeeResult.EmployeeName}, your CompOff attendance has been rejected.
    <p style='color: #000; font-size: 13px;'>
        To no longer receive messages from Eternity Logistics, please click to 
        <strong><a href='http://unsubscribe.eternitylogistics.co/'>Unsubscribe</a></strong>.<br/><br/>

        If you are happy with our services or want to share any feedback, do email us at 
        <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

        All email correspondence is sent only through our official domain: 
        <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

        <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
        If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
        Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
        This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
        Your cooperation is greatly appreciated.
    </p>";
                    }
                    break;

                default:
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress))
                    {
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
Hi, {employeeResult.EmployeeName}, your CompOff attendance has been updated.
    <p style='color: #000; font-size: 13px;'>
        To no longer receive messages from Eternity Logistics, please click to 
        <strong><a href='http://unsubscribe.eternitylogistics.co/'>Unsubscribe</a></strong>.<br/><br/>

        If you are happy with our services or want to share any feedback, do email us at 
        <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

        All email correspondence is sent only through our official domain: 
        <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

        <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
        If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
        Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
        This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
        Your cooperation is greatly appreciated.
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
        public async Task<JsonResult> GetTeamAttendenceList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.Month = DateTime.Now.Month;
            //attendenceListParams.Month =1;
            attendenceListParams.Year = DateTime.Now.Year;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            attendenceListParams.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var data = _businessLayer.SendPostAPIRequest(attendenceListParams,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidays>(data);
            return Json(new { data = model });
        }

        public async Task<IActionResult> ApprovedAttendance()
        {
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission =await _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.ApprovedAttendance);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetApprovedAttendance([FromBody] AttendanceStatusRequest request)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.AttendanceStatusId = request.AttendanceStatus;
            attendenceListParams.Year = request.Year;
            attendenceListParams.Month = request.Month;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            attendenceListParams.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var data = _businessLayer.SendPostAPIRequest(
                attendenceListParams,
              await  _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetApprovedAttendance), HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<Attendance>>(data).ToList();
            return Json(new { data = model });
        }
        [HttpPost]
        public async Task<JsonResult> GetManagerApprovedAttendance([FromBody] AttendanceStatusRequest request)
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
              await  _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetManagerApprovedAttendance), HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<Attendance>>(data).ToList();
            return Json(new { data = model });
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeAttendanceShiftDetails(int employeeID, int Id)
        {
            ShiftTypeModel shiftTypeModel = new ShiftTypeModel();
            EmployeeAttendance EmployeeAttendanceModel = new EmployeeAttendance();
            var CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var employeeDetails = await GetEmployeeDetails(CompanyID, employeeID);
            shiftTypeModel.ShiftTypeID = employeeDetails.ShiftTypeID;
            shiftTypeModel.CompanyID = CompanyID;
            var data = _businessLayer.SendPostAPIRequest(shiftTypeModel,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.ShiftType, APIApiActionConstants.GetAllShiftTypes), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
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
        public async Task<JsonResult> GetTeamAttendanceLogs(string EmployeeId)
        {
            AttendanceDeviceLog attendenceDeviceLogs = new AttendanceDeviceLog();
            attendenceDeviceLogs.EmployeeId = EmployeeId;
            attendenceDeviceLogs.CreatedBy = HttpContext.Session.GetString(Constants.EmployeeID);
            var data = _businessLayer.SendPostAPIRequest(attendenceDeviceLogs,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendanceDeviceLogs), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
            var model = JsonConvert.DeserializeObject<AttendanceLogResponse>(data);
            return Json(new { data = model.AttendanceLogs });
        }
        [HttpPost]
        public async Task<JsonResult> GetTeamEmployeeList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            try
            {
                List<EmployeeDetails> employeeDetails = new List<EmployeeDetails>();
                EmployeeInputParams model = new EmployeeInputParams();
                model.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                model.RoleID = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
                var data = _businessLayer.SendPostAPIRequest(model,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmployeeListByManagerID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
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
        public async Task<JsonResult> GetCompOffAttendanceLogs(long attendanceStatus)
        {
            CompOffAttendanceInputParams inputParams = new CompOffAttendanceInputParams
            {
                EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)),
                JobLocationTypeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.JobLocationID)),
                AttendanceStatus = attendanceStatus,
            };

            var data = _businessLayer.SendPostAPIRequest(
                inputParams,
              await  _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetCompOffAttendanceList),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var model = JsonConvert.DeserializeObject<List<CompOffAttendanceRequestModel>>(data);

            return Json(new { data = model });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitCompOffRequest([FromBody] CompOffLogSubmission submission)
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
                    var responseString = _businessLayer.SendPostAPIRequest(compOff,await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateCompOffAttendace),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result?.ToString();

                    if (!string.IsNullOrWhiteSpace(responseString))
                    {

                        var employeeApiResponse = _businessLayer.SendPostAPIRequest(
            employeeobj,
         await   _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
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
                To no longer receive messages from Eternity Logistics, please click to <strong><a href='http://unsubscribe.eternitylogistics.co/'> Unsubscribe </a></strong>.<br/><br/>

                If you are happy with our services or want to share any feedback, do email us at 
                <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

                All email correspondence is sent only through our official domain: 
                <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

                <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
                If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
                Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
                This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
                Your cooperation is greatly appreciated.
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
        public async Task<JsonResult> GetManagerApprovedCompOff([FromBody] AttendanceStatusRequest request)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.AttendanceStatusId = request.AttendanceStatus;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(
                attendenceListParams,
              await  _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetManagerApprovedAttendance), HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<Attendance>>(data).ToList();
            return Json(new { data = model });
        }

        [HttpPost]
        public async Task<JsonResult> GetApprovedCompOff([FromBody] AttendanceStatusRequest request)
        {
            CompOffInputParams attendenceListParams = new CompOffInputParams();
            attendenceListParams.AttendanceStatusId = request.CompOffStatus;
            attendenceListParams.RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(
                attendenceListParams,
              await  _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetApprovedCompOff), HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<CompOffAttendanceRequestModel>>(data).ToList();
            return Json(new { data = model });
        }


        [HttpPost]
        public async Task<JsonResult> ApproveRejectCompOff(long compOffId, long attendanceId, long employeeId, string status, string approveRejectComment, DateTime startDate, DateTime endDate, DateTime workDate, int attendanceStatusId, string actionText)
        {

            var modifiedBy = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var RoleId = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            var managerFirstName = HttpContext.Session.GetString(Constants.FirstName);
            string manager2Email = HttpContext.Session.GetString(Constants.Manager2Email) ?? string.Empty;
            string bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
            EmployeePersonalDetailsById employeeobj = new EmployeePersonalDetailsById();
            employeeobj.EmployeeID = employeeId;
            // Get employee details
            var employeeApiResponse = _businessLayer.SendPostAPIRequest(
                employeeobj,
             await   _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeeDetails),
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



            // Submit updated attendance
            var updateApiResponse = _businessLayer.SendPostAPIRequest(
                attendanceModel,
             await   _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateCompOffAttendace),
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
                To no longer receive messages from Eternity Logistics, please click to <strong><a href='http://unsubscribe.eternitylogistics.co/'> Unsubscribe </a></strong>.<br/><br/>

                If you are happy with our services or want to share any feedback, do email us at 
                <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

                All email correspondence is sent only through our official domain: 
                <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

                <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
                If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
                Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
                This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
                Your cooperation is greatly appreciated.
            </p>
        </div>",

                            EmailToList = new List<string> { manager2Email }
                        };
                        EmailSender.SendEmail(managerEmailProps);
                    }

                    // Email to employee
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress))
                    {
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
   Hi, {employeeResult.EmployeeName}, your CompOff has been approved.
    <p style='color: #000; font-size: 13px;'>
        To no longer receive messages from Eternity Logistics, please click to 
        <strong><a href='http://unsubscribe.eternitylogistics.co/'>Unsubscribe</a></strong>.<br/><br/>

        If you are happy with our services or want to share any feedback, do email us at 
        <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

        All email correspondence is sent only through our official domain: 
        <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

        <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
        If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
        Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
        This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
        Your cooperation is greatly appreciated.
    </p>";
                    }
                    break;

                case (int)AttendanceStatusId.L2Rejected:
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress))
                    {
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";
                        body = $@"
    Hi {employeeResult.EmployeeName}, your CompOff attendance has been rejected.
    <p style='color: #000; font-size: 13px;'>
        To no longer receive messages from Eternity Logistics, please click to 
        <strong><a href='http://unsubscribe.eternitylogistics.co/'>Unsubscribe</a></strong>.<br/><br/>

        If you are happy with our services or want to share any feedback, do email us at 
        <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

        All email correspondence is sent only through our official domain: 
        <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

        <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
        If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
        Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
        This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
        Your cooperation is greatly appreciated.
    </p>";
                    }
                    break;

                default:
                    if (!string.IsNullOrEmpty(employeeResult?.PersonalEmailAddress))
                    {
                        emailList.Add(employeeResult.PersonalEmailAddress);
                        subject = "Attendance request status";

                        body = $@"
Hi, {employeeResult.EmployeeName}, your CompOff attendance has been updated.
    <p style='color: #000; font-size: 13px;'>
        To no longer receive messages from Eternity Logistics, please click to 
        <strong><a href='http://unsubscribe.eternitylogistics.co/'>Unsubscribe</a></strong>.<br/><br/>

        If you are happy with our services or want to share any feedback, do email us at 
        <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

        All email correspondence is sent only through our official domain: 
        <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

        <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
        If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
        Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
        This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
        Your cooperation is greatly appreciated.
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
        private async Task<EmployeeModel> GetEmployeeDetails(long companyId, long employeeId)
        {
            var employeeDetailsJson = _businessLayer.SendPostAPIRequest(new EmployeeInputParams { CompanyID = companyId, EmployeeID = employeeId },await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).ToString();
            var employeeDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(employeeDetailsJson).employeeModel;
            return employeeDetails;
        }

        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }
    }

}
