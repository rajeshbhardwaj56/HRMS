using HRMS.Models.Common;
using HRMS.Models.Employee;
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
        public Result AddUpdateEmployee(EmployeeModel model);
        public Results GetAllEmployees(EmployeeInputParans model);
    }
}
