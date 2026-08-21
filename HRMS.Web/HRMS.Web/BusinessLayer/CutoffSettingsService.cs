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
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(response))
            {
                return new CutoffDateSettingViewModel();
            }

            var result =
                JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(
                    response);

            var model = new CutoffDateSettingViewModel();

            if (result?.CutoffDateSettingsList != null)
            {
                foreach (var item in result.CutoffDateSettingsList)
                {
                    switch (item.SettingKey)
                    {
                        case "ApplyCutoffDate":

                            if (DateTime.TryParse(
                                item.SettingValue,
                                out var applyCutoffDate))
                            {
                                model.ApplyCutoffDate =
                                    applyCutoffDate;
                            }

                            break;


                        case "ApprovalCutoffDate":

                            if (DateTime.TryParse(
                                item.SettingValue,
                                out var approvalCutoffDate))
                            {
                                model.ApprovalCutoffDate =
                                    approvalCutoffDate;
                            }

                            break;


                        case "AttendanceCutoffDate":

                            if (DateTime.TryParse(
                                item.SettingValue,
                                out var attendanceCutoffDate))
                            {
                                model.AttendanceCutoffDate =
                                    attendanceCutoffDate;
                            }

                            break;


                        case "AdminEditCutoffDate":

                            if (DateTime.TryParse(
                                item.SettingValue,
                                out var adminEditCutoffDate))
                            {
                                model.AdminEditCutoffDate =
                                    adminEditCutoffDate;
                            }

                            break;


                        case "AllowSuperAdminEdit":

                            model.AllowSuperAdminEdit =
                                item.SettingValue == "1" ||
                                item.SettingValue.Equals(
                                    "true",
                                    StringComparison.OrdinalIgnoreCase);

                            break;


                        // =========================================
                        // SHOW IMPORT ATTENDANCE EXCEL
                        // =========================================

                        case "ShowImportAttendanceExcel":

                            model.ShowImportAttendanceExcel =
                                item.SettingValue == "1" ||
                                item.SettingValue.Equals(
                                    "true",
                                    StringComparison.OrdinalIgnoreCase);

                            break;


                        // =========================================
                        // SHOW AUTO CALCULATE MONTH SALARY
                        // =========================================

                        case "ShowAutoCalculateMonthSalary":

                            model.ShowAutoCalculateMonthSalary =
                                item.SettingValue == "1" ||
                                item.SettingValue.Equals(
                                    "true",
                                    StringComparison.OrdinalIgnoreCase);

                            break;
                    }
                }
            }

            return model;
        }
    }
}
