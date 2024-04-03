using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers.Templates
{
    public class TemplateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
