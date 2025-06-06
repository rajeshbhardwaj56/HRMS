using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.FormPermission;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Employee + "," + RoleConstants.Manager + "," + RoleConstants.SuperAdmin))]
    public class FormPermissionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        public FormPermissionController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult FormPermission()
        {
            FormPermissionViewModel objmodel = new FormPermissionViewModel();

            var CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendGetAPIRequest("Common/GetAllCompanyDepartments?CompanyID=" + CompanyID, HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            objmodel.Departments = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data).Departments;


            var Forms = _businessLayer.SendGetAPIRequest("Common/GetAllCompanyFormsPermission?CompanyID=" + CompanyID, HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            objmodel.FormsPermission = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(Forms).FormsPermission;

            return View(objmodel);
        }

        [HttpPost]
        public ActionResult FormPermission(FormPermissionViewModel obj)
        {
            long insertedId = 0;
            try
            {
                obj.CreatedByID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

                var data = _businessLayer.SendPostAPIRequest(obj,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.AddFormPermissions),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();
                insertedId = JsonConvert.DeserializeObject<long>(data);
                if (insertedId == 1)
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "Forms Permission saved successfully.";
                }
                else
                {
                    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                    TempData[HRMS.Models.Common.Constants.toastMessage] = "something went wrong try later!.";
                }
                return RedirectToActionPermanent(WebControllarsConstants.FormPermission, WebControllarsConstants.FormPermission);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving permissions: " + ex.Message);
                return View(obj);
            }

        }



        [HttpGet]
        public JsonResult loadForms(int DepartmentId)
        {
            try
            {
                List<FormPermissionViewModel> objmodel = new List<FormPermissionViewModel>();

                var response = _businessLayer.SendGetAPIRequest(
                    "Common/GetFormByDepartmentID?DepartmentId=" + DepartmentId,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                );

                if (response != null)
                {
                    objmodel = JsonConvert.DeserializeObject<List<FormPermissionViewModel>>(response.Result.ToString());
                }

                return Json(objmodel);
            }
            catch (Exception ex)
            { 
                return Json(new
                {
                    success = false,
                    message = "An error occurred while loading form permissions.",
                    error = ex.Message
                });
            }
        }

       
    }
}
