using HRMS.Models;
using HRMS.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Areas.HR.Controllers
{
    [Area(Constants.ManageHR)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin))]
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            EmployeeModel employee = new EmployeeModel();   
            return View(employee);
        }
    }
}
