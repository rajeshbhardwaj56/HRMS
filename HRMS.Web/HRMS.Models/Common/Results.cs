using HRMS.Models.Company;
using HRMS.Models.Employee;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class Results
    {
        public Result Result { get; set; } = new Result();
        public List<SelectListItem> Countries { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Currencies { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Languages { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> EmploymentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<EmployeeModel> Employees { get; set; } = new List<EmployeeModel>();
        public EmployeeModel employeeModel { get; set; } = new EmployeeModel();
        public CompanyModel companyModel { get; set; } = new CompanyModel();
        public List<CompanyModel> Companies { get; set; } = new List<CompanyModel>();

    }



    public class Result
    {
        public long PKNo { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }
}
