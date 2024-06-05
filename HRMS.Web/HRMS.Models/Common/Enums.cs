using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
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
    public enum LeaveTypeName
    {
        [Display(Name = "Casual")]
        Casual,
        [Display(Name = "Medical")]
        Medical,
        [Display(Name = "Annual Leave")]
        AnnualLeave,
        [Display(Name = "Comp Off")]
        CompOff
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
    public static class LeaveTypeNameExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())[0]
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }
    }
}