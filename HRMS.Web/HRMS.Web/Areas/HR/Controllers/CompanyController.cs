using DocumentFormat.OpenXml.ExtendedProperties;
using HRMS.Web.BusinessLayer;
using HRMS.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using HRMS.Models.Company;
using Microsoft.AspNetCore.Authorization;
using HRMS.Models.Employee;

namespace HRMS.Web.Areas.HR.Controllers
{
    [Area(Constants.ManageHR)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin))]
    public class CompanyController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;

        private IHostingEnvironment Environment;
       

        public CompanyController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment)
        {           
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        public IActionResult Index(string id)
        {
            CompanyModel model = new CompanyModel();

            model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            if (!string.IsNullOrEmpty(id))
            {

                model.CompanyID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetAllCompanies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).companyModel;

            }

            HRMS.Models.Common.Results results = GetAllResults(model.CompanyID);
            model.Countries = results.Countries;
            model.Currencies = results.Currencies;
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(CompanyModel model, List<IFormFile> postedFiles)
        {
            if (ModelState.IsValid)
            {

                HRMS.Models.Common.Results results = GetAllResults(model.CompanyID);
                model.Countries = results.Countries;
                model.Currencies = results.Currencies;

                string wwwPath = Environment.WebRootPath;
                string contentPath = this.Environment.ContentRootPath;


                string fileName = null;
                foreach (IFormFile postedFile in postedFiles)
                {
                    fileName = postedFile.FileName.Replace(" ", "");
                }
                model.CompanyLogo = fileName;
                var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdateEmployee), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);

                string path = Path.Combine(this.Environment.WebRootPath, Constants.CompanyLogoPath + result.PKNo.ToString());

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
              

                return RedirectToActionPermanent(
                   Constants.Index,
                WebControllarsConstants.Company,
                  new { id = result.PKNo.ToString() }
               );
            }
            else
            {
                HRMS.Models.Common.Results results = GetAllResults(model.CompanyID);
                model.Countries = results.Countries;
                model.Currencies = results.Currencies;
                return View(model);
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
    }
}
