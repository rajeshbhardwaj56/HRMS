using HRMS.Models.Employee;
using HRMS.Models.Template;
using HRMS.Models.Company;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public List<TemplateModel> Template { get; set; } = new List<TemplateModel>();
        public TemplateModel templateModel { get; set; } = new TemplateModel();
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
