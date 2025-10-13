using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.ImportFromExcel;
using HRMS.Models.LeavePolicy;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Formats.Asn1;
using System.Globalization;
using System.Data;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using DocumentFormat.OpenXml.Spreadsheet;
using ClosedXML.Excel;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HRMS.Web.BusinessLayer.S3;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using System.Diagnostics;
using HRMS.Models.ExportEmployeeExcel;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authentication;
using System.Text.RegularExpressions;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize]
    public class DashBoardController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        IHttpContextAccessor _context;
        private readonly IS3Service _s3Service;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;
        public DashBoardController(ICheckUserFormPermission CheckUserFormPermission,IConfiguration configuration, IBusinessLayer businessLayer, Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IHttpContextAccessor context, IS3Service s3Service)
        {
            Environment = _environment;
            _configuration = configuration;
            _context = context;
            _businessLayer = businessLayer;
            _s3Service = s3Service;
            _CheckUserFormPermission = CheckUserFormPermission;

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

            if (model != null)
            {
                var leavePolicy = GetLeavePolicyData(companyId, model.LeavePolicyId ?? 0);

                // Fiscal year start date (March 21)
                DateTime today = DateTime.Today;
                DateTime fiscalYearStart = (today.Month > 3 || (today.Month == 3 && today.Day >= 21))
                    ? new DateTime(today.Year, 3, 21)
                    : new DateTime(today.Year - 1, 3, 21);

                // Filter approved Annual and Medical leaves from current fiscal year
                var approvedLeaves = model.leaveResults?.leavesSummary?
                    .Where(x => x.StartDate >= fiscalYearStart &&
                                x.LeaveStatusID == (int)LeaveStatus.Approved &&
                                (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                    .ToList();

                decimal approvedLeaveDays = approvedLeaves?.Sum(x => x.NoOfDays) ?? 0.0m;
                double approvedLeaveTotal = (double)approvedLeaveDays;
                const double maxAnnualLeaveLimit = 30;

                if (leavePolicy != null && model.JoiningDate != null)
                {
                    DateTime joinDate = model.JoiningDate.Value;

                    double accruedLeave = 0;
                    double carryForward = 0;

                    // Only calculate accrued leave if approved leave < max limit
                    if (approvedLeaveTotal < maxAnnualLeaveLimit)
                    {
                        accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(joinDate, leavePolicy.Annual_MaximumLeaveAllocationAllowed);
                        if (leavePolicy.Annual_IsCarryForward)
                        {
                            carryForward = Convert.ToDouble(model.CarryForword);
                        }
                    }

                    // Total earned leave capped by max limit
                    double totalEarnedLeave = Math.Min(accruedLeave + carryForward, maxAnnualLeaveLimit);

                    // Remaining leave = total earned minus approved leaves (minimum 0)
                    double remainingLeave = Math.Max(totalEarnedLeave - approvedLeaveTotal, 0);

                    // Assign values to model for the View
                    //   model.TotalLeave = (decimal)approvedLeaveTotal;            // Leaves already taken
                    model.NoOfLeaves = Convert.ToInt64(remainingLeave);        // Leaves remaining (available)
                    ViewBag.NoOfLeaves = remainingLeave;        // Leaves remaining (available)
                    ViewBag.RoleID = roleId;
                    
                    ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicy.Annual_MaximumConsecutiveLeavesAllowed);
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

            // Fiscal year starts from March 21st of current or previous year
            //DateTime fiscalYearStart = new DateTime(today.Month > 3 || (today.Month == 3 && today.Day >= 21)
            //                                        ? today.Year : today.Year - 1, 3, 21);
            //DateTime fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-1); // Ends on March 20th next year


            DateTime fiscalYearStart;
            DateTime fiscalYearEnd;

            if (today.Year == 2025)
            {
                // ✅ Special case: 2025 fiscal year is from 21 May 2025 to 20 March 2026
                fiscalYearStart = new DateTime(2025, 8, 21);
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
            return totalAccruedLeave;
        }

        // Helper method: Gets the 21st-based accrual period start for any given date
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
                var employmentDetailsDictionaries = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long>>>(EmploymentDetailsDictionaries);
                EmployeeInputParams employmentSubDepartmentInputParams = new EmployeeInputParams()
                {
                    CompanyID = companyId ?? 0,
                };
                var EmploymentSubDepartment = _businessLayer.SendPostAPIRequest(employmentSubDepartmentInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetSubDepartmentDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var SubDepartmentDictionaries = JsonConvert.DeserializeObject<Dictionary<string, long>>(EmploymentSubDepartment);
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
                                        if (DateTime.TryParse(cellValue, out DateTime dob))
                                        {
                                            prop.SetValue(item, dob.ToString("yyyy-MM-dd"));
                                        }
                                    }
                                    else
                                    {
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: DateOfBirth is mandatory.");
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
                                case "JoiningDate":
                                    if (!string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        if (DateTime.TryParse(cellValue, out DateTime joiningDate))
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
                                    else if (employmentDetailsDictionaries.TryGetValue("JobLocations", out var JobLocationNameDict))
                                    {
                                        var matchedPolicy = JobLocationNameDict
                                            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));

                                        JobLocationId = matchedPolicy.Value;
                                        prop.SetValue(item, JobLocationId.ToString());

                                        if (JobLocationId == 0)
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: Location  not found in master data.");
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
                                        var matchedPolicy = leavePolicyDict
                                            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                        LeavePolicyId = matchedPolicy.Value;
                                        prop.SetValue(item, LeavePolicyId.ToString());
                                        if (LeavePolicyId == 0)
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: LeavePolicy  not found in master data.");
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
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: IsRelativesWorkingWithCompany is mandatory.");
                                        hasError = true;
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
                                        AddErrorRow(errorDataTable, columnName, $"Row {row}: IsReferredByExistingEmployee is mandatory.");
                                        hasError = true;
                                    }
                                    break;

                                case "PayrollTypeName":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: PayrollTypeName is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("PayrollTypes", out var PayrollTypeNameDict))
                                    {
                                        var matchedPolicy = PayrollTypeNameDict
                                            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                        PayrollTypeId = matchedPolicy.Value;
                                        prop.SetValue(item, PayrollTypeId.ToString());

                                        if (PayrollTypeId == 0)
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: PayrollTypeName  not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: PayrollTypeName dictionary is missing or empty.");
                                        hasError = true;
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
                                        var departmentMatch = departmentDict
                                            .FirstOrDefault(kvp => kvp.Key.Equals(cellValue, StringComparison.OrdinalIgnoreCase));
                                        if (departmentMatch.Equals(default(KeyValuePair<string, int>)))
                                        {
                                            departmentMatch = departmentDict
                                                .FirstOrDefault(kvp => kvp.Key.IndexOf(cellValue, StringComparison.OrdinalIgnoreCase) >= 0);
                                        }
                                        if (!departmentMatch.Equals(default(KeyValuePair<string, int>)) && departmentMatch.Value != 0)
                                        {
                                            DepartmentId = departmentMatch.Value;
                                            prop.SetValue(item, DepartmentId.ToString());
                                        }
                                        else
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: DepartmentName  not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: DepartmentName dictionary is missing or empty.");
                                        hasError = true;
                                    }
                                    break;
                                case "SubDepartmentName":
                                    string departmentNameValue = worksheet.Cells[row, columnIndexMap["DepartmentName"]].Text?.Trim();

                                    if (string.IsNullOrWhiteSpace(cellValue) || string.IsNullOrWhiteSpace(departmentNameValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Both DepartmentName and SubDepartmentName are required.");
                                        hasError = true;
                                    }
                                    else if (SubDepartmentDictionaries != null)
                                    {
                                        string combinedKey = $"{cellValue}";

                                        var matchedSubDept = SubDepartmentDictionaries
                                            .FirstOrDefault(kvp => kvp.Key.Trim().Equals(combinedKey, StringComparison.OrdinalIgnoreCase));

                                        SubDepartmentNameId = matchedSubDept.Value;
                                        prop.SetValue(item, SubDepartmentNameId.ToString());

                                        if (SubDepartmentNameId == 0)
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: SubDepartment  not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: SubDepartment dictionary is missing or empty.");
                                        hasError = true;
                                    }
                                    break;
                                case "DesignationName":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: DesignationName is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("Designations", out var DesignationsDict))
                                    {
                                        var DesignationName = DesignationsDict
                                            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                        DesignationsId = DesignationName.Value;
                                        prop.SetValue(item, DesignationsId.ToString());

                                        if (DesignationsId == 0)
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: DesignationName  not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: DesignationName dictionary is missing or empty.");
                                        hasError = true;
                                    }
                                    break;
                                case "Category":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Category is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("EmploymentTypes", out var EmploymentTypesDict))
                                    {
                                        var matchedPolicy = EmploymentTypesDict
                                            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));

                                        EmploymentTypesId = matchedPolicy.Value;
                                        prop.SetValue(item, EmploymentTypesId.ToString());

                                        if (EmploymentTypesId == 0)
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: Category not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Category dictionary is missing or empty.");
                                        hasError = true;
                                    }
                                    break;
                                case "ShiftTypeName":
                                    if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("ShiftTypes", out var ShiftTypeNameDict))
                                    {
                                        var matchedPolicy = ShiftTypeNameDict
                                            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                        ShiftTypeId = matchedPolicy.Value;
                                        prop.SetValue(item, ShiftTypeId.ToString());
                                        if (ShiftTypeId == 0)
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: ShiftTypeName  not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: ShiftTypeName dictionary is missing or empty.");
                                        hasError = true;
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: ShiftTypeName is required.");
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
                                case "AgeOnNetwork":
                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        if (int.TryParse(cellValue, out int aonValue))
                                        {
                                            prop.SetValue(item, aonValue.ToString());
                                        }
                                        else
                                        {
                                            prop.SetValue(item, "0");
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: AON is required.");
                                        hasError = true;
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
        "ReportingToIDL2Name" };
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
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);
            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.WeekOffRoster);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
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

                // Save uploaded file temporarily
                tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(file.FileName));
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // Read Excel to DataTable
                var dataTable = ReadExcelToDataTable(tempFilePath);

                // Convert DataTable to strongly-typed model list
                var modelList =await ConvertDataTableToModelList(dataTable, month, week);

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
        private async Task<List<WeekOffUploadModel>> ConvertDataTableToModelList(DataTable dt, int month, DateTime  weekStartDate)
        {
            var list = new List<WeekOffUploadModel>();
            
            try
            {

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

                    
                    var shiftName = row.Table.Columns.Contains("Shift") ? row["Shift"]?.ToString() : null;
                    if (!string.IsNullOrWhiteSpace(shiftName))
                    {
                        try
                        {
                            var match = Regex.Match(shiftName, @"^(.*?)\(");
                            string shiftCode = match.Success ? match.Groups[1].Value.Trim() : shiftName;

                            var token = HttpContext.Session.GetString(Constants.SessionBearerToken);

                            var response = await _businessLayer.SendGetAPIRequest(
                                $"Employee/GetShiftTypeId?ShiftTypeName={shiftCode}",
                                token,
                                true);

                            if (!string.IsNullOrEmpty(response?.ToString()))
                            {
                                model.ShiftTypeId = JsonConvert.DeserializeObject<long>(response.ToString());
                            }
                        }
                        catch (Exception apiEx)
                        {
                            throw new Exception($"Failed to fetch ShiftTypeId for shift '{shiftName}'.", apiEx);
                        }
                    }
                    int currentDay = DateTime.Today.Day;
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
            if (dt.Columns.Contains(columnName))
            {
                var value = row[columnName]?.ToString();
                if (DateTime.TryParse(value, out var date))
                    return date;
            }
            return null;
        }


        private string ValidateModelList(List<WeekOffUploadModel> models, DateTime week)
        {

            var session = HttpContext.Session;
            var employeeId = Convert.ToInt64(session.GetString(Constants.EmployeeID));
            var companyId = Convert.ToInt64(session.GetString(Constants.CompanyID ));
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






    }
}
