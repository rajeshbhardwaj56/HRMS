using HRMS.Models.Employee;
using HRMS.Models.Template;
using HRMS.Models.Company;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRMS.Models.LeavePolicy;
using HRMS.Models.AttendenceList;
using HRMS.Models.ShiftType;
using HRMS.Models.WhatsHappeningModel;

namespace HRMS.Models.Common
{
    public class Results
    {
        public Result Result { get; set; } = new Result();
        public List<SelectListItem> Employee { get; set; } = new List<SelectListItem>();
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
        public CompanyLoginModel companyLoginModel { get; set; } = new CompanyLoginModel();
        public List<CompanyModel> Companies { get; set; } = new List<CompanyModel>();
        public List<SelectListItem> JobLocationList { get; set; } = new List<SelectListItem>();
        public List<LeavePolicyModel> LeavePolicy { get; set; } = new List<LeavePolicyModel>();
        public LeavePolicyModel leavePolicyModel { get; set; } = new LeavePolicyModel();
        // public WhatsHappeningModel leavePolicyModel { get; set; } = new WhatsHappeningModel();
        public LeavePolicyDetailsModel LeavePolicyDetailsModel { get; set; } = new LeavePolicyDetailsModel();

        public List<HolidayModel> Holiday { get; set; } = new List<HolidayModel>();
        public List<LeavePolicyDetailsModel> LeavePolicyDetailsList { get; set; } = new List<LeavePolicyDetailsModel>();
        public List<WhatsHappeningModels> WhatsHappeningList { get; set; } = new List<WhatsHappeningModels>();
        public WhatsHappeningModels WhatsHappeningModel { get; set; } = new WhatsHappeningModels();

        public HolidayModel holidayModel { get; set; } = new HolidayModel();
        public List<AttendenceListModel> AttendenceList { get; set; } = new List<AttendenceListModel>();
        public AttendenceListModel AttendenceListModel { get; set; } = new AttendenceListModel();
        public List<ShiftTypeModel> ShiftType { get; set; } = new List<ShiftTypeModel>();
        public ShiftTypeModel shiftTypeModel { get; set; } = new ShiftTypeModel();
        public List<SelectListItem> leaveTypes { get; set; } = new List<SelectListItem>();
        public List<PolicyCategoryModel> PolicyCategoryList { get; set; } = new List<PolicyCategoryModel>();
        public PolicyCategoryModel PolicyCategoryModel { get; set; } = new PolicyCategoryModel();
        public List<Attendance> AttandanceList { get; set; } = new List<Attendance>();
        public Attendance AttendanceModel { get; set; } = new Attendance();
        public List<SelectListItem> FormsPermission { get; set; } = new List<SelectListItem>();

    }

    public class Result
    {
        public long? PKNo { get; set; }
        public long? UserID { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public bool IsResetPasswordRequired { get; set; }

        public object? Data { get; set; }
    }


}
