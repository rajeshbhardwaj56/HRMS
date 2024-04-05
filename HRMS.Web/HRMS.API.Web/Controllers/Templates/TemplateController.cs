using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Template;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HRMS.API.Web.Controllers.Template
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TemplateController : ControllerBase
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public TemplateController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        public IActionResult AddUpdateTemplate(TemplateModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateTemplate(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetAllTemplates(TemplateInputParans model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllTemplates(model));
            return response;
        }
    }
}
