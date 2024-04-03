using HRMS.Models.Common;
using HRMS.Models.Template;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageHR)]
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
        public IActionResult Index()
        {
            return View();
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
            Template.TemplateID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(Template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.GetAllTemplates), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);

            return Json(new { data = results.Template });

        }
    }
}
