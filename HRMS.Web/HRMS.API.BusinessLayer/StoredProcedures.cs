using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.API.BusinessLayer
{
    internal class StoredProcedures
    {
        public const string usp_GetDeviceLogsByMonth = "usp_GetDeviceLogsByMonth";


        public const string usp_LoginUser = "usp_LoginUser";
        public const string usp_Get_Counteres = "usp_Get_Counteres";
        public const string usp_Get_Currencies = "usp_Get_Currencies";
        public const string usp_Get_Languages = "usp_Get_Languages";
        public const string usp_AddUpdate_Employee = "usp_AddUpdate_Employee";
        public const string usp_AddUpdateExcelImport = "usp_AddUpdateExcelImport";
        public const string usp_Get_FogotPasswordDetails = "usp_Get_FogotPasswordDetails";
        public const string usp_Get_Celebrations = "usp_Get_Celebrations";
        public const string usp_Get_CompanyDepartments = "usp_Get_CompanyDepartments";
        public const string usp_Get_CompanyEmployeeTypes = "usp_Get_CompanyEmployeeTypes";
        public const string usp_Get_CompanyLanguages = "usp_Get_CompanyLanguages";
        public const string usp_Get_EmployeeDetails = "usp_Get_EmployeeDetails";
        public const string usp_Get_ActiveEmployeeDetails = "usp_Get_ActiveEmployeeDetails";
        public const string usp_Get_TemplateDetails = "usp_Get_TemplateDetails";
        public const string usp_AddUpdate_Template = "usp_AddUpdate_Template";

        public const string usp_Get_LeavePolicyDetails = "usp_Get_LeavePolicyDetails";
        public const string usp_AddUpdate_LeavePolicy = "usp_AddUpdate_LeavePolicy";

        public const string usp_Get_CompanySubDepartments = "usp_Get_CompanySubDepartments";
        public const string usp_Get_Companies = "usp_Get_Companies";
        public const string usp_AddUpdate_Company = "usp_AddUpdate_Company";
        public const string usp_Get_LeavesSummary = "usp_Get_LeavesSummary";
        public const string usp_Delete_LeaveSummary = "usp_Delete_LeaveSummary";

        public const string usp_AddUpdate_LeaveSummary = "usp_AddUpdate_LeaveSummary";
        public const string usp_Get_LeaveDurationTypes = "usp_Get_LeaveDurationTypes";
        public const string usp_Get_LeaveTypes = "usp_Get_LeaveTypes";

        public const string usp_Get_HolidayDetails = "usp_Get_HolidayDetails";
        public const string usp_AddUpdate_Holiday = "usp_AddUpdate_Holiday";
        public const string usp_Get_AttendanceList = "usp_Get_AttendanceList";
        public const string usp_AddUpdate_AttendenceList = "usp_AddUpdate_AttendenceList";
        public const string usp_Get_Employees = "usp_Get_Employees";
        public const string usp_Get_HolidayList = "usp_Get_HolidayList";


        public const string usp_InsertUserRole = "usp_InsertUserRole";
        public const string usp_AddUpdate_EmploymentDetails = "usp_AddUpdate_EmploymentDetails";
        public const string usp_Get_EmployeeDetailsFormDetails = "usp_Get_EmployeeDetailsFormDetails";
        public const string usp_Get_FilterEmployeeDetailsFormDetails = "usp_Get_FilterEmployeeDetailsFormDetails";
        public const string usp_AddUpdate_EmployeeBankDetails = "usp_AddUpdate_EmployeeBankDetails";
        public const string usp_Get_EmployeeBankDetails = "usp_Get_EmployeeBankDetails";
        public const string usp_AddUpdate_EmployeeSeparation = "usp_AddUpdate_EmployeeSeparation";
        public const string usp_Get_EmployeeSeparationDetail = "usp_Get_EmployeeSeparationDetail";

        public const string usp_GetDashBoardDetails = "usp_GetDashBoardDetails";
        public const string usp_ResetPassword = "usp_ResetPassword";
        public const string usp_Get_LeaveForApprovals = "usp_Get_LeaveForApprovals";

        public const string usp_Get_ShiftTypeDetails = "usp_Get_ShiftTypeDetails";
        public const string usp_AddUpdate_ShiftType = "usp_AddUpdate_ShiftType";




        public const string usp_AddUpdate_LeavePolicyDetails = "usp_AddUpdate_LeavePolicyDetails";
        public const string usp_Get_LeavePrivacyPolicyDetails = "usp_Get_LeavePrivacyPolicyDetails ";
        public const string usp_Get_LeavePolicyDetailsList = "usp_Get_LeavePolicyDetailsList";
        public const string usp_Delete_LeavePolicyDetails = "usp_Delete_LeavePolicyDetails";

        public const string usp_GetEmployeeListByManagerID = "usp_GetEmployeeListByManagerID";


        public const string usp_AddUpdate_PolicyCategory = "usp_AddUpdate_PolicyCategory";
        public const string usp_Get_PolicyCategory = "usp_Get_PolicyCategory ";
        public const string usp_Get_PolicyCategoryList = "usp_Get_PolicyCategoryList";
        public const string usp_Delete_PolicyCategory = "usp_Delete_PolicyCategory";
      
        public const string usp_Get_DistinctPolicyCategoryDetails = "usp_Get_DistinctPolicyCategoryDetails";
        public const string sp_GetAttendanceDeviceLog = "sp_GetAttendanceDeviceLog";
        public const string sp_GetTeamAttendanceDeviceLog = "sp_GetTeamAttendanceDeviceLog";
        public const string sp_GetMyAttendanceLog = "sp_GetMyAttendanceLog";
        public const string usp_Get_AttendanceForApprovals = "usp_Get_AttendanceForApprovals";
        public const string usp_Get_ApprovedAttendance = "usp_Get_ApprovedAttendance";
        public const string usp_Get_Manager_ApprovedAttendance = "usp_Get_Manager_ApprovedAttendance";


        public const string sp_SaveAttendanceDeviceLog = "sp_SaveAttendanceDeviceLog";
        public const string sp_SaveAttendanceManualLog = "sp_SaveAttendanceManualLog";
        public const string GetAttendanceDeviceLogById = "GetAttendanceDeviceLogById";
        public const string sp_DeleteAttendanceDeviceLog = "sp_DeleteAttendanceDeviceLog";
        public const string usp_Get_WhatsHappeningS = "usp_Get_WhatsHappeningS";
        public const string usp_AddUpdate_WhatsHappening = "usp_AddUpdate_WhatsHappening";
        public const string usp_Delete_WhatsHappening = "usp_Delete_WhatsHappening";
    }
}
