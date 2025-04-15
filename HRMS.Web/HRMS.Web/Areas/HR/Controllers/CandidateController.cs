using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.HR.Controllers
{
<<<<<<< HEAD
    [Area(Constants.ManageHR + "," + RoleConstants.SuperAdmin)]
=======
    [Area(Constants.ManageHR)]
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
    public class CandidateController : Controller
    {

        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        public CandidateController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            EmailSender.configuration = _configuration;
        }

        public IActionResult Index(string id)
        {
            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
            TempData[HRMS.Models.Common.Constants.toastMessage] = "Candidate registration completed successfully.";
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToActionPermanent(
                         "Registration",
                          WebControllarsConstants.Candidate);
            }
            return View();
        }
        public IActionResult Registration(string id)
        {
            EmployeeModel employee = new EmployeeModel();
            employee.CompanyID = Convert.ToInt64("1");

            if (string.IsNullOrEmpty(id))
            {
                employee.FamilyDetails.Add(new FamilyDetail());
                employee.EducationalDetails.Add(new EducationalDetail());
                employee.LanguageDetails.Add(new LanguageDetail());
                employee.EmploymentHistory.Add(new EmploymentHistory());
                employee.References = new List<Reference>() {
                    new Reference(),
                    new Reference()
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
                    employee.References = new List<Reference>() {
                    new Reference(),
                    new Reference()
                    };
                }
                else if (employee.References.Count == 1)
                {
                    employee.References.Add(new Reference());
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
        public IActionResult Registration(EmployeeModel employee, List<IFormFile> postedFiles)
        {
            HRMS.Models.Common.Results results = GetAllResults(employee.CompanyID);
            try
            {
                string wwwPath = Environment.WebRootPath;
                string contentPath = this.Environment.ContentRootPath;

                if (ModelState.IsValid)
                {
                    string fileName = null;
                    foreach (IFormFile postedFile in postedFiles)
                    {
                        fileName = postedFile.FileName.Replace(" ", "");
                    }
                    employee.ProfilePhoto = fileName;
                    var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmployee), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                    var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);

                    string path = Path.Combine(this.Environment.WebRootPath, Constants.EmployeePhotoPath + result.PKNo.ToString());

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

                    employee.Languages = results.Languages;
                    employee.Countries = results.Countries;
                    employee.EmploymentTypes = results.EmploymentTypes;
                    employee.Departments = results.Departments;

                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Candidate registration completed successfully.";

                    return RedirectToActionPermanent(
                       Constants.Index,
                        WebControllarsConstants.Candidate,
                      new { id = _businessLayer.EncodeStringBase64(result.PKNo.ToString()) }
                   );
                }
                else
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Please check all data and try again.";

                    employee.Languages = results.Languages;
                    employee.Countries = results.Countries;
                    employee.EmploymentTypes = results.EmploymentTypes;
                    employee.Departments = results.Departments;
                    return View(employee);

                }
            }
            catch (Exception ex)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypetWarning;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Some error occured, please try later.";
            }
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
    }
}
