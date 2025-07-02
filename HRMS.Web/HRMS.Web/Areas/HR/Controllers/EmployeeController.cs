
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2019.Drawing.Model3D;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.Employee;
using HRMS.Models.ExportEmployeeExcel;
using HRMS.Models.FormPermission;
using HRMS.Models.ImportFromExcel;
using HRMS.Models.User;
using HRMS.Models.WhatsHappeningModel;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.Style;
using System;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.Xml;
using System.Text;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.HR.Controllers
{
    [Area(Constants.ManageHR)]
    [Authorize]
    //[Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.SuperAdmin))]
    public class EmployeeController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        private readonly IS3Service _s3Service;
        private readonly IHttpContextAccessor _context;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;
        public EmployeeController(ICheckUserFormPermission CheckUserFormPermission, IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IS3Service s3Service, IHttpContextAccessor context)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            EmailSender.configuration = _configuration;
            _s3Service = s3Service;
            _context = context;
            _CheckUserFormPermission = CheckUserFormPermission;
        }
        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }
        public IActionResult EmployeeListing()
        {
            var employeeId = GetSessionInt(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);
            // Check if the user has permission for Employee Listing
            var formPermission = _CheckUserFormPermission.GetFormPermission(employeeId, (int)PageName.EmployeeListing);
            // If no permission and not an admin
            if (formPermission.HasPermission == 0 && roleId != (int)Roles.Admin && roleId != (int)Roles.SuperAdmin)
            {
                // Check if user has permission for My Team page
                var teamPermission = _CheckUserFormPermission.GetFormPermission(employeeId, (int)PageName.MyTeam);
                // Redirect to My Team page if user is not an employee and has permission
                if (roleId != (int)Roles.Employee && teamPermission.HasPermission > 0)
                {
                    return RedirectToAction("GetTeamEmployeeList", "MyInfo", new { area = "employee" });
                }
                // Else, log the user out and redirect to home
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(new HRMS.Models.Common.Results());
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult EmployeeListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            EmployeeInputParams employee = new EmployeeInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employee.RoleID = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID));
            employee.DisplayStart = iDisplayStart;
            employee.DisplayLength = iDisplayLength;
            employee.Searching = string.IsNullOrEmpty(sSearch) ? null : sSearch;
            var data = _businessLayer.SendPostAPIRequest(
                employee,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            results.Employees.ForEach(x =>
            {
                x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.EmployeeID.ToString());
                x.EncodedDesignationID = _businessLayer.EncodeStringBase64(x.DesignationID.ToString());
                x.EncodedDepartmentIDID = _businessLayer.EncodeStringBase64(x.DepartmentID.ToString());
                x.ProfilePhoto = string.IsNullOrEmpty(x.ProfilePhoto)
                    ? "/assets/img/No_image.png"
                    : _s3Service.GetFileUrl(x.ProfilePhoto);
            });
            return Json(new
            {
                draw = sEcho,
                recordsTotal = results.Employees.Select(x => x.TotalRecords).FirstOrDefault() ?? 0,
                recordsFiltered = results.Employees.Select(x => x.FilteredRecords).FirstOrDefault() ?? 0,
                data = results.Employees
            });
        }

        public IActionResult Index(string id)
        {
            EmployeeModel employee = new EmployeeModel();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            if (string.IsNullOrEmpty(id))
            {
                employee.FamilyDetails.Add(new FamilyDetail());
                employee.EducationalDetails.Add(new EducationalDetail());
                employee.LanguageDetails.Add(new LanguageDetail());
                employee.EmploymentHistory.Add(new EmploymentHistory());
                employee.References = new List<HRMS.Models.Employee.Reference>() {
                    new HRMS.Models.Employee.Reference(),
                    new HRMS.Models.Employee.Reference()
                };
            }
            else
            {
                var encrpt = id;
                id = _businessLayer.DecodeStringBase64(id);
                employee.EmployeeID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                employee = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).employeeModel;
                if (!string.IsNullOrEmpty(employee.ProfilePhoto))
                {
                    employee.ProfilePhoto = _s3Service.GetFileUrl(employee.ProfilePhoto);
                }
                if (!string.IsNullOrEmpty(employee.AadhaarCardImage))
                {
                    employee.AadhaarCardImage = _s3Service.GetFileUrl(employee.AadhaarCardImage);
                }
                if (!string.IsNullOrEmpty(employee.PanCardImage))
                {
                    employee.PanCardImage = _s3Service.GetFileUrl(employee.PanCardImage);
                }
                employee.EncryptedIdentity = encrpt;
                if (employee.References == null || employee.References.Count == 0)
                {
                    employee.References = new List<HRMS.Models.Employee.Reference>() {
                    new HRMS.Models.Employee.Reference(),
                    new HRMS.Models.Employee.Reference()
                    };
                }
                else if (employee.References.Count == 1)
                {
                    employee.References.Add(new HRMS.Models.Employee.Reference());
                }
                ;
            }

            HRMS.Models.Common.Results results = GetAllResults(employee.CompanyID);
            employee.Languages = results.Languages;
            employee.Countries = results.Countries;
            employee.EmploymentTypes = results.EmploymentTypes;
            employee.Departments = results.Departments;

            return View(employee);
        }

        [HttpPost]
        public IActionResult Index(EmployeeModel employee, List<IFormFile> postedFiles, List<IFormFile> PanPostedFile, List<IFormFile> AadhaarPostedFile)
        {
            var results = GetAllResults(employee.CompanyID);
            try
            {
                if (!ModelState.IsValid)
                {
                    SetWarningToast("Please check all data and try again.");
                    return ReturnEmployeeViewWithData(employee, results);
                }
                _s3Service.ProcessFileUpload(postedFiles, employee.ProfilePhoto, out string newProfileKey);
                if (!string.IsNullOrEmpty(newProfileKey))
                {
                    if (!string.IsNullOrEmpty(employee.ProfilePhoto))
                    {
                        _s3Service.DeleteFile(employee.ProfilePhoto);
                    }
                    employee.ProfilePhoto = newProfileKey;
                }
                else
                {
                    employee.ProfilePhoto = _s3Service.ExtractKeyFromUrl(employee.ProfilePhoto);
                }
                var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmployee);
                var apiResult = _businessLayer.SendPostAPIRequest(employee, apiUrl, HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(apiResult);

                if (result != null && result.PKNo > 0)
                {
                    SetSuccessToast("Data saved successfully.");
                    return RedirectToActionPermanent(WebControllarsConstants.EmployeeListing, WebControllarsConstants.Employee);
                }

                SetWarningToast("Please check all data and try again.");
            }
            catch (Exception)
            {
                SetWarningToast("Some error occurred, please try later.");
            }

            return ReturnEmployeeViewWithData(employee, results);
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

        private IActionResult ReturnEmployeeViewWithData(EmployeeModel employee, HRMS.Models.Common.Results results)
        {
            employee.Languages = results.Languages;
            employee.Countries = results.Countries;
            employee.EmploymentTypes = results.EmploymentTypes;
            employee.Departments = results.Departments;

            return View(employee);
        }

        public HRMS.Models.Common.Results GetAllResults(long CompanyID)
        {
            HRMS.Models.Common.Results result = null;
            var data = "";
            if (HttpContext.Session.GetString(Constants.ResultsData) != null)
            {
                data = HttpContext.Session.GetString(Constants.ResultsData);
            }
            else
            {
                data = _businessLayer.SendGetAPIRequest("Common/GetAllResults?CompanyID=" + CompanyID, HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            }
            HttpContext.Session.SetString(Constants.ResultsData, data);
            result = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            return result;
        }

        [HttpPost]
        public ActionResult AddNewEmploymentHistory(EmployeeModel employee, bool isDeleted)
        {
            if (!isDeleted)
            {
                employee.EmploymentHistory.Add(new EmploymentHistory() { });
            }
            HRMS.Models.Common.Results results = GetAllResults(employee.CompanyID);
            employee.Countries = results.Countries;
            return PartialView("_EmploymentHistory", employee);
        }


        [HttpPost]
        public ActionResult AddNewFamilyMember(EmployeeModel employee, bool isDeleted)
        {
            if (!isDeleted)
            {
                employee.FamilyDetails.Add(new FamilyDetail() { });
            }
            return PartialView("_FamilyDetails", employee);
        }


        [HttpPost]
        public ActionResult AddNewEducationalDetail(EmployeeModel employee, bool isDeleted)
        {
            if (!isDeleted)
            {
                employee.EducationalDetails.Add(new EducationalDetail() { });
            }
            return PartialView("_EducationalDetails", employee);
        }

        [HttpPost]
        public ActionResult AddNewLanguageDetail(EmployeeModel employee, bool isDeleted)
        {
            employee.Languages = GetAllResults(employee.CompanyID).Languages;
            if (!isDeleted)
            {
                employee.LanguageDetails.Add(new LanguageDetail() { });
            }
            return PartialView("_LanguageDetails", employee);
        }

        public IActionResult ActiveEmployeeListing()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult ActiveEmployeeListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            EmployeeInputParams employee = new EmployeeInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllActiveEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            results.Employees.ForEach(x => x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.EmployeeID.ToString()));
            return Json(new { data = results.Employees });
        }
        [HttpPost]
        public JsonResult GetL2Manager(int l1EmployeeId)
        {
            try
            {

                L2ManagerInputParams input = new L2ManagerInputParams
                {
                    L1EmployeeID = l1EmployeeId
                };


                var data = _businessLayer.SendPostAPIRequest(input,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee,
                    APIApiActionConstants.GetL2ManagerDetails),
                    HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();


                L2ManagerDetail managerDetail = JsonConvert.DeserializeObject<L2ManagerDetail>(data);
                if (managerDetail != null)
                {
                    return Json(new
                    {
                        success = true,
                        managerId = managerDetail.ManagerID,
                        managerName = managerDetail.ManagerName
                    });
                }
                else
                {
                    return Json(new { success = false, message = "L2 manager not found." });
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An error occurred while fetching the L2 manager.", error = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult loadForms(int DepartmentId, int EmployeeId)
        {
            try
            {
                List<FormPermissionViewModel> objmodel = new List<FormPermissionViewModel>();
                FormPermissionVM objPermission = new FormPermissionVM();
                objPermission.DepartmentId = DepartmentId;
                objPermission.EmployeeID = EmployeeId;
                var response = _businessLayer.SendPostAPIRequest(objPermission, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.GetUserFormByDepartmentID), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                if (response != null)
                {
                    objmodel = JsonConvert.DeserializeObject<List<FormPermissionViewModel>>(response.ToString());
                }

                return Json(objmodel);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while loading form permissions.",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public ActionResult EmploymentDetails(string id, string DegtId, string DeptId)
        {
            var CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            EmploymentDetailInputParams employmentDetailInputParams = new EmploymentDetailInputParams()
            {
                UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID))
            };
            CompanyLoginModel model = new CompanyLoginModel();
            {

                model.CompanyID = Convert.ToInt64(CompanyID);

                var Companydata = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetCompaniesLogo), " ", false).Result.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(Companydata).companyLoginModel;
            }
            if (!string.IsNullOrEmpty(model.CompanyLogo))
            {
                model.CompanyLogo = _s3Service.GetFileUrl(model.CompanyLogo);
            }
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                employmentDetailInputParams.DesignationID = long.Parse(_businessLayer.DecodeStringBase64(DegtId));
                employmentDetailInputParams.DepartmentID = long.Parse(_businessLayer.DecodeStringBase64(DeptId));
                employmentDetailInputParams.EmployeeID = long.Parse(id);
            }
            employmentDetailInputParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            EmploymentDetail employmentDetail = new EmploymentDetail();
            var data = _businessLayer.SendPostAPIRequest(employmentDetailInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFilterEmploymentDetailsByEmployee), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            employmentDetail = JsonConvert.DeserializeObject<EmploymentDetail>(data);
            employmentDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentDetail.EmployeeID.ToString());
            employmentDetail.EmployeNumber = model.Abbr + employmentDetail.EmployeNumber;
            employmentDetail.CompanyAbbr = model.Abbr;
            return View(employmentDetail);
        }

        [HttpGet]
        public ActionResult FilterEmploymentDetails(string id, long departmentID, long designationID)
        {
            EmploymentDetailInputParams employmentDetailInputParams = new EmploymentDetailInputParams()
            {
                UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID))
            };
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                employmentDetailInputParams.EmployeeID = long.Parse(id);
            }
            employmentDetailInputParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            employmentDetailInputParams.DepartmentID = departmentID;
            employmentDetailInputParams.DesignationID = designationID;
            EmploymentDetail employmentDetail = new EmploymentDetail();
            var data = _businessLayer.SendPostAPIRequest(employmentDetailInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFilterEmploymentDetailsByEmployee), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            employmentDetail = JsonConvert.DeserializeObject<EmploymentDetail>(data);
            employmentDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentDetail.EmployeeID.ToString());
            return Json(employmentDetail);
        }

        [HttpPost]
        public ActionResult EmploymentDetails(EmploymentDetail employmentDetail, List<string> SelectedFormIds)
        {
            if (ModelState.IsValid)
            {
                var CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                employmentDetail.CompanyID = Convert.ToInt64(CompanyID);
                employmentDetail.EmployeNumber = employmentDetail.EmployeNumber.Split(employmentDetail.CompanyAbbr)[1];
                var data = _businessLayer.SendPostAPIRequest(employmentDetail, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmploymentDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                Result result = JsonConvert.DeserializeObject<Result>(data);
                if (result != null && result.PKNo > 0
    && result.Message != null
    && result.Message.StartsWith("Duplicate Email found:", StringComparison.OrdinalIgnoreCase))
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;

                    TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                }
                else if (result != null && result.PKNo > 0 && result.Message.Contains("Some error occurred, please try again later.", StringComparison.OrdinalIgnoreCase))
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                }
                else
                {
                    FormPermissionVM objmodel = new FormPermissionVM();
                    objmodel.EmployeeID = employmentDetail.EmployeeID;
                    objmodel.SelectedFormIds = SelectedFormIds;
                    var Permissionsdata = _businessLayer.SendPostAPIRequest(objmodel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.AddUserFormPermissions), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                    var Permissionresponse = JsonConvert.DeserializeObject<long>(Permissionsdata);
                    if (Permissionresponse < 0)
                    {
                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = "Some error occurred, please try again later";
                        return View(employmentDetail);
                    }
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                    if (result.IsResetPasswordRequired)
                    {
                        ChangePasswordModel model = new ChangePasswordModel();
                        model.EmailId = employmentDetail.CompanyAbbr + employmentDetail.EmployeNumber;
                        var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.GetFogotPasswordDetails);
                        var datamodel = _businessLayer.SendPostAPIRequest(model, apiUrl, null, false).Result.ToString();
                        var resultdata = JsonConvert.DeserializeObject<Result>(datamodel);

                        if (resultdata == null || resultdata.Data == null)
                        {
                            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                            TempData[HRMS.Models.Common.Constants.toastMessage] = "Invalid response from server.";
                            return View(model);
                        }

                        UserModel userModel = JsonConvert.DeserializeObject<UserModel>((string)resultdata.Data);

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
                To no longer receive messages from Eternity Logistics, please click to <strong><a href='http://unsubscribe.eternitylogistics.co/'> Unsubscribe </a></strong>.<br/><br/>

                If you are happy with our services or want to share any feedback, do email us at 
                <a href='mailto:feedback@eternitylogistics.co' style='color: #000;'>feedback@eternitylogistics.co</a>.<br/><br/>

                All email correspondence is sent only through our official domain: 
                <strong>@eternitylogistics.co</strong>. Please verify carefully the domain from which the messages are sent to avoid potential scams.<br/><br/>

                <strong>CONFIDENTIALITY NOTICE:</strong> This e-mail message, including all attachments, is for the sole use of the intended recipient(s) and may contain confidential and privileged information. 
                If you are not the intended recipient, you may NOT use, disclose, copy, or disseminate this information. 
                Please contact the sender by reply e-mail immediately and destroy all copies of the original message, including all attachments. 
                This communication is for informational purposes only and is not an offer, solicitation, recommendation, or commitment for any transaction. 
                Your cooperation is greatly appreciated.
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
                        }





                    }
                }




                EmploymentDetailInputParams employmentDetailInputParams = new EmploymentDetailInputParams();
                employmentDetailInputParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                employmentDetailInputParams.DepartmentID = employmentDetail.DepartmentID;
                employmentDetailInputParams.DesignationID = employmentDetail.DesignationID;
                var dataBody = _businessLayer.SendPostAPIRequest(employmentDetailInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFilterEmploymentDetailsByEmployee), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                EmploymentDetail employmentDetailtemp = JsonConvert.DeserializeObject<EmploymentDetail>(dataBody);
                employmentDetail.EmployeeList = employmentDetailtemp.EmployeeList;
                employmentDetail.Departments = employmentDetailtemp.Departments;
                employmentDetail.JobLocations = employmentDetailtemp.JobLocations;
                employmentDetail.Designations = employmentDetailtemp.Designations;
                employmentDetail.EmploymentTypes = employmentDetailtemp.EmploymentTypes;
                employmentDetail.PayrollTypes = employmentDetailtemp.PayrollTypes;
                employmentDetail.LeavePolicyList = employmentDetailtemp.LeavePolicyList;
                employmentDetail.RoleList = employmentDetailtemp.RoleList;
                employmentDetail.SubDepartments = employmentDetailtemp.SubDepartments;
                employmentDetail.ShiftTypes = employmentDetailtemp.ShiftTypes;
                employmentDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentDetail.EmployeeID.ToString());
            }
            else
            {
                EmploymentDetailInputParams employmentDetailInputParams = new EmploymentDetailInputParams();
                employmentDetailInputParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                var data = _businessLayer.SendPostAPIRequest(employmentDetailInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFilterEmploymentDetailsByEmployee), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                EmploymentDetail employmentDetailtemp = JsonConvert.DeserializeObject<EmploymentDetail>(data);
                employmentDetail.EmployeeList = employmentDetailtemp.EmployeeList;
                employmentDetail.Departments = employmentDetailtemp.Departments;
                employmentDetail.JobLocations = employmentDetailtemp.JobLocations;
                employmentDetail.Designations = employmentDetailtemp.Designations;
                employmentDetail.EmploymentTypes = employmentDetailtemp.EmploymentTypes;
                employmentDetail.PayrollTypes = employmentDetailtemp.PayrollTypes;
                employmentDetail.LeavePolicyList = employmentDetailtemp.LeavePolicyList;
                employmentDetail.RoleList = employmentDetailtemp.RoleList;
                employmentDetail.SubDepartments = employmentDetailtemp.SubDepartments;
                employmentDetail.ShiftTypes = employmentDetailtemp.ShiftTypes;
                employmentDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentDetail.EmployeeID.ToString());
            }

            //return RedirectToActionPermanent(WebControllarsConstants.EmployeeListing, WebControllarsConstants.Employee );


            return View(employmentDetail);
        }

        [HttpGet]
        public ActionResult EmploymentBankDetails(string id)
        {
            EmploymentBankDetailInputParams employmentBankInputParams = new EmploymentBankDetailInputParams()
            {
                UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID))
            };
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                employmentBankInputParams.EmployeeID = long.Parse(id);
            }

            EmploymentBankDetail employmentBankDetail = new EmploymentBankDetail();
            var data = _businessLayer.SendPostAPIRequest(employmentBankInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmploymentBankDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            employmentBankDetail = JsonConvert.DeserializeObject<EmploymentBankDetail>(data);
            employmentBankDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentBankDetail.EmployeeID.ToString());
            if (!string.IsNullOrEmpty(employmentBankDetail.AadhaarCardImage))
            {
                employmentBankDetail.AadhaarCardImage = _s3Service.GetFileUrl(employmentBankDetail.AadhaarCardImage);
            }
            if (!string.IsNullOrEmpty(employmentBankDetail.PanCardImage))
            {
                employmentBankDetail.PanCardImage = _s3Service.GetFileUrl(employmentBankDetail.PanCardImage);
            }

            return View(employmentBankDetail);
        }

        [HttpPost]
        public ActionResult EmploymentBankDetails(EmploymentBankDetail employmentBankDetail, List<IFormFile> PanPostedFile, List<IFormFile> AadhaarPostedFile)
        {
            if (ModelState.IsValid)
            {
                employmentBankDetail.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                _s3Service.ProcessFileUpload(PanPostedFile, employmentBankDetail.PanCardImage, out string newPanKey);
                if (!string.IsNullOrEmpty(newPanKey))
                {
                    if (!string.IsNullOrEmpty(employmentBankDetail.PanCardImage))
                    {
                        _s3Service.DeleteFile(employmentBankDetail.PanCardImage);
                    }
                    employmentBankDetail.PanCardImage = newPanKey;
                }
                else
                {
                    employmentBankDetail.PanCardImage = _s3Service.ExtractKeyFromUrl(employmentBankDetail.PanCardImage);
                }

                _s3Service.ProcessFileUpload(AadhaarPostedFile, employmentBankDetail.AadhaarCardImage, out string newAadhaarKey);
                if (!string.IsNullOrEmpty(newAadhaarKey))
                {
                    if (!string.IsNullOrEmpty(employmentBankDetail.AadhaarCardImage))
                    {
                        _s3Service.DeleteFile(employmentBankDetail.AadhaarCardImage);
                    }
                    employmentBankDetail.AadhaarCardImage = newAadhaarKey;
                }
                else
                {
                    employmentBankDetail.AadhaarCardImage = _s3Service.ExtractKeyFromUrl(employmentBankDetail.AadhaarCardImage);
                }

                var data = _businessLayer.SendPostAPIRequest(employmentBankDetail, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmploymentBankDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

                Result result = JsonConvert.DeserializeObject<Result>(data);

                if (result != null && result.PKNo > 0)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Bank details saved successfully.";
                    return RedirectToActionPermanent(WebControllarsConstants.EmployeeListing, WebControllarsConstants.Employee);
                }
                else
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Failed to save bank details.";
                }
            }
            return View(employmentBankDetail);
        }

        [HttpGet]
        public ActionResult EmploymentSeparation(string id)
        {
            EmploymentSeparationInputParams employmentSeparationInputParams = new EmploymentSeparationInputParams()
            {
                UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID))
            };
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                employmentSeparationInputParams.EmployeeID = long.Parse(id);
            }

            EmploymentSeparationDetail employmentSeparationDetail = new EmploymentSeparationDetail();
            var data = _businessLayer.SendPostAPIRequest(employmentSeparationInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmploymentSeparationDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            employmentSeparationDetail = JsonConvert.DeserializeObject<EmploymentSeparationDetail>(data);
            employmentSeparationDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentSeparationDetail.EmployeeID.ToString());
            return View(employmentSeparationDetail);
        }

        [HttpPost]
        public ActionResult EmploymentSeparation(EmploymentSeparationDetail employmentSeparationDetail)
        {
            if (ModelState.IsValid)
            {
                employmentSeparationDetail.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                var data = _businessLayer.SendPostAPIRequest(employmentSeparationDetail, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmploymentSeparationDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

                Result result = JsonConvert.DeserializeObject<Result>(data);

                if (result != null && result.PKNo > 0)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Employee Separation details saved successfully.";
                    return RedirectToActionPermanent(WebControllarsConstants.EmployeeListing, WebControllarsConstants.Employee);
                }
                else
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Failed to save Employee Separation details.";
                }
            }




            return View(employmentSeparationDetail);
        }

        public IActionResult Whatshappening()
        {
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.Whatshappening);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult Whatshappening(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            WhatsHappeningModelParans WhatsHappeningModelParams = new WhatsHappeningModelParans();
            WhatsHappeningModelParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(WhatsHappeningModelParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllWhatsHappeningDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            results.WhatsHappeningList.ForEach(x => x.IconImage = _s3Service.GetFileUrl(x.IconImage));
            return Json(new { data = results.WhatsHappeningList });

        }



        [HttpGet]
        public IActionResult InActiveEmployee(int employeeId, int isActive)
        {
            ReportingStatus obj = new ReportingStatus();
            obj.EmployeeId = employeeId;
            obj.Status = isActive;
            var data = _businessLayer.SendPostAPIRequest(obj, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.CheckEmployeeReporting), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var ReportingData = JsonConvert.DeserializeObject<ReportingStatus>(data);
            return Ok(new { success = true, data = ReportingData });
        }
        [HttpGet]
        public async Task<IActionResult> ExportEmployeeSheet()
        {
            try
            {
                var companyIdString = HttpContext.Session.GetString(Constants.CompanyID);
                if (!long.TryParse(companyIdString, out var companyId))
                    return BadRequest("Invalid Company ID");

                var models = new EmployeeInputParams
                {
                    CompanyID = companyId
                };

                var response = await _businessLayer.SendPostAPIRequest(
                    models,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.FetchExportEmployeeExcelSheet),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true);

                var responseString = response.ToString();
                var employees = JsonConvert.DeserializeObject<List<ExportEmployeeDetailsExcel>>(responseString);

                if (employees == null || !employees.Any())
                {
                    return NotFound("No employee data found.");
                }

                // Project the data into a flat, exportable structure (anonymous or DTO)
                var exportData = employees.Select(emp => new
                {
                    emp.EmployeeNumber,
                    emp.CompanyName,
                    emp.FirstName,
                    emp.MiddleName,
                    emp.Surname,
                    emp.Gender,
                    DateOfBirth = emp.DateOfBirth?.ToString("yyyy-MM-dd"),
                    emp.PlaceOfBirth,
                    emp.EmailAddress,
                    emp.PersonalEmailAddress,
                    emp.Mobile,
                    emp.Landline,
                    emp.Telephone,
                    emp.CorrespondenceAddress,
                    emp.CorrespondenceCity,
                    emp.CorrespondenceState,
                    emp.CorrespondencePinCode,
                    emp.PermanentAddress,
                    emp.PermanentCity,
                    emp.PermanentState,
                    emp.PermanentPinCode,
                    emp.PANNo,
                    emp.AadharCardNo,
                    emp.BloodGroup,
                    emp.Allergies,
                    emp.MajorIllnessOrDisability,
                    emp.AwardsAchievements,
                    emp.EducationGap,
                    emp.ExtraCuricuarActivities,
                    emp.ForiegnCountryVisits,
                    emp.ContactPersonName,
                    emp.ContactPersonMobile,
                    emp.ContactPersonTelephone,
                    emp.ContactPersonRelationship,
                    emp.ITSkillsKnowledge,
                    emp.Designation,
                    emp.EmployeeType,
                    emp.Department,
                    emp.SubDepartment,
                    emp.JobLocation,
                    emp.ShiftType,
                    emp.OfficialEmailID,
                    emp.OfficialContactNo,
                    JoiningDate = emp.JoiningDate?.ToString("yyyy-MM-dd"),
                    emp.ReportingManager,
                    emp.PolicyName,
                    emp.PayrollType,
                    emp.ClientName,
                    emp.ESINumber,
                    ESIRegistrationDate = emp.ESIRegistrationDate?.ToString("yyyy-MM-dd"),
                    emp.BankAccountNumber,
                    emp.IFSCCode,
                    emp.BankName,
                    emp.UANNumber,
                    DateOfResignation = emp.DateOfResignation?.ToString("yyyy-MM-dd"),
                    DateOfLeaving = emp.DateOfLeaving?.ToString("yyyy-MM-dd"),
                    emp.LeavingType,
                    emp.NoticeServed,
                    emp.AgeOnNetwork,
                    emp.PreviousExperience,
                    DateOfJoiningTraining = emp.DateOfJoiningTraining?.ToString("yyyy-MM-dd"),
                    DateOfJoiningFloor = emp.DateOfJoiningFloor?.ToString("yyyy-MM-dd"),
                    DateOfJoiningOJT = emp.DateOfJoiningOJT?.ToString("yyyy-MM-dd"),
                    DateOfJoiningOnroll = emp.DateOfJoiningOnroll?.ToString("yyyy-MM-dd"),
                    BackOnFloorDate = emp.BackOnFloorDate?.ToString("yyyy-MM-dd"),
                    emp.LeavingRemarks,
                    MailReceivedFromAndDate = emp.MailReceivedFromAndDate?.ToString("yyyy-MM-dd"),
                    EmailSentToITDate = emp.EmailSentToITDate?.ToString("yyyy-MM-dd"),
                    Status = emp.Status
                }).ToList();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Employees");

                // Load data and headers
                worksheet.Cells["A1"].LoadFromCollection(exportData, PrintHeaders: true);

                // Style header row
                using (var range = worksheet.Cells[1, 1, 1, exportData[0].GetType().GetProperties().Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                var fileName = $"EmployeeDetails_{DateTime.Now:yyyyMMdd}.xlsx";

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Optionally log exception
                return StatusCode(500, "An error occurred while exporting employee details.");
            }
        }


        [HttpGet]
        public IActionResult EducationalDetail(string id)
        {
            var decodedEmployeeId = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
            var model = new EducationalDetail
            {
                EmployeeID = decodedEmployeeId
            };
            return View(model);
        }
        [HttpPost]
        public JsonResult GetEducationDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            EducationDetailParams educationDetailParams = new EducationDetailParams();
            educationDetailParams.EmployeeID = EmployeeID;
            var data = _businessLayer.SendPostAPIRequest(educationDetailParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEducationDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<List<EducationalDetail>>(data);

            if (results != null)
            {
                results.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.CertificateImage))
                    {
                        x.CertificateImage = _s3Service.GetFileUrl(x.CertificateImage);
                    }
                });
            }
            return Json(new
            {
                data = results
            });
        }

        [HttpPost]
        public JsonResult EducationalDetail(EducationalDetail eduDetail, List<IFormFile> CertificateFile)
        {
            if (ModelState.IsValid)
            {
                eduDetail.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
                _s3Service.ProcessFileUpload(CertificateFile, eduDetail.CertificateImage, out string newPanKey);
                if (!string.IsNullOrEmpty(newPanKey))
                {
                    if (!string.IsNullOrEmpty(eduDetail.CertificateImage))
                    {
                        _s3Service.DeleteFile(eduDetail.CertificateImage);
                    }
                    eduDetail.CertificateImage = newPanKey;
                }
                else
                {
                    eduDetail.CertificateImage = _s3Service.ExtractKeyFromUrl(eduDetail.CertificateImage);
                }

                var data = _businessLayer.SendPostAPIRequest(
                    eduDetail,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEducationDetail),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                Result result = JsonConvert.DeserializeObject<Result>(data);

                if (result != null && result.PKNo > 0)
                {
                    return Json(new { success = true, message = "Education details saved successfully." });
                }
                return Json(new { success = false, message = "Failed to save family details." });
            }

            return Json(new { success = false, message = "Validation failed." });
        }
        [HttpPost]
        public IActionResult DeleteEducationDetail(long encodedId)
        {

            EducationDetailParams model = new EducationDetailParams
            {
                EducationDetailID = encodedId
            };

            var response = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteEducationDetail),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            if (!string.IsNullOrEmpty(response))
            {


                return Json(new { success = true, message = response });
            }

            return Json(new { success = false, message = "Failed to delete the record." });
        }


        [HttpGet]
        public IActionResult FamilyDetail(string id)
        {
            var decodedEmployeeId = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
            var model = new FamilyDetail
            {
                EmployeeID = decodedEmployeeId
            };
            return View(model);
        }
        [HttpPost]
        public JsonResult GetFamilyDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            FamilyDetailParams familyDetailParams = new FamilyDetailParams();
            familyDetailParams.EmployeeID = EmployeeID;
            var data = _businessLayer.SendPostAPIRequest(familyDetailParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFamilyDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<List<FamilyDetail>>(data);
            return Json(new
            {
                data = results
            });
        }

        [HttpPost]
        public JsonResult FamilyDetail(FamilyDetail famDetail)
        {
            if (ModelState.IsValid)
            {
                famDetail.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));

                var data = _businessLayer.SendPostAPIRequest(
                    famDetail,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateFamilyDetail),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                Result result = JsonConvert.DeserializeObject<Result>(data);

                if (result != null && result.PKNo > 0)
                {
                    return Json(new { success = true, message = "Family details saved successfully." });
                }

                return Json(new { success = false, message = "Failed to save family details." });
            }

            return Json(new { success = false, message = "Validation failed." });
        }
        [HttpPost]
        public IActionResult DeleteFamilyDetail(long encodedId)
        {

            FamilyDetailParams model = new FamilyDetailParams
            {
                EmployeesFamilyDetailID = encodedId
            };

            var response = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteFamilyDetail),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            if (!string.IsNullOrEmpty(response))
            {


                return Json(new { success = true, message = response });
            }

            return Json(new { success = false, message = "Failed to delete the record." });
        }

        [HttpGet]
        public IActionResult ReferenceDetail(string id)
        {
            var decodedEmployeeId = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
            var model = new HRMS.Models.Employee.Reference
            {
                EmployeeID = decodedEmployeeId
            };
            return View(model);
        }

        [HttpPost]
        public JsonResult GetReferenceDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            ReferenceParams referenceParams = new ReferenceParams
            {
                EmployeeID = EmployeeID
            };

            var data = _businessLayer.SendPostAPIRequest(
                referenceParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetReferenceDetails),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var results = JsonConvert.DeserializeObject<List<HRMS.Models.Employee.Reference>>(data);

            return Json(new { data = results });
        }

        [HttpPost]
        public JsonResult ReferenceDetail(HRMS.Models.Employee.Reference reference)
        {
            if (ModelState.IsValid)
            {
                reference.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));

                var data = _businessLayer.SendPostAPIRequest(
                    reference,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateReferenceDetail),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                Result result = JsonConvert.DeserializeObject<Result>(data);

                if (result != null && result.PKNo > 0)
                {
                    return Json(new { success = true, message = "ReferenceDetail details saved successfully." });
                }
                return Json(new { success = false, message = "Failed to save reference details." });
            }

            return Json(new { success = false, message = "Validation failed." });
        }

        [HttpPost]
        public IActionResult DeleteReferenceDetail(long encodedId)
        {
            ReferenceParams model = new ReferenceParams
            {
                ReferenceDetailID = encodedId
            };

            var response = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteReferenceDetail),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            if (!string.IsNullOrEmpty(response))
            {

                return Json(new { success = true, message = response });
            }

            return Json(new { success = false, message = "Failed to delete the record." });
        }


        [HttpGet]
        public IActionResult EmploymentHistory(string id)
        {
            var decodedEmployeeId = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
            var companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var results = GetAllResults(companyId);
            var model = new HRMS.Models.Employee.EmploymentHistory
            {
                EmployeeID = decodedEmployeeId,
                Countries = results.Countries
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult GetEmploymentHistoryDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            var requestParams = new EmploymentHistoryParams
            {
                EmployeeID = EmployeeID

            };

            var data = _businessLayer.SendPostAPIRequest(
                requestParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmploymentHistory),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var results = JsonConvert.DeserializeObject<List<HRMS.Models.Employee.EmploymentHistory>>(data);

            return Json(new { data = results });
        }

        [HttpPost]
        public JsonResult EmploymentHistory(HRMS.Models.Employee.EmploymentHistory history)
        {
            if (ModelState.IsValid)
            {
                history.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));

                var data = _businessLayer.SendPostAPIRequest(
                    history,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmploymentHistory),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                Result result = JsonConvert.DeserializeObject<Result>(data);

                if (result != null && result.PKNo > 0)
                {
                    return Json(new { success = true, message = "Employment history saved successfully." });
                }
                return Json(new { success = false, message = "Failed to save employment history." });
            }

            return Json(new { success = false, message = "Validation failed." });
        }

        [HttpPost]
        public IActionResult DeleteEmploymentHistory(long encodedId)
        {
            EmploymentHistoryParams model = new EmploymentHistoryParams
            {
                EmploymentHistoryID = encodedId
            };

            var response = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteEmploymentHistory),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            if (!string.IsNullOrEmpty(response))
            {

                return Json(new { success = true, message = response });
            }

            return Json(new { success = false, message = "Failed to delete the record." });
        }

        [HttpGet]
        public IActionResult LanguageDetail(string id)
        {
            var decodedEmployeeId = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
            var companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var results = GetAllResults(companyId);

            var model = new HRMS.Models.Employee.LanguageDetail
            {
                EmployeeID = decodedEmployeeId,
                Languages = results.Languages,
            };
            return View(model);
        }

        [HttpPost]
        public JsonResult GetLanguageDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            var requestParams = new HRMS.Models.Employee.LanguageDetailParams
            {
                EmployeeID = EmployeeID
            };

            var data = _businessLayer.SendPostAPIRequest(
                requestParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLanguageDetails),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var results = JsonConvert.DeserializeObject<List<HRMS.Models.Employee.LanguageDetail>>(data);

            return Json(new { data = results });
        }

        [HttpPost]
        public JsonResult LanguageDetail(HRMS.Models.Employee.LanguageDetail language)
        {
            if (ModelState.IsValid)
            {
                language.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));

                var data = _businessLayer.SendPostAPIRequest(
                    language,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLanguageDetail),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(data);

                if (result != null && result.PKNo > 0)
                {
                    return Json(new { success = true, message = "Language detail saved successfully." });
                }

                return Json(new { success = false, message = "Failed to save language detail." });
            }

            return Json(new { success = false, message = "Validation failed." });
        }

        [HttpPost]
        public IActionResult DeleteLanguageDetail(long encodedId)
        {
            var model = new HRMS.Models.Employee.LanguageDetailParams
            {
                EmployeeLanguageDetailID = encodedId
            };

            var response = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteLanguageDetail),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            if (!string.IsNullOrEmpty(response))
            {

                return Json(new { success = true, message = response });
            }

            return Json(new { success = false, message = "Failed to delete the record." });
        }





        [HttpGet]
        public IActionResult EmployeesWeekOffRoster()
        {
            var employeeId = GetSessionInt(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);
            // Check if the user has permission for Employee Listing
            var formPermission = _CheckUserFormPermission.GetFormPermission(employeeId, (int)PageName.WeekOffRoster);
            // If no permission and not an admin
            if (formPermission.HasPermission == 0 && roleId != (int)Roles.Admin && roleId != (int)Roles.SuperAdmin)
            {
                // Check if user has permission for My Team page
                var teamPermission = _CheckUserFormPermission.GetFormPermission(employeeId, (int)PageName.MyTeam);
                // Redirect to My Team page if user is not an employee and has permission
                if (roleId != (int)Roles.Employee && teamPermission.HasPermission > 0)
                {
                    return RedirectToAction("GetTeamEmployeeList", "MyInfo", new { area = "employee" });
                }
                // Else, log the user out and redirect to home
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(new HRMS.Models.Common.Results());
        }

        [HttpPost]
        public JsonResult GetEmployeesWeekOffRoster(string sEcho, int PageNumber, int PageSize, string sSearch, int Year, int Month)
        {
            WeekOfInputParams employee = new WeekOfInputParams();
            employee.PageNumber = PageNumber;
            employee.PageSize = PageSize;
            employee.Month = Month;
            employee.Year = Year;
            employee.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            employee.RoleId = Convert.ToInt32(HttpContext.Session.GetString(Constants.RoleID)); ;
            employee.SearchTerm = string.IsNullOrEmpty(sSearch) ? null : sSearch;
            var data = _businessLayer.SendPostAPIRequest(
                employee,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmployeesWeekOffRoster),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var results = JsonConvert.DeserializeObject<List<WeekOffUploadModel>>(data);
            results.ForEach(x =>
            {
                x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.Id.ToString());
                x.EncryptedEmployeeId = _businessLayer.EncodeStringBase64(x.EmployeeId.ToString());
            });
            return Json(new
            {
                draw = sEcho,
                recordsTotal = results.Select(x => x.TotalCount).FirstOrDefault() ?? 0,
                recordsFiltered = results.Select(x => x.TotalCount).FirstOrDefault() ?? 0,
                data = results
            });
        }


        [HttpGet]
        public async Task<IActionResult> AddUpdateWeekOffRoster(string id, string emp)
        {

            WeekOfEmployeeId Employeemodel = new WeekOfEmployeeId();
            WeekOffUploadModel modeldata = new WeekOffUploadModel();
            WeekOfInputParams modeldatas = new WeekOfInputParams();

            if (id == null)
            {
                modeldatas.Id = 0;
            }
            else
            {
                modeldatas.Id = Convert.ToInt32(_businessLayer.DecodeStringBase64(id));
            }

            if (id != null)
            {
                var getdata = _businessLayer.SendPostAPIRequest(
                    modeldatas,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmployeesWeekOffRoster),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();
                modeldata = JsonConvert.DeserializeObject<List<WeekOffUploadModel>>(getdata).FirstOrDefault();
                modeldata.EmployeeIdWithEmployeeNo = modeldata.EmployeeId.ToString();
            }
            if (emp == null)
            {
                Employeemodel.EmployeeID = 0;
            }
            else
            {
                Employeemodel.EmployeeID = Convert.ToInt64(_businessLayer.DecodeStringBase64(emp));
            }

            var data = _businessLayer.SendPostAPIRequest(
                    Employeemodel,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployeesList),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();


            var employeeList = JsonConvert.DeserializeObject<List<SelectListItem>>(data);

            if (id != null)
            {
                if (employeeList != null && employeeList.Count > 0)
                {
                    foreach (var item in employeeList)
                    {
                        if (!string.IsNullOrEmpty(item.Value) && item.Value.Contains("_"))
                        {
                            item.Value = item.Value.Split('_')[0];
                        }
                    }
                }

                modeldata.Employee = employeeList;
            }
            else
            {
                modeldata.Employee = JsonConvert.DeserializeObject<List<SelectListItem>>(data);
            }
            return View(modeldata);
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateWeekOffRoster(WeekOffUploadModel model)
        {
            try
            {
                var session = HttpContext.Session;
                var employeeIdString = session.GetString(Constants.EmployeeID);
                var token = session.GetString(Constants.SessionBearerToken);

                if (string.IsNullOrEmpty(employeeIdString) || string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { success = false, message = "Session expired. Please log in again." });
                }

                var employeeId = Convert.ToInt64(employeeIdString);

                if (model.EmployeeNumberWithOutAbbr == null)
                {
                    model.EmployeeNumber = model.EmployeeIdWithEmployeeNo.Split('_')[1];
                }
                else
                {
                    model.EmployeeNumber = model.EmployeeNumberWithOutAbbr;
                }

                    var weekOffUploadModel = new WeekOffUploadModelList
                    {
                        WeekOffList = new List<WeekOffUploadModel> { model },
                        CreatedBy = employeeId
                    };

                var validationError = ValidateModelList(weekOffUploadModel.WeekOffList);
                if (!string.IsNullOrEmpty(validationError))
                {
                    TempData[Constants.toastType] = Constants.toastTypeError;
                    TempData[Constants.toastMessage] = validationError;
                    return RedirectToAction(WebControllarsConstants.EmployeesWeekOffRoster, WebControllarsConstants.Employee);
                }

                var apiUrl = _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Employee,
                    APIApiActionConstants.GetRosterWeekOff
                );

                var apiResponse = await _businessLayer.SendPostAPIRequest(
                    weekOffUploadModel, apiUrl, token, true
                );

                if (apiResponse == null)
                {
                    TempData[Constants.toastType] = Constants.toastTypeError;
                    TempData[Constants.toastMessage] = "Failed to upload data to the server. Please try again later.";
                }
                else
                {
                    var results = JsonConvert.DeserializeObject<bool>(apiResponse.ToString());
                    if (results ==true)
                    {
                        TempData[Constants.toastType] = Constants.toastTypeSuccess;
                        TempData[Constants.toastMessage] = "Data saved successfully.";
                    }
                    else
                    {
                        TempData[Constants.toastType] = Constants.toastTypeError;
                        TempData[Constants.toastMessage] = "Something went wrong. Please try again later.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData[Constants.toastType] = Constants.toastTypeError;
                TempData[Constants.toastMessage] = "An unexpected error occurred. Please try again later.";
            }

            return RedirectToAction(WebControllarsConstants.EmployeesWeekOffRoster, WebControllarsConstants.Employee);
        }


        [HttpGet]
        public IActionResult DeleteWeekOffRoster(string id)
        {
            id = _businessLayer.DecodeStringBase64(id);
            WeekOffUploadDeleteModel model = new WeekOffUploadDeleteModel()
            {
                RecordId = Convert.ToInt64(id),
                ModifiedBy = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)),

            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteWeekOffRoster), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(WebControllarsConstants.EmployeesWeekOffRoster , WebControllarsConstants.Employee);
        }


        private string ValidateModelList(List<WeekOffUploadModel> models)
        {
            var errors = new List<string>();

            // ✅ Build a dictionary: EmployeeNumber => List of row numbers where it appears
            var employeeNumberToRows = new Dictionary<string, List<int>>();
            for (int i = 0; i < models.Count; i++)
            {
                var item = models[i];
                var rowNum = i + 2; // Excel rows start at 2

                if (string.IsNullOrWhiteSpace(item.EmployeeNumber) || item.EmployeeNumber == "0")
                    continue; // skip invalid or empty EmployeeNumber

                var key = item.EmployeeNumber;
                if (!employeeNumberToRows.ContainsKey(key))
                    employeeNumberToRows[key] = new List<int>();

                employeeNumberToRows[key].Add(rowNum);
            }

            // ✅ Report duplicates with their row numbers
            var duplicatesWithRows = employeeNumberToRows
                .Where(kvp => kvp.Value.Count > 1)
                .ToList();

            if (duplicatesWithRows.Any())
            {
                var details = duplicatesWithRows
                    .Select(kvp => $"EmployeeNumber '{kvp.Key}' at rows {string.Join(", ", kvp.Value)}")
                    .ToList();

                errors.Add($"Duplicate EmployeeNumber(s) found:\n{string.Join("\n", details)}");
            }

            for (int i = 0; i < models.Count; i++)
            {
                var item = models[i];
                var rowNum = i + 2; // Excel row

                if (string.IsNullOrWhiteSpace(item.EmployeeNumber) || item.EmployeeNumber == "0")
                {
                    errors.Add($"Row {rowNum}: Missing or invalid EmployeeNumber.");
                    continue; // Skip further checks if EmployeeNumber is missing
                }

                // ✅ Collect week-off dates into one list
                var weekOffDates = new List<DateTime>();
                AddDateIfValid(weekOffDates, item.WeekOff1, rowNum, "WeekOff1", errors);
                AddDateIfValid(weekOffDates, item.WeekOff2, rowNum, "WeekOff2", errors);
                AddDateIfValid(weekOffDates, item.WeekOff3, rowNum, "WeekOff3", errors);
                AddDateIfValid(weekOffDates, item.WeekOff4, rowNum, "WeekOff4", errors);

                // ✅ Check that at least one date was filled
                if (!weekOffDates.Any())
                    errors.Add($"Row {rowNum}: At least one WeekOff date must be filled in.");

                // ✅ Check for duplicate dates within this row
                var duplicateDates = weekOffDates
                    .GroupBy(d => d)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key.ToString("yyyy-MM-dd"))
                    .ToList();

                if (duplicateDates.Any())
                    errors.Add($"Row {rowNum}: Duplicate WeekOff dates found: {string.Join(", ", duplicateDates)}.");
            }

            return errors.Any() ? string.Join("\n", errors) : null;
        }

        /// <summary>
        /// Adds a date to the list if it's valid and not unreasonable, otherwise records an error.
        /// </summary>
        private void AddDateIfValid(List<DateTime> dates, DateTime? date, int rowNum, string fieldName, List<string> errors)
        {
            if (date.HasValue)
            {
                if (date.Value.Year < 2000)
                    errors.Add($"Row {rowNum}: {fieldName} '{date.Value}' is an invalid or unreasonable date.");
                else
                    dates.Add(date.Value.Date);  // Always add as date-only (ignores time part)
            }
        }
    }
}
