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
using HRMS.Models.ImportFromExcel;
using HRMS.Models.WhatsHappeningModel;
using HRMS.Models.ExportEmployeeExcel;
using Microsoft.AspNetCore.Mvc;
using HRMS.Models.FormPermission;

namespace HRMS.API.BusinessLayer.ITF
{
    public interface IBusinessLayer
    {
        Task<LoginUser> LoginUser(LoginUser loginUser);
        Task<Result> ResetPassword(ResetPasswordModel loginUser);
        Task<Results> GetAllCountries();
        Task<Results> GetAllLanguages();
        Task<Results> GetAllCompanyLanguages(long companyID);
        Task<Results> GetAllCompanyDepartments(long companyID);
        Task<Results> GetAllCompanyEmployeeTypes(long companyID);
        Task<Results> GetAllCurrencies(long companyID);
        Task<Result> AddUpdateEmployee(EmployeeModel model);
        Task<Results> GetAllEmployees(EmployeeInputParams model);
        Task<Results> GetAllActiveEmployees(EmployeeInputParams model);
        Task<Result> AddUpdateTemplate(TemplateModel model);
        Task<Results> GetAllTemplates(TemplateInputParams model);
        Task<Result> AddUpdateCompany(CompanyModel model);
        Task<Results> GetAllCompanies(EmployeeInputParams model);
        Task<LeaveResults> GetlLeavesSummary(MyInfoInputParams model);
        Task<Result> AddUpdateLeave(LeaveSummaryModel leaveSummaryModel);
        Task<LeaveResults> GetLeaveForApprovals(MyInfoInputParams model);
        Task<LeaveResults> GetLeaveDurationTypes(MyInfoInputParams leaveInputParams);
        Task<LeaveResults> GetLeaveTypes(MyInfoInputParams leaveInputParams);
        Task<Result> AddUpdateLeavePolicy(LeavePolicyModel model);
        Task<Results> GetAllLeavePolicies(LeavePolicyInputParans model);
        Task<MyInfoResults> GetMyInfo(MyInfoInputParams model);
        Task<Result> AddUpdateEmploymentDetails(EmploymentDetail employmentDetails);
        Task<EmploymentDetail> GetEmploymentDetailsByEmployee(EmploymentDetailInputParams model);
        Task<Result> AddUpdateEmploymentBankDetails(EmploymentBankDetail employmentBankDetails);
        Task<EmploymentBankDetail> GetEmploymentBankDetails(EmploymentBankDetailInputParams model);
        Task<Result> AddUpdateEmploymentSeparationDetails(EmploymentSeparationDetail employmentSeparationDetails);
        Task<EmploymentSeparationDetail> GetEmploymentSeparationDetails(EmploymentSeparationInputParams model);
        Task<Result> GetFogotPasswordDetails(ChangePasswordModel model);
        Task<Result> AddUpdateHoliday(HolidayModel model);
        Task<Results> GetAllHolidays(HolidayInputParams model);
        Task<DashBoardModel> GetDashBoardModel(DashBoardModelInputParams model);
        Task<Results> GetAllAttendenceList(AttendenceListInputParans model);
        Task<Result> AddUpdateAttendenceList(AttendenceListModel model);
        Task<Results> GetAllEmployees();
        Task<Result> AddUpdateShiftType(ShiftTypeModel shiftTypeModel);
        Task<Results> GetAllShiftTypes(ShiftTypeInputParans model);
        Task<List<SelectListItem>> GetHolidayList(HolidayInputParams model);
        Task<Results> GetAllHolidayList(HolidayInputParams model);
        Task<string> DeleteLeavesSummary(MyInfoInputParams model);
        Task<Result> AddUpdateLeavePolicyDetails(LeavePolicyDetailsModel LeavePolicyModel);
        Task<Results> GetLeavePolicyList(LeavePolicyDetailsInputParams model);
        Task<Results> GetAllLeavePolicyDetails(LeavePolicyDetailsInputParams model);
        Task<string> DeleteLeavePolicyDetails(LeavePolicyDetailsInputParams model);
        Task<Results> GetAllCompaniesList(EmployeeInputParams model);
        Task<Results> GetAllLeavePolicyDetailsByCompanyId(LeavePolicyDetailsInputParams model);
        Task<List<EmployeeDetails>> GetEmployeeListByManagerID(EmployeeInputParams model);
        Task<Result> AddUpdatePolicyCategory(PolicyCategoryModel LeavePolicyModel);
        Task<Results> GetAllPolicyCategory(PolicyCategoryInputParams model);
        Task<Results> GetPolicyCategoryList(PolicyCategoryInputParams model);
        Task<string> DeletePolicyCategory(PolicyCategoryInputParams model);
        Task<List<LeavePolicyDetailsModel>> PolicyCategoryDetails(PolicyCategoryInputParams model);
        Task<EmploymentDetail> GetFilterEmploymentDetailsByEmployee(EmploymentDetailInputParams model);
        Task<L2ManagerDetail> GetL2ManagerDetails(L2ManagerInputParams model);
        Task<AttendanceInputParams> GetAttendance(AttendanceInputParams model);
        Task<MonthlyViewAttendance> GetAttendanceForMonthlyViewCalendar(AttendanceInputParams model);
        Task<AttendanceWithHolidaysVM> GetTeamAttendanceForCalendar(AttendanceInputParams model);
        Task<Result> AddUpdateAttendace(Attendance att);
        Task<Results> GetAttendenceListID(Attendance model);
        Task<AttendanceLogResponse> GetAttendanceDeviceLogs(AttendanceDeviceLog model);
        Task<string> DeleteAttendanceDetails(Attendance model);
        Task<MyAttendanceList> GetMyAttendanceList(AttendanceInputParams model);
        Task<MyAttendanceList> GetAttendanceForApproval(AttendanceInputParams model);
        Task<List<Attendance>> GetApprovedAttendance(AttendanceInputParams model);
        Task<Dictionary<string, long>> GetCountryDictionary();
        Task<Dictionary<string, Dictionary<string, long>>> GetEmploymentDetailsDictionaries(EmploymentDetailInputParams model);
        Task<Dictionary<string, CompanyInfo>> GetCompaniesDictionary();
        Task<Result> AddUpdateEmployeeFromExecel(ImportEmployeeDetail employeeModel);
        Task<Result> AddUpdateEmployeeFromExecelBulk(BulkEmployeeImportModel importDataTable);
        Task<Dictionary<string, long>> GetSubDepartmentDictionary(EmployeeInputParams model);
        Task<Result> AddUpdateWhatsHappeningDetails(WhatsHappeningModels Model);
        Task<Results> GetAllWhatsHappeningDetails(WhatsHappeningModelParans model);
        Task<string> DeleteWhatsHappening(WhatsHappeningModelParans model);
        Task<List<Attendance>> GetManagerApprovedAttendance(AttendanceInputParams model);
        Task<EmployeePersonalDetails> GetEmployeeDetails(EmployeePersonalDetailsById objmodel);
        Task<Results> GetCompaniesLogo(CompanyLoginModel model);
        Task<ReportingStatus> CheckEmployeeReporting(ReportingStatus obj);
        Task<AttendanceDetailsVM> FetchAttendanceHolidayAndLeaveInfo(AttendanceDetailsInputParams model);
        Task<List<ExportEmployeeDetailsExcel>> FetchExportEmployeeExcelSheet(EmployeeInputParams model);
        Task<CompOffValidationResult> GetValidateCompOffLeave(CampOffEligible model);
        Task<UpdateLeaveStatus> UpdateLeaveStatus(UpdateLeaveStatus model);
        Task<List<EducationalDetail>> GetEducationDetails(EducationDetailParams model);
        Task<Result> AddUpdateEducationDetail(EducationalDetail model);
        Task<string> DeleteEducationDetail(EducationDetailParams model);
        Task<List<EmploymentHistory>> GetEmploymentHistory(EmploymentHistoryParams model);
        Task<Result> AddUpdateEmploymentHistory(EmploymentHistory model);
        Task<string> DeleteEmploymentHistory(EmploymentHistoryParams model);
        Task<List<Reference>> GetReferenceDetails(ReferenceParams model);
        Task<Result> AddUpdateReferenceDetail(Reference model);
        Task<string> DeleteReferenceDetail(ReferenceParams model);
        Task<List<FamilyDetail>> GetFamilyDetails(FamilyDetailParams model);
        Task<Result> AddUpdateFamilyDetail(FamilyDetail model);
        Task<string> DeleteFamilyDetail(FamilyDetailParams model);
        Task<List<LanguageDetail>> GetLanguageDetails(LanguageDetailParams model);
        Task<Result> AddUpdateLanguageDetail(LanguageDetail model);
        Task<string> DeleteLanguageDetail(LanguageDetailParams model);
        Task<List<CompOffAttendanceRequestModel>> GetCompOffAttendanceList(CompOffAttendanceInputParams model);
        Task<List<CompOffAttendanceRequestModel>> GetApprovedCompOff(CompOffInputParams model);
        Task<Result> AddUpdateCompOffAttendace(CompOffAttendanceRequestModel att);
        #region Page Permission
        Task<Results> GetAllCompanyFormsPermission(long companyID);
        Task<long> AddFormPermissions(FormPermissionViewModel objmodel);
        Task<List<FormPermissionViewModel>> GetFormByDepartmentID(long DepartmentId);
        Task<List<FormPermissionViewModel>> GetUserFormPermissions(FormPermissionVM objmodel);
        Task<long> AddUserFormPermissions(FormPermissionVM objmodel);
        Task<List<FormPermissionViewModel>> GetUserFormByDepartmentID(FormPermissionVM obj);
        Task<EmployeePermissionVM> CheckUserFormPermissionByEmployeeID(FormPermissionVM obj);
        #endregion Page Permission
        Task<List<Joblcoations>> GetJobLocationsByCompany(Joblcoations model);
   

        #region Exception Handling
        void  InsertException(ExceptionLogModel model);
        #endregion Exception Handling
    }
}
