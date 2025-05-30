﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Models.ShiftType
{

    public class ShiftTypeInputParans
    {
        public long CompanyID { get; set; }
        public long ShiftTypeID { get; set; }
        public int? DisplayStart { get; set; } = 0;
        public int? DisplayLength { get; set; } = 0;
        public string? Searching { get; set; }
    }


    public class ShiftTypeModel
    {
        public long ShiftTypeID { get; set; }
        public long CompanyID { get; set; }
        public string ShiftTypeName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool AutoAttendance { get; set; }
        public bool IsCheckInAndOut { get; set; }
        public long HalfDayWorkingHours { get; set; }
        public long HolidayID { get; set; }
        public string? WorkingHoursCalculation { get; set; }
        public long AbsentDayWorkingHours { get; set; }
        public bool CheckInBeforeShift { get; set; }
        public bool CheckOutAfterShift { get; set; }
        public long ProcessAttendanceAfter { get; set; }
        public DateTime LastSyncDateTime { get; set; }
        public bool AutoAttendanceOnHoliday { get; set; }
        public long LastEntryGracePeriod { get; set; }
        public long EarlyExitGracePeriod { get; set; }
        public string? Comments { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime InsertedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long CreatedByID { get; set; }
        public long ModifiedByID { get; set; }
        public int? TotalRecords { get; set; } = 0;
        public int? FilteredRecords { get; set; } = 0;

        public List<SelectListItem> HolidayList = new List<SelectListItem>();
        public List<SelectListItem> WorkingHoursCalculationTypeList = new List<SelectListItem>();
    }

    public class EmployeeAttendance
    {
        public string? FullName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeJoiningdate { get; set; }
        public string? EmployeeDesignation { get; set; }
        public string? EmployeeDepartment { get; set; }
        public string? ManagerName { get; set; }
        public string? Employeeemail { get; set; }
        public TimeSpan ShiftStartDate { get; set; }
        public TimeSpan ShiftEndDate { get; set; }
    }



}