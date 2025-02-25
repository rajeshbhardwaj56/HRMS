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
using HRMS.Models.User;
using HRMS.Models.Employee;

namespace HRMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _context;
        private IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public HomeController(ILogger<HomeController> logger, IBusinessLayer businessLayer, IHttpContextAccessor context, IConfiguration configuration)
        {
            _logger = logger;
            _businessLayer = businessLayer;
            _context = context;
            _configuration =
            _configuration = configuration;
            EmailSender.configuration = _configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult ForgotPassword()
        {
            ChangePasswordModel model = new ChangePasswordModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult ForgotPassword(ChangePasswordModel model)
        {
            try
            {
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.GetFogotPasswordDetails), null, false).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);
                UserModel userModel = JsonConvert.DeserializeObject<UserModel>((string)result.Data);

                sendEmailProperties sendEmailProperties = new sendEmailProperties();
                sendEmailProperties.emailSubject = "Reset Password Email";
                sendEmailProperties.emailBody = ("Hi, <br/><br/> Please click on below link to reset password. <br/> <a target='_blank' href='" + string.Format(_configuration["AppSettings:RootUrl"] + _configuration["AppSettings:ResetPasswordURL"], _businessLayer.EncodeStringBase64((userModel.EmployeeID == null ? "" : userModel.EmployeeID.ToString()).ToString()), _businessLayer.EncodeStringBase64(DateTime.Now.ToString())) + "'> Click here to reset password</a>" + "<br/><br/>");
                sendEmailProperties.EmailToList.Add(userModel.UserName);
                emailSendResponse response = EmailSender.SendEmail(sendEmailProperties);
                if (response.responseCode == "200")
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email have been sent, Please reset password for Login.";
                }
                else
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email sending failed, Please try again later.";
                }
            }
            catch (Exception ce)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Some error occured, please try later.";
            }
            finally
            {

            }
            return View(model);

        }

        public ActionResult ResetPassword(string Id, string dt, string cm)
        {
            ResetPasswordModel model = new ResetPasswordModel();
            try
            {
                DateTime date = Convert.ToDateTime(_businessLayer.DecodeStringBase64(dt));
                model.EmployeeID = _businessLayer.DecodeStringBase64(Id);
                model.UserID = _businessLayer.DecodeStringBase64(Id);
                model.CompanyID = _businessLayer.DecodeStringBase64(cm);
                model.dt = date;

                if ((DateTime.Now - date).Days > 1)
                {
                    TempData[HRMS.Models.Common.Constants.IsLinkExpired] = true;
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password link is expired.";
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            try
            {

                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.ResetPassword), null, false).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
            }
            catch (Exception ce)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Some error occured, please try later.";
            }
            finally
            {

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
            finally
            {

            }
            return View(loginModel);
        }

        private IActionResult LoginAndRedirect(LoginUser loginModel)
        {
            var data = _businessLayer.SendPostAPIRequest(loginModel, "Login", HttpContext.Session.GetString(Constants.SessionBearerToken), false).Result.ToString();
            var result = JsonConvert.DeserializeObject<LoginUser>(data);

            if (result != null && !string.IsNullOrEmpty(result.token))
            {
                EmployeeInputParams objmodel = new EmployeeInputParams();
                objmodel.CompanyID = result.CompanyID;


                _context.HttpContext.Session.SetString(Constants.SessionBearerToken, result.token);
                _context.HttpContext.Session.SetString(Constants.UserID, result.UserID.ToString());
                _context.HttpContext.Session.SetString(Constants.CompanyID, result.CompanyID.ToString());
                _context.HttpContext.Session.SetString(Constants.EmployeeID, result.EmployeeID.ToString());
                _context.HttpContext.Session.SetString(Constants.Gender, result.GenderId.ToString());
                _context.HttpContext.Session.SetString(Constants.RoleID, result.RoleId.ToString());
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
                if (string.IsNullOrEmpty(model.ProfilePhoto))
                {
                    model.ProfilePhoto = "";
                }
                _context.HttpContext.Session.SetString(Constants.ProfilePhoto, model.ProfilePhoto);
                _context.HttpContext.Session.SetString(Constants.FirstName, model.FirstName);
                _context.HttpContext.Session.SetString(Constants.MiddleName, model.MiddleName);
                _context.HttpContext.Session.SetString(Constants.Surname, model.Surname);
                _context.HttpContext.Session.SetString(Constants.OfficialEmailID, model.OfficialEmailID);
                var CompanyDatas = _businessLayer.SendPostAPIRequest(objmodel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetAllCompanies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(CompanyDatas);
                var CompanyData = results.companyModel;
                var CompanyLogo = "/Uploads/CompanyLogo/" + CompanyData.CompanyID + "/"+ CompanyData.CompanyLogo;
                _context.HttpContext.Session.SetString(Constants.CompanyLogo, CompanyLogo);

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
