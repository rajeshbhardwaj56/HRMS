using DocumentFormat.OpenXml.ExtendedProperties;
using HRMS.Web.BusinessLayer;
using HRMS.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using HRMS.Models.Company;
using Microsoft.AspNetCore.Authorization;
using HRMS.Models.Employee;
using DocumentFormat.OpenXml.InkML;
using System.ComponentModel.Design;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authentication;

namespace HRMS.Web.Areas.HR.Controllers
{
	[Area(Constants.ManageHR)]
	//[Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.SuperAdmin))]
	public class CompanyController : Controller
	{
		IConfiguration _configuration;
		IBusinessLayer _businessLayer;
		private readonly IS3Service _s3Service;
		private IHostingEnvironment Environment;
        private readonly IHttpContextAccessor _context;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;
        public CompanyController(ICheckUserFormPermission CheckUserFormPermission,IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IHttpContextAccessor context, IS3Service s3Service)
		{
			Environment = _environment;
			_configuration = configuration;
			_businessLayer = businessLayer;
			_context = context;
			_s3Service = s3Service;
            _CheckUserFormPermission = CheckUserFormPermission;

        }
		public IActionResult CompanyListing()
		{
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.CompanyListing);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public JsonResult CompanyListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
		{
			EmployeeInputParams employee = new EmployeeInputParams();
			var data = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetAllCompanies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
			var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);
			return Json(new { data = results.Companies });
		}
		public IActionResult Index(string id)
        {
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.company);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            CompanyModel model = new CompanyModel();
			{
				if (id == null)
				{
					model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
				}
				else
				{
					model.CompanyID = Convert.ToInt64(id);
				}
				var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetAllCompanies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
				model = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).companyModel;
			}
			if (!string.IsNullOrEmpty(model.CompanyLogo))
			{
				model.CompanyLogo = _s3Service.GetFileUrl(model.CompanyLogo);
			}
			HRMS.Models.Common.Results results = GetAllResults(Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID)));
			model.Countries = results.Countries;
			model.Currencies = results.Currencies;
			return View(model);
		}
		[HttpPost]
		public IActionResult Index(CompanyModel model, List<IFormFile> postedFiles)
		{
			string s3uploadUrl = _configuration["AWS:S3UploadUrl"];
			if (ModelState.IsValid)
			{
				model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
				HRMS.Models.Common.Results results = GetAllResults(model.CompanyID);
				model.Countries = results.Countries;
				model.Currencies = results.Currencies;

                _s3Service.ProcessFileUpload(postedFiles, model.CompanyLogo, out string newProfileKey);
                if (!string.IsNullOrEmpty(newProfileKey))
                {
                    if (!string.IsNullOrEmpty(model.CompanyLogo))
                    {
                        _s3Service.DeleteFile(model.CompanyLogo);
                    }
                    model.CompanyLogo = newProfileKey;
                }
                else
                {
                    model.CompanyLogo = _s3Service.ExtractKeyFromUrl(model.CompanyLogo);
                }               
				var CompanyLogo = _s3Service.GetFileUrl(newProfileKey);
				_context.HttpContext.Session.SetString(Constants.CompanyLogo, "");
				_context.HttpContext.Session.SetString(Constants.CompanyLogo, CompanyLogo.ToString());
				var data = _businessLayer.SendPostAPIRequest(
					model,
					_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.AddUpdateCompany),
					HttpContext.Session.GetString(Constants.SessionBearerToken),
					true
				).Result.ToString();
				var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);
				TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
				TempData[HRMS.Models.Common.Constants.toastMessage] = "Data saved successfully.";
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
       
        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }
    }
}
