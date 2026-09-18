using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.EmployeeOnboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]

    public class EmployeeOnboarding : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;

        public EmployeeOnboarding(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult StartOnboarding(EmployeeOnboardingViewModel model)
        {


            var response = Ok(
                _businessLayer.StartOnboarding(model)
            );

            return response;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetEmployeeDetailsOnboarding(EmployeeOnboardingInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.GetEmployeeDetailsOnboarding(
                    model.OnboardingID
                )
            );

            return response;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult AddUpdateEmployeeDetailsOnboarding(
            [FromBody] EmployeeDetailsOnboardingModel model)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Employee onboarding details are required."
                });
            }

            return Ok(
                _businessLayer.AddUpdateEmployeeDetailsOnboarding(model)
            );
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetEmployeeOnboardingFamily(EmployeeOnboardingInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.GetEmployeeOnboardingFamily(
                    model.OnboardingID
                )
            );

            return response;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult AddUpdateEmployeeOnboardingFamily(EmployeeOnboardingFamilyModel model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.AddUpdateEmployeeOnboardingFamily(model)
            );

            return response;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetEmployeeOnboardingEducation(EmployeeOnboardingInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.GetEmployeeOnboardingEducation(
                    model.OnboardingID
                )
            );

            return response;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult AddUpdateEmployeeOnboardingEducation(EmployeeOnboardingEducationModel model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.AddUpdateEmployeeOnboardingEducation(model)
            );

            return response;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetEmployeeOnboardingEmployment(EmployeeOnboardingInputParams model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.GetEmployeeOnboardingEmployment(
                    model.OnboardingID
                )
            );

            return response;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult AddUpdateEmployeeOnboardingEmployment(
            EmployeeOnboardingEmploymentModel model)
        {
            IActionResult response = Unauthorized();

            response = Ok(
                _businessLayer.AddUpdateEmployeeOnboardingEmployment(model)
            );

            return response;
        }
        [HttpPost]
        public IActionResult GetEmployeeOnboardingRequests(
    EmployeeOnboardingRequestInputParams model)
        {
            var result =
                _businessLayer.GetEmployeeOnboardingRequests(model);

            return Ok(result);
        }
    }
}
