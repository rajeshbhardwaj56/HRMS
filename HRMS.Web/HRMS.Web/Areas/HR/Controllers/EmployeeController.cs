using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.InkML;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography.Xml;
using System.Text;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.HR.Controllers
{
    [Area(Constants.ManageHR)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin))]
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
            EmployeeInputParans employee = new EmployeeInputParans();
            employee.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllEmployees), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
            return Json(new { data = results.Employees });
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
                employee.EmploymentDetails.Add(new EmploymentDetail());
                employee.References = new List<HRMS.Models.Employee.Reference>() {
                    new HRMS.Models.Employee.Reference(),
                    new HRMS.Models.Employee.Reference()
                };
            }
            else
            {
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
        public IActionResult Index(EmployeeModel employee, List<IFormFile> postedFiles)
        {
            HRMS.Models.Common.Results results = GetAllResults(employee.CompanyID);

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
                return RedirectToActionPermanent(
                   Constants.Index,
                    WebControllarsConstants.Employee,
                  new { id = result.PKNo.ToString() }
               );
            }
            else
            {
                employee.Languages = results.Languages;
                employee.Countries = results.Countries;
                employee.EmploymentTypes = results.EmploymentTypes;
                employee.Departments = results.Departments;
                return View(employee);

            }
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
        public ActionResult AddNewEmploymentDetail(EmployeeModel employee, bool isDeleted)
        {
            if (!isDeleted)
            {
                employee.EmploymentDetails.Add(new EmploymentDetail() { });
            }
            return PartialView("_EmploymentDetails", employee);
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
    }
}
