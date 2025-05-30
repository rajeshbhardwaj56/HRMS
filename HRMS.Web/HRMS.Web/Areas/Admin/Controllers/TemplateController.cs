﻿using DocumentFormat.OpenXml.EMMA;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Models.LeavePolicy;
using HRMS.Models.Template;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
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
        private readonly IS3Service _s3Service;

        public TemplateController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IS3Service s3Service)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            _s3Service = s3Service;           
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
            if (results?.Template != null)
            {
                results.Template.ForEach(x =>

            {
                x.HeaderImage = string.IsNullOrEmpty(x.HeaderImage)
                    ? "/assets/img/No_image.png"
                    : _s3Service.GetFileUrl(x.HeaderImage);
                x.FooterImage = string.IsNullOrEmpty(x.FooterImage)
                    ? "/assets/img/No_image.png"
                    : _s3Service.GetFileUrl(x.FooterImage);
            });
            }
            results.Template.ForEach(x => x.EncodedId = _businessLayer.EncodeStringBase64(x.TemplateID.ToString()));

            return Json(new { data = results.Template });
        }
        public IActionResult Index(string id)
        {
            TemplateModel Template = new TemplateModel();
            Template.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                Template.TemplateID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(Template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.GetAllTemplates), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                Template = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).templateModel;
            }
            if (!string.IsNullOrEmpty(Template.HeaderImage)) { 
                Template.HeaderImage = _s3Service.GetFileUrl(Template.HeaderImage); 
            }
            if (!string.IsNullOrEmpty(Template.HeaderImage))
            {
                Template.FooterImage = _s3Service.GetFileUrl(Template.FooterImage);
            }          
            return View(Template);
        }

        [HttpPost]
        public IActionResult Index(TemplateModel template, List<IFormFile> HeaderImageFile, List<IFormFile> FooterImageFile)
        {
            template.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));    
            _s3Service.ProcessFileUpload(HeaderImageFile, template.HeaderImage, out string newProfileKey);
            if (!string.IsNullOrEmpty(newProfileKey))
            {
                if (!string.IsNullOrEmpty(template.HeaderImage))
                {
                    _s3Service.DeleteFile(template.HeaderImage);
                }
                template.HeaderImage = newProfileKey;
            }
            else
            {
                template.HeaderImage = _s3Service.ExtractKeyFromUrl(template.HeaderImage);
            }
            _s3Service.ProcessFileUpload(FooterImageFile, template.FooterImage, out string newFooterKey);
            if (!string.IsNullOrEmpty(newFooterKey))
            {
                if (!string.IsNullOrEmpty(template.FooterImage))
                {
                    _s3Service.DeleteFile(template.FooterImage);
                }
                template.FooterImage = newFooterKey;
            }
            else
            {
                template.FooterImage = _s3Service.ExtractKeyFromUrl(template.FooterImage);
            }
            var data = _businessLayer.SendPostAPIRequest(template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.AddUpdateTemplate), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);                                     
            if (template.TemplateID > 0)
            {
                return RedirectToActionPermanent(WebControllarsConstants.TemplateListing, WebControllarsConstants.Template);
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
                id = _businessLayer.DecodeStringBase64(id);
                Template.TemplateID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(Template, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Template, APIApiActionConstants.GetAllTemplates), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                Template = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).templateModel;
                if (!string.IsNullOrEmpty(Template.HeaderImage))
                {
                    Template.HeaderImage = _s3Service.GetFileUrl(Template.HeaderImage);
                }
                if (!string.IsNullOrEmpty(Template.HeaderImage))
                {
                    Template.FooterImage = _s3Service.GetFileUrl(Template.FooterImage);
                }
            }


            return View(Template);
        }
      
    }
}
