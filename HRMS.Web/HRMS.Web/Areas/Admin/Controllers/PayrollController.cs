using Microsoft.AspNetCore.Mvc;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using HRMS.Models.Common;
using HRMS.Models.PayRoll;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Microsoft.AspNetCore.Mvc.Razor;
namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        public PayrollController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }

        public IActionResult MonthlySalary()
        {

            return View();
        }

        [HttpPost]
        public JsonResult MonthlySalary(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, string sortCol, string sortDir,
    int year,
    int month)
        {
            var companyId = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            SalaryInputParams salaryInputParams = new SalaryInputParams
            {
                Month = month,
                Year = year,
                DisplayStart = iDisplayStart,
                DisplayLength = iDisplayLength,
                Searching = sSearch,
                SortCol = sortCol,
                SortDir = sortDir,
                CompanyID = companyId,
                EmployeeID = 0
            };
            var data = _businessLayer.SendPostAPIRequest(
                salaryInputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Payroll, APIApiActionConstants.GetEmployeesMonthlySalary),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();
            var model = JsonConvert.DeserializeObject<List<SalaryDetails>>(data);
            if (model.Any())
            {
                model.ForEach(x => { x.EncryptedSalaryID = _businessLayer.EncodeStringBase64(x.MonthlySalaryID.ToString()); });

            }
            return Json(new
            {
                draw = sEcho,
                recordsTotal = model.Count,
                recordsFiltered = model.Count,
                data = model
            });
        }




        [HttpGet]
        public IActionResult SalaryDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("MonthlySalary");
            var monthlySalaryID = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
            SalaryInputParams salaryInputParams = new SalaryInputParams
            {
                CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID)),
                MonthlySalaryID = monthlySalaryID,
                EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)),
                Year = 0,
                Month = 0,
                Searching = null,
                DisplayStart = 0,
                DisplayLength = 1,
                SortCol = null,
                SortDir = null
            };

            var salaryDetailsJson = _businessLayer.SendPostAPIRequest(
                salaryInputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Payroll, APIApiActionConstants.GetEmployeesMonthlySalary),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            if (string.IsNullOrEmpty(salaryDetailsJson))
                return NotFound();
            var salaryDetails = JsonConvert.DeserializeObject<List<SalaryDetails>>(salaryDetailsJson)?.FirstOrDefault();
            if (salaryDetails == null)
                return NotFound();

            return View(salaryDetails);
        }


        public IActionResult EditSalary(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("MonthlySalary");
            var monthlySalaryID = Convert.ToInt64(_businessLayer.DecodeStringBase64(id));
            SalaryInputParams salaryInputParams = new SalaryInputParams
            {
                CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID)),
                MonthlySalaryID = monthlySalaryID,
                EmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)),
                Year = 0,
                Month = 0,
                Searching = null,
                DisplayStart = 0,
                DisplayLength = 1,
                SortCol = null,
                SortDir = null
            };
            // Call API to get salary details
            var salaryDetailsJson = _businessLayer.SendPostAPIRequest(

                    salaryInputParams,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Payroll, APIApiActionConstants.GetEmployeesMonthlySalary),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(salaryDetailsJson))
                return NotFound();

            // Deserialize as a list but pick the first record
            var salaryDetails = JsonConvert.DeserializeObject<List<EmployeeMonthlySalaryModel>>(salaryDetailsJson)?.FirstOrDefault();

            if (salaryDetails == null)
                return NotFound();

            // Set UpdatedByUserID from session
            salaryDetails.UpdatedByUserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            return View(salaryDetails);
        }


        [HttpPost]
        public IActionResult EditSalary(EmployeeMonthlySalaryModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            model.UpdatedByUserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

            var apiResponse = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Payroll, APIApiActionConstants.AddUpdateEmployeeMonthlySalary),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result;
            var result = JsonConvert.DeserializeObject<Result>(apiResponse?.ToString() ?? "{}");
            if (result != null && !string.IsNullOrEmpty(result.Message) &&
                result.Message.Contains("success", StringComparison.OrdinalIgnoreCase))
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = result.Message;
                return RedirectToAction("MonthlySalary");
            }
            else
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
                TempData[HRMS.Models.Common.Constants.toastMessage] = result?.Message ?? "Failed to update salary.";
                return View(model);
            }
        }


      

    }
}
