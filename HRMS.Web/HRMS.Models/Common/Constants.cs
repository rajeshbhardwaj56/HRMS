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
        public const string Role = "Role";
        public const string Alias = "Alias";
        public const string Index = "index";
        public const string RootUrlFormat = "{0}/{1}";
        public const string ManageAdmin = "admin";
        public const string ManageHR = "hr";
        public const string ManageEmployee = "employee";
        public const string SelectLanguage = "-- Select Language --";
        public const string SelectCountry = "-- Select Country --";
        public const string SelectEmployeeType = "-- Select Employee Type --";
        public const string SelectDeportment = "-- Select Deportment --";
        public const string Languages = "Languages";
        public const string Countries = "Countries";
        public const string EmploymentTypes = "EmploymentTypes";
        public const string Departments = "Departments";
        public const string ResultsData = "ResultsData";
        
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
    }

    public class APIApiActionConstants
    {
        public const string AddUpdateEmployee = "AddUpdateEmployee";

    }
}
