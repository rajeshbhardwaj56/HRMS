
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
using Microsoft.IdentityModel.Xml;
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
using System.Threading.Tasks;
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
        public EmployeeController(ICheckUserFormPermission CheckUserFormPermission,IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IS3Service s3Service, IHttpContextAccessor context)
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
        public async Task<IActionResult> EmployeeListing()
        {
            var employeeId = GetSessionInt(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);
            // Check if the user has permission for Employee Listing
            var formPermission =await _CheckUserFormPermission.GetFormPermission(employeeId, (int)PageName.EmployeeListing);
            // If no permission and not an admin
            if (formPermission.HasPermission == 0 && roleId != (int)Roles.Admin && roleId != (int)Roles.SuperAdmin)
            {
                // Check if user has permission for My Team page
                var teamPermission =await _CheckUserFormPermission.GetFormPermission(employeeId, (int)PageName.MyTeam);
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
        public async Task<JsonResult> EmployeeListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            var employee = new EmployeeInputParams
            {
                CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID)),
                RoleID = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID)),
                DisplayStart = iDisplayStart,
                DisplayLength = iDisplayLength,
                Searching = string.IsNullOrEmpty(sSearch) ? null : sSearch
            };

            var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.Employee,
                APIApiActionConstants.GetAllEmployees
            );

            var response = await _businessLayer.SendPostAPIRequest(
                employee,
                apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );

            var data = response?.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);

            if (results?.Employees != null)
            {
                var employeeTasks = results.Employees.Select(async x =>
                {
                    x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.EmployeeID.ToString());
                    x.EncodedDesignationID = _businessLayer.EncodeStringBase64(x.DesignationID.ToString());
                    x.EncodedDepartmentIDID = _businessLayer.EncodeStringBase64(x.DepartmentID.ToString());
                    x.ProfilePhoto = string.IsNullOrEmpty(x.ProfilePhoto)
                        ? "/assets/img/No_image.png"
                        : await _s3Service.GetFileUrl(x.ProfilePhoto);
                });

                await Task.WhenAll(employeeTasks); // Await all tasks to ensure updates are applied before return
            }

            return Json(new
            {
                draw = sEcho,
                recordsTotal = results?.Employees?.FirstOrDefault()?.TotalRecords ?? 0,
                recordsFiltered = results?.Employees?.FirstOrDefault()?.FilteredRecords ?? 0,
                data = results?.Employees
            });
        }


        public async Task<IActionResult> Index(string id)
        {
            var companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);

            var employee = new EmployeeModel
            {
                CompanyID = companyId
            };

            if (string.IsNullOrEmpty(id))
            {
                // Initialize empty details for new employee entry
                employee.FamilyDetails.Add(new FamilyDetail());
                employee.EducationalDetails.Add(new EducationalDetail());
                employee.LanguageDetails.Add(new LanguageDetail());
                employee.EmploymentHistory.Add(new EmploymentHistory());
                employee.References = new List<HRMS.Models.Employee.Reference>
        {
            new HRMS.Models.Employee.Reference(),
            new HRMS.Models.Employee.Reference()
        };
            }
            else
            {
                // Decode and fetch existing employee data
                var encryptedId = id;
                id = _businessLayer.DecodeStringBase64(id);
                employee.EmployeeID = Convert.ToInt64(id);

                var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees);
                var response = await _businessLayer.SendPostAPIRequest(employee, apiUrl, bearerToken, true);
                var employeeData = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(response.ToString()).employeeModel;

                employee = employeeData;
                employee.EncryptedIdentity = encryptedId;

                // Resolve S3 URLs
                if (!string.IsNullOrEmpty(employee.ProfilePhoto))
                    employee.ProfilePhoto = await _s3Service.GetFileUrl(employee.ProfilePhoto);

                if (!string.IsNullOrEmpty(employee.AadhaarCardImage))
                    employee.AadhaarCardImage = await _s3Service.GetFileUrl(employee.AadhaarCardImage);

                if (!string.IsNullOrEmpty(employee.PanCardImage))
                    employee.PanCardImage = await _s3Service.GetFileUrl(employee.PanCardImage);

                // Ensure References always has two items
                if (employee.References == null || employee.References.Count == 0)
                {
                    employee.References = new List<HRMS.Models.Employee.Reference> { new HRMS.Models.Employee.Reference(), new HRMS.Models.Employee.Reference() };
                }
                else if (employee.References.Count == 1)
                {
                    employee.References.Add(new HRMS.Models.Employee.Reference());
                }
            }

            // Load dropdown dictionaries
            var results = GetAllResults(employee.CompanyID);
            employee.Languages = results.Languages;
            employee.Countries = results.Countries;
            employee.EmploymentTypes = results.EmploymentTypes;
            employee.Departments = results.Departments;

            return View(employee);
        }


        [HttpPost]
        public async Task<IActionResult> Index(
        EmployeeModel employee,
        List<IFormFile> postedFiles,
        List<IFormFile> PanPostedFile,
        List<IFormFile> AadhaarPostedFile)
        {
            var results = GetAllResults(employee.CompanyID);

            try
            {
                if (!ModelState.IsValid)
                {
                    SetWarningToast("Please check all data and try again.");
                    return ReturnEmployeeViewWithData(employee, results);
                }

                // Process profile photo upload
                string newProfileKey = await _s3Service.ProcessFileUploadAsync(postedFiles, employee.ProfilePhoto);
              
                if (!string.IsNullOrEmpty(newProfileKey))
                {
                    if (!string.IsNullOrEmpty(employee.ProfilePhoto))
                    {
                       await _s3Service.DeleteFileAsync(employee.ProfilePhoto);
                    }
                    employee.ProfilePhoto = newProfileKey;
                }
                else
                {
                    employee.ProfilePhoto = _s3Service.ExtractKeyFromUrl(employee.ProfilePhoto);
                }

            
                string newPanKey = await _s3Service.ProcessFileUploadAsync(postedFiles, employee.PanCardImage);              
                if (!string.IsNullOrEmpty(newPanKey))
                {
                    if (!string.IsNullOrEmpty(employee.PanCardImage))
                    {
                       await _s3Service.DeleteFileAsync(employee.PanCardImage);
                    }
                    employee.PanCardImage = newPanKey;
                }
                else
                {
                    employee.PanCardImage = _s3Service.ExtractKeyFromUrl(employee.PanCardImage);
                }

                // Process Aadhaar file upload (optional)
                string newAadhaarKey = await _s3Service.ProcessFileUploadAsync(AadhaarPostedFile, employee.AadhaarCardImage);

           
                if (!string.IsNullOrEmpty(newAadhaarKey))
                {
                    if (!string.IsNullOrEmpty(employee.AadhaarCardImage))
                    {
                       await _s3Service.DeleteFileAsync(employee.AadhaarCardImage);
                    }
                    employee.AadhaarCardImage = newAadhaarKey;
                }
                else
                {
                    employee.AadhaarCardImage = _s3Service.ExtractKeyFromUrl(employee.AadhaarCardImage);
                }

                // Call API
                var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Employee,
                    APIApiActionConstants.AddUpdateEmployee);

                var apiResult = await _businessLayer.SendPostAPIRequest(
                    employee,
                    apiUrl,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true);

                var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(apiResult.ToString());

                if (result != null && result.PKNo > 0)
                {
                    SetSuccessToast("Data saved successfully.");
                    return RedirectToActionPermanent(WebControllarsConstants.EmployeeListing, WebControllarsConstants.Employee);
                }

                SetWarningToast("Please check all data and try again.");
            }
            catch (Exception ex)
            {
                // Consider logging the error here using ILogger
                SetWarningToast(ex.ToString());
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
        public async Task<JsonResult> ActiveEmployeeListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            EmployeeInputParams employee = new EmployeeInputParams();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllActiveEmployees);
            var apiResponse = await _businessLayer.SendPostAPIRequest(employee, apiUrl, token, true);
            var data = apiResponse?.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            results.Employees.ForEach(x => x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.EmployeeID.ToString()));
            return Json(new { data = results.Employees });
        }
        [HttpPost]
        public async Task<JsonResult> GetL2Manager(int l1EmployeeId)
        {
            try
            {
                var input = new L2ManagerInputParams
                {
                    L1EmployeeID = l1EmployeeId
                };

                var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Employee,
                    APIApiActionConstants.GetL2ManagerDetails
                );

                var apiResponse =await _businessLayer.SendPostAPIRequest(
                    input,
                    apiUrl,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                );

                if (!string.IsNullOrEmpty(apiResponse.ToString()))
                {
                    var managerDetail = JsonConvert.DeserializeObject<L2ManagerDetail>(apiResponse.ToString());
                    if (managerDetail != null && managerDetail.ManagerID > 0)
                    {
                        return Json(new
                        {
                            success = true,
                            managerId = managerDetail.ManagerID,
                            managerName = managerDetail.ManagerName
                        });
                    }
                }

                return Json(new
                {
                    success = false,
                    message = "L2 manager not found."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while fetching the L2 manager.",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<JsonResult> LoadForms(int departmentId, int employeeId)
        {
            try
            {
                var objPermission = new FormPermissionVM
                {
                    DepartmentId = departmentId,
                    EmployeeID = employeeId
                };

                var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Common,
                    APIApiActionConstants.GetUserFormByDepartmentID
                );

                var response = await _businessLayer.SendPostAPIRequest(
                    objPermission,
                    apiUrl,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                );

                if (response != null && !string.IsNullOrWhiteSpace(response.ToString()))
                {
                    var formList = JsonConvert.DeserializeObject<List<FormPermissionViewModel>>(response.ToString());
                    return Json(new { success = true, data = formList });
                }

                return Json(new { success = false, message = "No forms found." });
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
        public async Task<ActionResult> EmploymentDetails(string id, string DegtId, string DeptId)
        {
            var companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var userId = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
            var bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);

            // Prepare input params
            var employmentDetailInputParams = new EmploymentDetailInputParams
            {
                CompanyID = companyId,
                UserID = userId
            };

            // Fetch company logo and abbreviation
            var companyModel = new CompanyLoginModel { CompanyID = companyId };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetCompaniesLogo);
            var apiResponse = await _businessLayer.SendPostAPIRequest(companyModel, apiUrl, token, true);
            var data = apiResponse?.ToString();


            companyModel = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(apiResponse.ToString()).companyLoginModel;

            if (!string.IsNullOrEmpty(companyModel.CompanyLogo))
            {
                companyModel.CompanyLogo = await _s3Service.GetFileUrl(companyModel.CompanyLogo);
            }

            // Decode and populate employee info if present
            if (!string.IsNullOrEmpty(id))
            {
                employmentDetailInputParams.EmployeeID = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
                employmentDetailInputParams.DesignationID = Convert.ToInt64(_businessLayer.DecodeStringBase64(DegtId));
                employmentDetailInputParams.DepartmentID = Convert.ToInt64(_businessLayer.DecodeStringBase64(DeptId));
            }
            var employeeToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var employeeApiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFilterEmploymentDetailsByEmployee);
            var employeeApiResponse = await _businessLayer.SendPostAPIRequest(employmentDetailInputParams, employeeApiUrl, employeeToken, true);
           

            var employmentDetail = JsonConvert.DeserializeObject<EmploymentDetail>(employeeApiResponse.ToString());

            // Append values
            employmentDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentDetail.EmployeeID.ToString());
            employmentDetail.EmployeNumber = companyModel.Abbr + employmentDetail.EmployeNumber;
            employmentDetail.CompanyAbbr = companyModel.Abbr;

            return View(employmentDetail);
        }

        [HttpGet]
        public async Task<ActionResult> FilterEmploymentDetails(string id, long departmentID, long designationID)
        {
            var companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var userId = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
          

            var inputParams = new EmploymentDetailInputParams
            {
                CompanyID = companyId,
                DepartmentID = departmentID,
                DesignationID = designationID,
                UserID = userId
            };

            if (!string.IsNullOrEmpty(id))
            {
                var decodedId = _businessLayer.DecodeStringBase64(id);
                inputParams.EmployeeID = Convert.ToInt64(decodedId);
            }
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFilterEmploymentDetailsByEmployee);
            var apiResponse = await _businessLayer.SendPostAPIRequest(inputParams, apiUrl, token, true);
         

            var employmentDetail = JsonConvert.DeserializeObject<EmploymentDetail>(apiResponse.ToString());

            if (employmentDetail != null && employmentDetail.EmployeeID > 0)
            {
                employmentDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentDetail.EmployeeID.ToString());
            }

            return Json(employmentDetail);
        }


        [HttpPost]
        public async Task<ActionResult> EmploymentDetails(EmploymentDetail employmentDetail, List<string> SelectedFormIds)
        {
            if (ModelState.IsValid)
            {
                long companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                employmentDetail.CompanyID = companyId;
                employmentDetail.EmployeNumber = employmentDetail.EmployeNumber.Split(employmentDetail.CompanyAbbr)[1];
                var bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);

                string employmentApiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmploymentDetails);
                var employmentResponse = await _businessLayer.SendPostAPIRequest(employmentDetail, employmentApiUrl, bearerToken, true);

                Result result = JsonConvert.DeserializeObject<Result>(employmentResponse.ToString());
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
                
                    // Save permissions
                    FormPermissionVM permissionModel = new FormPermissionVM
                    {
                        EmployeeID = employmentDetail.EmployeeID,
                        SelectedFormIds = SelectedFormIds
                    };
                    string permissionApiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.AddUserFormPermissions);
                    var permissionResponseStr = await _businessLayer.SendPostAPIRequest(permissionModel, permissionApiUrl, bearerToken, true);
                    long permissionResponse = JsonConvert.DeserializeObject<long>(permissionResponseStr.ToString());
                    if (permissionResponse < 0)
                    {
                        TempData[Constants.toastType] = Constants.toastTypeError;
                        TempData[Constants.toastMessage] = "Some error occurred, please try again later.";
                        return View(employmentDetail);
                    }
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                    if (result.IsResetPasswordRequired)
                    {
                        ChangePasswordModel model = new ChangePasswordModel();
                        model.EmailId = employmentDetail.CompanyAbbr+  employmentDetail.EmployeNumber;
                        var apiUrls =await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.GetFogotPasswordDetails);
                        var datamodel =await _businessLayer.SendPostAPIRequest(model, apiUrls, null, false);
                        var resultdata = JsonConvert.DeserializeObject<Result>(datamodel.ToString());

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
                                emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);

                                if (responses.responseCode == "200")
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
                // ModelState is not valid — load supporting lists
                var inputParams = new EmploymentDetailInputParams { CompanyID = companyId };
                var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFilterEmploymentDetailsByEmployee);
                var response = await _businessLayer.SendPostAPIRequest(inputParams, apiUrl, bearerToken, true);
                EmploymentDetail employmentDetailtemp = JsonConvert.DeserializeObject<EmploymentDetail>(response.ToString());
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
                // ModelState is not valid — load supporting lists
                var companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                var inputParams = new EmploymentDetailInputParams { CompanyID = companyId };

                var bearerToken = HttpContext.Session.GetString(Constants.SessionBearerToken);
                var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFilterEmploymentDetailsByEmployee);
                var response = await _businessLayer.SendPostAPIRequest(inputParams, apiUrl, bearerToken, true);
                EmploymentDetail employmentDetailtemp = JsonConvert.DeserializeObject<EmploymentDetail>(response.ToString());

                MapEmploymentDetailLists(employmentDetail, employmentDetailtemp);
                employmentDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentDetail.EmployeeID.ToString());
            }

            return View(employmentDetail);
        }

        private void MapEmploymentDetailLists(EmploymentDetail target, EmploymentDetail source)
        {
            target.EmployeeList = source.EmployeeList;
            target.Departments = source.Departments;
            target.JobLocations = source.JobLocations;
            target.Designations = source.Designations;
            target.EmploymentTypes = source.EmploymentTypes;
            target.PayrollTypes = source.PayrollTypes;
            target.LeavePolicyList = source.LeavePolicyList;
            target.RoleList = source.RoleList;
            target.SubDepartments = source.SubDepartments;
            target.ShiftTypes = source.ShiftTypes;
        }


        [HttpGet]
        public async Task<ActionResult> EmploymentBankDetails(string id)
        {
            var userId = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
            var companyToken = HttpContext.Session.GetString(Constants.SessionBearerToken);

            var inputParams = new EmploymentBankDetailInputParams
            {
                UserID = userId
            };

            if (!string.IsNullOrEmpty(id))
            {
                var decodedId = _businessLayer.DecodeStringBase64(id);
                inputParams.EmployeeID = Convert.ToInt64(decodedId);
            }

            var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.Employee,
                APIApiActionConstants.GetEmploymentBankDetails
            );

            var response = await _businessLayer.SendPostAPIRequest(
                inputParams, apiUrl, companyToken, true
            );

            var employmentBankDetail = JsonConvert.DeserializeObject<EmploymentBankDetail>(response.ToString());

            if (employmentBankDetail != null)
            {
                employmentBankDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(employmentBankDetail.EmployeeID.ToString());

                if (!string.IsNullOrEmpty(employmentBankDetail.AadhaarCardImage))
                {
                    employmentBankDetail.AadhaarCardImage = await _s3Service.GetFileUrl(employmentBankDetail.AadhaarCardImage);
                }

                if (!string.IsNullOrEmpty(employmentBankDetail.PanCardImage))
                {
                    employmentBankDetail.PanCardImage = await _s3Service.GetFileUrl(employmentBankDetail.PanCardImage);
                }
            }

            return View(employmentBankDetail);
        }


         
        [HttpPost]
        public async Task<ActionResult> EmploymentBankDetails(EmploymentBankDetail employmentBankDetail,List<IFormFile> PanPostedFile,List<IFormFile> AadhaarPostedFile)
        {
            if (ModelState.IsValid)
            {
                employmentBankDetail.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

                // Process PAN file
                string newPanKey = await _s3Service.ProcessFileUploadAsync(PanPostedFile, employmentBankDetail.PanCardImage);

               
                if (!string.IsNullOrEmpty(newPanKey))
                {
                    if (!string.IsNullOrEmpty(employmentBankDetail.PanCardImage))
                    {
                       await _s3Service.DeleteFileAsync(employmentBankDetail.PanCardImage);
                    }
                    employmentBankDetail.PanCardImage = newPanKey;
                }
                else
                {
                    employmentBankDetail.PanCardImage = _s3Service.ExtractKeyFromUrl(employmentBankDetail.PanCardImage);
                }              
                string newAadhaarKey= await _s3Service.ProcessFileUploadAsync(AadhaarPostedFile, employmentBankDetail.AadhaarCardImage);
                if (!string.IsNullOrEmpty(newAadhaarKey))
                {
                    if (!string.IsNullOrEmpty(employmentBankDetail.AadhaarCardImage))
                    {
                       await _s3Service.DeleteFileAsync(employmentBankDetail.AadhaarCardImage);
                    }
                    employmentBankDetail.AadhaarCardImage = newAadhaarKey;
                }
                else
                {
                    employmentBankDetail.AadhaarCardImage = _s3Service.ExtractKeyFromUrl(employmentBankDetail.AadhaarCardImage);
                }

           
                var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Employee,
                    APIApiActionConstants.AddUpdateEmploymentBankDetails);

                var response =await _businessLayer.SendPostAPIRequest(
                    employmentBankDetail,
                    apiUrl,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true);

                var result = JsonConvert.DeserializeObject<Result>(response.ToString());

                if (result != null && result.PKNo > 0)
                {
                    TempData[Constants.toastType] = Constants.toastTypeSuccess;
                    TempData[Constants.toastMessage] = "Bank details saved successfully.";
                    return RedirectToActionPermanent(
                        WebControllarsConstants.EmployeeListing,
                        WebControllarsConstants.Employee);
                }
                else
                {
                    TempData[Constants.toastType] = Constants.toastTypeError;
                    TempData[Constants.toastMessage] = "Failed to save bank details.";
                }
            }

            // Return view with validation errors or failed upload
            return View(employmentBankDetail);
        }

        [HttpGet]
        public async Task<ActionResult> EmploymentSeparation(string id)
        {
            var userId = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));

            var inputParams = new EmploymentSeparationInputParams
            {
                UserID = userId
            };

            if (!string.IsNullOrEmpty(id))
            {
                var decodedId = _businessLayer.DecodeStringBase64(id);
                inputParams.EmployeeID = long.Parse(decodedId);
            }

            var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.Employee,
                APIApiActionConstants.GetEmploymentSeparationDetails);

            var jsonData = await _businessLayer.SendPostAPIRequest(
                inputParams,
                apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true);

            var separationDetail = JsonConvert.DeserializeObject<EmploymentSeparationDetail>(jsonData.ToString());
            separationDetail.EncryptedIdentity = _businessLayer.EncodeStringBase64(separationDetail.EmployeeID.ToString());

            return View(separationDetail);
        }

        [HttpPost]
        public async Task<ActionResult> EmploymentSeparation(EmploymentSeparationDetail model)
        {
            if (ModelState.IsValid)
            {
                model.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

                var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Employee,
                    APIApiActionConstants.AddUpdateEmploymentSeparationDetails);

                var jsonData =await _businessLayer.SendPostAPIRequest(
                    model,
                    apiUrl,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true);

                var result = JsonConvert.DeserializeObject<Result>(jsonData.ToString());

                if (result != null && result.PKNo > 0)
                {
                    TempData[Constants.toastType] = Constants.toastTypeSuccess;
                    TempData[Constants.toastMessage] = "Employee Separation details saved successfully.";
                    return RedirectToActionPermanent(
                        WebControllarsConstants.EmployeeListing,
                        WebControllarsConstants.Employee);
                }
                else
                {
                    TempData[Constants.toastType] = Constants.toastTypeError;
                    TempData[Constants.toastMessage] = "Failed to save Employee Separation details.";
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Whatshappening()
        {
            var employeeId = GetSessionInt(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);

            var formPermission = await _CheckUserFormPermission.GetFormPermission(employeeId, (int)PageName.Whatshappening);

            if (formPermission.HasPermission == 0 && roleId != (int)Roles.Admin && roleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(); // Don't forget to await async calls
                return RedirectToAction("Index", "Home");
            }

            var results = new HRMS.Models.Common.Results();
            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Whatshappening(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            var companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            var inputParams = new WhatsHappeningModelParans
            {
                CompanyID = companyId
            };

            var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.LeavePolicy,
                APIApiActionConstants.GetAllWhatsHappeningDetails
            );

            var jsonResponse =await _businessLayer.SendPostAPIRequest(
                inputParams,
                apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );

            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(jsonResponse.ToString());

            // Fetch image URLs asynchronously and wait for all to finish
            var tasks = results.WhatsHappeningList
                .Select(async x => x.IconImage = await _s3Service.GetFileUrl(x.IconImage))
                .ToList();

            await Task.WhenAll(tasks);

            return Json(new
            {
                draw = sEcho,
                recordsTotal = results.WhatsHappeningList.Count,
                recordsFiltered = results.WhatsHappeningList.Count,
                data = results.WhatsHappeningList
            });
        }


        [HttpGet]
        public async Task<IActionResult> InActiveEmployee(int employeeId, int isActive)
        {
            var input = new ReportingStatus
            {
                EmployeeId = employeeId,
                Status = isActive
            };

            var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.Employee,
                APIApiActionConstants.CheckEmployeeReporting
            );

            var responseJson =await _businessLayer.SendPostAPIRequest(
                input,
                apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );

            var reportingData = JsonConvert.DeserializeObject<ReportingStatus>(responseJson.ToString());

            return Ok(new
            {
                success = true,
                data = reportingData
            });
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
                var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
                var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.FetchExportEmployeeExcelSheet);
                var apiResponse = await _businessLayer.SendPostAPIRequest(models, apiUrl, token, true);      
                var responseString = apiResponse.ToString();
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
        public async Task<JsonResult> GetEducationDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            var educationDetailParams = new EducationDetailParams
            {
                EmployeeID = EmployeeID
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEducationDetails);
            var apiResponse = await _businessLayer.SendPostAPIRequest(educationDetailParams, apiUrl, token, true);

            
            var data = apiResponse?.ToString();
            var results = JsonConvert.DeserializeObject<List<EducationalDetail>>(data) ?? new List<EducationalDetail>();

           
            var tasks = results
                .Where(x => !string.IsNullOrEmpty(x.CertificateImage))
                .Select(async x =>
                {
                    x.CertificateImage = await _s3Service.GetFileUrl(x.CertificateImage);
                });

            await Task.WhenAll(tasks);

            return Json(new
            {
                draw = sEcho,
                recordsTotal = results.Count,
                recordsFiltered = results.Count,
                data = results
            });
        }

        [HttpPost]
        public async Task<JsonResult> EducationalDetail(EducationalDetail eduDetail, List<IFormFile> CertificateFile)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            eduDetail.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));


          string newCertificateKey= await _s3Service.ProcessFileUploadAsync(CertificateFile, eduDetail.CertificateImage);

            if (!string.IsNullOrEmpty(newCertificateKey))
            {
                if (!string.IsNullOrEmpty(eduDetail.CertificateImage))
                   await _s3Service.DeleteFileAsync(eduDetail.CertificateImage);

                eduDetail.CertificateImage = newCertificateKey;
            }
            else
            {
                eduDetail.CertificateImage = _s3Service.ExtractKeyFromUrl(eduDetail.CertificateImage);
            }
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEducationDetail);
            var apiResponse = await _businessLayer.SendPostAPIRequest(eduDetail, apiUrl, token, true);
           

            var result = JsonConvert.DeserializeObject<Result>(apiResponse.ToString());

            if (result != null && result.PKNo > 0)
                return Json(new { success = true, message = "Education details saved successfully." });

            return Json(new { success = false, message = "Failed to save education details." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEducationDetail(long encodedId)
        {
            var model = new EducationDetailParams
            {
                EducationDetailID = encodedId
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteEducationDetail);
            var apiResponse = await _businessLayer.SendPostAPIRequest(model, apiUrl, token, true);
            var responseStr = apiResponse?.ToString();
            if (!string.IsNullOrWhiteSpace(responseStr))
            {
                return Json(new { success = true, message = responseStr });
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
        public async Task<JsonResult> GetFamilyDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            var familyDetailParams = new FamilyDetailParams
            {
                EmployeeID = EmployeeID
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetFamilyDetails);
            var apiResponse = await _businessLayer.SendPostAPIRequest(familyDetailParams, apiUrl, token, true);
         
            var data = apiResponse?.ToString();
            var results = JsonConvert.DeserializeObject<List<FamilyDetail>>(data) ?? new List<FamilyDetail>();

            return Json(new
            {
                draw = sEcho,
                recordsTotal = results.Count,
                recordsFiltered = results.Count,
                data = results
            });
        }

        [HttpPost]
        public async Task<JsonResult> FamilyDetail(FamilyDetail famDetail)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            famDetail.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateFamilyDetail);
            var apiResponse = await _businessLayer.SendPostAPIRequest(famDetail, apiUrl, token, true);        
            var result = JsonConvert.DeserializeObject<Result>(apiResponse?.ToString());

            if (result != null && result.PKNo > 0)
            {
                return Json(new { success = true, message = "Family details saved successfully." });
            }

            return Json(new { success = false, message = "Failed to save family details." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFamilyDetail(long encodedId)
        {
            var model = new FamilyDetailParams
            {
                EmployeesFamilyDetailID = encodedId
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteFamilyDetail);
            var apiResponse = await _businessLayer.SendPostAPIRequest(model, apiUrl, token, true); 

            var responseStr = apiResponse?.ToString();
            if (!string.IsNullOrWhiteSpace(responseStr))
            {
                return Json(new { success = true, message = responseStr });
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
        public async Task<JsonResult> GetReferenceDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            var referenceParams = new ReferenceParams
            {
                EmployeeID = EmployeeID
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetReferenceDetails);
            var apiResponse = await _businessLayer.SendPostAPIRequest(referenceParams, apiUrl, token, true);
          

            var data = apiResponse?.ToString();
            var results = JsonConvert.DeserializeObject<List<HRMS.Models.Employee.Reference>>(data) ?? new List<HRMS.Models.Employee.Reference>();

            return Json(new
            {
                draw = sEcho,
                recordsTotal = results.Count,
                recordsFiltered = results.Count,
                data = results
            });
        }

        [HttpPost]
        public async Task<JsonResult> ReferenceDetail(HRMS.Models.Employee.Reference reference)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            reference.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateReferenceDetail);
            var apiResponse = await _businessLayer.SendPostAPIRequest(reference, apiUrl, token, true);
            var result = JsonConvert.DeserializeObject<Result>(apiResponse?.ToString());

            if (result != null && result.PKNo > 0)
            {
                return Json(new { success = true, message = "Reference details saved successfully." });
            }

            return Json(new { success = false, message = "Failed to save reference details." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReferenceDetail(long encodedId)
        {
            var model = new ReferenceParams
            {
                ReferenceDetailID = encodedId
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteReferenceDetail);
            var apiResponse = await _businessLayer.SendPostAPIRequest(model, apiUrl, token, true);


            var resultStr = apiResponse?.ToString();

            if (!string.IsNullOrWhiteSpace(resultStr))
            {
                return Json(new { success = true, message = resultStr });
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
        public async Task<JsonResult> GetEmploymentHistoryDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            var requestParams = new EmploymentHistoryParams
            {
                EmployeeID = EmployeeID
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmploymentHistory);
            var apiResponse = await _businessLayer.SendPostAPIRequest(requestParams, apiUrl, token, true);
            var data = apiResponse?.ToString();
            var results = JsonConvert.DeserializeObject<List<HRMS.Models.Employee.EmploymentHistory>>(data) ?? new List<HRMS.Models.Employee.EmploymentHistory>();

            return Json(new
            {
                draw = sEcho,
                recordsTotal = results.Count,
                recordsFiltered = results.Count,
                data = results
            });
        }

        [HttpPost]
        public async Task<JsonResult> EmploymentHistory(HRMS.Models.Employee.EmploymentHistory history)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            history.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmploymentHistory);
            var apiResponse = await _businessLayer.SendPostAPIRequest(history, apiUrl, token, true);
           

            var result = JsonConvert.DeserializeObject<Result>(apiResponse?.ToString());

            if (result != null && result.PKNo > 0)
            {
                return Json(new { success = true, message = "Employment history saved successfully." });
            }

            return Json(new { success = false, message = "Failed to save employment history." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmploymentHistory(long encodedId)
        {
            var model = new EmploymentHistoryParams
            {
                EmploymentHistoryID = encodedId
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteEmploymentHistory);
            var apiResponse = await _businessLayer.SendPostAPIRequest(model, apiUrl, token, true);       
            var resultStr = apiResponse?.ToString();

            if (!string.IsNullOrWhiteSpace(resultStr))
            {
                return Json(new { success = true, message = resultStr });
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
        public async Task<JsonResult> GetLanguageDetails(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long EmployeeID)
        {
            var requestParams = new HRMS.Models.Employee.LanguageDetailParams
            {
                EmployeeID = EmployeeID
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetLanguageDetails);
            var apiResponse = await _businessLayer.SendPostAPIRequest(requestParams, apiUrl, token, true);
          

            var data = apiResponse?.ToString();
            var results = JsonConvert.DeserializeObject<List<HRMS.Models.Employee.LanguageDetail>>(data) ?? new List<HRMS.Models.Employee.LanguageDetail>();

            return Json(new
            {
                draw = sEcho,
                recordsTotal = results.Count,
                recordsFiltered = results.Count,
                data = results
            });
        }

        [HttpPost]
        public async Task<JsonResult> LanguageDetail(HRMS.Models.Employee.LanguageDetail language)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            language.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID));
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateLanguageDetail);
            var apiResponse = await _businessLayer.SendPostAPIRequest(language, apiUrl, token, true);      
            var result = JsonConvert.DeserializeObject<Result>(apiResponse?.ToString());

            if (result != null && result.PKNo > 0)
                return Json(new { success = true, message = "Language detail saved successfully." });

            return Json(new { success = false, message = "Failed to save language detail." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLanguageDetail(long encodedId)
        {
            var model = new HRMS.Models.Employee.LanguageDetailParams
            {
                EmployeeLanguageDetailID = encodedId
            };
            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);
            var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeleteLanguageDetail);
            var apiResponse = await _businessLayer.SendPostAPIRequest(model, apiUrl, token, true);


       

            var resultStr = apiResponse?.ToString();

            if (!string.IsNullOrWhiteSpace(resultStr))
                return Json(new { success = true, message = resultStr });

            return Json(new { success = false, message = "Failed to delete the record." });
        }


    }
}
