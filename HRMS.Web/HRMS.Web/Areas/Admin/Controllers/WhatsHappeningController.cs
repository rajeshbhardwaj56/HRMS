using System.Threading.Tasks;
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using HRMS.Models.WhatsHappening;
using HRMS.Web.BusinessLayer;
using HRMS.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]

    public class WhatsHappeningController : Controller
    {
        private readonly ILogger<WhatsHappeningController> _logger;
        private readonly IHttpContextAccessor _context;
        private IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        public WhatsHappeningController(ILogger<WhatsHappeningController> logger, IBusinessLayer businessLayer, IHttpContextAccessor context, IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _businessLayer = businessLayer;
            _context = context;
            _configuration =
            _configuration = configuration;
            Environment = hostingEnvironment;

        }
        public async Task<IActionResult> Index(String id)
        {
            WhatsHappening whatsHappening = new WhatsHappening();
            if (!string.IsNullOrEmpty(id))
            {
                whatsHappening.WhatsHappeningID = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
                var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetWhatsHappenings);
                var apiResponse = await _businessLayer.SendPostAPIRequest(
                    whatsHappening,
                  apiUrl,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                );
                var data = apiResponse?.ToString();
                whatsHappening = JsonConvert.DeserializeObject<WhatsHappeningModel>(data)._WhatsHappenings.Where(x => x.WhatsHappeningID == whatsHappening.WhatsHappeningID).FirstOrDefault();
            }
            return View(whatsHappening);
        }



        public async Task<IActionResult> Manage()
        {
            WhatsHappening whatsHappening = new WhatsHappening();
            whatsHappening.WhatsHappeningID = 0;
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetWhatsHappenings);
            var apiResponse = await _businessLayer.SendPostAPIRequest(
                whatsHappening,
              apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );
            var data = apiResponse?.ToString(); 
            var result = JsonConvert.DeserializeObject<WhatsHappeningModel>(data);
            return View(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> WhatsHappeningListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            WhatsHappening whatsHappening = new WhatsHappening();
            whatsHappening.WhatsHappeningID = 0;
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetWhatsHappenings);
            var apiResponse = await _businessLayer.SendPostAPIRequest(
                whatsHappening,
              apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );
            var data = apiResponse?.ToString();
            var results = JsonConvert.DeserializeObject<WhatsHappeningModel>(data);
            results._WhatsHappenings.ForEach(x =>
            {
                x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.WhatsHappeningID.ToString());
                x.StringFromDate = Convert.ToDateTime(x.FromDate).ToString("dd/MM/yyyy");
                x.StringToDate = Convert.ToDateTime(x.ToDate).ToString("dd/MM/yyyy");   
            });
            return Json(new { data = results._WhatsHappenings });
        }


        [HttpPost]
        public async Task<IActionResult> Index(WhatsHappening whatsHappening, List<IFormFile> postedFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string fileName = null;
                    foreach (IFormFile postedFile in postedFiles)
                    {
                        fileName = postedFile.FileName.Replace(" ", "");
                    }
                    whatsHappening.IconImage = fileName;
                    whatsHappening.CompanyID = long.Parse(HttpContext.Session.GetString(Constants.CompanyID));
                    whatsHappening.UserID = long.Parse(HttpContext.Session.GetString(Constants.UserID));
                    var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.AddUpdateWhatsHappening);
                    var apiResponse = await _businessLayer.SendPostAPIRequest(
                        whatsHappening,
                      apiUrl,
                        HttpContext.Session.GetString(Constants.SessionBearerToken),
                        true
                    );
                    var data = apiResponse?.ToString();
                    var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);

                    string path = Path.Combine(this.Environment.WebRootPath, Constants.WhatHapenningIconPath + result.PKNo.ToString());

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
                    whatsHappening = new WhatsHappening();
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Data saved successfully.";
                }
                else
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Please check all data and try again.";
                }

            }
            catch (Exception ex)
            {

            }
            return RedirectToActionPermanent("Index");
            // return View(whatsHappening);
        }
    }
}
