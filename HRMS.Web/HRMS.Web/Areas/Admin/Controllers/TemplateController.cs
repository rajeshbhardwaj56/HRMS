using HRMS.Models.Common;
using HRMS.Models.Template;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = (RoleConstants.Admin + "," + RoleConstants.HR))]
    public class TemplateController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer; private IHostingEnvironment Environment;
        public TemplateController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
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
            TemplateInputParams Template = new TemplateInputParams();
            Template.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(Template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.GetAllTemplates), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data);

            return Json(new { data = results.Template });

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
        public IActionResult Index(TemplateModel template, IFormFile HeaderImage, IFormFile FooterImage)
        {
            if (ModelState.IsValid)
            {
                template.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                template.HeaderImage = _businessLayer.ConvertIFormFileToBase64(HeaderImage);
                template.FooterImage = _businessLayer.ConvertIFormFileToBase64(FooterImage);

                var data = _businessLayer.SendPostAPIRequest(template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.AddUpdateTemplate), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);

                if (template.TemplateID > 0)
                {
                    return RedirectToActionPermanent(Constants.Index, WebControllarsConstants.Template, new { id = template.TemplateID.ToString() }
                 );
                }
                else
                {
                    return RedirectToActionPermanent(WebControllarsConstants.TemplateListing, WebControllarsConstants.Template);
                }

            }
            else
            {
                return View(template);

            }
        }


    }
}
