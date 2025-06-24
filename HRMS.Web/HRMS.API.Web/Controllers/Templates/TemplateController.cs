using System.Threading.Tasks;
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
        public async Task<IActionResult> AddUpdateTemplate(TemplateModel model)
        {
            IActionResult response = Unauthorized();
            Result result = await _businessLayer.AddUpdateTemplate(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllTemplates(TemplateInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllTemplates(model);
            response = Ok(result);
            return response;
        }

    }
}
