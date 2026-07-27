using HRMS.Models;

namespace HRMS.Web.BusinessLayer
{
    public interface ICutoffSettingsService
    {
        CutoffDateSettingViewModel GetCutoffSettings(string token); 
    }
}
