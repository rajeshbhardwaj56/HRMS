using HRMS.Models.Leave;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class MyInfoModel
    {
        public LeaveSummaryModel leaveSummaryModel { get; set; } = new LeaveSummaryModel();
        public List<LeaveSummaryModel> leavesSummary { get; set; } = new List<LeaveSummaryModel>();
        public List<SelectListItem> leaveTypes = new List<SelectListItem>();
        public List<SelectListItem> leaveDurationTypes = new List<SelectListItem>();
        public List<SelectListItem> leaveStatuses = new List<SelectListItem>();
    }
}
