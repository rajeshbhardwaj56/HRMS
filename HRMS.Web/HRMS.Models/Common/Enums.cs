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
    public enum Status
    {
        Present,
        Absent,
        OnLeave,
        HalfDay,
        Holiday,
        WorkFromHome
    }
    public enum ShiftSelection
    {
        Day = 1,
        Night = 2
    }
    public enum LeaveType
    {
        Casual = 1,
        Medical = 2,
        AnnualLeave = 3,
        CompOff = 4
    }
}