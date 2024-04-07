using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.API.BusinessLayer
{
    internal class StoredProcedures
    {
        public const string usp_LoginUser = "usp_LoginUser";
        public const string usp_Get_Counteres = "usp_Get_Counteres";
        public const string usp_Get_Currencies = "usp_Get_Currencies";
        public const string usp_Get_Languages = "usp_Get_Languages";
        public const string usp_AddUpdate_Employee = "usp_AddUpdate_Employee";
        public const string usp_AddUpdate_Template = "usp_AddUpdate_Template";

        public const string usp_Get_CompanyDepartments = "usp_Get_CompanyDepartments";
        public const string usp_Get_CompanyEmployeeTypes = "usp_Get_CompanyEmployeeTypes";
        public const string usp_Get_CompanyLanguages = "usp_Get_CompanyLanguages";
        public const string usp_Get_EmployeeDetails = "usp_Get_EmployeeDetails";
        public const string usp_Get_TemplateDetails = "usp_Get_TemplateDetails";

        public const string usp_Get_Companies = "usp_Get_Companies";
        public const string usp_AddUpdate_Company = "usp_AddUpdate_Company";

    }
}
