using DocumentFormat.OpenXml.InkML;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.Employee.Controllers
{
    [Area(Constants.ManageEmployee)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Employee))]
    public class MyInfoController : Controller
    {
        IHttpContextAccessor _context;
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        public MyInfoController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IHttpContextAccessor context)
        {
            Environment = _environment;
            _configuration = configuration;
            _businessLayer = businessLayer;
            _context = context;
        }

        public IActionResult Index(string id)
        {
            MyInfoModel model = new MyInfoModel();
            model.leaveSummayModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            model.leaveSummayModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(MyInfoModel model)
        {
            model.leaveSummayModel.EmployeeID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            model.leaveSummayModel.UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
            return View(model);
        }
    }
}
