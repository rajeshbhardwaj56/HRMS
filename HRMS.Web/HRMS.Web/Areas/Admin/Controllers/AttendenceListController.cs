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
using HRMS.Models.DashBoard;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using HRMS.Models;
using System.Threading.Tasks;
using HRMS.Models.WhatsHappening;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]

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
        public async Task<JsonResult> GetAllAttendenceList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            AttendanceInputParams attendenceListParams = new AttendanceInputParams();
             attendenceListParams.Month = DateTime.Now.Month;
            attendenceListParams.Year = DateTime.Now.Year;
            attendenceListParams.UserId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetTeamAttendanceForCalendar);
            var apiResponse = await _businessLayer.SendPostAPIRequest(
                attendenceListParams,
              apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );
            var data = apiResponse?.ToString();   
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidays>(data);
            return Json(new { data = model });
        }
        public async Task<IActionResult> Index(string id)
        {
            Attendance model = new Attendance();

            if (!string.IsNullOrEmpty(id))
            {
                model.ID = Convert.ToInt64(id);
                var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendenceListID);
                var apiResponse = await _businessLayer.SendPostAPIRequest(
                    model,
                  apiUrl,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                );
                var data = apiResponse?.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).AttendanceModel;

            }

            HRMS.Models.Common.Results results = await GetAllEmployees(Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)));
            model.Employeelist = results.Employee;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Index(Attendance AttendenceListModel)
        {
            AttendenceListModel.WorkDate = AttendenceListModel.FirstLogDate;
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.AddUpdateAttendace);
            var apiResponse = await _businessLayer.SendPostAPIRequest(
                AttendenceListModel,
              apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );
            var data = apiResponse?.ToString();
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
        public async Task<HRMS.Models.Common.Results> GetAllEmployees(long EmployeeID)
        {
            HRMS.Models.Common.Results result = null;
            var data = "";
            if (HttpContext.Session.GetString(Constants.ResultsData) != null)
            {
                data = HttpContext.Session.GetString(Constants.ResultsData);
            }
            else
            {
                var apiResponse = await _businessLayer.SendGetAPIRequest("Common/GetAllEmployees?EmployeeID=" + EmployeeID, HttpContext.Session.GetString(Constants.SessionBearerToken), true);
                data = apiResponse?.ToString();
            }
            HttpContext.Session.SetString(Constants.ResultsData, data);
            result = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAttendanceDetails(int id)
        {
            Attendance model = new Attendance()
            {
                ID = id,
            };
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.DeleteAttendanceDetails);
            var apiResponse = await _businessLayer.SendPostAPIRequest(
                model,
              apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );
            var data = apiResponse?.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(WebControllarsConstants.AttendenceListing, WebControllarsConstants.Attendance);
        }


        [HttpGet]
        [AllowAnonymous] 
        public IActionResult AttendenceList()
        {            
            return View( );
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AttendenceCalendarList(int year, int month)
        {
            var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var employeeName = Convert.ToString(HttpContext.Session.GetString(Constants.FirstName));
            var employeeMiddleName = Convert.ToString(HttpContext.Session.GetString(Constants.MiddleName));
            var employeeLastName = Convert.ToString(HttpContext.Session.GetString(Constants.Surname));         
            var employeeFullName = string.Join(" ", new[] { employeeName, employeeMiddleName, employeeLastName }.Where(name => !string.IsNullOrWhiteSpace(name)));

            AttendanceInputParams models = new AttendanceInputParams
            {
                Year = year,
                Month = month,
                UserId = employeeId,
            };
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendanceForMonthlyViewCalendar);
            var apiResponse = await _businessLayer.SendPostAPIRequest(
                models,
              apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );
            var data = apiResponse?.ToString();
            var model = JsonConvert.DeserializeObject<AttendanceWithHolidays>(data);

            return Json(new { data = model, employeeFullName = employeeFullName });
        }
    

    }



}

