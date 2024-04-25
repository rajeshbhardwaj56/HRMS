using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
using HRMS.Models.MyInfo;
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
        public Results GetAllActiveEmployees(EmployeeInputParams model);
        public Result AddUpdateTemplate(TemplateModel model);
        public Results GetAllTemplates(TemplateInputParams model);
        public Result AddUpdateCompany(CompanyModel model);
        public Results GetAllCompanies(EmployeeInputParams model);
        public LeaveResults GetlLeavesSummary(MyInfoInputParams model);
        public Result AddUpdateLeave(LeaveSummaryModel leaveSummaryModel);       
        public LeaveResults GetLeaveDurationTypes(MyInfoInputParams leaveInputParams);      
        public LeaveResults GetLeaveTypes(MyInfoInputParams leaveInputParams);
        public Result AddUpdateLeavePolicy(LeavePolicyModel model);
        public Results GetAllLeavePolicies(LeavePolicyInputParans model);
        public MyInfoResults GetMyInfo(MyInfoInputParams model);
        public Result AddUpdateEmploymentDetails(EmploymentDetail employmentDetails);
    }
}
