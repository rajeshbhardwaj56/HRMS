using HRMS.Models.Common;
using HRMS.Web.BusinessLayer;
using HRMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace HRMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        IBusinessLayer _businessLayer;
        public HomeController(ILogger<HomeController> logger, IBusinessLayer businessLayer)
        {
            _logger = logger;
            _businessLayer = businessLayer;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginUser LoginUser)
        {

            var data = _businessLayer.SendPostAPIRequest(LoginUser, "Login", HttpContext.Session.GetString(Constants.SessionBearerToken), false).Result.ToString();
            LoginUser = JsonConvert.DeserializeObject<LoginUser>(data);
            if (LoginUser.Result == "-1")
            {

            }
            else
            {

                HttpContext.Session.SetString(Constants.SessionBearerToken, LoginUser.token);
                HttpContext.Session.SetString(Constants.UserName, LoginUser.FirstName + " " + LoginUser.LastName);
                HttpContext.Session.SetString(Constants.Email, LoginUser.Email);
                HttpContext.Session.SetString(Constants.UserID, LoginUser.UserID.ToString());
                HttpContext.Session.SetString(Constants.RoleID, LoginUser.RoleId.ToString());
                HttpContext.Session.SetString(Constants.Role, LoginUser.Role);
                HttpContext.Session.SetString(Constants.Alias, LoginUser.FirstName);
                // FormsAuthentication.SetAuthCookie(LoginUser.Email, false);
                TempData[Constants.RoleID] = LoginUser.RoleId;
                return RedirectToAction("Index", "Home");
            }
            return View(LoginUser);
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
