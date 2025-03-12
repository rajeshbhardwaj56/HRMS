using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using HRMS.Models.MyInfo;
using HRMS.Models.ShiftType;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;

namespace HRMS.Web.Areas.Employee.Controllers
{
    [Area(Constants.ManageEmployee)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Employee + "," + RoleConstants.Manager + "," + RoleConstants.SuperAdmin))]
    public class AttendenceListController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;

        public AttendenceListController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
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
        //public IActionResult Index(string id)
        //{
        //    AttendenceListModel model = new AttendenceListModel();

        //    if (!string.IsNullOrEmpty(id))
        //    {
        //        model.ID = Convert.ToInt64(id);
        //        var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAllAttendenceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
        //        model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).AttendenceListModel;

        //    }

        //    HRMS.Models.Common.Results results = GetAllEmployees(Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)));
        //    model.Employeelist = results.Employee;
        //    model.StatusList = new SelectList(Enum.GetValues(typeof(Status)));
        //    model.ShiftList = Enum.GetValues(typeof(ShiftSelection)).Cast<ShiftSelection>()
        //   .Select(e => new SelectListItem
        //   {
        //       Value = ((int)e).ToString(),
        //       Text = e.ToString(),
        //   })
        //  .ToList();
        //    return View(model);
        //}
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
                    return RedirectToActionPermanent(WebControllarsConstants.AttendenceListing, WebControllarsConstants.AttendenceList);

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
            return View();
        }
        [HttpGet]
        public IActionResult AttendenceCalendarList(int year, int month)
        {
            var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var employeeName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
            var employeeMiddleName = Convert.ToString(HttpContext.Session.GetString(Constants.MiddleName));
            var employeeLastName = Convert.ToString(HttpContext.Session.GetString(Constants.Surname));

            // Concatenate full name
            var employeeFullName = string.Join(" ", new[] { employeeName, employeeMiddleName, employeeLastName }.Where(name => !string.IsNullOrWhiteSpace(name)));

            AttendanceInputParams models = new AttendanceInputParams
            {
                Year = year,
                Month = month,
                UserId = employeeId,
            };

            var data = _businessLayer.SendPostAPIRequest(models, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendanceForCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidays>(data);

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
            return Json(new { data = model });
        }
        [HttpGet]
        public IActionResult MyAttendance(string id)
        {
            Attendance model = new Attendance();

            if (!string.IsNullOrEmpty(id))
            {
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
            AttendenceListModel.WorkDate = DateTime.Today;
            var UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            AttendenceListModel.UserId = UserId.ToString();
            AttendenceListModel.IsManual = true;
            AttendenceListModel.AttendanceStatus = AttendanceStatus.Submitted.ToString();
            var data = _businessLayer.SendPostAPIRequest(AttendenceListModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var result = JsonConvert.DeserializeObject<Result>(data);
            if (result != null && result.Message.Contains("Record for this user with the same date already exists!", StringComparison.OrdinalIgnoreCase))
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
            }
            else
            {
                var Manager1Email = HttpContext.Session.GetString(Constants.Manager1Email).ToString();
                var Manager2Email = HttpContext.Session.GetString(Constants.Manager2Email).ToString();
                var EmployeeFirstName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
                sendEmailProperties sendEmailProperties = new sendEmailProperties();
                sendEmailProperties.emailSubject = "Send a request for attendance approval";
                sendEmailProperties.emailBody = ("Hii," + EmployeeFirstName + ' ' + "Send a request for attendance approval");
                sendEmailProperties.EmailToList.Add(Manager1Email);
                //sendEmailProperties.EmailToList.Add(Manager2Email);
                emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);
                if (responses.responseCode == "200")
                {
                }
                else
                {
                }
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
            }
            return View(AttendenceListModel);
        }
        [HttpGet]
        public IActionResult DeleteAttendanceDetails(int id)
        {
            Attendance model = new Attendance()
            {
                ID = id,
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.DeleteAttendanceDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(WebControllarsConstants.MyAttendanceList, WebControllarsConstants.AttendenceList);
        }
        public IActionResult TeamAttendenceList()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }


        [HttpPost]
        public JsonResult ApproveRejectAttendance(long employeeID, bool isApproved, string ApproveRejectComment)
        {
            var attendanceListModel = new Attendance
            {
                WorkDate = DateTime.Today,
                UserId = employeeID.ToString(),               
                AttendanceStatus = AttendanceStatus.Submitted.ToString()
            };

            // Send request to API
            var data = _businessLayer.SendPostAPIRequest(
                attendanceListModel,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var result = JsonConvert.DeserializeObject<Result>(data);         
            if (result != null && result.Message.Contains("Record for this user with the same date already exists!", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = result.Message });
            }
            else
            {
            
                var manager1Email = HttpContext.Session.GetString(Constants.Manager1Email);
                var employeeFirstName = HttpContext.Session.GetString(Constants.FirstName);

                sendEmailProperties sendEmailProperties = new sendEmailProperties
                {
                    emailSubject = "Send a request for attendance approval",
                    emailBody = $"Hi, {employeeFirstName} has sent a request for attendance approval."
                };
                sendEmailProperties.EmailToList.Add(manager1Email);

             
                emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);

                
                if (responses.responseCode == "200")
                {
                    return Json(new { success = true, message = "Attendance approved/rejected successfully and email sent." });
                }
                else
                {
                    return Json(new { success = false, message = "Attendance approved/rejected, but email sending failed." });
                }
            }
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
            var data = _businessLayer.SendPostAPIRequest(attendenceListParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidays>(data);
            return Json(new { data = model });

        }

        public IActionResult ApprovedAttendance()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetApprovedAttendance(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
            attendenceListParams.Status = AttendanceStatus.Approved.ToString();
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(attendenceListParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetApprovedAttendance), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<MyAttendanceList>(data);
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
           // EmployeeAttendanceModel.EmployeeNumber = employeeDetails.EmployeeNumber;
            EmployeeAttendanceModel.EmployeeJoiningdate = employeeDetails.JoiningDate.Value.ToString("dd/MM/yyyy");
            //EmployeeAttendanceModel.EmployeeDesignation = employeeDetails.DesignationName;
            //EmployeeAttendanceModel.EmployeeDepartment = employeeDetails.DepartmentName;
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
      
        public JsonResult GetTeamAttendanceLogs(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            AttendanceDeviceLog attendenceDeviceLogs = new AttendanceDeviceLog();                               
            attendenceDeviceLogs.CreatedBy = HttpContext.Session.GetString(Constants.EmployeeID);
            var data = _businessLayer.SendPostAPIRequest(attendenceDeviceLogs, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendanceDeviceLogs), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceDeviceLog>(data);
            return Json(new { data = model });
        }


        private EmployeeModel GetEmployeeDetails(long companyId, long employeeId)
        {
            var employeeDetailsJson = _businessLayer.SendPostAPIRequest(new EmployeeInputParams { CompanyID = companyId, EmployeeID = employeeId }, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var employeeDetails = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(employeeDetailsJson).employeeModel;
            return employeeDetails;
        }
    }

}
