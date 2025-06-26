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
        public const string usp_GetManagerOfManager = "usp_GetManagerOfManager";
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
        public const string usp_GetAttendanceDeviceLog = "usp_GetAttendanceDeviceLog";
        public const string usp_GetTeamAttendanceDeviceLog = "usp_GetTeamAttendanceDeviceLog";
        public const string usp_GetMyAttendanceLog = "usp_GetMyAttendanceLog";
        public const string usp_Get_AttendanceForApprovals = "usp_Get_AttendanceForApprovals";
        public const string usp_Get_ApprovedAttendance = "usp_Get_ApprovedAttendance";
        public const string usp_Get_Manager_ApprovedAttendance = "usp_Get_Manager_ApprovedAttendance";
        public const string usp_GetDailyAttendanceDetails = "usp_GetDailyAttendanceDetails";


        public const string usp_SaveAttendanceDeviceLog = "usp_SaveAttendanceDeviceLog";
        public const string usp_SaveAttendanceManualLog = "usp_SaveAttendanceManualLog";
        public const string GetAttendanceDeviceLogById = "GetAttendanceDeviceLogById";
        public const string usp_DeleteAttendanceDeviceLog = "usp_DeleteAttendanceDeviceLog";
        public const string usp_Get_WhatsHappeningS = "usp_Get_WhatsHappeningS";
        public const string usp_AddUpdate_WhatsHappening = "usp_AddUpdate_WhatsHappening";
        public const string usp_Delete_WhatsHappening = "usp_Delete_WhatsHappening";
        public const string usp_CheckEmployeeReporting = "usp_CheckEmployeeReporting";


        public const string usp_ExportEmployeeFullDetails = "usp_ExportEmployeeFullDetails";
        public const string usp_Get_CampOffLeaves = "usp_Get_CampOffLeaves";
        public const string usp_Is_CampOff_Eligible = "usp_Is_CampOff_Eligible";
        public const string usp_UpdateLeaveStatus = "usp_UpdateLeaveStatus";

        public const string usp_Get_EducationDetailsByEmployee = "usp_Get_EducationDetailsByEmployee";
        public const string usp_AddUpdate_EducationDetails = "usp_AddUpdate_EducationDetails";
        public const string usp_Delete_EducationDetails = "usp_Delete_EducationDetails";

        public const string usp_Get_EmploymentHistoryByEmployee = "usp_Get_EmploymentHistoryByEmployee";
        public const string usp_AddUpdate_EmploymentHistory = "usp_AddUpdate_EmploymentHistory";
        public const string usp_Delete_EmploymentHistory = "usp_Delete_EmploymentHistory";

        public const string usp_Get_EmployeesFamilyDetailsByEmployee = "usp_Get_EmployeesFamilyDetailsByEmployee";
        public const string usp_AddUpdate_FamilyDetails = "usp_AddUpdate_FamilyDetails";
        public const string usp_Delete_EmployeesFamilyDetails = "usp_Delete_EmployeesFamilyDetails";

        public const string usp_Get_ReferenceDetailsByEmployee = "usp_Get_ReferenceDetailsByEmployee";
        public const string usp_AddUpdate_ReferenceDetails = "usp_AddUpdate_ReferenceDetails";
        public const string usp_Delete_ReferenceDetails = "usp_Delete_ReferenceDetails";

        public const string usp_Get_LanguageDetailsByEmployee = "usp_Get_LanguageDetailsByEmployee";
        public const string usp_AddUpdate_LanguageDetails = "usp_AddUpdate_LanguageDetails";
        public const string usp_Delete_LanguageDetails = "usp_Delete_LanguageDetails";
        public const string usp_Get_CompanyForms = "usp_Get_CompanyForms";
        public const string ups_InsupdGroupFormPermission = "ups_InsupdGroupFormPermission";
        public const string usp_GetFormByDepartmentID = "usp_GetFormByDepartmentID";
        public const string usp_GetUserFormPermissions = "usp_GetUserFormPermissions";
        public const string usp_InsertFormPermissions = "usp_InsertFormPermissions";
        public const string usp_GetUserFormByDepartmentID = "usp_GetUserFormByDepartmentID";
        public const string usp_CheckUserFormPermissionByEmployeeID = "usp_CheckUserFormPermissionByEmployeeID";

        public const string usp_GetEmployeeListByManagerIDs = "usp_GetEmployeeListByManagerIDs";

        public const string usp_GetHolidayOrSundayWorkLog = "usp_GetHolidayOrSundayWorkLog";
        public const string usp_GetCompOffLeaveRequestsForManagers = "usp_GetCompOffLeaveRequestsForManagers";
        public const string usp_SaveOrUpdateCompOffLeaveRequest = "usp_SaveOrUpdateCompOffLeaveRequest";
        public const string usp_UpdateAttendanceCompOffStatusByUserId = "usp_UpdateAttendanceCompOffStatusByUserId";
        public const string usp_EmployeeReportingManagersSame = "usp_EmployeeReportingManagersSame";
        public const string usp_GetJobLocationsByCompany = "usp_GetJobLocationsByCompany";

        public const string usp_InsertExceptionLog = "usp_InsertExceptionLog";


    }
}
