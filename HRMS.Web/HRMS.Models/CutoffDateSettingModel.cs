using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class CutoffDateSettingModel
    {
        public int SettingID { get; set; }

        public string? SettingKey { get; set; }

        public string? SettingValue { get; set; }

        public long? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
    public class CutoffDateSettingViewModel
    {
        public DateTime? ApplyCutoffDate { get; set; }

        public DateTime? ApprovalCutoffDate { get; set; }

        public DateTime? AttendanceCutoffDate { get; set; }

        public DateTime? AdminEditCutoffDate { get; set; }

        public bool AllowSuperAdminEdit { get; set; }
        public long? UpdatedBy { get; set; }
    }
}
