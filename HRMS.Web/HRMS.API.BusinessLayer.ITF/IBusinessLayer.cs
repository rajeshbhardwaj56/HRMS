using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.Employee;
using HRMS.Models.LeavePolicy;
using HRMS.Models.Template;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.API.BusinessLayer.ITF
{
    public interface IBusinessLayer
    {
        LoginUser LoginUser(LoginUser loginUser);
        public Results GetAllCountries();
        public Results GetAllLanguages();
        public Results GetAllCompanyLanguages(long companyID);
        public Results GetAllCompanyDepartments(long companyID);
        public Results GetAllCompanyEmployeeTypes(long companyID);
        public Results GetAllCurrencies(long companyID);
        public Result AddUpdateEmployee(EmployeeModel model);
        public Results GetAllEmployees(EmployeeInputParans model);
        public Result AddUpdateTemplate(TemplateModel model);
        public Results GetAllTemplates(TemplateInputParans model);
        public Result AddUpdateLeavePolicy(LeavePolicyModel model);
        public Results GetAllLeavePolicys(LeavePolicyInputParans model);
        public Result AddUpdateCompany(CompanyModel model);
        public Results GetAllCompanies(EmployeeInputParans model);
    }
}
