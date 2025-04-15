using HRMS.Models.Common;
using HRMS.Web.BusinessLayer;
using HRMS.Models.AttendenceList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Results = HRMS.Models.Common.Results;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRMS.Models.Company;
using HRMS.Models.LeavePolicy;
using DocumentFormat.OpenXml.Wordprocessing;
<<<<<<< HEAD
using HRMS.Models.DashBoard;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using HRMS.Models;
=======
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
<<<<<<< HEAD
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR + "," + RoleConstants.SuperAdmin+ "," + RoleConstants.Manager)]
=======
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR)]
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
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
            Results results = new Results();
            return View(results);
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetAllAttendenceList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
<<<<<<< HEAD
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
             attendenceListParams.Month = DateTime.Now.Month;
            //attendenceListParams.Month =1;
            attendenceListParams.Year = DateTime.Now.Year;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var data = _businessLayer.SendPostAPIRequest(attendenceListParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
   
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidays>(data);

            return Json(new { data = model });
=======
            AttendenceListInputParans attendenceListParams = new AttendenceListInputParans();
            attendenceListParams.ID = Convert.ToInt64(HttpContext.Session.GetString(Constants.ID));

            var data = _businessLayer.SendPostAPIRequest(attendenceListParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAllAttendenceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);

            return Json(new { data = results.AttendenceList });
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39

        }
        public IActionResult Index(string id)
        {
<<<<<<< HEAD
            Attendance model = new Attendance();
=======
            AttendenceListModel model = new AttendenceListModel();
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39

            if (!string.IsNullOrEmpty(id))
            {
                model.ID = Convert.ToInt64(id);
<<<<<<< HEAD
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendenceListID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).AttendanceModel;
=======
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAllAttendenceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).AttendenceListModel;
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39

            }

            HRMS.Models.Common.Results results = GetAllEmployees(Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)));
            model.Employeelist = results.Employee;
<<<<<<< HEAD
=======
            model.StatusList = new SelectList(Enum.GetValues(typeof(Status)));
            model.ShiftList = Enum.GetValues(typeof(ShiftSelection)).Cast<ShiftSelection>()
           .Select(e => new SelectListItem
           {
               Value = ((int)e).ToString(),
               Text = e.ToString(),
           })
          .ToList();
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
            return View(model);
        }



        [HttpPost]
<<<<<<< HEAD
        public IActionResult Index(Attendance AttendenceListModel)
        {
            AttendenceListModel.WorkDate = AttendenceListModel.FirstLogDate;
                var data = _businessLayer.SendPostAPIRequest(AttendenceListModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
=======
        public IActionResult Index(AttendenceListModel AttendenceListModel)
        {
            if (ModelState.IsValid)
            {

                var data = _businessLayer.SendPostAPIRequest(AttendenceListModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendenceList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
                var result = JsonConvert.DeserializeObject<Result>(data);

                if (AttendenceListModel.ID > 0)
                {

                    return RedirectToAction("AttendenceListing");/*Constants.Index, WebControllarsConstants.AttendenceList, new { id = AttendenceListModel.ID.ToString() });*/

                }
                else
                {
                    return RedirectToActionPermanent(WebControllarsConstants.AttendenceListing, WebControllarsConstants.AttendenceList);

                }

<<<<<<< HEAD
           
=======
            }
            else
            {
                return View(AttendenceListModel);
            }
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39



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
<<<<<<< HEAD

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
            return RedirectToActionPermanent(WebControllarsConstants.AttendenceListing, WebControllarsConstants.AttendenceList);
        }


        [HttpGet]
        [AllowAnonymous] 
        public IActionResult AttendenceList()
        {
            
            return View( );
        }
        [HttpGet]
        [AllowAnonymous]
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
    

=======
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
    }



}

