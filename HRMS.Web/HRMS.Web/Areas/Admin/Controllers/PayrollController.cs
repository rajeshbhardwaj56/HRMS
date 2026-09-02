using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.PayRoll;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Playwright;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using HRMSResults = HRMS.Models.Common.Results;
namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        private readonly IS3Service _s3Service;
        private readonly SalarySlipPdfService _salarySlipPdfService;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICutoffSettingsService _cutoffSettingsService;

        public PayrollController(
            IConfiguration configuration,
            IBusinessLayer businessLayer,
            IS3Service s3Service,
            SalarySlipPdfService salarySlipPdfService,
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IWebHostEnvironment webHostEnvironment, ICutoffSettingsService cutoffSettingsService
            )
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
            _s3Service = s3Service;
            _salarySlipPdfService = salarySlipPdfService;
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _webHostEnvironment = webHostEnvironment;
            _cutoffSettingsService = cutoffSettingsService; ;

        }
        [HttpGet]
        public IActionResult MonthlySalary()
        {
            // ============================================
            // GET CUTOFF SETTINGS
            // ============================================

            var cutoffSettings = _cutoffSettingsService.GetCutoffSettings(
                HttpContext.Session.GetString(
                    Constants.SessionBearerToken));

            // ============================================
            // PASS SETTINGS TO VIEW
            // ============================================

            ViewBag.ShowAutoCalculateMonthSalary =
                cutoffSettings.ShowAutoCalculateMonthSalary;



            return View();
        }

        [HttpPost]
        public JsonResult MonthlySalary(
            string sEcho,
            int iDisplayStart,
            int iDisplayLength,
            string sSearch,
            string sortCol,
            string sortDir,
            int year,
            int month)
        {
            var companyId = Convert.ToInt64(
                HttpContext.Session.GetString(Constants.CompanyID)
            );

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
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Payroll,
                    APIApiActionConstants.GetEmployeesMonthlySalary
                ),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result.ToString();

            var model = JsonConvert.DeserializeObject<List<SalaryDetails>>(data);

            if (model.Any())
            {
                model.ForEach(x =>
                {
                    x.EncryptedSalaryID =
                        _businessLayer.EncodeStringBase64(x.SalaryID.ToString());
                });
            }

            // Get total counts from stored procedure
            var totalRecords = model.FirstOrDefault()?.TotalRecords ?? 0;
            var filteredRecords = model.FirstOrDefault()?.FilteredRecords ?? 0;

            return Json(new
            {
                draw = sEcho,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = model
            });
        }



        [HttpGet]
        public JsonResult GetEmployeeSalaryMonths(long employeeId)
        {
            var request = new EmployeeSalaryMonthRequestModel
            {
                EmployeeID = employeeId
            };

            var apiResponse = _businessLayer.SendPostAPIRequest(
                request,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Payroll,
                    APIApiActionConstants.GetEmployeeSalaryMonths
                ),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(apiResponse))
                return Json(null);

            var salaryMonths =
                JsonConvert.DeserializeObject<List<EmployeeSalaryMonth>>(apiResponse);

            return Json(salaryMonths);
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

        public IActionResult CalculateSalary(string id)
        {
            EmployeeSalaryRequestModel model = new EmployeeSalaryRequestModel();

            if (!string.IsNullOrEmpty(id))
            {
                long salaryId = Convert.ToInt64(
                    _businessLayer.DecodeStringBase64(id)
                );

                var companyId = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.CompanyID)
                );

                SalaryInputParams param = new SalaryInputParams
                {
                    SalaryID = salaryId,
                    CompanyID = companyId
                };

                var data = _businessLayer.SendPostAPIRequest(
                    param,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Payroll,
                        APIApiActionConstants.GetEmployeesMonthlySalary
                    ),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result?.ToString();

                if (!string.IsNullOrEmpty(data))
                {
                    var result =
                        JsonConvert.DeserializeObject<List<SalaryDetails>>(data);

                    if (result != null && result.Any())
                    {
                        var salary = result.First();

                        model = new EmployeeSalaryRequestModel
                        {
                            // ------------------------------------------------
                            // BASIC INFORMATION
                            // ------------------------------------------------
                            SalaryID = salary.SalaryID,
                            EmployeeID = salary.EmployeeID,
                            EmployeeNumber = salary.EmployeeNumber,
                            EmployeeName = salary.EmployeeName,

                            PayrollTypeID = salary.PayrollTypeID,
                            Year = salary.SalaryYear,
                            Month = salary.SalaryMonth,

                            // ------------------------------------------------
                            // SALARY
                            // ------------------------------------------------
                            RevisedGross = salary.RevisedGross,
                            MonthDays = salary.MonthDays,
                            PayableDays = salary.PayableDays,

                            // ------------------------------------------------
                            // FIXED SALARY
                            // ------------------------------------------------
                            BasicFixed = salary.BasicFixed,
                            HRAFixed = salary.HRAFixed,
                            ConveyanceFixed = salary.ConveyanceFixed,
                            SpecialAllowanceFixed = salary.SpecialAllowanceFixed,
                            GrossSalaryFixed = salary.GrossSalaryFixed,

                            // ------------------------------------------------
                            // PAYABLE SALARY
                            // ------------------------------------------------
                            BasicPayable = salary.BasicPayable,
                            HRAPayable = salary.HRAPayable,
                            ConveyancePayable = salary.ConveyancePayable,
                            SpecialAllowancePayable = salary.SpecialAllowancePayable,
                            GrossSalaryPayable = salary.GrossSalaryPayable,

                            // ------------------------------------------------
                            // EARNINGS
                            // ------------------------------------------------
                            ClientIncentive = salary.ClientIncentive,
                            PLI = salary.PLI,
                            FloorIncentive = salary.FloorIncentive,
                            EmpReferal = salary.EmpReferal,
                            TrainingFee = salary.TrainingFee,
                            GWR = salary.GWR,
                            OtherAdditonArrear = salary.OtherAdditonArrear,

                            // ------------------------------------------------
                            // EMPLOYEE DEDUCTIONS
                            // ------------------------------------------------
                            EMPLWF = salary.EMPLWF,
                            TDS = salary.TDS,
                            DbtDeduction = salary.DbtDeduction,
                            Advanceded = salary.Advanceded,
                            InsuranceDeduction = salary.InsuranceDeduction,
                            OtherDeduction = salary.OtherDeduction,

                            // ------------------------------------------------
                            // EMPLOYEE STATUTORY
                            // ------------------------------------------------
                            EMPPF = salary.EMPPF,
                            EMPESI = salary.EMPESI,
                            PTAX = salary.PTAX,

                            // ------------------------------------------------
                            // TOTAL
                            // ------------------------------------------------
                            TotalDeduction = salary.TotalDeduction,
                            NetPayable = salary.NetPayable,

                            // ------------------------------------------------
                            // EMPLOYER CONTRIBUTION
                            // ------------------------------------------------
                            EmployerPF = salary.EmployerPF,
                            EmployerESI = salary.EmployerESI,
                            EmployerLWF = salary.EmployerLWF,
                            TotalEmployerContribution =
                                salary.TotalEmployerContribution,

                            // ------------------------------------------------
                            // EPF / EPS / EDLI
                            // ------------------------------------------------
                            EPFWages = salary.EPFWages,
                            EPSWages = salary.EPSWages,
                            EDLIWages = salary.EDLIWages,
                            EPFAdminCharges = salary.EPFAdminCharges,
                            EDLIContribution = salary.EDLIContribution,
                            EDLIAdminCharges = salary.EDLIAdminCharges,

                            // ------------------------------------------------
                            // CTC
                            // ------------------------------------------------
                            CTC = salary.CTC
                        };
                    }
                }
            }

            return View(model);
        }


        [HttpPost]
        public JsonResult CalculateSalary(EmployeeSalaryRequestModel model)
        {
            // =========================================
            // CHECK SALARY CALCULATION CUTOFF
            // =========================================

            var cutoffSettings = _cutoffSettingsService.GetCutoffSettings(
                HttpContext.Session.GetString(Constants.SessionBearerToken));

            DateTime? salaryCalculationCutoffDate =
                cutoffSettings.SalaryCalculationCutoffDate;

            if (salaryCalculationCutoffDate.HasValue)
            {
                // Convert cutoff date to Year + Month
                int cutoffYear = salaryCalculationCutoffDate.Value.Year;
                int cutoffMonth = salaryCalculationCutoffDate.Value.Month;

                // Compare selected Salary Year + Month
                // First compare Year, then Month
                bool isBeforeCutoff =
                    model.Year < cutoffYear ||
                    (model.Year == cutoffYear &&
                     model.Month < cutoffMonth);

                if (isBeforeCutoff)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            $"Salary calculation for " +
                            $"{new DateTime(model.Year, model.Month, 1):MMMM yyyy} " +
                            $"is locked. Salary calculation is allowed from " +
                            $"{new DateTime(cutoffYear, cutoffMonth, 1):MMMM yyyy} onward."
                    });
                }
            }
            model.InsertedByUserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

            var apiResponse = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Payroll, APIApiActionConstants.CalculateEmployeeSalary),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(apiResponse))
                return Json(null);

            var salary = JsonConvert.DeserializeObject<EmployeeSalaryCalculationModel>(apiResponse);

            return Json(salary);
        }

        [HttpPost]
        public JsonResult GetSalaryDetails(EmployeeSalaryGetRequestModel request)
        {
            var apiResponse = _businessLayer.SendPostAPIRequest(
                request,
                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Payroll, APIApiActionConstants.GetEmployeeSalary),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(apiResponse))
                return Json(null);

            var salary = JsonConvert.DeserializeObject<EmployeeSalaryCalculationModel>(apiResponse);

            return Json(salary);
        }
        [HttpPost]
        public JsonResult GetPayrollPeriodDetails(PayrollPeriodRequestModel request)
        {
            var apiResponse = _businessLayer.SendPostAPIRequest(
                request,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Payroll,
                    APIApiActionConstants.GetPayrollPeriodDetails
                ),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(apiResponse))
                return Json(null);

            var payrollPeriod =
                JsonConvert.DeserializeObject<PayrollPeriodDetailsModel>(apiResponse);

            return Json(payrollPeriod);
        }
        [HttpPost]
        public JsonResult GetPayrollPeriodsForDropdown()
        {
            var apiResponse = _businessLayer.SendPostAPIRequest(
                null,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Payroll,
                    APIApiActionConstants.GetPayrollPeriodsForDropdown
                ),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(apiResponse))
                return Json(new List<PayrollPeriodDropdownModel>());

            var payrollPeriods =
                JsonConvert.DeserializeObject<List<PayrollPeriodDropdownModel>>(
                    apiResponse
                );

            return Json(payrollPeriods);
        }
        [HttpPost]

        public JsonResult SaveSalary([FromBody] EmployeeSalaryRequestModel model)
        {
            // Get cutoff settings
            var cutoffSettings = _cutoffSettingsService.GetCutoffSettings(
                HttpContext.Session.GetString(Constants.SessionBearerToken));

            DateTime? salaryCalculationCutoffDate =
                cutoffSettings.SalaryCalculationCutoffDate;

            // Check salary calculation cutoff
            if (salaryCalculationCutoffDate.HasValue)
            {
                int cutoffYear = salaryCalculationCutoffDate.Value.Year;
                int cutoffMonth = salaryCalculationCutoffDate.Value.Month;

                // Check whether selected salary Year + Month is before cutoff
                bool isBeforeCutoff =
                    model.Year < cutoffYear ||
                    (model.Year == cutoffYear &&
                     model.Month < cutoffMonth);

                if (isBeforeCutoff)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            $"Salary calculation for " +
                            $"{new DateTime(model.Year, model.Month, 1):MMMM yyyy} " +
                            $"is locked. Salary calculation is allowed from " +
                            $"{new DateTime(cutoffYear, cutoffMonth, 1):MMMM yyyy} onward."
                    });
                }
            }

            model.InsertedByUserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
            var apiResponse = _businessLayer.SendPostAPIRequest(
        model,
        _businessLayer.GetFormattedAPIUrl(
            APIControllarsConstants.Payroll,
            APIApiActionConstants.SaveEmployeeSalary
        ),
        HttpContext.Session.GetString(Constants.SessionBearerToken),
        true
    ).Result?.ToString();
            if (string.IsNullOrEmpty(apiResponse))
            {
                return Json(new { success = false, message = "No response from API" });
            }
            var result = JsonConvert.DeserializeObject<Result>(apiResponse);

            return Json(new
            {
                success = result != null && result.PKNo.HasValue && result.PKNo.Value > 0,
                message = result?.Message ?? "Failed to save salary",
                pkNo = result?.PKNo
            });
        }


        [HttpPost]
        public JsonResult GetSalarySlipSettings(
            SalarySlipSettingsInputParams request)
        {
            var apiResponse = _businessLayer.SendPostAPIRequest(
                request,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Payroll,
                    APIApiActionConstants.GetSalarySlipSettings
                ),
                HttpContext.Session.GetString(
                    Constants.SessionBearerToken
                ),
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(apiResponse))
            {
                return Json(new
                {
                    success = false,
                    message = "No response received."
                });
            }

            var result = JsonConvert.DeserializeObject<HRMSResults>(apiResponse);

            if (result == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unable to load salary slip templates."
                });
            }

            return Json(new
            {
                success = true,
                templates = result.SalarySlipSettingsList ?? new List<SalarySlipSettingsModel>()
            });
        }

        [HttpGet]
        public IActionResult SalarySlipTemplate(long id = 0)
        {
            long companyID = Convert.ToInt64(
                HttpContext.Session.GetString(Constants.CompanyID)
            );

            SalarySlipSettingsModel model = new SalarySlipSettingsModel
            {
                CompanyID = companyID
            };

            if (id > 0)
            {
                var request = new SalarySlipSettingsInputParams
                {
                    CompanyID = companyID,
                    SalarySlipSettingID = id
                };

                var apiResponse = _businessLayer.SendPostAPIRequest(
                    request,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Payroll,
                        APIApiActionConstants.GetSalarySlipSettings
                    ),
                    HttpContext.Session.GetString(
                        Constants.SessionBearerToken
                    ),
                    true
                ).Result?.ToString();

                if (!string.IsNullOrEmpty(apiResponse))
                {
                    var result =
                        JsonConvert.DeserializeObject<HRMSResults>(
                            apiResponse
                        );

                    if (result != null &&
                        result.SalarySlipSettingsList != null &&
                        result.SalarySlipSettingsList.Count > 0)
                    {
                        model =
                            result.SalarySlipSettingsList
                                .FirstOrDefault();

                        // Keep CompanyID
                        model.CompanyID = companyID;
                    }
                }
            }

            // Convert S3 key to URL for displaying existing logo
            if (!string.IsNullOrEmpty(model.LogoPath))
            {
                model.LogoPath =
                    _s3Service.GetFileUrl(model.LogoPath);
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult SaveSalarySlipSettings(
    SalarySlipSettingsModel model,
    List<IFormFile> CompanyLogo)
        {
            try
            {
                // =====================================================
                // COMPANY ID
                // =====================================================

                model.CompanyID = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.CompanyID)
                );


                // =====================================================
                // USER ID
                // =====================================================

                model.UserID = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.EmployeeID)
                );


                // =====================================================
                // LOGO UPLOAD
                // =====================================================

                _s3Service.ProcessFileUpload(
                    CompanyLogo,
                    model.LogoPath,
                    out string newLogoKey
                );


                // =====================================================
                // NEW LOGO UPLOADED
                // =====================================================

                if (!string.IsNullOrEmpty(newLogoKey))
                {
                    // Delete old logo if it exists

                    if (!string.IsNullOrEmpty(model.LogoPath))
                    {
                        var oldLogoKey =
                            _s3Service.ExtractKeyFromUrl(
                                model.LogoPath
                            );

                        if (!string.IsNullOrEmpty(oldLogoKey))
                        {
                            _s3Service.DeleteFile(oldLogoKey);
                        }
                    }


                    // Save new S3 key

                    model.LogoPath = newLogoKey;
                }


                // =====================================================
                // NO NEW LOGO
                // KEEP EXISTING LOGO
                // =====================================================

                else
                {
                    model.LogoPath =
                        _s3Service.ExtractKeyFromUrl(
                            model.LogoPath
                        );
                }


                // =====================================================
                // CALL API
                // =====================================================

                var apiResponse =
                    _businessLayer.SendPostAPIRequest(
                        model,
                        _businessLayer.GetFormattedAPIUrl(
                            APIControllarsConstants.Payroll,
                            APIApiActionConstants.AddUpdateSalarySlipSettings
                        ),
                        HttpContext.Session.GetString(
                            Constants.SessionBearerToken
                        ),
                        true
                    ).Result?.ToString();


                // =====================================================
                // CHECK API RESPONSE
                // =====================================================

                if (string.IsNullOrEmpty(apiResponse))
                {
                    TempData[
                        HRMS.Models.Common.Constants.toastType
                    ] =
                        HRMS.Models.Common.Constants.toastTypeError;

                    TempData[
                        HRMS.Models.Common.Constants.toastMessage
                    ] =
                        "No response from API.";

                    return RedirectToAction(
                        "SalarySlipTemplate",
                        "Payroll",
                        new
                        {
                            area = "Admin",
                            id = model.SalarySlipSettingID
                        }
                    );
                }


                var result =
                    JsonConvert.DeserializeObject<Result>(
                        apiResponse
                    );


                // =====================================================
                // SUCCESS
                // =====================================================

                if (result != null &&
                    result.PKNo.HasValue &&
                    result.PKNo.Value > 0)
                {
                    TempData[
                        HRMS.Models.Common.Constants.toastType
                    ] =
                        HRMS.Models.Common.Constants.toastTypeSuccess;

                    TempData[
                        HRMS.Models.Common.Constants.toastMessage
                    ] =
                        model.SalarySlipSettingID > 0
                            ? "Salary slip template updated successfully."
                            : "Salary slip template saved successfully.";

                    return RedirectToAction(
                        "SalarySlipTemplate",
                        "Payroll",
                        new
                        {
                            area = "Admin",
                            id = 0
                        }
                    );
                }


                // =====================================================
                // ERROR
                // =====================================================

                TempData[
                    HRMS.Models.Common.Constants.toastType
                ] =
                    HRMS.Models.Common.Constants.toastTypeError;

                TempData[
                    HRMS.Models.Common.Constants.toastMessage
                ] =
                    result?.Message ??
                    "Something went wrong.";

                return RedirectToAction(
                    "SalarySlipTemplate",
                    "Payroll",
                    new
                    {
                        area = "Admin",
                        id = model.SalarySlipSettingID
                    }
                );
            }
            catch (Exception ex)
            {
                TempData[
                    HRMS.Models.Common.Constants.toastType
                ] =
                    HRMS.Models.Common.Constants.toastTypeError;

                TempData[
                    HRMS.Models.Common.Constants.toastMessage
                ] =
                    ex.Message;

                return RedirectToAction(
                    "SalarySlipTemplate",
                    "Payroll",
                    new
                    {
                        area = "Admin",
                        id = model.SalarySlipSettingID
                    }
                );
            }
        }
        [HttpPost]
        public async Task<IActionResult> SalarySlipPreview(
            EmployeeSalaryGetRequestModel request)
        {
            try
            {
                var model = await GetSalarySlipModel(request);

                if (model == null)
                {
                    return Content(
                        "<div class='alert alert-danger m-3'>" +
                        "Salary record not found." +
                        "</div>",
                        "text/html"
                    );
                }

                return PartialView("_SalarySlip", model);
            }
            catch (Exception ex)
            {
                // log exception

                return Content(
                    "<div class='alert alert-danger m-3'>" +
                    "Unable to generate salary slip." +
                    "</div>",
                    "text/html"
                );
            }
        }
        [HttpPost]
        public async Task<IActionResult> SalarySlipPdf(
    EmployeeSalaryGetRequestModel request)
        {
            try
            {
                EmployeeSalaryRequestModel model =
                    await GetSalarySlipModel(request);

                if (model == null || model.EmployeeID <= 0)
                {
                    return BadRequest("Salary record not found.");
                }

                var html = await RenderViewToStringAsync(
                    "_SalarySlip",
                    model
                );

                using var playwright =
                    await Playwright.CreateAsync();

                var browserOptions = new BrowserTypeLaunchOptions
                {
                    Headless = true
                };

                var browserPath =
                    _configuration["Playwright:ExecutablePath"];

                if (!string.IsNullOrWhiteSpace(browserPath))
                {
                    browserOptions.ExecutablePath = browserPath;
                }

                await using var browser =
                    await playwright.Chromium.LaunchAsync(browserOptions);

                var page =
                    await browser.NewPageAsync();

                await page.SetContentAsync(
                    html,
                    new PageSetContentOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle
                    }
                );

                var pdf = await page.PdfAsync(
                    new PagePdfOptions
                    {
                        Format = "A4",
                        PrintBackground = true,
                        PreferCSSPageSize = true,

                        Margin = new Margin
                        {
                            Top = "0",
                            Bottom = "0",
                            Left = "0",
                            Right = "0"
                        }
                    }
                );

                var fileName =
                    $"SalarySlip_{model.EmployeeName}_{model.MonthName}_{model.Year}.pdf";

                return File(
                    pdf,
                    "application/pdf",
                    fileName
                );
            }
            catch (Exception ex)
            {
                // log ex

                return BadRequest(
                    "Unable to generate salary slip PDF."
                );
            }
        }
        private async Task<EmployeeSalaryRequestModel> GetSalarySlipModel(
            EmployeeSalaryGetRequestModel request)
        {
            var model = new EmployeeSalaryRequestModel();

            var data = _businessLayer.SendPostAPIRequest(
                request,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Payroll,
                    APIApiActionConstants.GetEmployeeSalary
                ),
                HttpContext.Session.GetString(
                    Constants.SessionBearerToken
                ),
                true
            ).Result?.ToString();

            if (string.IsNullOrEmpty(data))
                return null;

            var salary =
                JsonConvert.DeserializeObject<EmployeeSalaryCalculationModel>(
                    data
                );

            if (salary == null)
                return null;

            model = new EmployeeSalaryRequestModel
            {
                EmployeeID = salary.EmployeeID,
                EmployeeNumber = salary.EmployeeNumber,
                EmployeeName = salary.EmployeeName,
                PayrollTypeID = salary.PayrollTypeID,

                Year = salary.SalaryYear,
                Month = salary.SalaryMonth,

                RevisedGross = salary.RevisedGross,
                MonthDays = salary.MonthDays,
                PayableDays = salary.PayableDays,

                BasicFixed = salary.BasicFixed,
                HRAFixed = salary.HRAFixed,
                ConveyanceFixed = salary.ConveyanceFixed,
                SpecialAllowanceFixed = salary.SpecialAllowanceFixed,
                GrossSalaryFixed = salary.GrossSalaryFixed,

                BasicPayable = salary.BasicPayable,
                HRAPayable = salary.HRAPayable,
                ConveyancePayable = salary.ConveyancePayable,
                SpecialAllowancePayable =
                    salary.SpecialAllowancePayable,
                GrossSalaryPayable = salary.GrossSalaryPayable,

                ClientIncentive = salary.ClientIncentive,
                PLI = salary.PLI,
                FloorIncentive = salary.FloorIncentive,
                EmpReferal = salary.EmpReferal,
                TrainingFee = salary.TrainingFee,
                GWR = salary.GWR,
                OtherAdditonArrear = salary.OtherAdditonArrear,

                EMPLWF = salary.EMPLWF,
                TDS = salary.TDS,
                DbtDeduction = salary.DbtDeduction,
                Advanceded = salary.Advanceded,
                InsuranceDeduction = salary.InsuranceDeduction,
                OtherDeduction = salary.OtherDeduction,

                EMPPF = salary.EMPPF,
                EMPESI = salary.EMPESI,
                PTAX = salary.PTAX,

                TotalDeduction = salary.TotalDeduction,
                NetPayable = salary.NetPayable,

                EmployerPF = salary.EmployerPF,
                EmployerESI = salary.EmployerESI,
                EmployerLWF = salary.EmployerLWF,
                TotalEmployerContribution =
                    salary.TotalEmployerContribution,

                EPFWages = salary.EPFWages,
                EPSWages = salary.EPSWages,
                EDLIWages = salary.EDLIWages,
                EPFAdminCharges = salary.EPFAdminCharges,
                EDLIContribution = salary.EDLIContribution,
                EDLIAdminCharges = salary.EDLIAdminCharges,

                CTC = salary.CTC,

                BankAccountNumber = salary.BankAccountNumber,
                BankName = salary.BankName,
                Designation = salary.Designation,
                Department = salary.Department,
                Location = salary.Location,
                DateOfJoining = salary.DateOfJoining,
                MonthName = salary.MonthName,
                NetPayableInWords = salary.NetPayableInWords,
                OfficialEmail = salary.OfficialEmail,
                CompanyLogo = await ConvertImageToBase64Async(
        HttpContext.Session.GetString(
            Constants.CompanyLogo
        )

    )
            };

            return model;
        }
        private async Task<string> RenderViewToStringAsync(
    string viewName,
    object model)
        {
            var actionContext = new ActionContext(
                HttpContext,
                RouteData,
                ControllerContext.ActionDescriptor,
                ModelState
            );

            var viewEngineResult = _razorViewEngine.FindView(
                actionContext,
                viewName,
                false
            );

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(
                    $"View '{viewName}' could not be found."
                );
            }

            var view = viewEngineResult.View;

            await using var sw = new StringWriter();

            var viewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(
                HttpContext,
                _tempDataProvider
            );

            var viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await view.RenderAsync(viewContext);

            return sw.ToString();
        }
        private async Task<string> ConvertImageToBase64Async(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return string.Empty;

            try
            {
                // Already Base64
                if (imageUrl.StartsWith(
                    "data:image/",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return imageUrl;
                }

                // -----------------------------------------
                // Build absolute URL
                // -----------------------------------------

                var request = HttpContext.Request;

                var absoluteUrl =
                    $"{request.Scheme}://{request.Host}{imageUrl}";

                // -----------------------------------------
                // Download image
                // -----------------------------------------

                using var httpClient = new HttpClient();

                // If your Document/GetFile requires
                // authentication, forward the token/cookie
                var bearerToken = HttpContext.Session.GetString(
                    Constants.SessionBearerToken
                );

                if (!string.IsNullOrWhiteSpace(bearerToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            bearerToken
                        );
                }

                var response =
                    await httpClient.GetAsync(absoluteUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                var bytes =
                    await response.Content.ReadAsByteArrayAsync();

                if (bytes.Length == 0)
                    return string.Empty;

                // -----------------------------------------
                // Determine content type
                // -----------------------------------------

                var contentType =
                    response.Content.Headers.ContentType?.MediaType;

                if (string.IsNullOrWhiteSpace(contentType))
                {
                    contentType = "image/webp";
                }

                // -----------------------------------------
                // Return Base64
                // -----------------------------------------

                return
                    $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
            }
            catch
            {
                return string.Empty;
            }
        }
        [HttpPost]
        public async Task<IActionResult> EmailSalarySlip(
    EmployeeSalaryGetRequestModel request)
        {
            try
            {
                var model = await GetSalarySlipModel(request);

                if (model == null || model.EmployeeID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Salary record not found."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.OfficialEmail))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Employee official email address not found."
                    });
                }

                // Generate PDF
                var pdf = await GenerateSalarySlipPdf(model);

                if (pdf == null || pdf.Length == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Unable to generate salary slip PDF."
                    });
                }

                var fileName =
                    $"SalarySlip_{model.EmployeeName}_{model.MonthName}_{model.Year}.pdf";

                // IMPORTANT: SendSalarySlipEmail is synchronous
                var emailResponse = SendSalarySlipEmail(
                    model,
                    pdf,
                    fileName
                );

                if (emailResponse != null &&
                    emailResponse.responseCode == "200")
                {
                    return Json(new
                    {
                        success = true,
                        message =
                            $"Salary slip sent successfully to {model.OfficialEmail}."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = emailResponse?.responseFailed
                               ?? "Unable to send salary slip."
                });
            }
            catch (Exception ex)
            {
                // IMPORTANT: temporarily return exception while debugging
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        private async Task<byte[]> GenerateSalarySlipPdf(
     EmployeeSalaryRequestModel model)
        {
            var html = await RenderViewToStringAsync(
                "_SalarySlip",
                model
            );

            using var playwright =
                await Playwright.CreateAsync();

            var browserOptions = new BrowserTypeLaunchOptions
            {
                Headless = true
            };

            var browserPath =
                _configuration["Playwright:ExecutablePath"];

            if (!string.IsNullOrWhiteSpace(browserPath))
            {
                browserOptions.ExecutablePath = browserPath;
            }

            await using var browser =
                await playwright.Chromium.LaunchAsync(browserOptions);

            var page =
                await browser.NewPageAsync();

            await page.SetContentAsync(
                html,
                new PageSetContentOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                }
            );

            var pdf = await page.PdfAsync(
                new PagePdfOptions
                {
                    Format = "A4",
                    PrintBackground = true,
                    PreferCSSPageSize = true,

                    Margin = new Margin
                    {
                        Top = "0",
                        Bottom = "0",
                        Left = "0",
                        Right = "0"
                    }
                }
            );

            return pdf;
        }
        private emailSendResponse SendSalarySlipEmail(
     EmployeeSalaryRequestModel model,
     byte[] pdf,
     string fileName)
        {
            var subject =
                $"Salary Slip - {model.MonthName} {model.Year}";

            var employeeName = string.IsNullOrWhiteSpace(model.EmployeeName)
                ? "Employee"
                : model.EmployeeName;

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
</head>

<body style='margin:0; padding:0; background-color:#f5f6f8; font-family:Arial, Helvetica, sans-serif;'>

    <table width='100%' cellpadding='0' cellspacing='0'
           style='background-color:#f5f6f8; padding:30px 0;'>
        <tr>
            <td align='center'>

                <table width='650' cellpadding='0' cellspacing='0'
                       style='background-color:#ffffff;
                              border:1px solid #e1e4e8;
                              border-radius:8px;
                              overflow:hidden;'>

                    <!-- Header -->
                    <tr>
                        <td style='background-color:#1f4e78;
                                   padding:22px 30px;
                                   color:#ffffff;'>

                            <div style='font-size:22px;
                                        font-weight:bold;'>
                                Salary Slip
                            </div>

                            <div style='font-size:13px;
                                        margin-top:5px;
                                        opacity:0.9;'>
                                {model.MonthName} {model.Year}
                            </div>

                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style='padding:30px;'>

                            <p style='font-size:15px;
                                      color:#333333;
                                      margin:0 0 18px 0;'>
                                Dear <strong>{employeeName}</strong>,
                            </p>

                            <p style='font-size:14px;
                                      line-height:1.6;
                                      color:#555555;
                                      margin:0 0 18px 0;'>
                                Please find attached your salary slip for
                                <strong>{model.MonthName} {model.Year}</strong>.
                            </p>

                            <table width='100%'
                                   cellpadding='0'
                                   cellspacing='0'
                                   style='margin:20px 0;
                                          background-color:#f8f9fa;
                                          border:1px solid #e5e7eb;
                                          border-radius:5px;'>

                                <tr>
                                    <td style='padding:12px 15px;
                                               font-size:13px;
                                               color:#666666;
                                               width:40%;'>
                                        Employee Number
                                    </td>

                                    <td style='padding:12px 15px;
                                               font-size:13px;
                                               color:#222222;
                                               font-weight:bold;'>
                                        {model.EmployeeNumber}
                                    </td>
                                </tr>

                                <tr>
                                    <td style='padding:12px 15px;
                                               font-size:13px;
                                               color:#666666;'>
                                        Salary Month
                                    </td>

                                    <td style='padding:12px 15px;
                                               font-size:13px;
                                               color:#222222;
                                               font-weight:bold;'>
                                        {model.MonthName} {model.Year}
                                    </td>
                                </tr>

                            </table>

                            <p style='font-size:14px;
                                      line-height:1.6;
                                      color:#555555;
                                      margin:20px 0;'>
                                The salary slip is attached to this email as a
                                PDF document for your reference.
                            </p>

                            <p style='font-size:14px;
                                      line-height:1.6;
                                      color:#555555;
                                      margin:0;'>
                                If you have any questions regarding your salary
                                slip, please contact the HR department.
                            </p>

                            <p style='font-size:14px;
                                      color:#333333;
                                      margin-top:30px;
                                      margin-bottom:0;'>
                                Regards,<br/>
                                <strong>HR Team</strong>
                            </p>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color:#f8f9fa;
                                   border-top:1px solid #e5e7eb;
                                   padding:15px 30px;
                                   text-align:center;'>

                            <p style='font-size:11px;
                                      color:#888888;
                                      margin:0;'>
                                This is an automated email. Please do not reply
                                to this email.
                            </p>

                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";

            using var stream = new MemoryStream(pdf);

            using var attachment = new Attachment(
                stream,
                fileName,
                "application/pdf"
            );

            var emailProperties = new sendEmailProperties
            {
                emailSubject = subject,
                emailBody = body
            };

            emailProperties.EmailToList.Add(model.OfficialEmail);
            emailProperties.attachments.Add(attachment);

            return EmailSender.SendEmail(emailProperties);
        }

        [HttpPost]
        public async Task<IActionResult> EmailMultipleSalarySlips(
    [FromBody] EmailSalarySlipsRequest request)
        {
            try
            {
                if (request == null ||
                    request.EmployeeID <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid employee."
                    });
                }

                if (request.Months == null ||
                    request.Months.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please select at least one salary month."
                    });
                }

                // Get employee/salary information
                // for the selected months

                var attachments =
                    new List<Attachment>();

                try
                {
                    foreach (var month in request.Months)
                    {
                        var salaryRequest =
                            new EmployeeSalaryGetRequestModel
                            {
                                EmployeeID = request.EmployeeID,
                                SalaryMonth = month.Month,
                                SalaryYear = month.Year
                            };

                        var model =
                            await GetSalarySlipModel(salaryRequest);

                        if (model == null ||
                            model.EmployeeID <= 0)
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(
                            model.OfficialEmail))
                        {
                            return Json(new
                            {
                                success = false,
                                message =
                                    "Employee official email address not found."
                            });
                        }

                        var pdf =
                            await GenerateSalarySlipPdf(model);

                        if (pdf == null ||
                            pdf.Length == 0)
                        {
                            continue;
                        }

                        var fileName =
                            $"SalarySlip_{model.EmployeeNumber}_{model.MonthName}_{model.Year}.pdf";

                        var stream =
                            new MemoryStream(pdf);

                        var attachment =
                            new Attachment(
                                stream,
                                fileName,
                                "application/pdf"
                            );

                        attachments.Add(attachment);
                    }

                    if (attachments.Count == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message =
                                "No salary slips were available for the selected months."
                        });
                    }

                    // Get official email from the first model
                    // rather than trusting browser data.

                    var firstMonth = request.Months.First();

                    var firstRequest =
                        new EmployeeSalaryGetRequestModel
                        {
                            EmployeeID = request.EmployeeID,
                            SalaryMonth = firstMonth.Month,
                            SalaryYear = firstMonth.Year
                        };

                    var employeeModel =
                        await GetSalarySlipModel(firstRequest);

                    var response =
                        SendMultipleSalarySlipsEmail(
                            employeeModel,
                            attachments
                        );

                    if (response.responseCode == "200")
                    {
                        return Json(new
                        {
                            success = true,
                            message =
                                $"{attachments.Count} salary slip(s) sent successfully."
                        });
                    }

                    return Json(new
                    {
                        success = false,
                        message =
                            response.responseFailed
                            ?? "Unable to send salary slips."
                    });
                }
                finally
                {
                    foreach (var attachment in attachments)
                    {
                        attachment.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log ex

                return Json(new
                {
                    success = false,
                    message =
                        "Unable to send salary slips."
                });
            }
        }
        private emailSendResponse SendMultipleSalarySlipsEmail(
    EmployeeSalaryRequestModel model,
    List<Attachment> attachments)
        {
            var subject =
                $"Salary Slips - {model.EmployeeName}";

            var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial, Helvetica, sans-serif;
             background:#f5f6f8;
             padding:30px;'>

    <div style='max-width:650px;
                margin:auto;
                background:#ffffff;
                border:1px solid #ddd;
                border-radius:8px;
                padding:30px;'>

        <h2 style='color:#1f4e78;'>
            Salary Slips
        </h2>

        <p>
            Dear <strong>{model.EmployeeName}</strong>,
        </p>

        <p style='line-height:1.6;'>
            Please find attached your salary slips for the
            selected salary months.
        </p>

        <p style='line-height:1.6;'>
            The salary slips are attached as PDF documents
            for your reference.
        </p>

        <p>
            Regards,<br/>
            <strong>HR Team</strong>
        </p>

        <hr style='border:0;
                   border-top:1px solid #ddd;
                   margin-top:30px;'>

        <p style='font-size:11px;color:#888;'>
            This is an automated email. Please do not reply
            to this email.
        </p>

    </div>

</body>
</html>";

            var emailProperties =
                new sendEmailProperties
                {
                    emailSubject = subject,
                    emailBody = body
                };

            emailProperties.EmailToList.Add(
                model.OfficialEmail
            );

            foreach (var attachment in attachments)
            {
                emailProperties.attachments.Add(
                    attachment
                );
            }

            return EmailSender.SendEmail(
                emailProperties
            );
        }

        [HttpGet]
        public IActionResult ExportMonthlySalary(
     int year,
     int month,
     string search = "")
        {
            try
            {
                if (year <= 0)
                {
                    return BadRequest("Invalid year.");
                }

                if (month < 0 || month > 12)
                {
                    return BadRequest("Invalid month.");
                }

                var employeeId = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

                int roleId = Convert.ToInt32(HttpContext.Session.GetString(Constants.RoleID));

                if (roleId != (int)Roles.Admin &&
                    roleId != (int)Roles.SuperAdmin)
                {
                    return Unauthorized();
                }

                // ---------------------------------------------------------
                // REQUEST
                // ---------------------------------------------------------

                var request = new SalaryInputParams
                {
                    Year = year,
                    Month = month
                };

                // ---------------------------------------------------------
                // CALL PAYROLL API
                // ---------------------------------------------------------

                var apiResponse =
                    _businessLayer.SendPostAPIRequest(
                        request,
                        _businessLayer.GetFormattedAPIUrl(
                            APIControllarsConstants.Payroll,
                            APIApiActionConstants.ExportEmployeeSalary
                        ),
                        HttpContext.Session.GetString(
                            Constants.SessionBearerToken
                        ),
                        true
                    ).Result?.ToString();

                if (string.IsNullOrWhiteSpace(apiResponse))
                {
                    return BadRequest(
                        "No salary records found."
                    );
                }

                // ---------------------------------------------------------
                // DESERIALIZE RESPONSE
                // ---------------------------------------------------------

                var salaryList =
                    JsonConvert.DeserializeObject<List<SalaryDetails>>(
                        apiResponse
                    );

                if (salaryList == null ||
                    salaryList.Count == 0)
                {
                    return BadRequest(
                        "No salary records found for the selected period."
                    );
                }

                // ---------------------------------------------------------
                // SEARCH
                // ---------------------------------------------------------

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    salaryList = salaryList
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.EmployeeNumber) &&
                             x.EmployeeNumber.Contains(
                                 search,
                                 StringComparison.OrdinalIgnoreCase))
                            ||
                            (!string.IsNullOrEmpty(x.EmployeeName) &&
                             x.EmployeeName.Contains(
                                 search,
                                 StringComparison.OrdinalIgnoreCase))
                        )
                        .ToList();
                }

                if (salaryList.Count == 0)
                {
                    return BadRequest(
                        "No salary records found."
                    );
                }

                // ---------------------------------------------------------
                // CREATE EXCEL
                // ---------------------------------------------------------

                using var package = new ExcelPackage();

                var worksheet =
                    package.Workbook.Worksheets.Add(
                        "Salary"
                    );

                // ---------------------------------------------------------
                // HEADERS
                // ---------------------------------------------------------

                worksheet.Cells[1, 1].Value = "Employee Number";
                worksheet.Cells[1, 2].Value = "Employee Name";
                worksheet.Cells[1, 3].Value = "Department";
                worksheet.Cells[1, 4].Value = "Designation";
                worksheet.Cells[1, 5].Value = "Payroll Type";
                worksheet.Cells[1, 6].Value = "Month";
                worksheet.Cells[1, 7].Value = "Year";
                worksheet.Cells[1, 8].Value = "Month Days";
                worksheet.Cells[1, 9].Value = "Payable Days";
                worksheet.Cells[1, 10].Value = "Revised Gross";
                worksheet.Cells[1, 11].Value = "Basic";
                worksheet.Cells[1, 12].Value = "HRA";
                worksheet.Cells[1, 13].Value = "Conveyance";
                worksheet.Cells[1, 14].Value = "Special Allowance";
                worksheet.Cells[1, 15].Value = "Gross Payable";
                worksheet.Cells[1, 16].Value = "Client Incentive";
                worksheet.Cells[1, 17].Value = "PLI";
                worksheet.Cells[1, 18].Value = "Floor Incentive";
                worksheet.Cells[1, 19].Value = "Employee Referral";
                worksheet.Cells[1, 20].Value = "Training Fee";
                worksheet.Cells[1, 21].Value = "GWR";
                worksheet.Cells[1, 22].Value = "Other Arrear";
                worksheet.Cells[1, 23].Value = "PF";
                worksheet.Cells[1, 24].Value = "ESI";
                worksheet.Cells[1, 25].Value = "LWF";
                worksheet.Cells[1, 26].Value = "PTAX";
                worksheet.Cells[1, 27].Value = "TDS";
                worksheet.Cells[1, 28].Value = "Debt Deduction";
                worksheet.Cells[1, 29].Value = "Advance Deduction";
                worksheet.Cells[1, 30].Value = "Insurance Deduction";
                worksheet.Cells[1, 31].Value = "Other Deduction";
                worksheet.Cells[1, 32].Value = "Total Deduction";
                worksheet.Cells[1, 33].Value = "Net Payable";
                worksheet.Cells[1, 34].Value = "Employer PF";
                worksheet.Cells[1, 35].Value = "Employer ESI";
                worksheet.Cells[1, 36].Value = "Employer LWF";
                worksheet.Cells[1, 37].Value =
                    "Total Employer Contribution";
                worksheet.Cells[1, 38].Value = "EPF Wages";
                worksheet.Cells[1, 39].Value = "EPS Wages";
                worksheet.Cells[1, 40].Value = "EDLI Wages";
                worksheet.Cells[1, 41].Value =
                    "EPF Admin Charges";
                worksheet.Cells[1, 42].Value =
                    "EDLI Contribution";
                worksheet.Cells[1, 43].Value =
                    "EDLI Admin Charges";
                worksheet.Cells[1, 44].Value = "CTC";

                // ---------------------------------------------------------
                // DATA
                // ---------------------------------------------------------

                int row = 2;

                foreach (var salary in salaryList)
                {
                    worksheet.Cells[row, 1].Value =
                        salary.EmployeeNumber;

                    worksheet.Cells[row, 2].Value =
                        salary.EmployeeName;

                    worksheet.Cells[row, 3].Value =
                        salary.Department;

                    worksheet.Cells[row, 4].Value =
                        salary.Designation;

                    worksheet.Cells[row, 5].Value =
                        salary.PayrollTypeName;

                    worksheet.Cells[row, 6].Value =
                        salary.SalaryMonth;

                    worksheet.Cells[row, 7].Value =
                        salary.SalaryYear;

                    // No .00 for days
                    worksheet.Cells[row, 8].Value =
                        Convert.ToInt32(salary.MonthDays);

                    worksheet.Cells[row, 9].Value =
                        Convert.ToInt32(salary.PayableDays);

                    worksheet.Cells[row, 10].Value =
                        salary.RevisedGross;

                    worksheet.Cells[row, 11].Value =
                        salary.BasicFixed;

                    worksheet.Cells[row, 12].Value =
                        salary.HRAFixed;

                    worksheet.Cells[row, 13].Value =
                        salary.ConveyanceFixed;

                    worksheet.Cells[row, 14].Value =
                        salary.SpecialAllowanceFixed;

                    worksheet.Cells[row, 15].Value =
                        salary.GrossSalaryPayable;

                    worksheet.Cells[row, 16].Value =
                        salary.ClientIncentive;

                    worksheet.Cells[row, 17].Value =
                        salary.PLI;

                    worksheet.Cells[row, 18].Value =
                        salary.FloorIncentive;

                    worksheet.Cells[row, 19].Value =
                        salary.EmpReferal;

                    worksheet.Cells[row, 20].Value =
                        salary.TrainingFee;

                    worksheet.Cells[row, 21].Value =
                        salary.GWR;

                    worksheet.Cells[row, 22].Value =
                        salary.OtherAdditonArrear;

                    worksheet.Cells[row, 23].Value =
                        salary.EMPPF;

                    worksheet.Cells[row, 24].Value =
                        salary.EMPESI;

                    worksheet.Cells[row, 25].Value =
                        salary.EMPLWF;

                    worksheet.Cells[row, 26].Value =
                        salary.PTAX;

                    worksheet.Cells[row, 27].Value =
                        salary.TDS;

                    worksheet.Cells[row, 28].Value =
                        salary.DbtDeduction;

                    worksheet.Cells[row, 29].Value =
                        salary.Advanceded;

                    worksheet.Cells[row, 30].Value =
                        salary.InsuranceDeduction;

                    worksheet.Cells[row, 31].Value =
                        salary.OtherDeduction;

                    worksheet.Cells[row, 32].Value =
                        salary.TotalDeduction;

                    worksheet.Cells[row, 33].Value =
                        salary.NetPayable;

                    worksheet.Cells[row, 34].Value =
                        salary.EmployerPF;

                    worksheet.Cells[row, 35].Value =
                        salary.EmployerESI;

                    worksheet.Cells[row, 36].Value =
                        salary.EmployerLWF;

                    worksheet.Cells[row, 37].Value =
                        salary.TotalEmployerContribution;

                    worksheet.Cells[row, 38].Value =
                        salary.EPFWages;

                    worksheet.Cells[row, 39].Value =
                        salary.EPSWages;

                    worksheet.Cells[row, 40].Value =
                        salary.EDLIWages;

                    worksheet.Cells[row, 41].Value =
                        salary.EPFAdminCharges;

                    worksheet.Cells[row, 42].Value =
                        salary.EDLIContribution;

                    worksheet.Cells[row, 43].Value =
                        salary.EDLIAdminCharges;

                    worksheet.Cells[row, 44].Value =
                        salary.CTC;

                    row++;
                }

                // ---------------------------------------------------------
                // FORMAT
                // ---------------------------------------------------------

                worksheet.Cells[1, 1, 1, 44]
                    .Style.Font.Bold = true;

                worksheet.Cells[
                    worksheet.Dimension.Address
                ].AutoFitColumns();

                // ---------------------------------------------------------
                // FILE NAME
                // ---------------------------------------------------------

                string periodName;

                if (month == 0)
                {
                    periodName = $"Year_{year}";
                }
                else
                {
                    periodName =
                        $"{CultureInfo.CurrentCulture.DateTimeFormat
                            .GetMonthName(month)}_{year}";
                }

                string fileName =
                    $"EmployeeSalary_{periodName}.xlsx";

                // ---------------------------------------------------------
                // RETURN FILE
                // ---------------------------------------------------------

                var fileBytes =
                    package.GetAsByteArray();

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    $"Error while exporting salary: {ex.Message}"
                );
            }
        }
        [HttpPost]
        public JsonResult AutoCalculateEmployeeSalary(
            [FromBody] AutoCalculateSalaryRequestModel request)
        {
            // ------------------------------------------------------------
            // GET CUTOFF SETTINGS
            // ------------------------------------------------------------

            var token =
                HttpContext.Session.GetString(Constants.SessionBearerToken);

            var cutoffSettings =
                _cutoffSettingsService.GetCutoffSettings(token);

            DateTime? salaryCalculationCutoffDate =
                cutoffSettings.SalaryCalculationCutoffDate;


            // ------------------------------------------------------------
            // SALARY CUTOFF VALIDATION
            // ------------------------------------------------------------

            if (salaryCalculationCutoffDate.HasValue)
            {
                int cutoffYear = salaryCalculationCutoffDate.Value.Year;
                int cutoffMonth = salaryCalculationCutoffDate.Value.Month;

                bool isBeforeCutoff =
                    request.Year.Value < cutoffYear ||
                    (request.Year.Value == cutoffYear &&
                     request.Month.Value < cutoffMonth);

                if (isBeforeCutoff)
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            $"Salary calculation for " +
                            $"{new DateTime(
                                request.Year.Value,
                                request.Month.Value,
                                1
                            ):MMMM yyyy} " +
                            $"is locked. Salary calculation is allowed from " +
                            $"{new DateTime(
                                cutoffYear,
                                cutoffMonth,
                                1
                            ):MMMM yyyy} onward."
                    });
                }
            }



            // ------------------------------------------------------------
            // SET USER ID
            // ------------------------------------------------------------

            request.UserID = Convert.ToInt64(
                HttpContext.Session.GetString(Constants.EmployeeID));


            // ------------------------------------------------------------
            // CALL API
            // ------------------------------------------------------------

            var apiResponse = _businessLayer.SendPostAPIRequest(
                request,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Payroll,
                    APIApiActionConstants.AutoCalculateEmployeeSalary
                ),
                token,
                true
            ).Result?.ToString();


            // ------------------------------------------------------------
            // API RESPONSE
            // ------------------------------------------------------------

            if (string.IsNullOrEmpty(apiResponse))
            {
                return Json(null);
            }

            // Deserialize the complete Results object
            var result =
                JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(
                    apiResponse);

            return Json(result);
        }
        [HttpPost]
        public JsonResult VerifyEmployeeSalary(
    [FromBody] VerifyEmployeeSalaryRequestModel request)
        {
            try
            {
                request.UserID =
                    Convert.ToInt64(
                        HttpContext.Session.GetString(
                            Constants.EmployeeID
                        )
                    );


                var apiResponse =
                    _businessLayer.SendPostAPIRequest(
                        request,
                        _businessLayer.GetFormattedAPIUrl(
                            APIControllarsConstants.Payroll,
                            APIApiActionConstants.VerifyEmployeeSalary
                        ),
                        HttpContext.Session.GetString(
                            Constants.SessionBearerToken
                        ),
                        true
                    ).Result?.ToString();


                if (string.IsNullOrEmpty(apiResponse))
                {
                    return Json(new
                    {
                        success = false,
                        message = "No response received from API.",
                        data = new
                        {
                            verifiedCount = 0,
                            failedCount = 0,
                            errors = new List<string>()
                        }
                    });
                }


                var result =
                    JsonConvert.DeserializeObject<Result>(
                        apiResponse
                    );


                if (result == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid response received from API.",
                        data = new
                        {
                            verifiedCount = 0,
                            failedCount = 0,
                            errors = new List<string>()
                        }
                    });
                }


                // IMPORTANT
                // Return success based on PKNo

                bool success =
                    (result.PKNo ?? 0) > 0;


                return Json(new
                {
                    success = success,

                    message = result.Message,

                    errorCode = result.ErrorCode,

                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,

                    message = ex.Message,

                    errorCode = "VERIFY_ERROR",

                    data = new
                    {
                        verifiedCount = 0,
                        failedCount = 0,
                        errors = new List<string>
                {
                    ex.Message
                }
                    }
                });
            }
        }
        [HttpGet]
        public IActionResult DownloadSlarySlip()
        {
            return View();
        }
   
    [HttpGet]
        public JsonResult GetMySalaryMonths()
        {
            var employeeId =
                Convert.ToInt64(
                    HttpContext.Session.GetString(
                        Constants.EmployeeID
                    )
                );

            if (employeeId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Employee session not found."
                });
            }


            var request =
                new EmployeeSalaryMonthRequestModel
                {
                    EmployeeID = employeeId
                };


            var apiResponse =
                _businessLayer.SendPostAPIRequest(
                    request,

                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Payroll,
                        APIApiActionConstants.GetEmployeeSalaryMonths
                    ),

                    HttpContext.Session.GetString(
                        Constants.SessionBearerToken
                    ),

                    true

                ).Result?.ToString();


            if (string.IsNullOrEmpty(apiResponse))
            {
                return Json(new List<EmployeeSalaryMonth>());
            }


            var salaryMonths =
                JsonConvert.DeserializeObject<
                    List<EmployeeSalaryMonth>
                >(apiResponse);


            return Json(salaryMonths);
        }
        [HttpPost]
        public async Task<IActionResult> DownloadMultipleSalarySlips(
    [FromBody] List<SalarySlipDownloadMonthModel> months)
        {
            try
            {
                if (months == null || months.Count == 0)
                {
                    return BadRequest(
                        "Please select at least one salary month."
                    );
                }


                // IMPORTANT:
                // Use the same method/session/claim that
                // GetMySalaryMonths() uses to get logged-in employee.

                var employeeIdString =
                    HttpContext.Session.GetString(
                        Constants.EmployeeID
                    );


                if (!long.TryParse(
                        employeeIdString,
                        out long employeeId))
                {
                    return Unauthorized(
                        "Employee not found."
                    );
                }


                using var zipStream =
                    new MemoryStream();


                using (
                    var archive =
                        new System.IO.Compression.ZipArchive(
                            zipStream,
                            System.IO.Compression.ZipArchiveMode.Create,
                            true
                        ))
                {

                    foreach (var selected in months)
                    {

                        var request =
                            new EmployeeSalaryGetRequestModel
                            {
                                EmployeeID = employeeId,

                                SalaryMonth = selected.Month,

                                SalaryYear = selected.Year
                            };


                        var model =
                            await GetSalarySlipModel(request);


                        if (model == null ||
                            model.EmployeeID <= 0)
                        {
                            continue;
                        }


                        var pdf =
                            await GenerateSalarySlipPdf(model);


                        if (pdf == null ||
                            pdf.Length == 0)
                        {
                            continue;
                        }


                        var fileName =
                            $"SalarySlip_{model.EmployeeName}_{model.MonthName}_{model.Year}.pdf";


                        foreach (
                            var invalidChar
                            in Path.GetInvalidFileNameChars())
                        {
                            fileName =
                                fileName.Replace(
                                    invalidChar.ToString(),
                                    "_"
                                );
                        }


                        var entry =
                            archive.CreateEntry(
                                fileName,
                                System.IO.Compression.CompressionLevel.Fastest
                            );


                        await using var entryStream =
                            entry.Open();


                        await entryStream.WriteAsync(
                            pdf,
                            0,
                            pdf.Length
                        );
                    }
                }


                zipStream.Position = 0;


                if (zipStream.Length == 0)
                {
                    return BadRequest(
                        "No salary slips were found."
                    );
                }


                return File(
                    zipStream.ToArray(),
                    "application/zip",
                    $"SalarySlips_{DateTime.Now:yyyyMMddHHmmss}.zip"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(
                    "Unable to download salary slips."
                );
            }
        }
        [HttpPost]
        public JsonResult GetMonthlySalaryEmployeeIDs(
     [FromBody] SalaryInputParams request)
        {
            try
            {
                var companyId = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.CompanyID)
                );

                request.CompanyID = companyId;

                // IMPORTANT:
                // Override DataTable pagination for Select All
                request.DisplayStart = 0;
                request.DisplayLength = int.MaxValue;

                var apiResponse = _businessLayer.SendPostAPIRequest(
                    request,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Payroll,
                        APIApiActionConstants.GetEmployeesMonthlySalary
                    ),
                    HttpContext.Session.GetString(
                        Constants.SessionBearerToken
                    ),
                    true
                ).Result?.ToString();

                if (string.IsNullOrWhiteSpace(apiResponse))
                {
                    return Json(new
                    {
                        success = false,
                        message = "No salary records found.",
                        employeeIDs = new List<long>()
                    });
                }

                var salaryList =
                    JsonConvert.DeserializeObject<List<SalaryDetails>>(
                        apiResponse
                    );

                if (salaryList == null || !salaryList.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No salary records found for the selected month.",
                        employeeIDs = new List<long>()
                    });
                }

                var employeeIDs = salaryList
                    .Where(x => x.EmployeeID > 0)
                    .Select(x => x.EmployeeID)
                    .Distinct()
                    .ToList();

                return Json(new
                {
                    success = true,
                    employeeIDs = employeeIDs
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    employeeIDs = new List<long>()
                });
            }
        }
    }
    }
