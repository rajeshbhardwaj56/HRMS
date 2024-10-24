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
        public const string FirstName = "FirstName";
        public const string MiddleName = "MiddleName";
        public const string Surname = "Surname";
        public const string OfficialEmailID = "OfficialEmailID";
        public const string IsLinkExpired = "IsLinkExpired";
        public const string AreaName = "AreaName";
        public const string Role = "Role";
        public const string Alias = "Alias";
        public const string Index = "index";
        public const string RootUrlFormat = "{0}/{1}";
        public const string ManageAdmin = "admin";
        public const string ManageHR = "hr";
        public const string ManageEmployee = "employee";
        public const string SelectLanguage = "-- Select Language --";
        public const string SelectJobLocation = "-- Select Job Location --";
        public const string SelectReportingManager = "-- Select Reporting Manager --";
        public const string SelectCountry = "-- Select Country --";
        public const string SelectCurrency = "-- Select Currency --";
        public const string SelectEmployeeType = "-- Select Employee Type --";
        public const string SelectDeportment = "-- Select Deportment --";
        public const string SelectLeaveType = "-- Select Leave Type --";
        public const string SelectLeaveDurationType = "-- Select --";
        public const string Languages = "Languages";
        public const string Countries = "Countries";
        public const string EmploymentTypes = "EmploymentTypes";
        public const string Departments = "Departments";
        public const string ResultsData = "ResultsData";
        public const string EmployeePhotoPath = "Uploads/ProfilePhoto/";
        public const string UploadCertificate = "Uploads/UploadCertificate/";
        public const string CompanyLogoPath = "Uploads/CompanyLogo/";
        public const string TemplatePath = "Uploads/Template/";
        public const string CKEditorImagesPath = "Uploads/ckeditor/";
        public const string NoImagePath = "/assets/img/No_image.png";
        public const string EmptySelection = "-- Please select a value --";
        public const string SelectEmployee = " --Select Employee --";
        public const string ID = "ID";

    }

    public class RoleConstants
    {
        // Roles
        public const string Admin = "Admin";
        public const string HR = "HR";
        public const string Employee = "Employee";
    }

    public enum Roles
    {
        HR = 1,
        Admin,
        Employee
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

        public const string LeavePolicyListing = "LeavePolicyListing";
        public const string LeavePolicy = "LeavePolicy";
        public const string Employee = "Employee";
        public const string Candidate = "Candidate";
        public const string MyInfo = "MyInfo";
        public const string Company = "Company";
        public const string Template = "Template";
        public const string TemplateListing = "TemplateListing";
        public const string Holiday = "Holiday";
        public const string HolidayListing = "HolidayListing";
        public const string AttendenceListing = "AttendenceListing";
        public const string AttendenceList = "AttendenceList";
        public const string ShiftTypeListing = "ShiftTypeListing";
        public const string ShiftType = "ShiftType";
    }

    public class APIApiActionConstants
    {

        public const string GetLeaveForApprovals = "GetLeaveForApprovals";
        public const string AddUpdateLeavePolicy = "AddUpdateLeavePolicy";
        public const string GetAllLeavePolicies = "GetAllLeavePolicies";
        public const string AddUpdateEmployee = "AddUpdateEmployee";
        public const string GetAllEmployees = "GetAllEmployees";
        public const string GetAllActiveEmployees = "GetAllActiveEmployees";
        public const string AddUpdateTemplate = "AddUpdateTemplate";
        public const string GetAllTemplates = "GetAllTemplates";
        public const string AddUpdateCompany = "AddUpdateCompany";
        public const string GetAllCompanies = "GetAllCompanies";
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
        public const string AddUpdateEmploymentDetails = "AddUpdateEmploymentDetails";
        public const string GetDashBoardodel = "GetDashBoardodel";
        public const string GetAllAttendenceList = "GetAllAttendenceList";
        public const string AddUpdateAttendenceList = "AddUpdateAttendenceList";
        public const string ResetPassword = "ResetPassword";
        public const string GetFogotPasswordDetails = "GetFogotPasswordDetails";
        public const string AddUpdateShiftType = "AddUpdateShiftType";
        public const string GetAllShiftTypes = "GetAllShiftTypes";
        public const string DeleteLeavesSummary = "DeleteLeavesSummary";

    }

    public class MyInfoTabs
    {
        public const string TabPersonalInfo = "TabPersonalInfo";
        public const string TabProfessionalInfo = "TabProfessionalInfo";
        public const string TabLeavesForApproval = "TabLeavesForApproval";
        public const string TabTimeOff = "TabTimeOff";
        public const string TabLeaveInfo = "TabLeaveInfo";
    }
}
