using HRMS.Models.Common;
using HRMS.Web.BusinessLayer;
using HRMS.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using HRMS.Models.DashBoard;

namespace HRMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _context;

        IBusinessLayer _businessLayer;
        public HomeController(ILogger<HomeController> logger, IBusinessLayer businessLayer, IHttpContextAccessor context)
        {
            _logger = logger;
            _businessLayer = businessLayer;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult ResetPassword(string un)
        {
            ResetPasswordModel model = new ResetPasswordModel();

            model.UserID = un;
            return View(model);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var _LoginService = new LoginService();
            try
            {
                if (_LoginService.CheckUserByUserID(model.UserID))
                {
                    var cus = _LoginService.ChangeUserPassword(model.UserID, model.Password);

                    ViewBag.Success = "Password changed successfully.";
                }
                else
                {
                    ViewBag.Error = "Some error occured. Please try again!";
                }
            }
            catch (Exception ce)
            {
                ViewBag.Error = "Some error occured. Please try again!";
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Index(LoginUser loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return LoginAndRedirect(loginModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Some error occured, please try later.";
            }
            return View(loginModel);
        }

        private IActionResult LoginAndRedirect(LoginUser loginModel)
        {           
            var data = _businessLayer.SendPostAPIRequest(loginModel, "Login", HttpContext.Session.GetString(Constants.SessionBearerToken), false).Result.ToString();
            var result = JsonConvert.DeserializeObject<LoginUser>(data);

            if (result != null && !string.IsNullOrEmpty(result.token))
            {
                _context.HttpContext.Session.SetString(Constants.SessionBearerToken, result.token);
                _context.HttpContext.Session.SetString(Constants.UserID, result.UserID.ToString());
                _context.HttpContext.Session.SetString(Constants.CompanyID, result.CompanyID.ToString());
                _context.HttpContext.Session.SetString(Constants.EmployeeID, result.EmployeeID.ToString());
                _context.HttpContext.Session.SetString(Constants.AreaName, _businessLayer.GetAreaNameByRole(result.RoleId));
                var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, result.UserID.ToString()),
                     new Claim(ClaimTypes.Role,  result.Role)
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                DashBoardModelInputParams dashBoardModelInputParams = new DashBoardModelInputParams() { EmployeeID = long.Parse(HttpContext.Session.GetString(Constants.EmployeeID)) };
                var dataDashBoardModel = _businessLayer.SendPostAPIRequest(dashBoardModelInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetDashBoardodel), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var model = JsonConvert.DeserializeObject<DashBoardModel>(dataDashBoardModel);
                if (string.IsNullOrEmpty(model.ProfilePhoto)) {
                    model.ProfilePhoto = "";
                }
                _context.HttpContext.Session.SetString(Constants.ProfilePhoto, model.ProfilePhoto);
                _context.HttpContext.Session.SetString(Constants.FirstName, model.FirstName);
                _context.HttpContext.Session.SetString(Constants.MiddleName, model.MiddleName);
                _context.HttpContext.Session.SetString(Constants.Surname, model.Surname);
                _context.HttpContext.Session.SetString(Constants.OfficialEmailID, model.OfficialEmailID);
                return RedirectToActionPermanent(
                   Constants.Index,
                  _businessLayer.GetControllarNameByRole(result.RoleId),
                  new { area = _businessLayer.GetAreaNameByRole(result.RoleId) }
               );
            }
            else
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Invalid login credentials, Please try with correct user name and password.";               
            }
            return View("Index", loginModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
