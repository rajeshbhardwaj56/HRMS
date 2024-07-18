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
    [Authorize(Roles = (RoleConstants.Admin + "," + RoleConstants.HR))]
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
        public IActionResult Index(String id)
        {
            WhatsHappening whatsHappening = new WhatsHappening();
            if (!string.IsNullOrEmpty(id))
            {
                whatsHappening.WhatsHappeningID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(whatsHappening, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetWhatsHappenings), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                whatsHappening = JsonConvert.DeserializeObject<WhatsHappeningModel>(data)._WhatsHappening;
            }
            return View(whatsHappening);
        }

        [HttpPost]
        public IActionResult Index(WhatsHappening whatsHappening, List<IFormFile> postedFiles)
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
                var data = _businessLayer.SendPostAPIRequest(whatsHappening, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.AddUpdateWhatsHappening), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                //whatsHappening = JsonConvert.DeserializeObject<WhatsHappeningModel>(data)._WhatsHappening;
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

                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Data saved successfully.";
            }
            else
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Please check all data and try again.";
            }
            return View(whatsHappening);
        }
    }
}
