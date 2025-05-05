using DocumentFormat.OpenXml.EMMA;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Models.LeavePolicy;
using HRMS.Models.Template;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Results = HRMS.Models.Common.Results;


namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = (RoleConstants.Admin + "," + RoleConstants.HR + "," + RoleConstants.SuperAdmin))]
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
        public IActionResult Index(TemplateModel template, IFormFile HeaderImageFile, IFormFile FooterImageFile)
        {
            template.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            string wwwPath = this.Environment.WebRootPath;

            string HeaderImageFileName = "";
            if (HeaderImageFile != null)
            {
                template.HeaderImage = HeaderImageFileName = Guid.NewGuid().ToString() + HeaderImageFile.FileName.Replace(" ", "");
            }

            string FooterImageFileName = "";
            if (FooterImageFile != null)
            {
                template.FooterImage = FooterImageFileName = Guid.NewGuid().ToString() + FooterImageFile.FileName.Replace(" ", "");
            }
            var data = _businessLayer.SendPostAPIRequest(template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.AddUpdateTemplate), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);
            string path = Path.Combine(wwwPath, Constants.TemplatePath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (HeaderImageFile != null)
            {
                using (FileStream stream = new FileStream(Path.Combine(path, HeaderImageFileName), FileMode.Create))
                {
                    HeaderImageFile.CopyTo(stream);
                }
            }

            if (FooterImageFile != null)
            {
                using (FileStream stream = new FileStream(Path.Combine(path, FooterImageFileName), FileMode.Create))
                {
                    FooterImageFile.CopyTo(stream);
                }
            }

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

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            // Check if a file was actually uploaded
            if (upload != null && upload.Length > 0)
            {
                // Check if the uploaded file is an image
                if (!IsImageFile(upload))
                {
                    return BadRequest("Only image files are allowed.");
                }

                // Generate a unique filename
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + upload.FileName;
                string path = Path.Combine(this.Environment.WebRootPath, Constants.CKEditorImagesPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                // Combine the path to the wwwroot directory and the filename
                string filePath = Path.Combine(path, uniqueFileName);

                // Save the file to the wwwroot directory
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileStream);
                }

                // Return the URL of the saved image
                return Ok("/" + filePath);
            }

            return BadRequest("No file uploaded or file is empty.");
        }

        private bool IsImageFile(IFormFile file)
        {
            // Check the file content type to determine if it's an image
            return file.ContentType.StartsWith("image/");
        }
        public IActionResult PreviewAndPrint(string id)
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
      
    }
}
