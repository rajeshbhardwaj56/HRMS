using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Models.WhatsHappeningModel;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Mail;
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
        public EmployeeController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            EmailSender.configuration = _configuration;
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

            // 🔹 Ensure search is passed correctly
            employee.Searching = string.IsNullOrEmpty(sSearch) ? null : sSearch;

            var data = _businessLayer.SendPostAPIRequest(
                employee,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            results.Employees.ForEach(x => x.EncryptedIdentity = _businessLayer.EncodeStringBase64(x.EmployeeID.ToString()));
            results.Employees.ForEach(x => x.EncodedDesignationID = _businessLayer.EncodeStringBase64(x.DesignationID.ToString()));

            results.Employees.ForEach(x => x.EncodedDepartmentIDID = _businessLayer.EncodeStringBase64(x.DepartmentID.ToString()));
            //return Json(new { data = results.Employees });
            return Json(new
            {
                draw = sEcho,
                 recordsTotal = results.Employees.Select(x=>x.TotalRecords).FirstOrDefault() ??0,   
                recordsFiltered = results.Employees.Select(x=>x.FilteredRecords).FirstOrDefault()??0,   
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
                id = _businessLayer.DecodeStringBase64(id);
                employee.EmployeeID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                employee = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).employeeModel;
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
                };
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
            HRMS.Models.Common.Results results = GetAllResults(employee.CompanyID);
            try
            {
                if (ModelState.IsValid)
                {
                    string wwwPath = Environment.WebRootPath;

                    // Profile Photo
                    employee.ProfilePhoto = SaveUploadedFile(postedFiles, wwwPath, Constants.EmployeePhotoPath);

                    // PAN Card
                    employee.PanCardImage = SaveUploadedFile(PanPostedFile, wwwPath, Constants.EmployeePanCardPath);

                    // Aadhaar Card
                    employee.AadhaarCardImage = SaveUploadedFile(AadhaarPostedFile, wwwPath, Constants.EmployeeAadharCardPath);

                    // Call API
                    var data = _businessLayer.SendPostAPIRequest(
                        employee,
                        _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmployee),
                        HttpContext.Session.GetString(Constants.SessionBearerToken),
                        true).Result.ToString();

                    var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);

                    if (result != null && result.PKNo > 0)
                    {
                        // Save files in respective directories
                        SaveFileToDirectory(postedFiles, wwwPath, Constants.EmployeePhotoPath, result.PKNo);
                        SaveFileToDirectory(PanPostedFile, wwwPath, Constants.EmployeePanCardPath, result.PKNo);
                        SaveFileToDirectory(AadhaarPostedFile, wwwPath, Constants.EmployeeAadharCardPath, result.PKNo);

                        TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                        TempData[HRMS.Models.Common.Constants.toastMessage] = "Data saved successfully.";

                        return RedirectToActionPermanent(WebControllarsConstants.EmployeeListing, WebControllarsConstants.Employee);
                    }
                }

                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Please check all data and try again.";
            }
            catch (Exception ex)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Some error occurred, please try later.";
                // Log exception (optional)
            }

            // Populate dropdown lists for the view
            employee.Languages = results.Languages;
            employee.Countries = results.Countries;
            employee.EmploymentTypes = results.EmploymentTypes;
            employee.Departments = results.Departments;

            return View(employee);
        }

        // Helper method to save and return file name
        private string SaveUploadedFile(List<IFormFile> files, string rootPath, string folderPath)
        {
            if (files != null && files.Count > 0)
            {
                string fileName = files[0].FileName.Replace(" ", "");
                return fileName;
            }
            return null;
        }

        // Helper method to save files in directories
        private void SaveFileToDirectory(List<IFormFile> files, string rootPath, string folderPath, long? pkNo)
        {
            if (files != null && files.Count > 0)
            {
                string path = Path.Combine(rootPath, folderPath + pkNo.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                foreach (IFormFile file in files)
                {
                    string filePath = Path.Combine(path, file.FileName.Replace(" ", ""));
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
            }
        }



        [HttpPost]
        public    JsonResult UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(this.Environment.WebRootPath, Constants.EmployeeDocuments + DateTime.Now.Second.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder); // Ensure folder exists
                }

                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                      file.CopyToAsync(stream); // Save file
                }

                // Return file URL for preview
                return Json(new { filePath = "/UploadedFiles/" + fileName });
            }
            return Json(new { error = "File upload failed" });
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

        [HttpGet]
        public ActionResult EmploymentDetails(string id, string DegtId, string DeptId)
        {
            EmploymentDetailInputParams employmentDetailInputParams = new EmploymentDetailInputParams()
            {
                UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.UserID))
            };
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
                employmentDetail.CompanyID =Convert.ToInt64(CompanyID);
                var data = _businessLayer.SendPostAPIRequest(employmentDetail, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmploymentDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                Result result = JsonConvert.DeserializeObject<Result>(data);
                if (result != null && result.PKNo>0 && result.Message.Contains("EmailID already Exists in System, please try again with another email.", StringComparison.OrdinalIgnoreCase))
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                }
               else if (result != null && result.PKNo>0 && result.Message.Contains("Some error occurred, please try again later.", StringComparison.OrdinalIgnoreCase))
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
                    sendEmailProperties.emailBody = ("Hi, <br/><br/> Please click on below link to reset password. <br/> <a target='_blank' href='" + string.Format(_configuration["AppSettings:RootUrl"] + _configuration["AppSettings:ResetPasswordURL"], _businessLayer.EncodeStringBase64((employmentDetail.EmployeeID == null ? "" : employmentDetail.EmployeeID.ToString()).ToString()), _businessLayer.EncodeStringBase64(DateTime.Now.ToString()), _businessLayer.EncodeStringBase64(CompanyID.ToString())) + "'> Click here to reset password</a>" + "<br/><br/>");
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
            return View(employmentBankDetail);
        }


        [HttpPost]
        public ActionResult EmploymentBankDetails(EmploymentBankDetail employmentBankDetail)
        {
            if (ModelState.IsValid)
            {
                employmentBankDetail.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
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
            return Json(new { data = results.WhatsHappeningList });

        }
    }
}
