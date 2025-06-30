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
using HRMS.Web.BusinessLayer.S3;
using DocumentFormat.OpenXml.Office2010.Excel;
using HRMS.Models.Company;
using HRMS.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace HRMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private IS3Service _s3Service;

        IBusinessLayer _businessLayer;
        public HomeController(ILogger<HomeController> logger, IBusinessLayer businessLayer, IHttpContextAccessor context, IConfiguration configuration, IS3Service s3Service)
        {
            _logger = logger;
            _businessLayer = businessLayer;
            _context = context;
            _configuration = configuration;
            _s3Service = s3Service;
            EmailSender.configuration = _configuration;

        }

        public IActionResult Index()
        {
            LoginUser obj = new LoginUser();
            CompanyLoginModel model = new CompanyLoginModel();
            {
                var companyId = _configuration["CompanyDetails:CompanyId"];
                model.CompanyID = Convert.ToInt64(companyId);

                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetCompaniesLogo), " ", false).Result.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).companyLoginModel;
            }
            if (!string.IsNullOrEmpty(model.CompanyLogo))
            {
                model.CompanyLogo = _s3Service.GetFileUrl(model.CompanyLogo);
            }
            obj.CompanyLogo = model.CompanyLogo;
            return View(obj);
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(); // important for authentication cookies

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ForgotPassword()
        {
            ChangePasswordModel objmodel = new ChangePasswordModel();
            CompanyLoginModel model = new CompanyLoginModel();
            {
                var companyId = _configuration["CompanyDetails:CompanyId"];
                model.CompanyID = Convert.ToInt64(companyId);

                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetCompaniesLogo), " ", false).Result.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).companyLoginModel;
            }
            if (!string.IsNullOrEmpty(model.CompanyLogo))
            {
                model.CompanyLogo = _s3Service.GetFileUrl(model.CompanyLogo);
            }
            objmodel.CompanyLogo = model.CompanyLogo;
            return View(objmodel);
        }

        [HttpPost]
        public ActionResult ForgotPassword(ChangePasswordModel model)
        {
            try
            {
                var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.GetFogotPasswordDetails);
                var data = _businessLayer.SendPostAPIRequest(model, apiUrl, null, false).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);

                if (result == null || result.Data == null)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Invalid response from server.";
                    return View(model);
                }

                UserModel userModel = JsonConvert.DeserializeObject<UserModel>((string)result.Data);

                if (userModel != null)
                {
                    if (userModel.EmployeeID > 0)
                    {


                        // Get the configuration values
                        var rootUrl = _configuration["AppSettings:RootUrl"];
                        var resetPasswordUrl = _configuration["AppSettings:ResetPasswordURL"]; // Should have {0}, {1}, {2}


                        // Encode required parameters
                        var encodedTimestamp = _businessLayer.EncodeStringBase64(DateTime.Now.ToString());
                        var encodedCompany = _businessLayer.EncodeStringBase64("YourCompanyValue"); // Replace with actual company ID
                        // Format the Reset Password URL correctly
                        var formattedResetUrl = string.Format(resetPasswordUrl, _businessLayer.EncodeStringBase64(userModel.EmployeeID == null ? "" : userModel.EmployeeID.ToString()), encodedTimestamp, _businessLayer.EncodeStringBase64(userModel.CompanyID.ToString()), _businessLayer.EncodeStringBase64(userModel.UserName.ToString()));

                        // Prepare email content
                        sendEmailProperties sendEmailProperties = new sendEmailProperties
                        {
                            emailSubject = "Reset Password",
                            emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
            Please click on the link below to reset your password.<br/><br/>

            <table style='width: 100%; max-width: 600px; border-collapse: collapse; border: 1px solid #000;'>
                <thead style='background-color: #f2f2f2;'>
                    <tr>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>UserId </th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Name</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Manager</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Department</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style='border: 1px solid #000; padding: 8px;'>{userModel.UserName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{userModel.FullName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{userModel.ManagerName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{userModel.DepartmentName}</td>
                    </tr>
                </tbody>
            </table><br/>

            <a target='_blank' 
               href='{rootUrl}{formattedResetUrl}' 
              >
                Click here to reset password
            </a><br/><br/>

            <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>
        </div>"
                        };
                        // Add recipient email
                        if ((userModel.RoleID == (int)Roles.Employee) && string.IsNullOrEmpty(userModel.Email))
                        {
                            sendEmailProperties.EmailToList.Add(_configuration["AppSettings:ITEmail"]);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(userModel.Email))
                            {
                                sendEmailProperties.EmailToList.Add(_configuration["AppSettings:ITEmail"]);
                            }
                            else
                            {
                                sendEmailProperties.EmailToList.Add(userModel.Email);
                            }
                        }

                        // Send email
                        emailSendResponse response = EmailSender.SendEmail(sendEmailProperties);

                        if (response.responseCode == "200")
                        {
                            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;

                            if ((userModel.RoleID == (int)Roles.Employee))
                            {
                                TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email has been sent to IT Team.";
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(userModel.Email))
                                {
                                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email has been sent to IT Team.";

                                }
                                else
                                {
                                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email has been sent to your email.";

                                }
                            }
                        }
                        else
                        {
                            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                            TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email sending failed. Please try again later.";
                        }
                    }
                }
                else
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Invalid User Please try again.";
                    return View(model);
                }

            }
            catch (Exception ex)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "An error occurred, please try again later.";

                // Log the error (Optional: If using logging in your project)
                Console.WriteLine($"Error in ForgotPassword: {ex.Message}");
            }

            return View(model);
        }


        public ActionResult ResetPassword(string Id, string dt, string cm, string um)
        {
            ResetPasswordModel objmodel = new ResetPasswordModel();
            try
            {
                CompanyLoginModel model = new CompanyLoginModel();
                {
                    var companyId = _configuration["CompanyDetails:CompanyId"];
                    model.CompanyID = Convert.ToInt64(companyId);

                    var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetCompaniesLogo), " ", false).Result.ToString();
                    model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).companyLoginModel;
                }
                if (!string.IsNullOrEmpty(model.CompanyLogo))
                {
                    model.CompanyLogo = _s3Service.GetFileUrl(model.CompanyLogo);
                }
                objmodel.CompanyLogo = model.CompanyLogo;
                DateTime date = Convert.ToDateTime(_businessLayer.DecodeStringBase64(dt));
                objmodel.EmployeeID = _businessLayer.DecodeStringBase64(Id);
                objmodel.UserID = _businessLayer.DecodeStringBase64(Id);
                objmodel.CompanyID = _businessLayer.DecodeStringBase64(cm);
                objmodel.UserName = _businessLayer.DecodeStringBase64(um);
                objmodel.dt = date;

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
            return View(objmodel);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            try
            {
                string apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.ResetPassword);
                string responseData = _businessLayer.SendPostAPIRequest(model, apiUrl, null, false).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(responseData);

                if (result?.UserID < 0)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "An error occurred. Please try again later.";
                }
                else
                {
                    ChangePasswordModel objmodel = new ChangePasswordModel();
                    objmodel.EmailId = model.UserName;
                    var ForgetapiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.GetFogotPasswordDetails);
                    var data = _businessLayer.SendPostAPIRequest(objmodel, ForgetapiUrl, null, false).Result.ToString();
                    var Forgeresult = JsonConvert.DeserializeObject<Result>(data);

                    if (Forgeresult == null || Forgeresult.Data == null)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = "Invalid response from server.";
                        return View(model);
                    }
                    UserModel userModel = JsonConvert.DeserializeObject<UserModel>((string)Forgeresult.Data);
                    if (userModel != null)
                    {
                        if (userModel.EmployeeID > 0)
                        {
                            // Prepare email content
                            sendEmailProperties sendEmailProperties = new sendEmailProperties
                            {
                                emailSubject = "Updated Password",
                                emailBody = $@"
        <div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
            Hi,<br/><br/>
            Please Find below the updated password.<br/><br/>

            <table style='width: 100%; max-width: 600px; border-collapse: collapse; border: 1px solid #000;'>
                <thead style='background-color: #f2f2f2;'>
                    <tr>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Employee No</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Employee Name</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Manager Name</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Department</th>
                        <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Updated Password</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style='border: 1px solid #000; padding: 8px;'>{userModel.UserName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{userModel.FullName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{userModel.ManagerName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{userModel.DepartmentName}</td>
                        <td style='border: 1px solid #000; padding: 8px;'>{model.Password}</td>
                    </tr>
                </tbody>
            </table><br/>
            </a><br/><br/>

            <p style='color: #000; font-size: 13px;'>
             Protalk Solutions is an ISO 27001:2022 certified. <br/>
             This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.
             We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws. If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited. If you received in error, please notify us immediately at <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a>  to resolve the matter.
            </p>
        </div>"
                            };
                            // Add recipient email
                            sendEmailProperties.EmailToList.Add(_configuration["AppSettings:ITEmail"]);

                            // Send email
                            emailSendResponse response = EmailSender.SendEmail(sendEmailProperties);
                        }
                    }

                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "An unexpected error occurred. Please try again later.";
            }

            return View(model);
        }
        public ActionResult ChangePassword()
        {
            ResetPasswordModel objmodel = new ResetPasswordModel();
            try
            {
                var CompanyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                var EmployeeNumber = HttpContext.Session.GetString(Constants.EmployeeNumber).ToString();

                CompanyLoginModel model = new CompanyLoginModel();
                {
                    model.CompanyID = Convert.ToInt64(CompanyId);

                    var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetCompaniesLogo), " ", false).Result.ToString();
                    model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).companyLoginModel;
                }
                if (!string.IsNullOrEmpty(model.CompanyLogo))
                {
                    model.CompanyLogo = _s3Service.GetFileUrl(model.CompanyLogo);
                }
                objmodel.CompanyLogo = model.CompanyLogo;
                objmodel.UserName = EmployeeNumber;
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }
            return View(objmodel);
        }

        [HttpPost]
        public ActionResult ChangePassword(ResetPasswordModel model)
        {
            try
            {
                model.EmployeeID = HttpContext.Session.GetString(Constants.EmployeeID).ToString();
                model.UserID = HttpContext.Session.GetString(Constants.EmployeeID).ToString();
                model.CompanyID = HttpContext.Session.GetString(Constants.CompanyID).ToString();
                string apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.ResetPassword);
                string responseData = _businessLayer.SendPostAPIRequest(model, apiUrl, null, false).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(responseData);

                if (result?.UserID < 0)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "An error occurred. Please try again later.";
                }
                else
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                    return RedirectToAction("Successpage", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "An unexpected error occurred. Please try again later.";
            }

            return View(model);
        }


        public ActionResult Successpage()
        {

            return View();
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
                _context.HttpContext.Session.SetString(Constants.Manager1Name, result.Manager1Name.ToString());
                _context.HttpContext.Session.SetString(Constants.Manager1Email, result.Manager1Email.ToString());
                _context.HttpContext.Session.SetString(Constants.Manager2Name, result.Manager2Name.ToString());
                _context.HttpContext.Session.SetString(Constants.EmployeeNumber, result.EmployeeNumber.ToString());
                _context.HttpContext.Session.SetString(Constants.EmployeeNumberWithoutAbbr, result.EmployeeNumberWithoutAbbr.ToString());
                _context.HttpContext.Session.SetString(Constants.Manager2Email, result.Manager2Email.ToString());
                _context.HttpContext.Session.SetString(Constants.AreaName, _businessLayer.GetAreaNameByRole(result.RoleId));
                var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, result.UserID.ToString()),
                     new Claim(ClaimTypes.Role,  result.Role)
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                DashBoardModelInputParams dashBoardModelInputParams = new DashBoardModelInputParams() { EmployeeID = long.Parse(HttpContext.Session.GetString(Constants.EmployeeID)) };
                var dataDashBoardModel = _businessLayer.SendPostAPIRequest(dashBoardModelInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetDashBoardModel), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var model = JsonConvert.DeserializeObject<DashBoardModel>(dataDashBoardModel);
                if (string.IsNullOrEmpty(model.ProfilePhoto))
                {
                    model.ProfilePhoto = "";
                    _context.HttpContext.Session.SetString(Constants.ProfilePhoto, model.ProfilePhoto);
                }
                else
                {
                    var ProfilePhoto = _s3Service.GetFileUrl(model.ProfilePhoto);
                    _context.HttpContext.Session.SetString(Constants.ProfilePhoto, ProfilePhoto.ToString());
                }
                _context.HttpContext.Session.SetString(Constants.FirstName, model.FirstName);
                _context.HttpContext.Session.SetString(Constants.MiddleName, model.MiddleName ?? string.Empty);
                _context.HttpContext.Session.SetString(Constants.Surname, model.Surname ?? string.Empty);
                _context.HttpContext.Session.SetString(Constants.OfficialEmailID, model.OfficialEmailID ?? string.Empty);
                _context.HttpContext.Session.SetString(Constants.JobLocationID, model.JobLocationID.ToString());
                _context.HttpContext.Session.SetString(Constants.DepartmentID, model.DepartmentID.ToString());
                var CompanyDatas = _businessLayer.SendPostAPIRequest(objmodel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetAllCompanies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(CompanyDatas);
                var CompanyData = results.companyModel;
                //  var CompanyLogo = "/Uploads/CompanyLogo/" + CompanyData.CompanyID + "/"+ CompanyData.CompanyLogo;
                var CompanyLogo = _s3Service.GetFileUrl(CompanyData.CompanyLogo);
                _context.HttpContext.Session.SetString(Constants.CompanyLogo, CompanyLogo.ToString());

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
        public IActionResult ErrorPage()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                var exception = exceptionFeature.Error;
                var path = exceptionFeature.Path;
                string controllerName = "Unknown";
                string actionName = "Unknown";
                string areaName = "Unknown";
                var stackLines = exception.StackTrace?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (stackLines != null)
                {
                    var controllerLine = stackLines.FirstOrDefault(line => line.Contains(".Controllers.") && line.Contains(" in "));
                    if (!string.IsNullOrEmpty(controllerLine))
                    {
              
                        var methodMatch = System.Text.RegularExpressions.Regex.Match(controllerLine, @"(?<=\.)\w+\s*\(");
                        if (methodMatch.Success)
                        {
                            actionName = methodMatch.Value.Replace("(", "").Trim();
                        }

                    
                        var parts = controllerLine.Split('.');
                        var areaIndex = Array.IndexOf(parts, "Areas");
                        if (areaIndex >= 0 && parts.Length > areaIndex + 3)
                        {
                            areaName = parts[areaIndex + 1];
                            controllerName = parts[areaIndex + 3].Replace("Controller", "");
                        }
                    }
                }

                var model = new ExceptionLogModel
                {
                    AreaName = areaName,
                    ControllerName = controllerName,
                    ActionName = actionName,
                    Url = path,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace ?? "",
                    Source = exception.Source ?? "",
                    EmployeeId = HttpContext.Session?.GetString("EmployeeID") != null
                        ? Convert.ToInt64(HttpContext.Session.GetString("EmployeeID"))
                        : (long?)null
                };             
                var bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
                _businessLayer.SendPostAPIRequest(
                    model,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.InsertException),
                    bearerToken,
                    true
                ).GetAwaiter().GetResult();
            }
            return View();
        }
    }
}
