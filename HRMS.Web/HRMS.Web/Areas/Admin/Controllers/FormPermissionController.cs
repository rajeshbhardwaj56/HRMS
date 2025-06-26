﻿using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.FormPermission;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    public class FormPermissionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;
        public FormPermissionController(ICheckUserFormPermission CheckUserFormPermission,IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
            _CheckUserFormPermission = CheckUserFormPermission;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> FormPermission()
        {
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = await _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.FormPermission);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            FormPermissionViewModel objmodel = new FormPermissionViewModel();

            var CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
          
            var data =await _businessLayer.SendGetAPIRequest("Common/GetAllCompanyDepartments?CompanyID=" + CompanyID, HttpContext.Session.GetString(Constants.SessionBearerToken), true);
            objmodel.Departments = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(data.ToString()).Departments;


            var Forms =await  _businessLayer.SendGetAPIRequest("Common/GetAllCompanyFormsPermission?CompanyID=" + CompanyID, HttpContext.Session.GetString(Constants.SessionBearerToken), true);
            objmodel.FormsPermission = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(Forms.ToString()).FormsPermission;

            return View(objmodel);
        }

        [HttpPost]
        public async Task<ActionResult> FormPermission(FormPermissionViewModel obj)
        {
            long insertedId = 0;
            try
            {
                obj.CreatedByID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

                var apiUrl = await _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.AddFormPermissions);
                var apiResponse = await _businessLayer.SendPostAPIRequest(
                    obj,
                  apiUrl,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                );
                var data = apiResponse?.ToString();
               
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
        public async Task<JsonResult> loadForms(int DepartmentId)
        {
            try
            {
                List<FormPermissionViewModel> objmodel = new List<FormPermissionViewModel>();

                var response = await _businessLayer.SendGetAPIRequest(
                    "Common/GetFormByDepartmentID?DepartmentId=" + DepartmentId,
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                );

                if (response != null)
                {
                    objmodel = JsonConvert.DeserializeObject<List<FormPermissionViewModel>>(response.ToString());
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
        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }

    }
}
