using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Spreadsheet;
using HRMS.Models;
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.ExportEmployeeExcel;
using HRMS.Models.ImportFromExcel;
using HRMS.Models.LeavePolicy;
using HRMS.Models.PayRoll;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize]
    public class DashBoardController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment Environment;
        IHttpContextAccessor _context;
        private readonly IS3Service _s3Service;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;
        private readonly ICutoffSettingsService _cutoffSettingsService;
        public DashBoardController(ICheckUserFormPermission CheckUserFormPermission, IConfiguration configuration, IBusinessLayer businessLayer, Microsoft.AspNetCore.Hosting.IWebHostEnvironment _environment, IHttpContextAccessor context, IS3Service s3Service, ICutoffSettingsService cutoffSettingsService)
        {
            Environment = _environment;
            _configuration = configuration;
            _context = context;
            _businessLayer = businessLayer;
            _s3Service = s3Service;
            _CheckUserFormPermission = CheckUserFormPermission;
            _cutoffSettingsService = cutoffSettingsService;
        }
        public async Task<IActionResult> Index()
        {
            var session = HttpContext.Session;
            var companyId = Convert.ToInt64(session.GetString(Constants.CompanyID));
            var employeeId = Convert.ToInt64(session.GetString(Constants.EmployeeID));
            var roleId = Convert.ToInt64(session.GetString(Constants.RoleID));
            var jobLocationId = Convert.ToInt64(session.GetString(Constants.JobLocationID));
            var token = session.GetString(Constants.SessionBearerToken);
            var inputParams = new DashBoardModelInputParams
            {
                EmployeeID = employeeId,
                RoleID = roleId,
                JobLocationId = jobLocationId
            };
            var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetDashBoardModel);
            var apiResponse = await _businessLayer.SendPostAPIRequest(inputParams, apiUrl, token, true);
            var model = JsonConvert.DeserializeObject<DashBoardModel>(apiResponse?.ToString());
            if (model?.EmployeeDetails != null)
            {
                foreach (var employee in model.EmployeeDetails.Where(x => !string.IsNullOrEmpty(x.EmployeePhoto)))
                {
                    employee.EmployeePhoto = _s3Service.GetFileUrl(employee.EmployeePhoto);
                }
            }
            if (model?.WhatsHappening != null)
            {
                foreach (var item in model.WhatsHappening.Where(x => !string.IsNullOrEmpty(x.IconImage)))
                {
                    item.IconImage = _s3Service.GetFileUrl(item.IconImage);
                }
            }

            if (model?.leaveResults?.leaveBalance != null)
            {
                {
                    ViewBag.NoOfLeaves = model.leaveResults.leaveBalance.AnnualLeaveBalance;
                    model.NoOfLeaves = Convert.ToInt64(model.leaveResults.leaveBalance.AnnualLeaveBalance);
                    //var leavePolicy = GetLeavePolicyData(companyId, model.LeavePolicyId ?? 0);
                    //ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicy.Annual_MaximumConsecutiveLeavesAllowed);
                }
            }

            return View(model);
        }

        private LeavePolicyModel GetLeavePolicyData(long companyId, long leavePolicyId)
        {
            var leavePolicyModel = new LeavePolicyModel { CompanyID = companyId, LeavePolicyID = leavePolicyId };
            var leavePolicyDataJson = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var leavePolicyModelResult = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(leavePolicyDataJson).leavePolicyModel;
            return leavePolicyModelResult;
        }
        private double CalculateAccruedLeaveForCurrentFiscalYear(DateTime joinDate, int Annual_MaximumLeaveAllocationAllowed)
        {
            DateTime today = DateTime.Today;
            DateTime fiscalYearStart;
            DateTime fiscalYearEnd;
            if (today <= new DateTime(2026, 3, 20))
            {

                fiscalYearStart = new DateTime(2026, 1, 21);
                fiscalYearEnd = new DateTime(2026, 3, 20);
            }
            else
            {
                // ✅ Default logic: Fiscal year from 21 March current/previous year to 20 March next year
                fiscalYearStart = new DateTime(today.Month > 3 || (today.Month == 3 && today.Day >= 21)
                                               ? today.Year : today.Year - 1, 3, 21);
                fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-1); // Ends on 20 March next year
            }


            double annualLeaveEntitlement = Annual_MaximumLeaveAllocationAllowed;
            double monthlyAccrual = annualLeaveEntitlement / 12;
            double totalAccruedLeave = 0;

            // If join date is before fiscal year, adjust to fiscal start
            if (joinDate < fiscalYearStart)
                joinDate = fiscalYearStart;

            // Start from the accrual period containing the join date
            DateTime accrualPeriodStart = GetAccrualPeriodStart(joinDate);
            DateTime accrualPeriodEnd = accrualPeriodStart.AddMonths(1).AddDays(-1); // 20th of next month
            if (today > new DateTime(2026, 3, 20))
            {
                while (accrualPeriodStart <= today && accrualPeriodStart <= fiscalYearEnd)
                {
                    // Adjust for join date or current date
                    DateTime effectiveStart = joinDate > accrualPeriodStart ? joinDate : accrualPeriodStart;
                    DateTime effectiveEnd = accrualPeriodEnd < today ? accrualPeriodEnd : today;

                    int daysWorked = (effectiveEnd - effectiveStart).Days + 1;

                    if (daysWorked > Convert.ToInt32(_configuration["DaysWorkedInMonth:DaysWorkedInMonth"]))
                    {
                        totalAccruedLeave += monthlyAccrual;
                    }

                    // Move to next accrual period
                    accrualPeriodStart = accrualPeriodStart.AddMonths(1);
                    accrualPeriodEnd = accrualPeriodStart.AddMonths(1).AddDays(-1);
                }
            }
            return totalAccruedLeave;
        }
        private DateTime GetAccrualPeriodStart(DateTime date)
        {
            if (date.Day >= 21)
                return new DateTime(date.Year, date.Month, 21);
            else
            {
                DateTime prevMonth = date.AddMonths(-1);
                return new DateTime(prevMonth.Year, prevMonth.Month, 21);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ImportExcel()
        {
            var roleId = GetSessionInt(Constants.RoleID);

            if (roleId != (int)Roles.Admin &&
                roleId != (int)Roles.SuperAdmin)
            {

                return RedirectToActionPermanent(
                    Constants.Index,
                    _businessLayer.GetControllarNameByRole(roleId),
                    new { area = "admin" }
                );
            }
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> ImportExcelBulk(IFormFile file)
        {
            if (file == null || !(file.ContentType.Contains("excel") || file.FileName.EndsWith(".xlsx")))
            {
                return Json(new { success = false, message = "Invalid file type." });
            }
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    string htmlTable = ProcessExcelFile(stream, file.FileName);

                    if (!string.IsNullOrEmpty(htmlTable) && !htmlTable.Contains("Employee data imported successfully."))
                    {
                        return Json(new
                        {
                            success = false,
                            hasErrors = true,
                            message = "Some rows contain errors.",
                            errorTable = htmlTable
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = true,
                            message = htmlTable
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"File not added and processed successfully"
                });
            }
        }
        public string ProcessExcelFile(Stream stream, string fileName)
        {
            var countryDictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(
                _businessLayer.SendGetAPIRequest(_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetCountryDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString()
            );
            List<ImportExcelDataTable> importList = new List<ImportExcelDataTable>();
            DataTable errorDataTable = new DataTable();
            foreach (var prop in typeof(ImportExcelDataTable).GetProperties())
            {
                errorDataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            errorDataTable.Columns.Add("ErrorColumn", typeof(string));
            errorDataTable.Columns.Add("ErrorMessage", typeof(string));
            using (var package = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    AddErrorRow(errorDataTable, "Excel file", "The Excel sheet is empty.");
                    return ConvertDataTableToHTML(errorDataTable);
                }
                int totalColumns = worksheet.Dimension?.Columns ?? 0;
                int totalRows = worksheet.Dimension?.Rows ?? 0;
                if (totalColumns == 0 || totalRows < 2)
                {
                    AddErrorRow(errorDataTable, "Excel file", "The Excel sheet is empty.");
                    return ConvertDataTableToHTML(errorDataTable);
                }
                var (isHeaderValid, mismatchedColumn) = ValidateHeaderRow(worksheet, typeof(ImportExcelDataTable));
                if (!isHeaderValid)
                {
                    AddErrorRow(errorDataTable, "Excel file", $"Header mismatch: {mismatchedColumn}");
                    return ConvertDataTableToHTML(errorDataTable);
                }
                var columnIndexMap = typeof(ImportExcelDataTable).GetProperties()
                    .Select((prop, idx) => new { prop.Name, Index = idx + 1 })
                    .ToDictionary(x => x.Name, x => x.Index);
                long DepartmentId = 0;
                long DesignationsId = 0;
                long EmploymentTypesId = 0;
                long SubDepartmentNameId = 0;
                long ShiftTypeId = 0;
                long JobLocationId = 0;

                long PayrollTypeId = 0;
                long LeavePolicyId = 0;
                long GenderId = 0;
                HashSet<string> uniqueEmployeeNumber = new HashSet<string>();
                long? companyId = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
                EmploymentDetailInputParams employmentDetailInputParams = new EmploymentDetailInputParams()
                {
                    CompanyID = companyId ?? 0,
                    EmployeeID = 0
                };
                var EmploymentDetailsDictionaries = _businessLayer.SendPostAPIRequest(employmentDetailInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetEmploymentDetailsDictionaries), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var employmentDetailsDictionaries = JsonConvert
    .DeserializeObject<Dictionary<string, Dictionary<string, long>>>(EmploymentDetailsDictionaries)
    .ToDictionary(
        d => d.Key,
        d => new Dictionary<string, long>(d.Value, StringComparer.OrdinalIgnoreCase)
    );
                EmployeeInputParams employmentSubDepartmentInputParams = new EmployeeInputParams()
                {
                    CompanyID = companyId ?? 0,
                };
                var EmploymentSubDepartment = _businessLayer.SendPostAPIRequest(employmentSubDepartmentInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetSubDepartmentDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var SubDepartmentDictionaries = new Dictionary<string, long>(
    JsonConvert.DeserializeObject<Dictionary<string, long>>(EmploymentSubDepartment),
    StringComparer.OrdinalIgnoreCase
);
                for (int row = 2; row <= totalRows; row++)
                {
                    if (IsRowEmpty(worksheet, row))
                        continue;
                    bool hasError = false;
                    var item = new ImportExcelDataTable();
                    foreach (var prop in typeof(ImportExcelDataTable).GetProperties())
                    {
                        string columnName = prop.Name;
                        string cellValue = worksheet.Cells[row, columnIndexMap[columnName]].Text?.Trim();
                        if (!string.IsNullOrEmpty(cellValue))
                        {
                            string normalized = cellValue.Trim().ToLowerInvariant();
                            if (normalized == "n/a" || normalized == "#n/a" || normalized == "na" || normalized == "-")
                            {
                                cellValue = string.Empty;
                            }
                        }
                        try
                        {
                            switch (columnName)
                            {
                                case "EMPID":
                                    if (!string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        if (!uniqueEmployeeNumber.Add(cellValue))
                                        {
                                            AddErrorRow(errorDataTable, columnName, $"Row {row}: Duplicate EmployeeNumber found.");
                                            hasError = true;
                                        }
                                        else
                                        {
                                            prop.SetValue(item, cellValue);
                                        }
                                    }
                                    else
                                    {
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: EmployeeNumber is mandatory.");
                                        hasError = true;
                                    }
                                    break;
                                case "DateOfBirth":
                                    if (!string.IsNullOrWhiteSpace(cellValue))
                                    {

                                        string[] formats = { "dd/MM/yyyy", "d/M/yyyy" };
                                        if (DateTime.TryParseExact(
        cellValue?.Trim(),
        formats,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out DateTime dob))
                                        {

                                            prop.SetValue(item, dob.ToString("yyyy-MM-dd"));
                                        }
                                        else
                                        {
                                            AddErrorRow(errorDataTable, columnName, $"Row {row}: Invalid Date Format.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: Date of birth is mandatory.");
                                        hasError = true;
                                    }
                                    break;
                                case "FirstName":
                                    if (!string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        prop.SetValue(item, cellValue);
                                    }
                                    else
                                    {
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: FirstName is mandatory.");
                                        hasError = true;
                                    }
                                    break;

                                case "DateOfResignation":
                                case "RegistrationDateInESIC":
                                case "DOJInTraining":
                                case "DOJOnFloor":
                                case "DOJInOJT":
                                case "DOJInOnroll":
                                case "DateOfLeaving":
                                case "BackOnFloor":
                                case "DateOfEmailSentToITForIDDeletion":

                                    if (!string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        string[] formats = { "dd/MM/yyyy", "d/M/yyyy" };

                                        if (DateTime.TryParseExact(
                                                cellValue.Trim(),
                                                formats,
                                                CultureInfo.InvariantCulture,
                                                DateTimeStyles.None,
                                                out DateTime parsedDate))
                                        {

                                            prop.SetValue(item, parsedDate.ToString("yyyy-MM-dd"));
                                        }
                                        else
                                        {
                                            AddErrorRow(
                                                errorDataTable,
                                                columnName,
                                                $"Row {row}: Invalid {columnName} format. Expected dd/MM/yyyy.");
                                            hasError = true;
                                        }
                                    }
                                    break;




                                case "JoiningDate":
                                    if (!string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        string[] formats = { "dd/MM/yyyy", "d/M/yyyy" };
                                        if (DateTime.TryParseExact(
        cellValue?.Trim(),
        formats,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out DateTime joiningDate))
                                        {
                                            prop.SetValue(item, joiningDate.ToString("yyyy-MM-dd"));
                                        }
                                        else
                                        {
                                            AddErrorRow(errorDataTable, columnName, $"Row {row}: Invalid JoiningDate format.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: JoiningDate is mandatory.");
                                        hasError = true;
                                    }
                                    break;
                                case "CompanyName":
                                    prop.SetValue(item, companyId.ToString());
                                    break;
                                case "PresentCountryName":
                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        long countryId = countryDictionary.TryGetValue(cellValue.ToLower(), out long cid) ? cid : 0;
                                        if (countryId != 0)
                                            prop.SetValue(item, countryId.ToString());
                                        else
                                        {
                                            AddErrorRow(errorDataTable, columnName, $"Row {row}: Country  not found.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: Country name is mandatory.");
                                        hasError = true;
                                    }
                                    break;
                                case "PermanentCountryName":
                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        long countryId = countryDictionary.TryGetValue(cellValue.ToLower(), out long cid) ? cid : 0;
                                        if (countryId != 0)
                                            prop.SetValue(item, countryId.ToString());
                                        else
                                        {
                                            AddErrorRow(errorDataTable, columnName, $"Row {row}: Country  not found.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: Country name is mandatory.");
                                        hasError = true;
                                    }
                                    break;
                                case "Location":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Location is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("JobLocations", out var jobLocationDict))
                                    {
                                        if (jobLocationDict.TryGetValue(cellValue.Trim(), out var locationId))
                                        {
                                            prop.SetValue(item, locationId.ToString());
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: Location not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Location dictionary is missing or empty.");
                                        hasError = true;
                                    }
                                    break;
                                case "LeavePolicyName":
                                    if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("LeavePolicies", out var leavePolicyDict))
                                    {
                                        if (leavePolicyDict.TryGetValue(cellValue.Trim(), out var policyId))
                                        {
                                            prop.SetValue(item, policyId.ToString());
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: LeavePolicy not found.");
                                            hasError = true;
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: LeavePolicies dictionary is missing or empty.");
                                        hasError = true;
                                    }
                                    break;
                                case "IsRelativesWorkingWithCompany":
                                    if (!string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        if (cellValue.Equals("Yes", StringComparison.OrdinalIgnoreCase) || cellValue.Equals("No", StringComparison.OrdinalIgnoreCase))
                                        {
                                            prop.SetValue(item, cellValue);
                                        }
                                        else
                                        {
                                            AddErrorRow(errorDataTable, columnName, $"Row {row}: IsRelativesWorkingWithCompany must be 'Yes' or 'No'.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        prop.SetValue(item, "No");
                                    }
                                    break;
                                case "IsReferredByExistingEmployee":
                                    if (!string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        if (cellValue.Equals("Yes", StringComparison.OrdinalIgnoreCase) || cellValue.Equals("No", StringComparison.OrdinalIgnoreCase))
                                        {
                                            prop.SetValue(item, cellValue);
                                        }
                                        else
                                        {
                                            AddErrorRow(errorDataTable, columnName, $"Row {row}: IsReferredByExistingEmployee must be 'Yes' or 'No'.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        // If nothing is entered, default to "No"
                                        prop.SetValue(item, "No");
                                    }
                                    break;

                                case "PayrollTypeName":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: PayrollTypeName is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("PayrollTypes", out var payrollDict))
                                    {
                                        if (payrollDict.TryGetValue(cellValue.Trim(), out var payrollId))
                                        {
                                            prop.SetValue(item, payrollId.ToString());
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: PayrollType not found.");
                                            hasError = true;
                                        }
                                    }
                                    break;
                                case "DepartmentName":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: DepartmentName is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("Departments", out var departmentDict))
                                    {
                                        if (departmentDict.TryGetValue(cellValue.Trim(), out var deptId))
                                        {
                                            prop.SetValue(item, deptId.ToString());
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: Department not found.");
                                            hasError = true;
                                        }
                                    }
                                    break;
                                case "SubDepartmentName":


                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: SubDepartmentName is required.");
                                        hasError = true;
                                    }
                                    else if (SubDepartmentDictionaries.TryGetValue(cellValue.Trim(), out var subDeptId))
                                    {
                                        prop.SetValue(item, subDeptId.ToString());
                                    }
                                    else
                                    {
                                        item.NewSubDepartmentName = cellValue;
                                    }
                                    break;

                                case "DesignationName":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: DesignationName is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("Designations", out var designationDict))
                                    {
                                        if (designationDict.TryGetValue(cellValue.Trim(), out var designationId))
                                        {
                                            prop.SetValue(item, designationId.ToString());
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: Designation not found.");
                                            hasError = true;
                                        }
                                    }
                                    break;
                                case "Category":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Category is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("EmploymentTypes", out var empTypeDict))
                                    {
                                        if (empTypeDict.TryGetValue(cellValue.Trim(), out var empTypeId))
                                        {
                                            prop.SetValue(item, empTypeId.ToString());
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: Category not found.");
                                            hasError = true;
                                        }
                                    }

                                    break;
                                case "ShiftTypeName":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: ShiftTypeName is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("ShiftTypes", out var shiftDict))
                                    {
                                        string excelShiftCode = cellValue.Trim();

                                        var matchedShift = shiftDict.FirstOrDefault(kvp =>
                                        {
                                            var apiShiftCode = kvp.Key
                                                .Split(' ', '(')[0]
                                                .Trim();

                                            return apiShiftCode.Equals(excelShiftCode, StringComparison.OrdinalIgnoreCase);
                                        });

                                        if (!matchedShift.Equals(default(KeyValuePair<string, long>)))
                                        {
                                            prop.SetValue(item, matchedShift.Value.ToString());
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: ShiftType not found.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: ShiftType dictionary is missing or empty.");
                                        hasError = true;
                                    }
                                    break;

                                case "NoticeServed":
                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        var normalized = cellValue.Trim().ToLower();

                                        if (normalized.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                        {
                                            prop.SetValue(item, "1");
                                        }
                                        else if (normalized.Equals("no", StringComparison.OrdinalIgnoreCase))
                                        {
                                            prop.SetValue(item, "2");
                                        }
                                        else if (normalized.Equals("nr", StringComparison.OrdinalIgnoreCase))
                                        {
                                            prop.SetValue(item, "3");
                                        }
                                        else
                                        {
                                            prop.SetValue(item, "0");
                                        }
                                    }
                                    else
                                    {
                                        prop.SetValue(item, "0");
                                    }
                                    break;
                                case "Status":
                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        var normalized = cellValue.Trim().ToLower();

                                        var activeValues = new HashSet<string> { "active", "1", "true", "yes" };
                                        var inactiveValues = new HashSet<string> { "inactive", "0", "false", "no" };

                                        if (activeValues.Contains(normalized))
                                        {
                                            prop.SetValue(item, "1");
                                        }
                                        else if (inactiveValues.Contains(normalized))
                                        {
                                            prop.SetValue(item, "0");
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: Status is invalid. Allowed values are Active or Inactive.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Status is required.");
                                        hasError = true;

                                    }
                                    break;
                                case "AON":

                                    if (int.TryParse(cellValue, out int aonValue))
                                    {
                                        prop.SetValue(item, aonValue.ToString());
                                    }
                                    else
                                    {
                                        prop.SetValue(item, "0");
                                    }

                                    break;
                                case "Gender":
                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        if (cellValue.Equals("Female", StringComparison.OrdinalIgnoreCase))
                                        {
                                            GenderId = 2;
                                            prop.SetValue(item, GenderId.ToString());
                                        }
                                        else if (cellValue.Equals("Male", StringComparison.OrdinalIgnoreCase))
                                        {
                                            GenderId = 1;
                                            prop.SetValue(item, GenderId.ToString());
                                        }
                                        else
                                        {
                                            GenderId = 3;
                                            prop.SetValue(item, GenderId.ToString());
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Gender is required.");
                                        hasError = true;
                                    }
                                    break;
                                default:

                                    if (!string.IsNullOrEmpty(cellValue))
                                        prop.SetValue(item, Convert.ChangeType(cellValue, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType));
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            AddErrorRow(errorDataTable, columnName, $"Row {row}: Invalid data. {ex.Message}");
                            hasError = true;
                        }
                    }
                    if (!hasError)
                    {

                        importList.Add(item);
                    }
                }
            }
            if (importList.Any() && errorDataTable.Rows.Count == 0)
            {



                var employeeList = importList.Select(item => new ImportExcelDataTable
                {
                    CompanyName = (item.CompanyName),
                    FirstName = item.FirstName,
                    MiddleName = item.MiddleName,
                    Surname = item.Surname,
                    PresentAddress = item.PresentAddress,
                    PresentCity = item.PresentCity,
                    PresentPinCode = item.PresentPinCode,
                    PresentState = item.PresentState,
                    PresentCountryName = item.PresentCountryName,
                    EmailId = item.EmailId,
                    Landline = item.Landline,
                    Mobile = item.Mobile,
                    Telephone = item.Telephone,
                    PermanentAddress = item.PermanentAddress,
                    PermanentCity = item.PermanentCity,
                    PermanentPinCode = item.PermanentPinCode,
                    PermanentState = item.PermanentState,
                    PermanentCountryName = item.PermanentCountryName,
                    PeriodOfStay = item.PeriodOfStay,
                    VerificationContactPersonName = item.VerificationContactPersonName,
                    VerificationContactPersonContactNo = item.VerificationContactPersonContactNo,
                    DateOfBirth = item.DateOfBirth,
                    PlaceOfBirth = item.PlaceOfBirth,
                    IsReferredByExistingEmployee = item.IsReferredByExistingEmployee,

                    BloodGroup = item.BloodGroup,
                    PANNo = item.PANNo,
                    AadharCardNo = item.AadharCardNo,
                    Allergies = item.Allergies,
                    IsRelativesWorkingWithCompany = item.IsRelativesWorkingWithCompany,
                    RelativesDetails = item.RelativesDetails,
                    MajorIllnessOrDisability = item.MajorIllnessOrDisability,
                    AwardsAchievements = item.AwardsAchievements,
                    EducationGap = item.EducationGap,
                    ExtraCurricularActivities = item.ExtraCurricularActivities,
                    ForeignCountryVisits = item.ForeignCountryVisits,
                    EmergencyContactPersonName = item.EmergencyContactPersonName,
                    EmergencyContactPersonMobile = item.EmergencyContactPersonMobile,
                    EmergencyContactPersonTelephone = item.EmergencyContactPersonTelephone,
                    EmergencyContactPersonRelationship = item.EmergencyContactPersonRelationship,
                    ITSkillsKnowledge = item.ITSkillsKnowledge,
                    LeavePolicyName = item.LeavePolicyName,
                    Gender = item.Gender,
                    EMPID = item.EMPID,
                    DesignationName = item.DesignationName,
                    Category = item.Category,
                    DepartmentName = item.DepartmentName,
                    Location = item.Location,
                    ProtalkId = item.ProtalkId,
                    OfficialContactNo = item.OfficialContactNo,
                    JoiningDate = item.JoiningDate,
                    DateOfResignation = item.DateOfResignation,
                    ReferredByEmployeeName = item.ReferredByEmployeeName,
                    PayrollTypeName = item.PayrollTypeName,
                    ClientName = item.ClientName,
                    SubDepartmentName = item.SubDepartmentName,
                    ShiftTypeName = item.ShiftTypeName,
                    ESINumber = item.ESINumber,
                    RegistrationDateInESIC = item.RegistrationDateInESIC,
                    BankAccountNumber = item.BankAccountNumber,
                    UANNumber = item.UANNumber,
                    IFSCCode = item.IFSCCode,
                    BankName = item.BankName,
                    AON = item.AON,
                    NoticeServed = item.NoticeServed,
                    LeavingType = item.LeavingType,
                    PreviousExperience = item.PreviousExperience,
                    DOJInTraining = item.DOJInTraining,
                    DOJOnFloor = item.DOJOnFloor,
                    DOJInOJT = item.DOJInOJT,
                    DOJInOnroll = item.DOJInOnroll,
                    DateOfLeaving = item.DateOfLeaving,
                    BackOnFloor = item.BackOnFloor,
                    LeavingRemarks = item.LeavingRemarks,
                    MailReceivedFromAndDate = item.MailReceivedFromAndDate,
                    DateOfEmailSentToITForIDDeletion = item.DateOfEmailSentToITForIDDeletion,
                    EmpCodeofReportingManager = item.EmpCodeofReportingManager,
                    ReportingToIDL2Name = HttpContext.Session.GetString(Constants.EmployeeID),
                    InsertedByUserID = HttpContext.Session.GetString(Constants.UserID),
                    Status = item.Status,
                    SourcingType = item.SourcingType,
                    RefereeName = item.RefereeName,
                    MobileNumberOfReferee = item.MobileNumberOfReferee,
                    DocumentationStatus = item.DocumentationStatus,
                    LOB = item.LOB,
                    NewSubDepartmentName = item.NewSubDepartmentName
                }).ToList();
                var companyNameModel = new BulkEmployeeImportModel
                {
                    Employees = employeeList
                };
                var employeeData = _businessLayer.SendPostAPIRequest(companyNameModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.AddUpdateEmployeeFromExecelBulk), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                if (employeeData != null)
                {
                    var model = JsonConvert.DeserializeObject<Result>(employeeData);

                    return model.Message;

                }

            }

            return ConvertDataTableToHTML(errorDataTable);
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string columnName)
        {
            return columnIndexes.ContainsKey(columnName)
                ? worksheet.Cells[row, columnIndexes[columnName]]?.Value?.ToString()?.Trim()
                : null;
        }
        private bool GetBooleanValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string columnName)
        {
            if (columnIndexes.ContainsKey(columnName) && worksheet.Cells[row, columnIndexes[columnName]]?.Value != null)
            {
                string value = worksheet.Cells[row, columnIndexes[columnName]].Value.ToString().Trim();
                return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("1");
            }
            return false;
        }
        private void AddErrorRow(DataTable errorDataTable, string errorColumn, string errorMessage)
        {
            var row = errorDataTable.NewRow();
            row["ErrorColumn"] = errorColumn;
            row["ErrorMessage"] = errorMessage;
            errorDataTable.Rows.Add(row);
        }
        private (bool isHeaderValid, string mismatchedColumn) ValidateHeaderRow(ExcelWorksheet worksheet, Type targetType)
        {
            var excludeHeaders = new List<string> {  "InsertedByUserID",
        "ExcelFile",
        "CompanyName",
        "ReportingToIDL2Name" ,"NewSubDepartmentName"};
            string Normalize(string input) =>
                string.Concat(input.Where(c => !char.IsWhiteSpace(c))).ToLowerInvariant();
            var expectedProperties = targetType.GetProperties()
                .Where(p => !excludeHeaders.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .Select(p => Normalize(p.Name))
                .ToList();
            for (int i = 0; i < expectedProperties.Count; i++)
            {
                string excelHeader = worksheet.Cells[1, i + 1].Text?.Trim() ?? string.Empty;
                string normalizedExcelHeader = Normalize(excelHeader);
                if (!string.Equals(expectedProperties[i], normalizedExcelHeader, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, $"Expected '{expectedProperties[i]}', Found '{excelHeader}' at Column {i + 1}");
                }
            }
            return (true, "");
        }
        private static string ConvertDataTableToHTML(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                var html = new System.Text.StringBuilder();
                html.Append("<table border='1'>");
                html.Append("<thead><tr>");
                html.Append("<th>Error Location</th>");
                html.Append("<th>Error Message</th>");
                html.Append("</tr></thead>");
                html.Append("<tbody>");
                foreach (DataRow row in dt.Rows)
                {
                    html.Append("<tr>");
                    html.Append("<td>").Append(row["ErrorColumn"]).Append("</td>");
                    html.Append("<td>").Append(row["ErrorMessage"]).Append("</td>");
                    html.Append("</tr>");
                }
                html.Append("</tbody></table>");
                return html.ToString();
            }
            return string.Empty;
        }
        private bool IsRowEmpty(ExcelWorksheet worksheet, int row)
        {
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                    return false;
            }
            return true;
        }
        private void AddError(DataTable errorTable, string col, string message)
        {
            if (errorTable.Select($"ErrorColumn = '{col}' AND ErrorMessage = '{message}'").Length == 0)
            {
                var errorRow = errorTable.NewRow();
                errorRow["ErrorColumn"] = col;
                errorRow["ErrorMessage"] = message;
                errorTable.Rows.Add(errorRow);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UploadRosterExcel()
        {
            var employeeId = GetSessionInt(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);

            var formPermission = _CheckUserFormPermission.GetFormPermission(
                employeeId,
                (int)PageName.WeekOffRoster);

            if (formPermission.HasPermission == 0 &&
                roleId != (int)Roles.Admin &&
                roleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // WeekOff shifts
            var apiResponse = _businessLayer.SendPostAPIRequest(
                null,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.DashBoard,
                    APIApiActionConstants.GetWeekOffShifts),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result?.ToString();

            var weekOffShifts = JsonConvert.DeserializeObject<List<WeekOffShift>>(apiResponse);

            // Cutoff settings
            var cutoff = _cutoffSettingsService.GetCutoffSettings(
                HttpContext.Session.GetString(Constants.SessionBearerToken));

            var model = new UploadRosterExcelViewModel
            {
                WeekOffShifts = weekOffShifts,
                ApplyCutoffDate = cutoff.ApplyCutoffDate,
                AdminEditCutoffDate = cutoff.AdminEditCutoffDate,
                AllowSuperAdminEdit = cutoff.AllowSuperAdminEdit
            };

            return View(model);
        }
        private long GetSessionLong(string key)
        {
            return long.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }

        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }
        [HttpPost]
        public async Task<IActionResult> UploadRosterExcel(IFormFile file, int month, DateTime week)
        {
            string tempFilePath = null;
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file uploaded." });
                var cutoffSettings = _cutoffSettingsService.GetCutoffSettings(
                    HttpContext.Session.GetString(Constants.SessionBearerToken));

                DateTime applyCutoffDate = cutoffSettings.ApplyCutoffDate.Value;
                DateTime adminEditCutoffDate = cutoffSettings.AdminEditCutoffDate.Value;
                bool allowSuperAdminEdit = cutoffSettings.AllowSuperAdminEdit;

                int roleId = Convert.ToInt32(HttpContext.Session.GetString(Constants.RoleID));

                // Default cutoff
                DateTime effectiveCutoffDate = applyCutoffDate;

                // Admin/SuperAdmin can edit only from AdminEditCutoffDate if enabled
                if (allowSuperAdminEdit &&
                    (roleId == (int)Roles.Admin || roleId == (int)Roles.SuperAdmin))
                {
                    effectiveCutoffDate = adminEditCutoffDate;
                }

                if (week.Date <= effectiveCutoffDate.Date)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Week Off cannot be uploaded for weeks up to {effectiveCutoffDate:dd-MMM-yyyy}."
                    });
                }

                tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(file.FileName));
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // Read Excel to DataTable
                var dataTable = ReadExcelToDataTable(tempFilePath);

                // Convert DataTable to strongly-typed model list
                var modelList = await ConvertDataTableToModelList(dataTable, month, week);

                // Validate model list
                var validationError = ValidateModelList(modelList, week);
                if (!string.IsNullOrEmpty(validationError))
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed.",
                        details = validationError
                    });

                var session = HttpContext.Session;
                var employeeIdString = session.GetString(Constants.EmployeeID);
                var token = session.GetString(Constants.SessionBearerToken);

                if (string.IsNullOrEmpty(employeeIdString) || string.IsNullOrEmpty(token))
                    return Unauthorized(new { success = false, message = "Session expired. Please log in again." });

                var employeeId = Convert.ToInt64(employeeIdString);

                var weekOffUploadModel = new WeekOffUploadModelList
                {
                    WeekOffList = modelList,
                    CreatedBy = employeeId
                };

                var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.UploadRosterWeekOff);

                var apiResponse = await _businessLayer.SendPostAPIRequest(weekOffUploadModel, apiUrl, token, true);

                if (apiResponse == null)
                    return StatusCode(500, new { success = false, message = "Failed to upload data to the server. Please try again later." });
                return Ok(new { success = true, message = "Excel uploaded & data inserted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while processing the file.",
                    details = ex.Message
                });
            }
            finally
            {
                // Cleanup temp file
                if (!string.IsNullOrEmpty(tempFilePath) && System.IO.File.Exists(tempFilePath))
                {
                    try { System.IO.File.Delete(tempFilePath); } catch { /* ignore cleanup errors */ }
                }
            }
        }

        private DataTable ReadExcelToDataTable(string filePath)
        {
            var dt = new DataTable();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var ws = package.Workbook.Worksheets.First();
            if (ws.Dimension == null)
                throw new InvalidOperationException("The uploaded Excel file is empty.");

            int colCount = ws.Dimension.End.Column;
            int rowCount = ws.Dimension.End.Row;

            // Add columns using header row (1st row in Excel)
            for (int col = 1; col <= colCount; col++)
                dt.Columns.Add(ws.Cells[1, col].Text);

            // Add data rows starting from row 2
            for (int row = 2; row <= rowCount; row++)
            {
                var dr = dt.NewRow();
                for (int col = 1; col <= colCount; col++)
                    dr[col - 1] = ws.Cells[row, col].Text;
                dt.Rows.Add(dr);
            }

            return dt;
        }
        private async Task<List<WeekOffUploadModel>> ConvertDataTableToModelList(DataTable dt, int month, DateTime weekStartDate)
        {
            var list = new List<WeekOffUploadModel>();

            try
            {
                var shiftResponse = _businessLayer.SendPostAPIRequest(
    null,
    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetShiftDictionary),
    HttpContext.Session.GetString(Constants.SessionBearerToken),
    true
).Result;

                var shiftDictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(shiftResponse.ToString());
                foreach (DataRow row in dt.Rows)
                {
                    var model = new WeekOffUploadModel
                    {
                        EmployeeNumber = row["EmployeeNumber"]?.ToString() ?? "0",
                        DayOff1 = ParseDateIfColumnExists(dt, row, "DayOff1"),
                        DayOff2 = ParseDateIfColumnExists(dt, row, "DayOff2"),
                        // will remain null if column is missing
                        ShiftTypeId = 0 // default
                    };


                    var shiftName = row.Table.Columns.Contains("Shift")
               ? row["Shift"]?.ToString()?.Trim()
               : null;

                    if (!string.IsNullOrWhiteSpace(shiftName))
                    {
                        // Example: "S1 (Morning)"
                        var match = Regex.Match(shiftName, @"^(.*?)\(");
                        string shiftCode = match.Success
                            ? match.Groups[1].Value.Trim().ToLower()
                            : shiftName.Trim().ToLower();

                        if (shiftDictionary.ContainsKey(shiftCode))
                        {
                            model.ShiftTypeId = shiftDictionary[shiftCode];
                        }
                        else
                        {
                            throw new Exception($"Shift '{shiftCode}' does not exist in the system.");
                        }
                    }
                    model.WeekStartDate = weekStartDate;
                    list.Add(model);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting DataTable to WeekOffUploadModel list.", ex);
            }

            return list;
        }

        private DateTime? ParseDateIfColumnExists(DataTable dt, DataRow row, string columnName)
        {
            if (!dt.Columns.Contains(columnName))
                return null;

            var value = row[columnName]?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string[] formats =
            {
                "dd/MM/yyyy",
                "d/M/yyyy",
                "MM/dd/yyyy",
                "M/d/yyyy",
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "dd-MM-yyyy",
                "d-M-yyyy",
                "MM-dd-yyyy",
                "M-d-yyyy",
                "dd MMM yyyy",
                "dd-MMM-yyyy",
                "MMM dd, yyyy",
                "MMMM dd, yyyy"
            };

            // Try known formats first
            if (DateTime.TryParseExact(
                    value,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date))
            {
                return date;
            }

            // Fall back to general parsing
            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out date))
            {
                return date;
            }

            return null;
        }


        private string ValidateModelList(List<WeekOffUploadModel> models, DateTime week)
        {

            var session = HttpContext.Session;
            var employeeId = Convert.ToInt64(session.GetString(Constants.EmployeeID));
            var companyId = Convert.ToInt64(session.GetString(Constants.CompanyID));
            var inputParams = new WeekOfInputParams
            {
                EmployeeID = employeeId,

            };
            var holidayParams = new HolidayInputparams
            {
                CompanyID = companyId,
            };
            var EmployeesHierarchyUnderManager = _businessLayer.SendPostAPIRequest(inputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetEmployeesHierarchyUnderManager), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var EmployeesHierarchyUnderManagerDictionaries = JsonConvert.DeserializeObject<Dictionary<string, long>>(EmployeesHierarchyUnderManager);
            var holidaymodelList = _businessLayer.SendPostAPIRequest(holidayParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetCompanyHoliday), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var holidayList = JsonConvert.DeserializeObject<List<HolidayCompanyList>>(holidaymodelList);
            var errors = new List<string>();

            var employeeNumberToRows = new Dictionary<string, List<int>>();
            for (int i = 0; i < models.Count; i++)
            {
                var item = models[i];
                var rowNum = i + 2;
                if (string.IsNullOrWhiteSpace(item.EmployeeNumber) || item.EmployeeNumber == "0")
                {
                    errors.Add($"Row {rowNum}: Missing or invalid EmployeeNumber.");
                    continue;
                }
                var empNumberLower = item.EmployeeNumber.Trim().ToLower();
                if (!EmployeesHierarchyUnderManagerDictionaries.ContainsKey(empNumberLower))
                {
                    errors.Add($"Row {rowNum}: EmployeeNumber '{item.EmployeeNumber}' does not exist under the current manager.");
                    continue;
                }
                var jobLocationTypeId = EmployeesHierarchyUnderManagerDictionaries[empNumberLower];
                if (!item.WeekStartDate.HasValue)
                {
                    errors.Add($"Row {rowNum}: WeekStartDate is missing.");
                    continue;
                }

                var weekStart = item.WeekStartDate.Value.Date;
                var weekEnd = weekStart.AddDays(6);
                if (!item.DayOff1.HasValue)
                {
                    errors.Add($"Row {rowNum}: DayOff1 is mandatory.");
                    continue;
                }


                var weekOffDates = new List<DateTime>();
                var fields = new Dictionary<string, DateTime?>()
        {
            { "DayOff1", item.DayOff1 },
            { "DayOff2", item.DayOff2 },

        };

                foreach (var kvp in fields)
                {
                    if (kvp.Value.HasValue)
                    {
                        var date = kvp.Value.Value.Date;


                        if (date < weekStart || date > weekEnd)
                        {
                            errors.Add($"Row {rowNum}: {kvp.Key} ({date:yyyy-MM-dd}) is outside the week range {weekStart:yyyy-MM-dd} to {weekEnd:yyyy-MM-dd}.");
                        }
                        else
                        {
                            var holiday = holidayList.FirstOrDefault(h =>
               h.JobLocationTypeID == jobLocationTypeId &&
               h.FromDate <= date && h.ToDate >= date &&
               h.Status == true);

                            if (holiday != null)
                            {
                                errors.Add($"Row {rowNum}: {kvp.Key} ({date:yyyy-MM-dd}) falls on a holiday ");
                            }
                            else
                            {
                                weekOffDates.Add(date);
                            }
                        }
                    }
                }


                var duplicateDates = weekOffDates
                    .GroupBy(d => d)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key.ToString("yyyy-MM-dd"))
                    .ToList();

                if (duplicateDates.Any())
                {
                    errors.Add($"Row {rowNum}: Duplicate WeekOff dates found: {string.Join(", ", duplicateDates)}.");
                }
            }



            return errors.Any() ? string.Join("\n", errors) : null;
        }


        private void AddDateIfValid(List<DateTime> dates, DateTime? date, int rowNum, string fieldName, List<string> errors)
        {
            if (date.HasValue)
            {
                if (date.Value.Year < 2000)
                    errors.Add($"Row {rowNum}: {fieldName} '{date.Value}' is an invalid or unreasonable date.");
                else
                    dates.Add(date.Value.Date);  // Always add as date-only (ignores time part)
            }
        }

        #region Update Excel

        public IActionResult ImportExcelUpdate()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ImportExcelUpdateOnly(IFormFile file)
        {
            if (file == null || !(file.ContentType.Contains("excel") || file.FileName.EndsWith(".xlsx")))
            {
                return Json(new { success = false, message = "Invalid file type." });
            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                string result = ProcessExcelFile_UpdateOnly(stream);

                if (result.StartsWith("<table")) // error HTML
                {
                    return Json(new
                    {
                        success = false,
                        hasErrors = true,
                        errorTable = result
                    });
                }

                return Json(new
                {
                    success = true,
                    message = result
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "File processing failed."
                });
            }
        }

        private string ProcessExcelFile_UpdateOnly(Stream stream)
        {
            var errorTable = new DataTable();
            errorTable.Columns.Add("EmpID");
            errorTable.Columns.Add("ErrorColumn");
            errorTable.Columns.Add("ErrorMessage");

            var updateList = new List<EmployeeUpdateImportModel>();
            var uniqueEmpNos = new HashSet<string>();

            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets.FirstOrDefault();

            if (sheet == null)
            {
                AddErrorRow(errorTable, "", "Excel sheet is empty.");
                return ConvertDataTableToHTML(errorTable);
            }
            var expectedHeaders = new[]
   {
        "Emp ID",
        "Employee Name",
        "Client Name",
        "LOB",
        "Reporting TL Emp ID",
        "Shift Timing",
        "Status"
    };
            string NormalizeHeader(string header)
            {
                return header?
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Replace("\"", "")
                    .Trim()
                    .ToLowerInvariant();
            }
            int actualColumnCount = sheet.Dimension.Columns;
            if (actualColumnCount < expectedHeaders.Length)
            {
                AddErrorRow(
                    errorTable,
                    "",
                    $"Invalid Excel format. Expected {expectedHeaders.Length} columns, found {actualColumnCount}."
                );
                return ConvertDataTableToHTML(errorTable);
            }

            for (int col = 1; col <= expectedHeaders.Length; col++)
            {
                var actualHeader = NormalizeHeader(sheet.Cells[1, col].Text);
                var expectedHeader = NormalizeHeader(expectedHeaders[col - 1]);

                if (actualHeader != expectedHeader)
                {
                    AddErrorRow(
                        errorTable,
                        "",
                        $"Invalid header at column {col}. Expected '{expectedHeaders[col - 1]}', found '{sheet.Cells[1, col].Text}'."
                    );
                }
            }


            if (errorTable.Rows.Count > 0)
            {
                return ConvertDataTableToHTML(errorTable);
            }
            long? companyId = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));
            var dictResponse = _businessLayer.SendPostAPIRequest(
     companyId ?? 0,
     _businessLayer.GetFormattedAPIUrl(
         APIControllarsConstants.DashBoard,
         APIApiActionConstants.GetEmployeeAndShiftDictionaries
     ),
     HttpContext.Session.GetString(Constants.SessionBearerToken),
     true
 ).Result?.ToString();
            if (string.IsNullOrWhiteSpace(dictResponse))
            {
                AddErrorRow(errorTable, "", "Failed to load employee/shift master data.");
                return ConvertDataTableToHTML(errorTable);
            }

            var dictionaries = JsonConvert.DeserializeObject<EmployeeShiftDictionaryResponse>(dictResponse);

            var employeeDict = dictionaries?.Employees ?? new Dictionary<string, long>();
            var shiftDict = dictionaries?.ShiftTypes ?? new Dictionary<string, long>();


            int totalRows = sheet.Dimension.Rows;

            for (int row = 2; row <= totalRows; row++)
            {
                bool hasError = false;

                string empNo = sheet.Cells[row, 1].Text?.Trim();
                string fullName = sheet.Cells[row, 2].Text?.Trim();
                string clientName = sheet.Cells[row, 3].Text?.Trim();
                string lob = sheet.Cells[row, 4].Text?.Trim();
                string reportingTLEmpId = sheet.Cells[row, 5].Text?.Trim();
                string shiftType = sheet.Cells[row, 6].Text?.Trim();
                string status = sheet.Cells[row, 7].Text?.Trim();

                if (string.IsNullOrWhiteSpace(empNo))
                {
                    AddErrorRow(errorTable, "", $"Row {row}: Emp ID is mandatory.");
                    hasError = true;
                }
                else
                {
                    if (!employeeDict.ContainsKey(empNo))
                    {
                        AddErrorRow(errorTable, empNo, $"Row {row}: Employee not found in system.");
                        hasError = true;
                    }

                    if (!uniqueEmpNos.Add(empNo))
                    {
                        AddErrorRow(errorTable, empNo, $"Row {row}: Duplicate EmployeeNumber in Excel.");
                        hasError = true;
                    }
                }


                int? shiftTypeId = null;

                if (string.IsNullOrWhiteSpace(shiftType))
                {
                    AddErrorRow(errorTable, empNo, $"Row {row}: ShiftType is required.");
                    hasError = true;
                }
                else
                {
                    var matchedShift = shiftDict.FirstOrDefault(kvp =>
                    {
                        var apiShiftCode = kvp.Key
                            .Split(' ', '(')[0]
                            .Trim();

                        return apiShiftCode.Equals(shiftType.Trim(), StringComparison.OrdinalIgnoreCase);
                    });

                    if (!matchedShift.Equals(default(KeyValuePair<string, long>)))
                    {
                        shiftTypeId = (int)matchedShift.Value;
                    }
                    else
                    {
                        AddErrorRow(errorTable, empNo, $"Row {row}: ShiftType '{shiftType}' not found.");
                        hasError = true;
                    }
                }


                bool? isActive = null;

                if (string.IsNullOrWhiteSpace(status))
                {
                    AddErrorRow(errorTable, empNo, $"Row {row}: Status is mandatory.");
                    hasError = true;
                }
                else
                {
                    switch (status.Trim().ToLowerInvariant())
                    {
                        case "1":
                        case "active":
                            isActive = true;
                            break;

                        case "0":
                        case "inactive":
                            isActive = false;
                            break;

                        default:
                            AddErrorRow(
                                errorTable,
                                empNo,
                                $"Row {row}: Invalid Status '{status}'. Allowed values: Active / Inactive / 1 / 0."
                            );
                            hasError = true;
                            break;
                    }
                }

                /* =========================
                   FINAL ADD (ONLY IF CLEAN)
                   ========================= */
                if (hasError)
                    continue;

                updateList.Add(new EmployeeUpdateImportModel
                {
                    EmployeeNumber = empNo,
                    FullName = fullName,
                    ReportingManagerNo = reportingTLEmpId,
                    ClientName = clientName,
                    LOB = lob,
                    ShiftTypeID = shiftTypeId,
                    IsActive = isActive
                });
            }

            if (errorTable.Rows.Count > 0)
                return ConvertDataTableToHTML(errorTable);
            var requestModel = new
            {
                Employees = updateList,
                LoggedInUserId = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.EmployeeID)
                )
            };
            var apiResponse = _businessLayer.SendPostAPIRequest(
                requestModel,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.DashBoard,
                    APIApiActionConstants.UpdateEmployeesFromExcel
                ),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            ).Result?.ToString();

            if (string.IsNullOrWhiteSpace(apiResponse))
                return "Employee update failed. No response from API.";

            var result = JsonConvert.DeserializeObject<Result>(apiResponse);

            return result?.Message ?? "Employee update completed.";
        }

        #endregion Update Excel


        #region Notification
        [HttpPost]
        public IActionResult MarkNotificationAsRead([FromBody] MarkNotificationReadInput input)
        {
            if (input == null) return BadRequest();

            try
            {
                var request = new
                {
                    ReferenceId = input.ReferenceId,
                    Type = input.Type
                };

                var apiResponse = _businessLayer.SendPostAPIRequest(
                    request,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.MarkAsReadNotification),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                return Content(apiResponse, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error marking notification as read", Details = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetManagerPendingNotifications()
        {
            try
            {
                var request = new
                {
                    ReportingToEmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID)),
                    NotificationType = 1
                };

                var apiResponse = _businessLayer.SendPostAPIRequest(
                    request,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetManagerPendingNotifications),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                return Content(apiResponse, "application/json");
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        public async Task<IActionResult> GetManagerApprovalCount()
        {
            var request= new
            {
                reportingToEmployeeID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID))
            };        
            var apiResponse = await _businessLayer.SendPostAPIRequest(
                request,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.DashBoard,
                    APIApiActionConstants.GetManagerApprovalCount
                ),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true
            );
            return Content(apiResponse.ToString(), "application/json");
        }


        #endregion Notification


        #region AttendanceCorrection
        [HttpGet]
        public IActionResult UploadAttendanceExcel()
        {
            var employeeId = GetSessionLong(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);

            if (roleId != (int)Roles.Admin &&
                roleId != (int)Roles.SuperAdmin)
            {

                return RedirectToActionPermanent(
                    Constants.Index,
                    _businessLayer.GetControllarNameByRole(roleId),
                    new { area = "admin" }
                );
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadAttendanceExcel(IFormFile file)
        {
            string tempFilePath = null;

            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please upload attendance file."
                    });
                }


                var roleId = GetSessionInt(Constants.RoleID);


                // Only Admin / Super Admin
                if (roleId != (int)Roles.Admin &&
                    roleId != (int)Roles.SuperAdmin)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "You do not have permission to import attendance."
                    });
                }



                var employeeIdString =
                    HttpContext.Session.GetString(Constants.EmployeeID);


                var token =
                    HttpContext.Session.GetString(Constants.SessionBearerToken);



                if (string.IsNullOrEmpty(employeeIdString) ||
                    string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Session expired."
                    });
                }



                long userId = Convert.ToInt64(employeeIdString);



                // Save temporary file

                tempFilePath = Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid() + Path.GetExtension(file.FileName)
                );


                using (FileStream stream = new FileStream(
                    tempFilePath,
                    FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }



                // Read Excel

                DataTable dataTable = ReadAttendanceExcelToDataTable(tempFilePath);



                // Convert Wide Excel to Normalized List

                List<AttendanceUploadModel> attendanceList =
                    ConvertAttendanceExcel(dataTable);
                // Attendance Cutoff Validation

                var cutoffSettings = _cutoffSettingsService.GetCutoffSettings(
            HttpContext.Session.GetString(Constants.SessionBearerToken));

                DateTime hardCutoffDate = cutoffSettings.AttendanceCutoffDate.Value;
                DateTime adminEditCutoffDate = cutoffSettings.AdminEditCutoffDate.Value;
                bool allowSuperAdminEdit = cutoffSettings.AllowSuperAdminEdit;

                DateTime effectiveCutoffDate = hardCutoffDate;


                // Admin/SuperAdmin allowed different cutoff
                if (allowSuperAdminEdit &&
                    (roleId == (int)Roles.Admin ||
                     roleId == (int)Roles.SuperAdmin))
                {
                    effectiveCutoffDate = adminEditCutoffDate;
                }


                // Block old attendance upload
                var invalidRecords = attendanceList
                    .Where(x => x.WorkDate.HasValue &&
                                x.WorkDate.Value.Date <= effectiveCutoffDate.Date)
                    .ToList();


                if (invalidRecords.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Attendance records before {effectiveCutoffDate.AddDays(1):dd-MMM-yyyy} cannot be uploaded."
                    });
                }


                if (attendanceList.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No attendance records found."
                    });
                }



                // Create API Model

                AttendanceUploadModelList request =
                    new AttendanceUploadModelList
                    {
                        UserID = userId,
                        AttendanceList = attendanceList
                    };



                var apiUrl =
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Employee,
                        APIApiActionConstants.UploadAttendanceCorrections
                    );



                var response = await _businessLayer.SendPostAPIRequest(
    request,
    apiUrl,
    token,
    true
);

                if (response == null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Attendance import failed."
                    });
                }

                var wrapper = JObject.Parse(response.ToString());

                var apiResult = JObject.Parse(wrapper["rawResult"]?.ToString() ?? "{}");

                return Ok(new
                {
                    success = apiResult["success"]?.Value<bool>() ?? false,
                    message = apiResult["message"]?.ToString(),
                    importID = apiResult["importID"]?.Value<long>() ?? 0,
                    processedRecords = apiResult["processedRecords"]?.Value<int>() ?? 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error while importing attendance.",
                    details = ex.Message
                });
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFilePath) &&
                   System.IO.File.Exists(tempFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    catch
                    {

                    }
                }
            }
        }
        private DataTable ReadAttendanceExcelToDataTable(string filePath)
        {
            var dt = new DataTable();

            using var package = new ExcelPackage(new FileInfo(filePath));

            var ws = package.Workbook.Worksheets.First();

            if (ws.Dimension == null)
                throw new Exception("Excel file is empty.");

            int columns = ws.Dimension.End.Column;
            int rows = ws.Dimension.End.Row;


            for (int col = 1; col <= columns; col++)
            {
                var header = ws.Cells[1, col].Text.Trim();

                if (string.IsNullOrEmpty(header))
                {
                    header = "Column" + col;
                }


                if (dt.Columns.Contains(header))
                {
                    throw new Exception(
                        $"Duplicate column found in Excel: {header}"
                    );
                }


                dt.Columns.Add(header);
            }


            for (int row = 2; row <= rows; row++)
            {
                DataRow dr = dt.NewRow();

                for (int col = 1; col <= columns; col++)
                {
                    dr[col - 1] = ws.Cells[row, col].Text.Trim();
                }

                dt.Rows.Add(dr);
            }


            return dt;
        }
        private List<AttendanceUploadModel> ConvertAttendanceExcel(DataTable dt)
        {
            var list = new List<AttendanceUploadModel>();

            if (!dt.Columns.Contains("EmployeeNumber"))
                throw new Exception("EmployeeNumber column missing.");

            if (!dt.Columns.Contains("EmployeeName"))
                throw new Exception("EmployeeName column missing.");

            var validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "P",
        "A",
        "HD",
        "WO",
        "PL",
        "HPL",
        "COL",
        "HCOL",
        "HCOL/HPL",
        "LWP",
        "HLWP",
        "HD/HPL",
                "HD/HCOL",
                "HD/HLWP",
            "LEFT",
            "H",
            "HECO",
            "ECO",
            "-"// if your business supports it
    };

            foreach (DataRow row in dt.Rows)
            {
                string employeeNumber = row["EmployeeNumber"]?.ToString()?.Trim();
                string employeeName = row["EmployeeName"]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(employeeNumber))
                    throw new Exception("Employee Number is required.");

                if (string.IsNullOrWhiteSpace(employeeName))
                    throw new Exception($"Employee Name is required for Employee '{employeeNumber}'.");

                for (int c = 3; c <= dt.Columns.Count; c++)
                {
                    string columnName = dt.Columns[c - 1].ColumnName;

                    if (columnName.Equals("Payable Day's", StringComparison.OrdinalIgnoreCase) ||
                        columnName.Equals("TotalWorkingDays", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!DateTime.TryParse(columnName, out DateTime workDate))
                        continue;

                    string status = row[c - 1]?.ToString()?.Trim().ToUpper();

                    if (string.IsNullOrWhiteSpace(status))
                        continue;

                    if (!validStatuses.Contains(status))
                    {
                        throw new Exception(
                            $"Invalid Status '{status}' for Employee '{employeeNumber}' on {workDate:dd-MMM-yyyy}."
                        );
                    }

                    if (status == "LEFT" ||
                        status == "H" ||
                        status == "HECO" ||
                        status == "ECO" ||
                        status == "-")
                    {
                        continue;
                    }


                    list.Add(new AttendanceUploadModel
                    {
                        EmployeeNumber = employeeNumber,
                        WorkDate = workDate,
                        AttendanceStatus = status,
                        Remarks = "Attendance corrected by Admin as per request."
                    });
                }
            }

            var duplicate = list
                .GroupBy(x => new { x.EmployeeNumber, x.WorkDate })
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate != null)
            {
                throw new Exception(
                    $"Duplicate attendance found for Employee '{duplicate.Key.EmployeeNumber}' on {duplicate.Key.WorkDate:dd-MMM-yyyy}."
                );
            }

            return list;
        }
        #endregion
        #region calcaulteSalary 
        [HttpGet]
        public IActionResult ImportBulkSalaryExcel()
        {
            var employeeId = GetSessionLong(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);

            if (roleId != (int)Roles.Admin &&
                roleId != (int)Roles.SuperAdmin)
            {

                return RedirectToActionPermanent(
                    Constants.Index,
                    _businessLayer.GetControllarNameByRole(roleId),
                    new { area = "admin" }
                );
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ImportBulkSalaryExcel(IFormFile file)
        {
            string tempFilePath = null;

            try
            {
                // ------------------------------------------------------------
                // FILE VALIDATION
                // ------------------------------------------------------------

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please upload salary file."
                    });
                }


                // ------------------------------------------------------------
                // GET USER ID AND TOKEN
                // ------------------------------------------------------------

                var employeeIdString =
                    HttpContext.Session.GetString(Constants.EmployeeID);

                var token =
                    HttpContext.Session.GetString(Constants.SessionBearerToken);

                if (string.IsNullOrEmpty(employeeIdString) ||
                    string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Session expired."
                    });
                }

                long userId = Convert.ToInt64(employeeIdString);


                // ------------------------------------------------------------
                // SAVE TEMPORARY FILE
                // ------------------------------------------------------------

                tempFilePath = Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid() + Path.GetExtension(file.FileName)
                );

                using (FileStream stream = new FileStream(
                    tempFilePath,
                    FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }


                // ------------------------------------------------------------
                // READ EXCEL
                // ------------------------------------------------------------

                DataTable dataTable =
                    ReadBulkSalaryExcelToDataTable(tempFilePath);


                // ------------------------------------------------------------
                // CONVERT EXCEL TO MODEL LIST
                // ------------------------------------------------------------

                List<BulkEmployeeSalaryRequestModel> salaryList =
                    ConvertBulkSalaryExcel(dataTable);


                if (salaryList == null || salaryList.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No salary records found."
                    });
                }


                // ------------------------------------------------------------
                // API REQUEST MODEL
                // ------------------------------------------------------------

                BulkSalaryImportRequestModel request =
                    new BulkSalaryImportRequestModel
                    {
                        UserID = userId,
                        FileName = file.FileName,
                        SalaryList = salaryList
                    };


                // ------------------------------------------------------------
                // API URL
                // ------------------------------------------------------------

                var apiUrl =
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Employee,
                        APIApiActionConstants.CalculateBulkEmployeeSalary
                    );


                // ------------------------------------------------------------
                // CALL API
                // ------------------------------------------------------------

                var response = await _businessLayer.SendPostAPIRequest(
                    request,
                    apiUrl,
                    token,
                    true
                );


                // ------------------------------------------------------------
                // API RESPONSE
                // ------------------------------------------------------------

                if (response == null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Bulk salary import failed."
                    });
                }

                string json = response.ToString();

                // response is JSON string, so unwrap it first
                if (json.StartsWith("\"") && json.EndsWith("\""))
                {
                    json = JsonConvert.DeserializeObject<string>(json);
                }

                JObject apiResult = JObject.Parse(json);

                bool hasFailures =
                    (apiResult["failedRecords"]?.Value<int>() ?? 0) > 0;

                Guid batchId = Guid.Empty;

                Guid.TryParse(
                    apiResult["batchID"]?.ToString(),
                    out batchId
                );

                // ------------------------------------------------------------
                // CONVERT API ERRORS PROPERLY
                // ------------------------------------------------------------

                var errors = new List<object>();

                var apiErrors = apiResult["errors"] as JArray;

                if (apiErrors != null)
                {
                    foreach (var error in apiErrors)
                    {
                        // If API returned proper object
                        if (error is JObject errorObject)
                        {
                            errors.Add(new
                            {
                                rowNumber =
                                    errorObject["rowNumber"]?.Value<int?>()
                                    ?? errorObject["RowNumber"]?.Value<int?>(),

                                employeeNumber =
                                    errorObject["employeeNumber"]?.ToString()
                                    ?? errorObject["EmployeeNumber"]?.ToString(),

                                payrollType =
                                    errorObject["payrollType"]?.ToString()
                                    ?? errorObject["PayrollType"]?.ToString(),

                                salaryYear =
                                    errorObject["salaryYear"]?.Value<int?>()
                                    ?? errorObject["SalaryYear"]?.Value<int?>(),

                                salaryMonth =
                                    errorObject["salaryMonth"]?.Value<int?>()
                                    ?? errorObject["SalaryMonth"]?.Value<int?>(),

                                errorMessage =
                                    errorObject["errorMessage"]?.ToString()
                                    ?? errorObject["ErrorMessage"]?.ToString()
                            });
                        }
                    }
                }


                // ------------------------------------------------------------
                // RETURN RESPONSE TO UI
                // ------------------------------------------------------------

                return Ok(new
                {
                    success = !hasFailures,

                    message = hasFailures
                        ? "Bulk salary import completed with some failed records."
                        : "Bulk salary import completed successfully.",

                    batchID = batchId,

                    totalRecords =
                        apiResult["totalRecords"]?.Value<int>() ?? 0,

                    validRecords =
                        apiResult["validRecords"]?.Value<int>() ?? 0,

                    failedRecords =
                        apiResult["failedRecords"]?.Value<int>() ?? 0,

                    insertedRecords =
                        apiResult["insertedRecords"]?.Value<int>() ?? 0,

                    updatedRecords =
                        apiResult["updatedRecords"]?.Value<int>() ?? 0,

                    skippedRecords =
                        apiResult["skippedRecords"]?.Value<int>() ?? 0,

                    errors = errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error while importing bulk salary.",
                    details = ex.Message
                });
            }
            finally
            {
                // ------------------------------------------------------------
                // DELETE TEMP FILE
                // ------------------------------------------------------------

                if (!string.IsNullOrEmpty(tempFilePath) &&
                    System.IO.File.Exists(tempFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    catch
                    {
                    }
                }
            }
        }
        private DataTable ReadBulkSalaryExcelToDataTable(string filePath)
        {
            var dt = new DataTable();

            using var package =
                new ExcelPackage(new FileInfo(filePath));

            var ws =
                package.Workbook.Worksheets.FirstOrDefault();

            if (ws == null || ws.Dimension == null)
            {
                throw new Exception("Excel file is empty.");
            }

            int columns = ws.Dimension.End.Column;
            int rows = ws.Dimension.End.Row;


            // ------------------------------------------------------------
            // READ HEADERS
            // ------------------------------------------------------------

            for (int col = 1; col <= columns; col++)
            {
                string header =
                    ws.Cells[1, col].Text.Trim();

                if (string.IsNullOrEmpty(header))
                {
                    header = "Column" + col;
                }

                if (dt.Columns.Contains(header))
                {
                    throw new Exception(
                        $"Duplicate column found in Excel: {header}"
                    );
                }

                dt.Columns.Add(header);
            }


            // ------------------------------------------------------------
            // READ DATA
            // ------------------------------------------------------------

            for (int row = 2; row <= rows; row++)
            {
                DataRow dr = dt.NewRow();

                for (int col = 1; col <= columns; col++)
                {
                    dr[col - 1] =
                        ws.Cells[row, col].Text.Trim();
                }

                dt.Rows.Add(dr);
            }


            return dt;
        }
        private List<BulkEmployeeSalaryRequestModel> ConvertBulkSalaryExcel(DataTable dt)
        {
            var list = new List<BulkEmployeeSalaryRequestModel>();

            // ------------------------------------------------------------
            // REQUIRED COLUMNS
            // ------------------------------------------------------------

            string[] requiredColumns =
            {
        "EmployeeNumber",
        "PayrollType",
        "Year",
        "Month",
        "GrossSalary"
    };

            foreach (string column in requiredColumns)
            {
                if (!dt.Columns.Contains(column))
                {
                    throw new Exception(
                        $"'{column}' column missing in Excel."
                    );
                }
            }

            // ------------------------------------------------------------
            // PROCESS ROWS
            // ------------------------------------------------------------

            int rowNumber = 1;

            foreach (DataRow row in dt.Rows)
            {
                rowNumber++;

                // --------------------------------------------------------
                // EMPLOYEE NUMBER
                // --------------------------------------------------------

                string employeeNumber =
                    row["EmployeeNumber"]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(employeeNumber))
                {
                    throw new Exception(
                        $"Employee Number is required at Excel row {rowNumber}."
                    );
                }

                // --------------------------------------------------------
                // PAYROLL TYPE
                // --------------------------------------------------------

                string payrollType =
                    row["PayrollType"]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(payrollType))
                {
                    throw new Exception(
                        $"Payroll Type is required for Employee '{employeeNumber}'."
                    );
                }

                // --------------------------------------------------------
                // YEAR
                // --------------------------------------------------------

                if (!int.TryParse(
                        row["Year"]?.ToString(),
                        out int year))
                {
                    throw new Exception(
                        $"Invalid Year for Employee '{employeeNumber}'."
                    );
                }

                if (year < 2000 || year > 2100)
                {
                    throw new Exception(
                        $"Invalid Year '{year}' for Employee '{employeeNumber}'."
                    );
                }

                // --------------------------------------------------------
                // MONTH
                // --------------------------------------------------------

                string monthValue =
                    row["Month"]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(monthValue))
                {
                    throw new Exception(
                        $"Month is required for Employee '{employeeNumber}'."
                    );
                }

                int month;

                // Excel contains numeric month: 1-12
                if (int.TryParse(monthValue, out int numericMonth))
                {
                    month = numericMonth;
                }
                // Excel contains month name: Jan / January / Jul / July
                else if (DateTime.TryParseExact(
                            monthValue,
                            new[]
                            {
                        "MMM",
                        "MMMM"
                            },
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime parsedMonth))
                {
                    month = parsedMonth.Month;
                }
                else
                {
                    throw new Exception(
                        $"Invalid Month '{monthValue}' for Employee '{employeeNumber}'."
                    );
                }

                if (month < 1 || month > 12)
                {
                    throw new Exception(
                        $"Invalid Month '{monthValue}' for Employee '{employeeNumber}'."
                    );
                }

                // --------------------------------------------------------
                // GROSS SALARY
                // --------------------------------------------------------

                if (!decimal.TryParse(
                        row["GrossSalary"]?.ToString(),
                        out decimal grossSalary))
                {
                    throw new Exception(
                        $"Invalid Gross Salary for Employee '{employeeNumber}'."
                    );
                }

                if (grossSalary < 0)
                {
                    throw new Exception(
                        $"Gross Salary cannot be negative for Employee '{employeeNumber}'."
                    );
                }

                // --------------------------------------------------------
                // CREATE MODEL
                // --------------------------------------------------------

                var model = new BulkEmployeeSalaryRequestModel
                {
                    // ----------------------------------------------------
                    // BASIC INFORMATION
                    // ----------------------------------------------------

                    RowNumber = rowNumber,

                    EmployeeNumber = employeeNumber,

                    PayrollType = payrollType,

                    Year = year,

                    Month = month,

                    GrossSalary = grossSalary,

                    // ----------------------------------------------------
                    // ATTENDANCE
                    // ----------------------------------------------------

                    MonthDays = GetNullableDecimal(
                        row,
                        "MonthDays"
                    ),

                    PayableDays = GetNullableDecimal(
                        row,
                        "PayableDays"
                    ),

                    // ----------------------------------------------------
                    // EARNINGS
                    // ----------------------------------------------------

                    ClientIncentive =
                        GetDecimal(
                            row,
                            "ClientIncentive"
                        ),

                    PLI =
                        GetDecimal(
                            row,
                            "PLI"
                        ),

                    FloorIncentive =
                        GetDecimal(
                            row,
                            "FloorIncentive"
                        ),

                    EmployeeReferral =
                        GetDecimal(
                            row,
                            "EmployeeReferral"
                        ),

                    TrainingFee =
                        GetDecimal(
                            row,
                            "TrainingFee"
                        ),

                    GWR =
                        GetDecimal(
                            row,
                            "GWR"
                        ),

                    // IMPORTANT:
                    // Excel header is "OtherAddition/Arrear"
                    // Model property is "OtherAdditionArrear"

                    OtherAdditionArrear =
                        GetDecimal(
                            row,
                            "OtherAddition/Arrear"
                        ),

                    // ----------------------------------------------------
                    // DEDUCTIONS
                    // ----------------------------------------------------

                    EMPLWF =
                        GetDecimal(
                            row,
                            "EMPLWF"
                        ),

                    TDS =
                        GetDecimal(
                            row,
                            "TDS"
                        ),

                    DBTDeduction =
                        GetDecimal(
                            row,
                            "DBTDeduction"
                        ),

                    // IMPORTANT:
                    // Excel header is "AdvanceDED"
                    // Model property is "AdvanceDeduction"

                    AdvanceDeduction =
                        GetDecimal(
                            row,
                            "AdvanceDED"
                        ),

                    InsuranceDeduction =
                        GetDecimal(
                            row,
                            "InsuranceDeduction"
                        ),

                    OtherDeduction =
                        GetDecimal(
                            row,
                            "OtherDeduction"
                        )
                };

                // --------------------------------------------------------
                // ADD MODEL TO LIST
                // --------------------------------------------------------

                list.Add(model);
            }

            // ------------------------------------------------------------
            // RETURN
            // ------------------------------------------------------------

            return list;
        }
        private decimal GetDecimal(
    DataRow row,
    string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return 0;
            }

            string value =
                row[columnName]?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }

            throw new Exception(
                $"Invalid decimal value '{value}' in column '{columnName}'."
            );
        }
        private decimal? GetNullableDecimal(
    DataRow row,
    string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                return null;
            }

            string value =
                row[columnName]?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }

            throw new Exception(
                $"Invalid decimal value '{value}' in column '{columnName}'."
            );
        }
        #endregion
    }
}
