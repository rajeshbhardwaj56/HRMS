using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public enum LeaveDay
    {
        FullDay = 1,
        HalfDay = 2
    }
    public enum LeaveStatus
    {
        Approved = 1,
        PendingApproval,
        NotApproved,
        Cancelled
    }
}