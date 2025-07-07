using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Models.ExportEmployeeExcel;
using HRMS.Models.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HRMS.API.Web.Controllers
{

    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        public EmployeeController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddUpdateEmployee(EmployeeModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateEmployee(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetAllEmployees(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllEmployees(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetAllActiveEmployees(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllActiveEmployees(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetlLeavesSummary(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetlLeavesSummary(model));
            return response;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetLeaveForApprovals(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveForApprovals(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdateLeave(LeaveSummaryModel model)
        {
            IActionResult response = Unauthorized();
            Result result = _businessLayer.AddUpdateLeave(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public IActionResult GetLeaveTypes(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveTypes(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetLeaveDurationTypes(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveDurationTypes(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetMyInfo(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetMyInfo(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdateEmploymentDetails(EmploymentDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmploymentDetails(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetEmploymentDetailsByEmployee(EmploymentDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmploymentDetailsByEmployee(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetFilterEmploymentDetailsByEmployee(EmploymentDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetFilterEmploymentDetailsByEmployee(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetL2ManagerDetails(L2ManagerInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetL2ManagerDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteLeavesSummary(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteLeavesSummary(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetEmployeeListByManagerID(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmployeeListByManagerID(model));
            return response;
        }

        [HttpPost]
        public IActionResult AddUpdatePolicyCategory(PolicyCategoryModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdatePolicyCategory(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetAllPolicyCategory(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllPolicyCategory(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetPolicyCategoryList(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetPolicyCategoryList(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeletePolicyCategory(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeletePolicyCategory(model));
            return response;
        }
        [HttpPost]
        public IActionResult PolicyCategoryDetails(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.PolicyCategoryDetails(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetEmploymentBankDetails(EmploymentBankDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmploymentBankDetails(model));
            return response;
        }


        [HttpPost]
        public IActionResult AddUpdateEmploymentBankDetails(EmploymentBankDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmploymentBankDetails(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetEmploymentSeparationDetails(EmploymentSeparationInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmploymentSeparationDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult AddUpdateEmploymentSeparationDetails(EmploymentSeparationDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmploymentSeparationDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult CheckEmployeeReporting(ReportingStatus obj)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.CheckEmployeeReporting(obj));
            return response;
        }

        [HttpPost]
        public IActionResult FetchExportEmployeeExcelSheet(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.FetchExportEmployeeExcelSheet(model));
            return response;
        }


        [HttpPost]
        public IActionResult GetValidateCompOffLeave(CampOffEligible model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetValidateCompOffLeave(model));
            return response;
        }

        [HttpPost]
        public IActionResult UpdateLeaveStatus(UpdateLeaveStatus model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.UpdateLeaveStatus(model));
            return response;
        }


        [HttpPost]
        public IActionResult GetEducationDetails(EducationDetailParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEducationDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult AddUpdateEducationDetail(EducationalDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEducationDetail(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteEducationDetail(EducationDetailParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteEducationDetail(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetEmploymentHistory(EmploymentHistoryParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmploymentHistory(model));
            return response;
        }
        [HttpPost]
        public IActionResult AddUpdateEmploymentHistory(EmploymentHistory model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateEmploymentHistory(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteEmploymentHistory(EmploymentHistoryParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteEmploymentHistory(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetReferenceDetails(ReferenceParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetReferenceDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult AddUpdateReferenceDetail(Reference model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateReferenceDetail(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteReferenceDetail(ReferenceParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteReferenceDetail(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetFamilyDetails(FamilyDetailParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetFamilyDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult AddUpdateFamilyDetail(FamilyDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateFamilyDetail(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteFamilyDetail(FamilyDetailParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteFamilyDetail(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetLanguageDetails(LanguageDetailParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLanguageDetails(model));
            return response;
        }
        [HttpPost]
        public IActionResult AddUpdateLanguageDetail(LanguageDetail model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.AddUpdateLanguageDetail(model));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteLanguageDetail(LanguageDetailParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteLanguageDetail(model));
            return response;
        }
        [HttpPost]
        public IActionResult GetEmployeesWeekOffRoster(WeekOfInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetEmployeesWeekOffRoster(model));
            return response;
        }

        [HttpPost]
        public IActionResult GetAllEmployeesList(WeekOfEmployeeId Employeemodel)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetAllEmployeesList(Employeemodel));
            return response;
        }
        [HttpPost]
        public IActionResult DeleteWeekOffRoster(WeekOffUploadDeleteModel model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.DeleteWeekOffRoster(model));
            return response;
        }
        [HttpPost]
        public IActionResult UploadRosterWeekOff(WeekOffUploadModelList model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.UploadRosterWeekOff(model));
            return response;
        }

        [HttpGet]
        public IActionResult GetShiftTypeId(string ShiftTypeName)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetShiftTypeId(ShiftTypeName));
            return response;
        }
        [HttpGet]
        public IActionResult GetShiftTypeList(string employeeNumber)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetShiftTypeList(employeeNumber));
            return response;
        }

        [HttpPost]
        public IActionResult GetLeaveWeekOffDates(LeaveWeekOfInputParams model)
        {
            IActionResult response = Unauthorized();
            response = Ok(_businessLayer.GetLeaveWeekOffDates(model));
            return response;
        }

    }
}
