using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
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
        public Results GetAllEmployees(EmployeeInputParams model);
        public Result AddUpdateTemplate(TemplateModel model);
        public Results GetAllTemplates(TemplateInputParams model);
        public Result AddUpdateCompany(CompanyModel model);
        public Results GetAllCompanies(EmployeeInputParams model);
        public LeaveResults GetlLeavesSummary(LeaveInputParams model);
        public Result AddUpdateLeave(LeaveSummayModel leaveSummayModel);       
        public LeaveResults GetLeaveDurationTypes(LeaveInputParams leaveInputParams);      
        public LeaveResults GetLeaveTypes(LeaveInputParams leaveInputParams);       
    }
}
