using HRMS.Models.Common;
using HRMS.Models.FormPermission;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HRMS.Web.Controllers
{
    public class TopMenuBarViewComponent: ViewComponent
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        public TopMenuBarViewComponent(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var objmodel = new FormPermissionVM
            {
                DepartmentId = Convert.ToInt64(HttpContext.Session.GetString(Constants.DepartmentID)),
                RoleID = Convert.ToInt64(HttpContext.Session.GetString(Constants.RoleID)),
                EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID))
            };

            var obj = new FormPermissionListViewModel();

            var apiUrl = await _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.Common,
                APIApiActionConstants.GetUserFormPermissions
            );

            var response = await _businessLayer.SendPostAPIRequest(
                objmodel,
                apiUrl,
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );

            // If `response` is a JSON string
            var jsonString = response?.ToString();
            if (!string.IsNullOrEmpty(jsonString))
            {
                obj.FormPermissionList = JsonConvert.DeserializeObject<List<FormPermissionViewModel>>(jsonString);
            }

            return View(obj);
        }





    }
}
