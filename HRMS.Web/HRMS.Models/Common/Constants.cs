using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class Constants
    {
        public const string SessionBearerToken = "SessionBearerToken";
        public const string UserName = "UserName";
        public const string Email = "Email";
        public const string UserID = "UserID";
        public const string RoleID = "RoleID";
        public const string CompanyID = "CompanyID";
        public const string EmployeeID = "EmployeeID";
        public const string Role = "Role";
        public const string Alias = "Alias";
        public const string Index = "index";
        public const string RootUrlFormat = "{0}/{1}";
        public const string ManageAdmin = "admin";
        public const string ManageHR = "hr";
        public const string ManageEmployee = "employee";
        public const string SelectLanguage = "-- Select Language --";
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
        public const string CompanyLogoPath = "Uploads/CompanyLogo/";
        public const string TemplatePath = "Uploads/Template/";
        public const string CKEditorImagesPath = "Uploads/ckeditor/";
        public const string NoImagePath = "/assets/img/No_image.png";
        public const string EmptySelection = "-- Please select a value --";


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
        public const string Employee = "Employee";
        public const string Template = "Template";
        public const string Company = "Company";
        public const string LeavePolicy = "LeavePolicy";
        public const string Holiday = "Holiday";
    }

    public class WebControllarsConstants
    {

        public const string LeavePolicyListing = "LeavePolicyListing";
        public const string LeavePolicy = "LeavePolicy";
        public const string Employee = "Employee";
        public const string MyInfo = "MyInfo";
        public const string Company = "Company";
        public const string Template = "Template";
        public const string TemplateListing = "TemplateListing";
        public const string Holiday = "Holiday";
        public const string HolidayListing = "HolidayListing";
    }

    public class APIApiActionConstants
    {
        public const string AddUpdateLeavePolicy = "AddUpdateLeavePolicy";
        public const string GetAllLeavePolicies = "GetAllLeavePolicies";
        public const string AddUpdateEmployee = "AddUpdateEmployee";
        public const string GetAllEmployees = "GetAllEmployees";
        public const string AddUpdateTemplate = "AddUpdateTemplate";
        public const string GetAllTemplates = "GetAllTemplates";
        public const string AddUpdateCompany = "AddUpdateCompany";
        public const string GetAllCompanies = "GetAllCompanies";
        public const string GetlLeavesSummary = "GetlLeavesSummary";
        public const string GetMyInfo = "GetMyInfo";
        public const string AddUpdateLeave = "AddUpdateLeave";
        public const string GetLeaveDurationTypes = "GetLeaveDurationTypes";
        public const string GetLeaveTypes = "GetLeaveTypes";
        public const string AddUpdateHoliday = "AddUpdateHoliday";
        public const string GetAllHolidays = "GetAllHolidays";
    }
}
