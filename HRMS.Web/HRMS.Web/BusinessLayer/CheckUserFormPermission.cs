using HRMS.Models.Common;
using HRMS.Models.FormPermission;
using HRMS.Models.MyInfo;
using Newtonsoft.Json;

namespace HRMS.Web.BusinessLayer
{
    public interface ICheckUserFormPermission
    {
        EmployeePermissionVM GetFormPermission(long employeeId, int formId);
    }
    public class CheckUserFormPermission : ICheckUserFormPermission
    {
        private readonly IBusinessLayer _businessLayer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CheckUserFormPermission(IBusinessLayer businessLayer, IHttpContextAccessor httpContextAccessor)
        {
            _businessLayer = businessLayer;
            _httpContextAccessor = httpContextAccessor;
        }

        public EmployeePermissionVM GetFormPermission(long employeeId, int formId)
        {
            var model = new FormPermissionVM
            {
                EmployeeID = employeeId,
                FormId = formId
            };

            var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Common, APIApiActionConstants.CheckUserFormPermissionByEmployeeID);
            var token = _httpContextAccessor.HttpContext.Session.GetString(Constants.SessionBearerToken);

            var response = _businessLayer.SendPostAPIRequest(model, apiUrl, token, true).Result.ToString();
            return JsonConvert.DeserializeObject<EmployeePermissionVM>(response);
        }
    }
}
