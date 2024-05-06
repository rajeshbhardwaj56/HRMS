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

        //[HttpPost]
        //public ActionResult Index(LoginUser LoginUser)
        //{

        //    var data = _businessLayer.SendPostAPIRequest(LoginUser, "Login", HttpContext.Session.GetString(Constants.SessionBearerToken), false).Result.ToString();
        //    LoginUser = JsonConvert.DeserializeObject<LoginUser>(data);
        //    if (LoginUser.Result == "-1")
        //    {

        //    }
        //    else
        //    {

        //        HttpContext.Session.SetString(Constants.SessionBearerToken, LoginUser.token);
        //        HttpContext.Session.SetString(Constants.UserName, LoginUser.FirstName + " " + LoginUser.LastName);
        //        HttpContext.Session.SetString(Constants.Email, LoginUser.Email);
        //        HttpContext.Session.SetString(Constants.UserID, LoginUser.UserID.ToString());
        //        HttpContext.Session.SetString(Constants.RoleID, LoginUser.RoleId.ToString());
        //        HttpContext.Session.SetString(Constants.Role, LoginUser.Role);
        //        HttpContext.Session.SetString(Constants.Alias, LoginUser.FirstName);
        //        // FormsAuthentication.SetAuthCookie(LoginUser.Email, false);
        //        TempData[Constants.RoleID] = LoginUser.RoleId;
        //        return RedirectToAction("Index", "Home");
        //    }
        //    return View(LoginUser);
        //}


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
