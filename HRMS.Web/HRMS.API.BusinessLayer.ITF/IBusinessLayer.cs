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
        public Result AddUpdateEmploymentBankDetails(EmploymentBankDetail employmentBankDetails);
        public EmploymentBankDetail GetEmploymentBankDetails(EmploymentBankDetailInputParams model);
        public Result AddUpdateEmploymentSeparationDetails(EmploymentSeparationDetail employmentSeparationDetails);
        public EmploymentSeparationDetail GetEmploymentSeparationDetails(EmploymentSeparationInputParams model);
        public Result GetFogotPasswordDetails(ChangePasswordModel model);
        public Result AddUpdateHoliday(HolidayModel model);
        public Results GetAllHolidays(HolidayInputParams model);
        public DashBoardModel GetDashBoardModel(DashBoardModelInputParams model);
        public Results GetAllAttendenceList(AttendenceListInputParans model);
        public Result AddUpdateAttendenceList(AttendenceListModel model);
        public List<SelectListItem> GetAllEmployeesList(WeekOfEmployeeId Employeemodel);
        public Result AddUpdateShiftType(ShiftTypeModel shiftTypeModel);
        public Results GetAllShiftTypes(ShiftTypeInputParans model);
        public List<SelectListItem> GetHolidayList(HolidayInputParams model);
        public Results GetAllHolidayList(HolidayInputParams model);
        public string DeleteLeavesSummary(MyInfoInputParams model);
        public Result AddUpdateLeavePolicyDetails(LeavePolicyDetailsModel LeavePolicyModel);
        public Results GetLeavePolicyList(LeavePolicyDetailsInputParams model);
        public Results GetAllLeavePolicyDetails(LeavePolicyDetailsInputParams model);
        public string DeleteLeavePolicyDetails(LeavePolicyDetailsInputParams model);
        public Results GetAllCompaniesList(EmployeeInputParams model);
        public Results GetAllLeavePolicyDetailsByCompanyId(LeavePolicyDetailsInputParams model);
        public List<EmployeeDetails> GetEmployeeListByManagerID(EmployeeInputParams model);
        public Result AddUpdatePolicyCategory(PolicyCategoryModel LeavePolicyModel);
        public Results GetAllPolicyCategory(PolicyCategoryInputParams model);
        public Results GetPolicyCategoryList(PolicyCategoryInputParams model);
        public string DeletePolicyCategory(PolicyCategoryInputParams model);
        public List<LeavePolicyDetailsModel> PolicyCategoryDetails(PolicyCategoryInputParams model);
        public EmploymentDetail GetFilterEmploymentDetailsByEmployee(EmploymentDetailInputParams model);
        public L2ManagerDetail GetL2ManagerDetails(L2ManagerInputParams model);
        public AttendanceInputParams GetAttendance(AttendanceInputParams model);
        public MonthlyViewAttendance GetAttendanceForMonthlyViewCalendar(AttendanceInputParams model);
        public AttendanceWithHolidaysVM GetTeamAttendanceForCalendar(AttendanceInputParams model);
        public AttendanceWithHolidaysVM GetExportAttendanceForCalendar(AttendanceInputParams model);
        public Result AddUpdateAttendace(Attendance att);
        public Results GetAttendenceListID(Attendance model);
        public AttendanceLogResponse GetAttendanceDeviceLogs(AttendanceDeviceLog model);
        public string DeleteAttendanceDetails(Attendance model);
        public MyAttendanceList GetMyAttendanceList(AttendanceInputParams model);
        public MyAttendanceList GetAttendanceForApproval(AttendanceInputParams model);
        public List<Attendance> GetApprovedAttendance(AttendanceInputParams model);
        public Dictionary<string, long> GetCountryDictionary();
        public Dictionary<string, Dictionary<string, long>> GetEmploymentDetailsDictionaries(EmploymentDetailInputParams model);
        public Dictionary<string, CompanyInfo> GetCompaniesDictionary();
        public Result AddUpdateEmployeeFromExecel(ImportEmployeeDetail employeeModel);
        public Result AddUpdateEmployeeFromExecelBulk(BulkEmployeeImportModel importDataTable);
        public Dictionary<string, long> GetSubDepartmentDictionary(EmployeeInputParams model);
        public Result AddUpdateWhatsHappeningDetails(WhatsHappeningModels Model);
        public Results GetAllWhatsHappeningDetails(WhatsHappeningModelParans model);
        public string DeleteWhatsHappening(WhatsHappeningModelParans model);
        public List<Attendance> GetManagerApprovedAttendance(AttendanceInputParams model);
        public EmployeePersonalDetails GetEmployeeDetails(EmployeePersonalDetailsById objmodel);
        public Results GetCompaniesLogo(CompanyLoginModel model);
        public ReportingStatus CheckEmployeeReporting(ReportingStatus obj);
        public AttendanceDetailsVM FetchAttendanceHolidayAndLeaveInfo(AttendanceDetailsInputParams model);
        public List<ExportEmployeeDetailsExcel> FetchExportEmployeeExcelSheet(EmployeeInputParams model);
        public CompOffValidationResult GetValidateCompOffLeave(CampOffEligible model);
        public UpdateLeaveStatus UpdateLeaveStatus(UpdateLeaveStatus model);
        public List<EducationalDetail> GetEducationDetails(EducationDetailParams model);
        public Result AddUpdateEducationDetail(EducationalDetail model);
        public string DeleteEducationDetail(EducationDetailParams model);

        public List<EmploymentHistory> GetEmploymentHistory(EmploymentHistoryParams model);
        public Result AddUpdateEmploymentHistory(EmploymentHistory model);
        public string DeleteEmploymentHistory(EmploymentHistoryParams model);
        public List<Reference> GetReferenceDetails(ReferenceParams model);
        public Result AddUpdateReferenceDetail(Reference model);
        public string DeleteReferenceDetail(ReferenceParams model);
        public List<FamilyDetail> GetFamilyDetails(FamilyDetailParams model);
        public Result AddUpdateFamilyDetail(FamilyDetail model);
        public string DeleteFamilyDetail(FamilyDetailParams model);
        public List<LanguageDetail> GetLanguageDetails(LanguageDetailParams model);
        public Result AddUpdateLanguageDetail(LanguageDetail model);
        public string DeleteLanguageDetail(LanguageDetailParams model);
        public List<CompOffAttendanceRequestModel> GetCompOffAttendanceList(CompOffAttendanceInputParams model);
        public List<CompOffAttendanceRequestModel> GetApprovedCompOff(CompOffInputParams model);

        public Result AddUpdateCompOffAttendace(CompOffAttendanceRequestModel att);
        #region Page Permission
        public Results GetAllCompanyFormsPermission(long companyID);
        public long AddFormPermissions(FormPermissionViewModel objmodel);
        public List<FormPermissionViewModel> GetFormByDepartmentID(long DepartmentId);
        public List<FormPermissionViewModel> GetUserFormPermissions(FormPermissionVM objmodel);
        public long AddUserFormPermissions(FormPermissionVM objmodel);
        public List<FormPermissionViewModel> GetUserFormByDepartmentID(FormPermissionVM obj);
        public EmployeePermissionVM CheckUserFormPermissionByEmployeeID(FormPermissionVM obj);
        #endregion Page Permission
        public List<Joblcoations> GetJobLocationsByCompany(Joblcoations model);

        #region Exception Handling
        void InsertException(ExceptionLogModel model);
        #endregion Exception Handling

        public bool UploadRosterWeekOff(WeekOffUploadModelList model);
        public List<WeekOffUploadModel> GetEmployeesWeekOffRoster(WeekOfInputParams model);

        public List<DateTime> GetLeaveWeekOffDates(LeaveWeekOfInputParams model);
        public string DeleteWeekOffRoster(WeekOffUploadDeleteModel model);
        public long GetShiftTypeId(string ShiftTypeName);
        public List<EmployeeShiftModel> GetShiftTypeList(string employeeNumber);
    }
}
