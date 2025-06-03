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
        PendingApproval = 2,
        NotApproved = 3,
        Cancelled = 4
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
        MaternityLeave = 1,
        Adoption = 2,
        AnnualLeavel = 3,
        CompOff = 4,
        Miscarriage = 5,
        MedicalLeave = 6,
        Paternity = 7,
        LeaveWithOutPay = 8

    }
    public enum CompOff
    {
        CompOff = 1,
        OtherLeaves =2,
        

    }

    public enum AttendanceStatusId
    {
        Pending = 1,        
        L1Approved = 2,
        L1Rejected = 3,
        L2Approved = 4,      
        L2Rejected = 5     
    }

    public enum MaxMedicalLeaveDoc
    {
        MaxMedicalDoc= 3
    }
    public enum AttendanceStatus
    {
        Submitted = 1,
        L1Approved = 2,
        L2Approve = 3,
        Approved = 4,
        Cancelled = 5
    }
}