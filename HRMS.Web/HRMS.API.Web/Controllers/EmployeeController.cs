using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Employee;
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
        public async Task<IActionResult> AddUpdateEmployee(EmployeeModel model)
        {
            IActionResult response = Unauthorized();
            Result result = await _businessLayer.AddUpdateEmployee(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllEmployees(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllEmployees(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllActiveEmployees(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllActiveEmployees(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetlLeavesSummary(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetlLeavesSummary(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetLeaveForApprovals(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetLeaveForApprovals(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateLeave(LeaveSummaryModel model)
        {
            IActionResult response = Unauthorized();
            Result result = await _businessLayer.AddUpdateLeave(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetLeaveTypes(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetLeaveTypes(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetLeaveDurationTypes(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetLeaveDurationTypes(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetMyInfo(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetMyInfo(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateEmploymentDetails(EmploymentDetail model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateEmploymentDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetEmploymentDetailsByEmployee(EmploymentDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetEmploymentDetailsByEmployee(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetFilterEmploymentDetailsByEmployee(EmploymentDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetFilterEmploymentDetailsByEmployee(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetL2ManagerDetails(L2ManagerInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetL2ManagerDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLeavesSummary(MyInfoInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteLeavesSummary(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeeListByManagerID(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetEmployeeListByManagerID(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdatePolicyCategory(PolicyCategoryModel model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdatePolicyCategory(model);
            response = Ok(result);
            return response;
        }


        [HttpPost]
        public async Task<IActionResult> GetAllPolicyCategory(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetAllPolicyCategory(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetPolicyCategoryList(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetPolicyCategoryList(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeletePolicyCategory(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeletePolicyCategory(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> PolicyCategoryDetails(PolicyCategoryInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.PolicyCategoryDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetEmploymentBankDetails(EmploymentBankDetailInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetEmploymentBankDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateEmploymentBankDetails(EmploymentBankDetail model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateEmploymentBankDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetEmploymentSeparationDetails(EmploymentSeparationInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetEmploymentSeparationDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateEmploymentSeparationDetails(EmploymentSeparationDetail model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateEmploymentSeparationDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmployeeReporting(ReportingStatus obj)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.CheckEmployeeReporting(obj);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> FetchExportEmployeeExcelSheet(EmployeeInputParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.FetchExportEmployeeExcelSheet(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetValidateCompOffLeave(CampOffEligible model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetValidateCompOffLeave(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLeaveStatus(UpdateLeaveStatus model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.UpdateLeaveStatus(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetEducationDetails(EducationDetailParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetEducationDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateEducationDetail(EducationalDetail model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateEducationDetail(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEducationDetail(EducationDetailParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteEducationDetail(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetEmploymentHistory(EmploymentHistoryParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetEmploymentHistory(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateEmploymentHistory(EmploymentHistory model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateEmploymentHistory(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmploymentHistory(EmploymentHistoryParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteEmploymentHistory(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetReferenceDetails(ReferenceParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetReferenceDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateReferenceDetail(Reference model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateReferenceDetail(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReferenceDetail(ReferenceParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteReferenceDetail(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetFamilyDetails(FamilyDetailParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetFamilyDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateFamilyDetail(FamilyDetail model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateFamilyDetail(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFamilyDetail(FamilyDetailParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteFamilyDetail(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> GetLanguageDetails(LanguageDetailParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.GetLanguageDetails(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateLanguageDetail(LanguageDetail model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.AddUpdateLanguageDetail(model);
            response = Ok(result);
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLanguageDetail(LanguageDetailParams model)
        {
            IActionResult response = Unauthorized();
            var result = await _businessLayer.DeleteLanguageDetail(model);
            response = Ok(result);
            return response;
        }
    }
}
