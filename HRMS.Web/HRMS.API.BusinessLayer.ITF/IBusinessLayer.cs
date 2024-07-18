using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
using HRMS.Models.MyInfo;
using HRMS.Models.Template;
using HRMS.Models.AttendenceList;
using HRMS.Models.ShiftType;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRMS.Models.WhatsHappening;

namespace HRMS.API.BusinessLayer.ITF
{
    public interface IBusinessLayer
    {
        LoginUser LoginUser(LoginUser loginUser);
        public Result ResetPassword(ResetPasswordModel loginUser);
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
        public LeaveResults GetLeaveForApprovals(MyInfoInputParams model);        
        public LeaveResults GetLeaveDurationTypes(MyInfoInputParams leaveInputParams);
        public LeaveResults GetLeaveTypes(MyInfoInputParams leaveInputParams);
        public Result AddUpdateLeavePolicy(LeavePolicyModel model);
        public Results GetAllLeavePolicies(LeavePolicyInputParans model);
        public MyInfoResults GetMyInfo(MyInfoInputParams model);
        public Result AddUpdateEmploymentDetails(EmploymentDetail employmentDetails);
        public EmploymentDetail GetEmploymentDetailsByEmployee(EmploymentDetailInputParams model);
        public Result GetFogotPasswordDetails(ChangePasswordModel model);
        public Result AddUpdateHoliday(HolidayModel model);
        public Results GetAllHolidays(HolidayInputParams model);
        public DashBoardModel GetDashBoardodel(DashBoardModelInputParams model);
        public Results GetAllAttendenceList(AttendenceListInputParans model);
        public Result AddUpdateAttendenceList(AttendenceListModel model);
        public Results GetAllEmployees();
        public Result AddUpdateShiftType(ShiftTypeModel shiftTypeModel);
        public Results GetAllShiftTypes(ShiftTypeInputParans model);
        public List<SelectListItem> GetHolidayList(HolidayInputParams model);
        public Result AddUpdateWhatsHappening(WhatsHappening model);
        public WhatsHappeningModel GetWhatsHappenings(WhatsHappening model);
    }
}
