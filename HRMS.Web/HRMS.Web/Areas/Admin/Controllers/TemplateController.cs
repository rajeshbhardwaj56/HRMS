using DocumentFormat.OpenXml.Office2010.Excel;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Models.Template;
using HRMS.Models.Template;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = ( RoleConstants.Admin))]
    public class TemplateController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public TemplateController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }
        public IActionResult Index(string id)
        {
            TemplateModel Template = new TemplateModel();
            Template.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            if (!string.IsNullOrEmpty(id))
            {
                Template.TemplateID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(Template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.GetAllTemplates), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                Template = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).templateModel;
            }

           
            return View(Template);
        }

        [HttpPost]
        public IActionResult Index(TemplateModel trmplate)
        {
            if (ModelState.IsValid)
            {
                var data = _businessLayer.SendPostAPIRequest(trmplate, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.AddUpdateTemplate), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);
                return View(trmplate);
            }
            else
            {
                return View(trmplate);

            }
        }

        public IActionResult TemplateListing()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            return View(results);
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult TemplateListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            TemplateInputParans Template = new TemplateInputParans();
            Template.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(Template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.GetAllTemplates), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);

            return Json(new { data = results.Template });

        }
    }
}
