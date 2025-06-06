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
            FormPermissionVM objmodel = new FormPermissionVM();
            //objmodel.DepartmentId = Convert.ToInt64(HttpContext.Session.GetString(Constants.Departments));
            objmodel.DepartmentId = 9;
            objmodel.EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            FormPermissionListViewModel obj = new FormPermissionListViewModel();
            var response = _businessLayer.SendPostAPIRequest(objmodel,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.GetUserFormPermissions),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();
            if (response != null)
            {
                obj.FormPermissionList = JsonConvert.DeserializeObject<List<FormPermissionViewModel>>(response);
            }
            return View(obj);
        }
    }
}
