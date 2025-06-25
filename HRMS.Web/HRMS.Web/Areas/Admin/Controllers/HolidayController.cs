using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Results = HRMS.Models.Common.Results;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize]
    public class HolidayController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;

        public HolidayController(ICheckUserFormPermission CheckUserFormPermission,IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
            _CheckUserFormPermission = CheckUserFormPermission;
        }
        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }
        public IActionResult HolidayListing()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.HolidayListing);
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
        public JsonResult HolidayListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long? locationId)
        {
            HolidayInputParams HolidayParams = new HolidayInputParams();
            HolidayParams.LocationID = locationId;
            HolidayParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(HolidayParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidayList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);
            results.Holiday.ForEach(x => x.EncodedId = _businessLayer.EncodeStringBase64(x.HolidayID.ToString()));
            return Json(new
            {
                data = results.Holiday,
                locations = results.JobLocationList.Select(j => new
                {
                    jobLocationID = j.Value,   
                    jobLocationName = j.Text
                })
            });




        }

        public IActionResult Index(string id)
        {
            HolidayModel HolidayModel = new HolidayModel();
            HolidayModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            HolidayModel.FromDate = DateTime.Now;
            HolidayModel.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                HolidayModel.HolidayID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(HolidayModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidays), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                HolidayModel = JsonConvert.DeserializeObject<Results>(data).holidayModel;
                var holidayResult = JsonConvert.DeserializeObject<Results>(data);
                HolidayModel = holidayResult.holidayModel ?? new HolidayModel();
                HolidayModel.JobLocationList = holidayResult.JobLocationList;

            }
            else
            {

                var data = _businessLayer.SendPostAPIRequest(
                    HolidayModel,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidays),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                var holidayResult = JsonConvert.DeserializeObject<Results>(data);
                HolidayModel.JobLocationList = holidayResult.JobLocationList;
            }
            return View(HolidayModel);
        }

        [HttpPost]
        public IActionResult Index(HolidayModel HolidayModel)
        {
            if (ModelState.IsValid)
            {
                HolidayModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

                var data = _businessLayer.SendPostAPIRequest(HolidayModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.AddUpdateHoliday), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);

                if (HolidayModel.HolidayID > 0)
                {
                    SetSuccessToast("Holiday data Modified successfully.");
                    return RedirectToActionPermanent(WebControllarsConstants.HolidayListing, WebControllarsConstants.Holiday);
                }
                else
                {
                    SetSuccessToast("Holiday details Added successfully");
                    return RedirectToActionPermanent(WebControllarsConstants.HolidayListing, WebControllarsConstants.Holiday);
                }
            }
            else
            {
                SetWarningToast("Please check all data and try again.");
                return View(HolidayModel);
            }
        }

        private void SetSuccessToast(string message)
        {
            TempData[Constants.toastType] = Constants.toastTypeSuccess;
            TempData[Constants.toastMessage] = message;
        }

        private void SetWarningToast(string message)
        {
            TempData[Constants.toastType] = Constants.toastTypetWarning;
            TempData[Constants.toastMessage] = message;
        }
         
    }
}
