 
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.Employee;
using HRMS.Models.ExportEmployeeExcel;
using HRMS.Models.ImportFromExcel;
using HRMS.Models.WhatsHappeningModel;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
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
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.SuperAdmin))]
    public class EmployeeController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        private readonly IS3Service _s3Service;
        private readonly IHttpContextAccessor _context;
        public EmployeeController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IS3Service s3Service, IHttpContextAccessor context)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            EmailSender.configuration = _configuration;
            _s3Service = s3Service;
            _context = context;
        }
        public IActionResult EmployeeListing()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
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
                    ? "/assets/img/No_image.png"   //Use default image if profile photo is missing
                    : _s3Service.GetFileUrl(x.ProfilePhoto);
            });

            //return Json(new { data = results.Employees });
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
            string s3UploadUrl = _configuration["AWS:S3UploadUrl"];
            var results = GetAllResults(employee.CompanyID);

            try
            {
                if (!ModelState.IsValid)
                {
                    SetWarningToast("Please check all data and try again.");
                    return ReturnEmployeeViewWithData(employee, results);
                }
                ProcessFileUpload(postedFiles, employee.ProfilePhoto, out string newProfileKey);
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
                    employee.ProfilePhoto = ExtractKeyFromUrl(employee.ProfilePhoto);
                }

                ProcessFileUpload(PanPostedFile, employee.PanCardImage, out string newPanKey);
                if (!string.IsNullOrEmpty(newPanKey))
                {
                    if (!string.IsNullOrEmpty(employee.PanCardImage))
                    {
                        _s3Service.DeleteFile(employee.PanCardImage);
                    }
                    employee.PanCardImage = newPanKey;
                }
                else
                {
                    employee.PanCardImage = ExtractKeyFromUrl(employee.PanCardImage);
                }

                ProcessFileUpload(AadhaarPostedFile, employee.AadhaarCardImage, out string newAadhaarKey);
                if (!string.IsNullOrEmpty(newAadhaarKey))
                {
                    if (!string.IsNullOrEmpty(employee.AadhaarCardImage))
                    {
                        _s3Service.DeleteFile(employee.AadhaarCardImage);
                    }
                    employee.AadhaarCardImage = newAadhaarKey;
                }
                else
                {
                    employee.AadhaarCardImage = ExtractKeyFromUrl(employee.AadhaarCardImage);
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

        private void ProcessFileUpload(List<IFormFile> files, string existingKey, out string uploadedKey)
        {
            uploadedKey = string.Empty;

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file?.Length > 0)
                    {
                        uploadedKey = _s3Service.UploadFile(file, file.FileName);
                        if (!string.IsNullOrEmpty(uploadedKey)) break;
                    }
                }
            }
        }
        private string ExtractKeyFromUrl(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return string.Empty;

            var fileName = fileUrl.Substring(fileUrl.LastIndexOf('/') + 1);
            return fileName.Split('?')[0];
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
        public ActionResult EmploymentDetails(EmploymentDetail employmentDetail)
        {
            if (ModelState.IsValid)
            {
                var CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                employmentDetail.CompanyID = Convert.ToInt64(CompanyID);
                employmentDetail.EmployeNumber = employmentDetail.EmployeNumber.Split(employmentDetail.CompanyAbbr)[1];
                var data = _businessLayer.SendPostAPIRequest(employmentDetail, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmploymentDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                Result result = JsonConvert.DeserializeObject<Result>(data);
                if (result != null && result.PKNo > 0 && result.Message.Contains("EmailID already Exists in System, please try again with another email.", StringComparison.OrdinalIgnoreCase))
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
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                    //sendEmailProperties sendEmailProperties = new sendEmailProperties();
                    //sendEmailProperties.emailSubject = "Reset Password Email";
                    //sendEmailProperties.emailBody = ("Hi, <br/><br/> Please click on below link to reset password. <br/> <a target='_blank' href='" + string.Format(_configuration["AppSettings:RootUrl"] + _configuration["AppSettings:ResetPasswordURL"], _businessLayer.EncodeStringBase64((employmentDetail.EmployeeID == null ? "" : employmentDetail.EmployeeID.ToString()).ToString()), _businessLayer.EncodeStringBase64(DateTime.Now.ToString()), _businessLayer.EncodeStringBase64(CompanyID.ToString())) + "'> Click here to reset password</a>" + "<br/><br/>");
                    //sendEmailProperties.EmailToList.Add(employmentDetail.OfficialEmailID);
                    //emailSendResponse response = EmailSender.SendEmail(sendEmailProperties);
                    //if (response.responseCode == "200")
                    //{
                    //    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    //    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email have been sent, Please reset password for Login.";
                    //}
                    //else
                    //{
                    //    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    //    TempData[HRMS.Models.Common.Constants.toastMessage] = "Reset password email sending failed, Please try again later.";
                    //}
                }
                if (result.IsResetPasswordRequired)
                {
                    sendEmailProperties sendEmailProperties = new sendEmailProperties();
                    sendEmailProperties.emailSubject = "Reset Password Email";
                    sendEmailProperties.emailBody = ("Hi, <br/><br/> Please click on below link to reset password. <br/> " +
                        "<a target='_blank' href='" + string.Format(_configuration["AppSettings:RootUrl"] + _configuration["AppSettings:ResetPasswordURL"], _businessLayer.EncodeStringBase64((employmentDetail.EmployeeID == null ? "" : employmentDetail.EmployeeID.ToString()).ToString()), _businessLayer.EncodeStringBase64(DateTime.Now.ToString()), _businessLayer.EncodeStringBase64(CompanyID.ToString()), _businessLayer.EncodeStringBase64(employmentDetail.CompanyAbbr + employmentDetail.EmployeNumber.ToString())) + "'> Click here to reset password</a>" + "<br/><br/>");
                    sendEmailProperties.EmailToList.Add(employmentDetail.OfficialEmailID);
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
                ProcessFileUpload(PanPostedFile, employmentBankDetail.PanCardImage, out string newPanKey);
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
                    employmentBankDetail.PanCardImage = ExtractKeyFromUrl(employmentBankDetail.PanCardImage);
                }

                ProcessFileUpload(AadhaarPostedFile, employmentBankDetail.AadhaarCardImage, out string newAadhaarKey);
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
                    employmentBankDetail.AadhaarCardImage = ExtractKeyFromUrl(employmentBankDetail.AadhaarCardImage);
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

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Employees");

                // Define headers (same order as your properties)
                string[] headers = new[] {
        "EmployeeNumber", "CompanyName", "FirstName", "MiddleName", "Surname", "Gender", "DateOfBirth",
        "PlaceOfBirth", "EmailAddress", "PersonalEmailAddress", "Mobile", "Landline", "Telephone",
        "CorrespondenceAddress", "CorrespondenceCity", "CorrespondenceState", "CorrespondencePinCode",
        "PermanentAddress", "PermanentCity", "PermanentState", "PermanentPinCode", "PANNo",
        "AadharCardNo", "BloodGroup", "Allergies", "MajorIllnessOrDisability", "AwardsAchievements",
        "EducationGap", "ExtraCuricuarActivities", "ForiegnCountryVisits", "ContactPersonName",
        "ContactPersonMobile", "ContactPersonTelephone", "ContactPersonRelationship", "ITSkillsKnowledge",
        "Designation", "EmployeeType", "Department", "SubDepartment", "JobLocation", "ShiftType",
        "OfficialEmail", "OfficialContactNo", "JoiningDate", "ReportingManager", "PolicyName",
        "PayrollType", "ClientName", "ESINumber", "ESIRegistrationDate", "BankAccountNumber",
        "IFSCCode", "BankName", "UANNumber", "DateOfResignation", "DateOfLeaving", "LeavingType",
        "NoticeServed", "AgeOnNetwork", "PreviousExperience", "DateOfJoiningTraining",
        "DateOfJoiningFloor", "DateOfJoiningOJT", "DateOfJoiningOnroll", "BackOnFloorDate",
        "LeavingRemarks", "MailReceivedFromAndDate", "EmailSentToITDate"
    };

                // Add header row
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Fill data rows
                for (int rowIndex = 0; rowIndex < employees.Count; rowIndex++)
                {
                    var emp = employees[rowIndex];
                    int excelRow = rowIndex + 2;

                    worksheet.Cells[excelRow, 1].Value = emp.EmployeeNumber;
                    worksheet.Cells[excelRow, 2].Value = emp.CompanyName;
                    worksheet.Cells[excelRow, 3].Value = emp.FirstName;
                    worksheet.Cells[excelRow, 4].Value = emp.MiddleName;
                    worksheet.Cells[excelRow, 5].Value = emp.Surname;
                    worksheet.Cells[excelRow, 6].Value = emp.Gender;
                    worksheet.Cells[excelRow, 7].Value = emp.DateOfBirth?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 8].Value = emp.PlaceOfBirth;
                    worksheet.Cells[excelRow, 9].Value = emp.EmailAddress;
                    worksheet.Cells[excelRow, 10].Value = emp.PersonalEmailAddress;
                    worksheet.Cells[excelRow, 11].Value = emp.Mobile;
                    worksheet.Cells[excelRow, 12].Value = emp.Landline;
                    worksheet.Cells[excelRow, 13].Value = emp.Telephone;
                    worksheet.Cells[excelRow, 14].Value = emp.CorrespondenceAddress;
                    worksheet.Cells[excelRow, 15].Value = emp.CorrespondenceCity;
                    worksheet.Cells[excelRow, 16].Value = emp.CorrespondenceState;
                    worksheet.Cells[excelRow, 17].Value = emp.CorrespondencePinCode;
                    worksheet.Cells[excelRow, 18].Value = emp.PermanentAddress;
                    worksheet.Cells[excelRow, 19].Value = emp.PermanentCity;
                    worksheet.Cells[excelRow, 20].Value = emp.PermanentState;
                    worksheet.Cells[excelRow, 21].Value = emp.PermanentPinCode;
                    worksheet.Cells[excelRow, 22].Value = emp.PANNo;
                    worksheet.Cells[excelRow, 23].Value = emp.AadharCardNo;
                    worksheet.Cells[excelRow, 24].Value = emp.BloodGroup;
                    worksheet.Cells[excelRow, 25].Value = emp.Allergies;
                    worksheet.Cells[excelRow, 26].Value = emp.MajorIllnessOrDisability;
                    worksheet.Cells[excelRow, 27].Value = emp.AwardsAchievements;
                    worksheet.Cells[excelRow, 28].Value = emp.EducationGap;
                    worksheet.Cells[excelRow, 29].Value = emp.ExtraCuricuarActivities;
                    worksheet.Cells[excelRow, 30].Value = emp.ForiegnCountryVisits;
                    worksheet.Cells[excelRow, 31].Value = emp.ContactPersonName;
                    worksheet.Cells[excelRow, 32].Value = emp.ContactPersonMobile;
                    worksheet.Cells[excelRow, 33].Value = emp.ContactPersonTelephone;
                    worksheet.Cells[excelRow, 34].Value = emp.ContactPersonRelationship;
                    worksheet.Cells[excelRow, 35].Value = emp.ITSkillsKnowledge;
                    worksheet.Cells[excelRow, 36].Value = emp.Designation;
                    worksheet.Cells[excelRow, 37].Value = emp.EmployeeType;
                    worksheet.Cells[excelRow, 38].Value = emp.Department;
                    worksheet.Cells[excelRow, 39].Value = emp.SubDepartment;
                    worksheet.Cells[excelRow, 40].Value = emp.JobLocation;
                    worksheet.Cells[excelRow, 41].Value = emp.ShiftType;
                    worksheet.Cells[excelRow, 42].Value = emp.OfficialEmailID;
                    worksheet.Cells[excelRow, 43].Value = emp.OfficialContactNo;
                    worksheet.Cells[excelRow, 44].Value = emp.JoiningDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 45].Value = emp.ReportingManager;
                    worksheet.Cells[excelRow, 46].Value = emp.PolicyName;
                    worksheet.Cells[excelRow, 47].Value = emp.PayrollType;
                    worksheet.Cells[excelRow, 48].Value = emp.ClientName;
                    worksheet.Cells[excelRow, 49].Value = emp.ESINumber;
                    worksheet.Cells[excelRow, 50].Value = emp.ESIRegistrationDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 51].Value = emp.BankAccountNumber;
                    worksheet.Cells[excelRow, 52].Value = emp.IFSCCode;
                    worksheet.Cells[excelRow, 53].Value = emp.BankName;
                    worksheet.Cells[excelRow, 54].Value = emp.UANNumber;
                    worksheet.Cells[excelRow, 55].Value = emp.DateOfResignation?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 56].Value = emp.DateOfLeaving?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 57].Value = emp.LeavingType;
                    worksheet.Cells[excelRow, 58].Value = emp.NoticeServed?.ToString();
                    worksheet.Cells[excelRow, 59].Value = emp.AgeOnNetwork?.ToString();
                    worksheet.Cells[excelRow, 60].Value = emp.PreviousExperience?.ToString();
                    worksheet.Cells[excelRow, 61].Value = emp.DateOfJoiningTraining?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 62].Value = emp.DateOfJoiningFloor?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 63].Value = emp.DateOfJoiningOJT?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 64].Value = emp.DateOfJoiningOnroll?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 65].Value = emp.BackOnFloorDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 66].Value = emp.LeavingRemarks;
                    worksheet.Cells[excelRow, 67].Value = emp.MailReceivedFromAndDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[excelRow, 68].Value = emp.EmailSentToITDate?.ToString("yyyy-MM-dd");
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                var fileName = $"EmployeeDetails_{DateTime.Now:yyyyMMdd}.xlsx";

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

            }
            catch (Exception ex)
            {
                // Log ex if needed
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
                ProcessFileUpload(CertificateFile, eduDetail.CertificateImage, out string newPanKey);
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
                    eduDetail.CertificateImage = ExtractKeyFromUrl(eduDetail.CertificateImage);
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
                EmployeesFamilyDetailID = encodedId
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
    }
}
