using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class Constants
    {

        public const string toastTypeSuccess = "Success";
        public const string toastTypeInformation = "Information";
        public const string toastTypetWarning = "Warning";
        public const string toastTypeError = "Error";
        public const string CompanyLogo = "CompanyLogo";
        public const string toastMessage = "toastMessage";
        public const string toastType = "toastType";
        public const string SessionBearerToken = "SessionBearerToken";
        public const string UserName = "UserName";
        public const string Email = "Email";
        public const string UserID = "UserID";
        public const string RoleID = "RoleID";
        public const string CompanyID = "CompanyID";
        public const string EmployeeID = "EmployeeID";
        public const string ProfilePhoto = "ProfilePhoto";
        public const string ProfileLogo = "ProfileLogo";
        public const string FirstName = "FirstName";
        public const string MiddleName = "MiddleName";
        public const string Surname = "Surname";
        public const string OfficialEmailID = "OfficialEmailID";
        public const string IsLinkExpired = "IsLinkExpired";
        public const string AreaName = "AreaName";
        public const string Role = "Role";
        public const string Alias = "Alias";
        public const string Index = "index";
        public const string InfoIndex = "InfoIndex";
        public const string EmployeeListing = "EmployeeListing";
        public const string RootUrlFormat = "{0}/{1}";
        public const string ManageAdmin = "admin";
        public const string ManageHR = "hr";
        public const string ManageEmployee = "employee";
        public const string SuperAdmin = "SuperAdmin";
        public const string SelectLanguage = "-- Select Language --";
        public const string SelectJobLocation = "-- Select Job Location --";
        public const string SelectRole = "-- Select Role --";
        public const string SelectReportingManager = "-- Select Reporting Manager --";
        public const string SelectCountry = "-- Select Country --";
        public const string SelectCurrency = "-- Select Currency --";
        public const string SelectEmployeeType = "-- Select Employee Type --";
        public const string SelectPayrollType = "-- Select Payroll Type --";
        public const string SelectDepartment = "-- Select Department --";
        public const string SubDepartment = "-- Select Sub Department --";
        public const string SelectSubDepartment = "-- Select SubDepartment --";
        public const string SelectDesignation = "-- Select Designation --";
        public const string SelectLeavePolicy = "-- Select Leave Policy --";
        public const string SelectLeaveType = "-- Select Leave Type --";
        public const string SelectLeaveDurationType = "-- Select --";
        public const string SelectShiftType = "-- Select Shift Type --";
        public const string Languages = "Languages";
        public const string Countries = "Countries";
        public const string EmploymentTypes = "EmploymentTypes";
        public const string Departments = "Departments";
        public const string ResultsData = "ResultsData";
        public const string EmployeePhotoPath = "Uploads/ProfilePhoto/";
        public const string EmployeeAadharCardPath = "Uploads/AadharCard/";
        public const string EmployeePanCardPath = "Uploads/PanCard/";
        public const string UploadCertificate = "Uploads/UploadCertificate/";
        public const string WhatHapenningIconPath = "Uploads/WhatHapenningIconPath/";

        public const string CompanyLogoPath = "Uploads/CompanyLogo/";
        public const string TemplatePath = "Uploads/Template/";
        public const string CKEditorImagesPath = "Uploads/ckeditor/";
        public const string NoImagePath = "/assets/img/No_image.png";
        public const string EmptySelection = "-- Please select a value --";
        public const string SelectEmployee = " --Select Employee --";
        public const string ID = "ID";

        public const string Gender = "Gender";
        public const string RoleName = "RoleName";
        public const string Manager1Name = "Manager1Name";
        public const string Manager1Email = "Manager1Email";
        public const string Manager2Name = "Manager2Name";
        public const string Manager2Email = "Manager2Email";
        public const string EmployeeNumber = "EmployeeNumber";
        public const string EmployeeNumberWithoutAbbr = "EmployeeNumberWithoutAbbr";
        public const string Whatshappening = "Uploads/Whatshappening/";
        public const string EmployeeDocuments = "Uploads/EmployeeDocuments/";
        public const string JobLocationID = "JobLocationID";
        public const string DepartmentID = "DepartmentID";
        public const string ApplyLeave = "ApplyLeave";
        public const string ApproveLeave = "ApproveLeave";


    }
    public class RoleConstants
    {
        // Roles
        public const string Admin = "Admin";
        public const string HR = "HR";
        public const string Employee = "Employee";
        public const string Manager = "Manager";
        public const string SuperAdmin = "SuperAdmin";
    }

    public enum Roles
    {
        HR = 1,
        Admin,
        Employee,
        Manager,
        SuperAdmin = 5
    }
    public enum Location
    {
        Mohali = 12,
        Kolkata = 8,
        Employee,
        Manager,
        SuperAdmin = 5
    }
       public enum PageName
    {
        EmployeeListing = 2,
        MyInfo=3,
        company = 4,
        Templates = 11,
        AttendenceList = 5,
        ApprovedAttendance = 6,
        TeamAttendenceList = 7,
        ShiftTypeListing = 8,
        Whatshappening = 9,
        Dashboard=10,
        LeavePolicyListing = 12,
        HolidayListing = 13,
        LeavePolicyDetailsListing = 14,
        WhatshappeningListing = 15,
        CompanyListing = 16,
        MyAttendanceList = 17,
        Support = 18,
        MyTeam = 19,
        FormPermission = 20,
        WeekOffRoster = 26,
      
    }


    public class APIControllarsConstants
    {

        public const string DashBoard = "DashBoard";
        public const string Employee = "Employee";
        public const string Template = "Template";
        public const string Company = "Company";
        public const string LeavePolicy = "LeavePolicy";
        public const string Holiday = "Holiday";
        public const string AttendenceList = "AttendenceList";
        public const string Common = "Common";
        public const string ShiftType = "ShiftType";


    }

    public class WebControllarsConstants
    {

        public const string EmployeesWeekOffRoster = "EmployeesWeekOffRoster";
        public const string PolicyCategoryListing = "PolicyCategoryListing";
        public const string LeavePolicyDetailsListing = "LeavePolicyDetailsListing";
        public const string LeavePolicyListing = "LeavePolicyListing";
        public const string LeavePolicy = "LeavePolicy";
        public const string Employee = "Employee";
        public const string EmployeeListing = "EmployeeListing"; 

        public const string Candidate = "Candidate";
        public const string MyInfo = "MyInfo";
        public const string Company = "Company";
        public const string Template = "Template";
        public const string TemplateListing = "TemplateListing";
        public const string Holiday = "Holiday";
        public const string HolidayListing = "HolidayListing";
        public const string AttendenceListing = "AttendenceListing";
        public const string Attendance = "Attendance";
        public const string ShiftTypeListing = "ShiftTypeListing";
        public const string ShiftType = "ShiftType";

        public const string MyAttendanceList = "MyAttendanceList";
        public const string MyAttendance = "MyAttendance";
        public const string WhatshappeningListing = "WhatshappeningListing";

        public const string EducationalDetail = "EducationalDetail";
        public const string FamilyDetail = "FamilyDetail";
        public const string ReferenceDetail = "ReferenceDetail";
        public const string EmploymentHistory = "EmploymentHistory";
        public const string FormPermission = "FormPermission";
    }

    public class APIApiActionConstants
    {
        public const string AddUpdatePolicyCategory = "AddUpdatePolicyCategory";
        public const string GetAllPolicyCategory = "GetAllPolicyCategory";
        public const string GetPolicyCategoryList = "GetPolicyCategoryList";
        public const string DeletePolicyCategory = "DeletePolicyCategory";
        public const string PolicyCategoryDetails = "PolicyCategoryDetails";




        public const string AddUpdateLeavePolicyDetails = "AddUpdateLeavePolicyDetails";
        public const string DeleteLeavePolicyDetails = "DeleteLeavePolicyDetails";
        public const string GetLeavePolicyList = "GetLeavePolicyList";
        public const string GetAllLeavePolicyDetails = "GetAllLeavePolicyDetails";
        public const string GetAllLeavePolicyDetailsByCompanyId = "GetAllLeavePolicyDetailsByCompanyId";


        public const string GetLeaveForApprovals = "GetLeaveForApprovals";
        public const string AddUpdateLeavePolicy = "AddUpdateLeavePolicy";
        public const string GetAllLeavePolicies = "GetAllLeavePolicies";
        public const string AddUpdateEmployee = "AddUpdateEmployee";
        public const string GetEmployeeListByManagerID = "GetEmployeeListByManagerID";
        public const string GetAllEmployees = "GetAllEmployees";
        public const string GetAllEmployeesList = "GetAllEmployeesList";
        public const string GetAllActiveEmployees = "GetAllActiveEmployees";
        public const string AddUpdateTemplate = "AddUpdateTemplate";
        public const string GetAllTemplates = "GetAllTemplates";
        public const string AddUpdateCompany = "AddUpdateCompany";
        public const string GetAllCompanies = "GetAllCompanies";
        public const string GetCompaniesLogo = "GetCompaniesLogo";
        public const string GetAllCompaniesList = "GetAllCompaniesList";
        public const string GetAllLeavePolicys = "GetAllLeavePolicys";
        public const string GetlLeavesSummary = "GetlLeavesSummary";
        public const string GetMyInfo = "GetMyInfo";
        public const string AddUpdateLeave = "AddUpdateLeave";
        public const string GetLeaveDurationTypes = "GetLeaveDurationTypes";
        public const string GetLeaveTypes = "GetLeaveTypes";
        public const string AddUpdateHoliday = "AddUpdateHoliday";
        public const string GetAllHolidays = "GetAllHolidays";
        public const string GetHolidayList = "GetHolidayList";
        public const string GetAllHolidayList = "GetAllHolidayList";
        public const string GetEmploymentDetailsByEmployee = "GetEmploymentDetailsByEmployee";
        public const string GetFilterEmploymentDetailsByEmployee = "GetFilterEmploymentDetailsByEmployee";
        public const string GetL2ManagerDetails = "GetL2ManagerDetails";
        public const string AddUpdateEmploymentDetails = "AddUpdateEmploymentDetails";
        public const string GetEmploymentBankDetails = "GetEmploymentBankDetails";
        public const string AddUpdateEmploymentBankDetails = "AddUpdateEmploymentBankDetails";
        public const string GetEmploymentSeparationDetails = "GetEmploymentSeparationDetails";
        public const string AddUpdateEmploymentSeparationDetails = "AddUpdateEmploymentSeparationDetails";
        public const string GetDashBoardModel = "GetDashBoardModel";
        public const string GetAllAttendenceList = "GetAllAttendenceList";
        public const string AddUpdateAttendenceList = "AddUpdateAttendenceList";
        public const string ResetPassword = "ResetPassword";
        public const string GetFogotPasswordDetails = "GetFogotPasswordDetails";
        public const string AddUpdateShiftType = "AddUpdateShiftType";
        public const string GetAllShiftTypes = "GetAllShiftTypes";
        public const string DeleteLeavesSummary = "DeleteLeavesSummary";
        public const string GetAttendance = "GetAttendance";
        public const string GetAttendanceForMonthlyViewCalendar = "GetAttendanceForMonthlyViewCalendar";
        public const string GetTeamAttendanceForCalendar = "GetTeamAttendanceForCalendar";
        public const string AddUpdateAttendace = "AddUpdateAttendace";
        public const string GetAttendenceListID = "GetAttendenceListID";
        public const string DeleteAttendanceDetails = "DeleteAttendanceDetails";
        public const string GetMyAttendanceList = "GetMyAttendanceList";
        public const string GetAttendanceForApproval = "GetAttendanceForApproval";
        public const string GetApprovedAttendance = "GetApprovedAttendance";
        public const string GetManagerApprovedAttendance = "GetManagerApprovedAttendance";
        public const string GetAttendanceDeviceLogs = "GetAttendanceDeviceLogs";
        public const string GetCountryDictionary = "GetCountryDictionary";
        public const string GetCompaniesDictionary = "GetCompaniesDictionary";
        public const string GetEmploymentDetailsDictionaries = "GetEmploymentDetailsDictionaries";
        public const string AddUpdateEmployeeFromExecel = "AddUpdateEmployeeFromExecel";
        public const string AddUpdateEmployeeFromExecelBulk = "AddUpdateEmployeeFromExecelBulk";
        public const string GetSubDepartmentDictionary = "GetSubDepartmentDictionary";
        public const string GetEmployeeDetails = "GetEmployeeDetails";
        public const string FetchAttendanceHolidayAndLeaveInfo = "FetchAttendanceHolidayAndLeaveInfo";
        public const string GetJobLocationsByCompany = "GetJobLocationsByCompany";
        
        
        public const string GetAllWhatsHappeningDetails = "GetAllWhatsHappeningDetails";
        public const string AddUpdateWhatsHappeningDetails = "AddUpdateWhatsHappeningDetails";
        public const string DeleteWhatsHappening = "DeleteWhatsHappening";


        public const string AddUpdateWhatsHappening = "AddUpdateWhatsHappening";
        public const string GetWhatsHappenings = "GetWhatsHappenings";
        public const string CheckEmployeeReporting = "CheckEmployeeReporting";

        public const string AddUpdateEducationDetail = "AddUpdateEducationDetail";
        public const string GetEducationDetails = "GetEducationDetails";
        public const string DeleteEducationDetail = "DeleteEducationDetail";

        public const string GetReferenceDetails = "GetReferenceDetails";
        public const string AddUpdateReferenceDetail = "AddUpdateReferenceDetail";
        public const string DeleteReferenceDetail = "DeleteReferenceDetail";

        public const string GetFamilyDetails = "GetFamilyDetails";
        public const string AddUpdateFamilyDetail = "AddUpdateFamilyDetail";
        public const string DeleteFamilyDetail = "DeleteFamilyDetail";

        public const string GetEmploymentHistory = "GetEmploymentHistory";
        public const string AddUpdateEmploymentHistory = "AddUpdateEmploymentHistory";
        public const string DeleteEmploymentHistory = "DeleteEmploymentHistory";

        public const string GetLanguageDetails = "GetLanguageDetails";
        public const string AddUpdateLanguageDetail = "AddUpdateLanguageDetail";
        public const string DeleteLanguageDetail = "DeleteLanguageDetail";
        public const string FetchExportEmployeeExcelSheet = "FetchExportEmployeeExcelSheet";
        public const string GetValidateCompOffLeave = "GetValidateCompOffLeave";
        public const string UpdateLeaveStatus = "UpdateLeaveStatus";
        public const string GetAllCompanyDepartments = "GetAllCompanyDepartments";
        public const string AddFormPermissions = "AddFormPermissions";
        public const string GetUserFormPermissions = "GetUserFormPermissions";
        public const string AddUserFormPermissions = "AddUserFormPermissions";
        public const string GetUserFormByDepartmentID = "GetUserFormByDepartmentID";
        public const string CheckUserFormPermissionByEmployeeID = "CheckUserFormPermissionByEmployeeID";

        public const string GetCompOffAttendanceList = "GetCompOffAttendanceList";
        public const string AddUpdateCompOffAttendace = "AddUpdateCompOffAttendace";
        public const string GetApprovedCompOff = "GetApprovedCompOff";

        public const string InsertException = "InsertException";
        public const string GetRosterWeekOff = "GetRosterWeekOff";
        public const string GetEmployeesWeekOffRoster = "GetEmployeesWeekOffRoster";
        public const string DeleteWeekOffRoster = "DeleteWeekOffRoster"; 


    }

    public class MyInfoTabs
    {
        public const string TabPersonalInfo = "TabPersonalInfo";
        public const string TabProfessionalInfo = "TabProfessionalInfo";
        public const string TabBankInfo = "TabBankInfo";
        public const string TabLeavesForApproval = "TabLeavesForApproval";
        public const string TabTimeOff = "TabTimeOff";
        public const string TabLeaveInfo = "TabLeaveInfo";
        public const string attendance = "attendance";

    }
}
