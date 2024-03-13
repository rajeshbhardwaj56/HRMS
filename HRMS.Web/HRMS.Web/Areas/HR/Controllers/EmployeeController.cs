using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Areas.HR.Controllers
{
    [Area(Constants.ManageHR)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin))]
    public class EmployeeController : Controller
    {
        public IActionResult Index(string id)
        {
            EmployeeModel employee = new EmployeeModel();
            employee.FamilyDetails.Add(new FamilyDetail());
            employee.EducationalDetails.Add(new EducationalDetail());
            employee.LanguageDetails.Add(new LanguageDetail());
            employee.EmploymentDetails.Add(new EmploymentDetail());
            return View(employee);
        }

        [HttpPost]
        public IActionResult Index(EmployeeModel employee)
        {
            return View(employee);
        }

        [HttpPost]
        public ActionResult AddNewEmploymentDetail(EmployeeModel employee, bool isDeleted)
        {
            if (!isDeleted)
            {
                employee.EmploymentDetails.Add(new EmploymentDetail() { });
            }
            return PartialView("_EmploymentDetails", employee);
        }


        [HttpPost]
        public ActionResult AddNewFamilyMember(EmployeeModel employee, bool isDeleted)
        {
            if (!isDeleted)
            {
                employee.FamilyDetails.Add(new FamilyDetail() { });
            }
            return PartialView("_FamilyDetails", employee);
        }


        [HttpPost]
        public ActionResult AddNewEducationalDetail(EmployeeModel employee, bool isDeleted)
        {
            if (!isDeleted)
            {
                employee.EducationalDetails.Add(new EducationalDetail() { });
            }
            return PartialView("_EducationalDetails", employee);
        }

        [HttpPost]
        public ActionResult AddNewLanguageDetail(EmployeeModel employee, bool isDeleted)
        {
            if (!isDeleted)
            {
                employee.LanguageDetails.Add(new LanguageDetail() { });
            }
            return PartialView("_LanguageDetails", employee);
        }



       
    }
}
