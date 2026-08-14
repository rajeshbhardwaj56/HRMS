using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.PayRoll;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
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

        public PayrollController(IConfiguration configuration, IBusinessLayer businessLayer, IS3Service s3Service)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
            _s3Service = s3Service;
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
                model.ForEach(x => { x.EncryptedSalaryID = _businessLayer.EncodeStringBase64(x.SalaryID.ToString()); });

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

        public JsonResult SaveSalary([FromBody] EmployeeSalaryRequestModel model)
        {
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
        public IActionResult SalarySlipPreview(EmployeeSalaryGetRequestModel request)
        {
            EmployeeSalaryRequestModel model = new EmployeeSalaryRequestModel();

            try
            {
                var data = _businessLayer.SendPostAPIRequest(
                    request,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Payroll,
                        APIApiActionConstants.GetEmployeeSalary
                    ),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result?.ToString();

                if (!string.IsNullOrEmpty(data))
                {
                    var salary =
                        JsonConvert.DeserializeObject<EmployeeSalaryCalculationModel>(data);

                    if (salary != null)
                    {
                        model = new EmployeeSalaryRequestModel
                        {
                            // BASIC INFORMATION
                            EmployeeID = salary.EmployeeID,
                            EmployeeNumber = salary.EmployeeNumber,
                            EmployeeName = salary.EmployeeName,
                            PayrollTypeID = salary.PayrollTypeID,
                            Year = salary.SalaryYear,
                            Month = salary.SalaryMonth,

                            // SALARY
                            RevisedGross = salary.RevisedGross,
                            MonthDays = salary.MonthDays,
                            PayableDays = salary.PayableDays,

                            // FIXED SALARY
                            BasicFixed = salary.BasicFixed,
                            HRAFixed = salary.HRAFixed,
                            ConveyanceFixed = salary.ConveyanceFixed,
                            SpecialAllowanceFixed = salary.SpecialAllowanceFixed,
                            GrossSalaryFixed = salary.GrossSalaryFixed,

                            // PAYABLE SALARY
                            BasicPayable = salary.BasicPayable,
                            HRAPayable = salary.HRAPayable,
                            ConveyancePayable = salary.ConveyancePayable,
                            SpecialAllowancePayable = salary.SpecialAllowancePayable,
                            GrossSalaryPayable = salary.GrossSalaryPayable,

                            // EARNINGS
                            ClientIncentive = salary.ClientIncentive,
                            PLI = salary.PLI,
                            FloorIncentive = salary.FloorIncentive,
                            EmpReferal = salary.EmpReferal,
                            TrainingFee = salary.TrainingFee,
                            GWR = salary.GWR,
                            OtherAdditonArrear = salary.OtherAdditonArrear,

                            // DEDUCTIONS
                            EMPLWF = salary.EMPLWF,
                            TDS = salary.TDS,
                            DbtDeduction = salary.DbtDeduction,
                            Advanceded = salary.Advanceded,
                            InsuranceDeduction = salary.InsuranceDeduction,
                            OtherDeduction = salary.OtherDeduction,

                            // STATUTORY
                            EMPPF = salary.EMPPF,
                            EMPESI = salary.EMPESI,
                            PTAX = salary.PTAX,

                            // TOTAL
                            TotalDeduction = salary.TotalDeduction,
                            NetPayable = salary.NetPayable,

                            // EMPLOYER
                            EmployerPF = salary.EmployerPF,
                            EmployerESI = salary.EmployerESI,
                            EmployerLWF = salary.EmployerLWF,
                            TotalEmployerContribution = salary.TotalEmployerContribution,

                            // EPF / EPS / EDLI
                            EPFWages = salary.EPFWages,
                            EPSWages = salary.EPSWages,
                            EDLIWages = salary.EDLIWages,
                            EPFAdminCharges = salary.EPFAdminCharges,
                            EDLIContribution = salary.EDLIContribution,
                            EDLIAdminCharges = salary.EDLIAdminCharges,

                            // CTC
                            CTC = salary.CTC,

                            // SALARY SLIP DISPLAY
                            BankAccountNumber = salary.BankAccountNumber,
                            BankName = salary.BankName,
                            Designation = salary.Designation,
                            Department = salary.Department,
                            Location = salary.Location,
                            DateOfJoining = salary.DateOfJoining,
                            MonthName = salary.MonthName,
                            NetPayableInWords = salary.NetPayableInWords
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // log exception
                return View(model);
            }

            return View(model);
        }

    }
}
