using HRMS.Models;
using HRMS.Models.Common;
using Newtonsoft.Json;

namespace HRMS.Web.BusinessLayer
{
    public class CutoffSettingsService : ICutoffSettingsService
    {
        private readonly IBusinessLayer _businessLayer;

        public CutoffSettingsService(IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
        }

        public CutoffDateSettingViewModel GetCutoffSettings(string token)
        {
            var response = _businessLayer.SendGetAPIRequest(
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Employee,
                    APIApiActionConstants.GetCutoffDateSettings),
                token,
                true).Result.ToString();

            var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(response);

            CutoffDateSettingViewModel model = new CutoffDateSettingViewModel();

            if (result?.CutoffDateSettingsList != null)
            {
                foreach (var item in result.CutoffDateSettingsList)
                {
                    switch (item.SettingKey)
                    {
                        case "ApplyCutoffDate":
                            model.ApplyCutoffDate = Convert.ToDateTime(item.SettingValue);
                            break;

                        case "ApprovalCutoffDate":
                            model.ApprovalCutoffDate = Convert.ToDateTime(item.SettingValue);
                            break;

                        case "AttendanceCutoffDate":
                            model.AttendanceCutoffDate = Convert.ToDateTime(item.SettingValue);
                            break;

                        case "AdminEditCutoffDate":
                            model.AdminEditCutoffDate = Convert.ToDateTime(item.SettingValue);
                            break;

                        case "AllowSuperAdminEdit":
                            model.AllowSuperAdminEdit = item.SettingValue == "1";
                            break;
                    }
                }
            }

            return model;
        }
    }
}
