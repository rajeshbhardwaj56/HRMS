using HRMS.Models.Common;
using HRMS.Models.ShiftType;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Results = HRMS.Models.Common.Results;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]

    public class ShiftTypeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        public ShiftTypeController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        public IActionResult ShiftTypeListing()
        {
            Results results = new Results();
            return View(results);
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult ShiftTypeListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
          ShiftTypeInputParans shiftTypeParams = new ShiftTypeInputParans();
            shiftTypeParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            shiftTypeParams.DisplayStart = iDisplayStart;
            shiftTypeParams.DisplayLength = iDisplayLength;
            shiftTypeParams.Searching = string.IsNullOrEmpty(sSearch) ? null : sSearch;
            var data = _businessLayer.SendPostAPIRequest(shiftTypeParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.ShiftType, APIApiActionConstants.GetAllShiftTypes), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);
            return Json(new {
                draw = sEcho,
                recordsTotal = results.ShiftType.Select(x => x.TotalRecords).FirstOrDefault() ?? 0,
                recordsFiltered = results.ShiftType.Select(x => x.FilteredRecords).FirstOrDefault() ?? 0,
                data = results.ShiftType });
        }
        public IActionResult Index(string id)
        {
            ShiftTypeModel shiftTypeModel = new ShiftTypeModel();
            shiftTypeModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            if (!string.IsNullOrEmpty(id))
            {
                shiftTypeModel.ShiftTypeID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(shiftTypeModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.ShiftType, APIApiActionConstants.GetAllShiftTypes), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                shiftTypeModel = JsonConvert.DeserializeObject<Results>(data).shiftTypeModel;
            }
            var holidayListData = _businessLayer.SendPostAPIRequest(shiftTypeModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetHolidayList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (holidayListData != null)
            {
                shiftTypeModel.HolidayList = JsonConvert.DeserializeObject<List<SelectListItem>>(holidayListData);
            }
            shiftTypeModel.WorkingHoursCalculationTypeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "first_check_in_and_last_check_out", Text = "First check in and last check out" },
                new SelectListItem { Value = "every_check_in_and_check_out", Text = "Every check in and check out" }
            };

            return View(shiftTypeModel);
        }
        [HttpPost]
        public IActionResult Index(ShiftTypeModel shiftTypeModel)
        {
            if (ModelState.IsValid)
            {
                shiftTypeModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

                var data = _businessLayer.SendPostAPIRequest(shiftTypeModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.ShiftType, APIApiActionConstants.AddUpdateShiftType), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var results = JsonConvert.DeserializeObject<Result>(data);

                if (shiftTypeModel.ShiftTypeID > 0)
                {
                    return RedirectToActionPermanent(WebControllarsConstants.ShiftTypeListing, WebControllarsConstants.ShiftType);
                    //return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.ShiftType, new { id = shiftTypeModel.ShiftTypeID.ToString() });
                }
                else
                {
                    return RedirectToActionPermanent(WebControllarsConstants.ShiftTypeListing, WebControllarsConstants.ShiftType);
                }
            }
            else
            {
                return View(shiftTypeModel);
            }
        }
    }
}