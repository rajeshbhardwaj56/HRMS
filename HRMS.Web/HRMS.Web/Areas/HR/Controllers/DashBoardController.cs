using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.HR.Controllers
{
    [Area(Constants.ManageHR)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.SuperAdmin))]
    public class DashBoardController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        IHttpContextAccessor _context;
        public DashBoardController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IHttpContextAccessor context)
        {
            Environment = _environment;
            _configuration = configuration;
            _context = context;
            _businessLayer = businessLayer;
        }
        public IActionResult Index()
        {
            DashBoardModelInputParams dashBoardModelInputParams = new DashBoardModelInputParams() { EmployeeID = long.Parse(HttpContext.Session.GetString(Constants.EmployeeID)) };
            var data = _businessLayer.SendPostAPIRequest(dashBoardModelInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetDashBoardModel), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<DashBoardModel>(data);
          //  _context.HttpContext.Session.SetString(Constants.ProfilePhoto, model.ProfilePhoto);
            return View(model);
        }
    }
}
