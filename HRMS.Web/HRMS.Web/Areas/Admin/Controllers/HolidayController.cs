﻿using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Results = HRMS.Models.Common.Results;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR + "," + RoleConstants.SuperAdmin)]
    public class HolidayController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;

        public HolidayController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        public IActionResult HolidayListing()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult HolidayListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            HolidayInputParams HolidayParams = new HolidayInputParams();
            HolidayParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(HolidayParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidayList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);
            results.Holiday.ForEach(x => x.EncodedId = _businessLayer.EncodeStringBase64(x.HolidayID.ToString()));

            return Json(new { data = results.Holiday });

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
                    return RedirectToActionPermanent(WebControllarsConstants.HolidayListing, WebControllarsConstants.Holiday);
                }
                else
                {
                    return RedirectToActionPermanent(WebControllarsConstants.HolidayListing, WebControllarsConstants.Holiday);
                }
            }
            else
            {
                return View(HolidayModel);
            }
        }
    }
}
