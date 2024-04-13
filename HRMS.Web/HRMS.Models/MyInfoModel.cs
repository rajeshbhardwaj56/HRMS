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
        public LeaveSummayModel leaveSummayModel { get; set; } = new LeaveSummayModel();
        public List<LeaveSummayModel> leavesSummay { get; set; } = new List<LeaveSummayModel>();
        public List<SelectListItem> leaveTypes = new List<SelectListItem>();
        public List<SelectListItem> leaveDurationTypes = new List<SelectListItem>();
        public List<SelectListItem> leaveStatuses = new List<SelectListItem>();
    }
}
