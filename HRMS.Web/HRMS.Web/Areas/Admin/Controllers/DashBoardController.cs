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
using System.Text.RegularExpressions;


namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = (RoleConstants.Admin + "," + RoleConstants.SuperAdmin))]
    public class DashBoardController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        IHttpContextAccessor _context;
        public DashBoardController(IConfiguration configuration, IBusinessLayer businessLayer, Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IHttpContextAccessor context)
        {
            Environment = _environment;
            _configuration = configuration;
            _context = context;
            _businessLayer = businessLayer;
        }
        public IActionResult Index()
        {
            var CompanyID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.CompanyID));

            DashBoardModelInputParams dashBoardModelInputParams = new DashBoardModelInputParams() { EmployeeID = long.Parse(HttpContext.Session.GetString(Constants.EmployeeID)) };
            dashBoardModelInputParams.RoleID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.RoleID));

            var data = _businessLayer.SendPostAPIRequest(dashBoardModelInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetDashBoardModel), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var model = JsonConvert.DeserializeObject<DashBoardModel>(data);
            var leavePolicyModel = GetLeavePolicyData(CompanyID, model.LeavePolicyId ?? 0);
            double accruedLeave1 = CalculateAccruedLeaveForCurrentFiscalYear(model.JoiningDate.Value, leavePolicyModel.Annual_MaximumLeaveAllocationAllowed);
            double Totacarryforword = 0.0;
            var Totaleavewithcarryforword = 0.0;
            var accruedLeaves = accruedLeave1 - Convert.ToDouble(model.TotalLeave);
            if (leavePolicyModel.Annual_IsCarryForward == true)
            {
                Totacarryforword = Convert.ToDouble(model.CarryForword);
                Totaleavewithcarryforword = Totacarryforword + accruedLeaves;
            }
            else
            {
                Totaleavewithcarryforword = accruedLeaves;
            }
            model.NoOfLeaves = Convert.ToInt64(Totaleavewithcarryforword);

            _context.HttpContext.Session.SetString(Constants.ProfilePhoto, model.ProfilePhoto);
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
            //DateTime today = new DateTime(2024, 6, 14);
            DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1); // Start from April 1st of current financial year
            DateTime fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-1); // March 31st of the next year

            // Annual entitlement and accrual per month

            double annualLeaveEntitlement = Annual_MaximumLeaveAllocationAllowed;
            double monthlyAccrual = annualLeaveEntitlement / 12;

            double totalAccruedLeave = 0;

            // If the join date is before the fiscal year start, adjust it to the start of the fiscal year
            if (joinDate < fiscalYearStart)
            {
                joinDate = fiscalYearStart;
            }

            // Start from the month of joining and calculate leave for completed months up to today
            DateTime current = new DateTime(joinDate.Year, joinDate.Month, 1);

            while (current <= today)
            {
                // Get the last day of the current month
                int daysInMonth = DateTime.DaysInMonth(current.Year, current.Month);
                DateTime lastDayOfMonth = new DateTime(current.Year, current.Month, daysInMonth);

                // Adjust the comparison in the current month
                if (current.Month == today.Month && current.Year == today.Year)
                {
                    // Special case: Compare the days worked in the current month up to today
                    int daysWorkedInMonth = (today - joinDate).Days + 1;

                    // Accrue leave if more than 15 days worked
                    if (daysWorkedInMonth > 15)
                    {
                        totalAccruedLeave += monthlyAccrual;
                    }
                }
                else
                {
                    // For past months, compare the join date with the last day of the month
                    if (joinDate <= lastDayOfMonth)
                    {
                        int daysWorkedInMonth = (lastDayOfMonth - joinDate).Days + 1;
                        if (daysWorkedInMonth > 15)
                        {
                            totalAccruedLeave += monthlyAccrual;
                        }
                    }
                }
                // Move to the next month
                current = current.AddMonths(1);

                // Adjust join date to the 1st of the next month after the first iteration
                if (current.Month > joinDate.Month || current.Year > joinDate.Year)
                {
                    joinDate = new DateTime(current.Year, current.Month, 1);
                }
            }

            return totalAccruedLeave;
        }


        //Upload Excel
        [HttpGet]
        public async Task<IActionResult> ImportExcel()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ImportExcel(IFormFile file)
        {
            var list = new List<ImportEmployeeDetail>();

            var data = _businessLayer.SendGetAPIRequest(_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetCountryDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var countryDictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(data);
            var GetCompaniesDictionary = _businessLayer.SendGetAPIRequest(_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetCompaniesDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var CompaniesDictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(GetCompaniesDictionary);
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0; // Reset the stream before reading
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (fileExtension != ".xls" && fileExtension != ".xlsx")
                    {
                        return Json(new { error = "Unsupported file format." });
                    }
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return Json(new { error = "No worksheet found in the file." });
                        }
                        int totalColumns = worksheet.Dimension?.Columns ?? 0;
                        int totalRows = worksheet.Dimension?.Rows ?? 0;
                        if (totalColumns == 0 || totalRows < 2)
                        {
                            return Json(new { error = "Excel file is empty or missing data." });
                        }
                        // Read headers from the first row
                        List<string> excelHeaders = new List<string>();
                        HashSet<string> headerCheck = new HashSet<string>(); // To check duplicates
                        Dictionary<string, int> columnIndexes = new Dictionary<string, int>();  
                        for (int col = 1; col <= totalColumns; col++)
                        {
                            string header = worksheet.Cells[1, col].Text.Trim();
                            // Skip empty headers
                            if (string.IsNullOrEmpty(header))
                            {
                                continue;
                            }
                            // Check for duplicate headers
                            if (!headerCheck.Add(header))
                            {
                                return Json(new { error = $"Duplicate header found: {header}" });
                            }
                            excelHeaders.Add(header);
                            columnIndexes[header] = col; // Store column index
                        }
                        HashSet<string> uniqueEmails = new HashSet<string>(); // Store unique emails

                        for (int row = 2; row <= totalRows; row++)
                        {
                            string email = GetCellValue(worksheet, row, columnIndexes, " PersonalEmailAddress");
                            string countryName = GetCellValue(worksheet, row, columnIndexes, "CorrespondenceCountryName");
                            // Get Country ID from the dictionary (if exists)
                            long? countryId = 0;
                            if (countryName != null)
                            {
                                 countryId = countryDictionary.TryGetValue(countryName.ToLower().Trim(), out long id) ? id : (long?)0;
                            }
                            else
                            {
                                countryId = 0;
                            }
                            string CompaniesName = GetCellValue(worksheet, row, columnIndexes, "Company Name");
                            CompaniesName = Regex.Replace(CompaniesName ?? "", @"\s+", " ").Trim().ToLower();

                            long? GetCompanyId = 0;

                            // Try contains-based matching
                            var matchedCompany = CompaniesDictionary
                                .FirstOrDefault(kvp => kvp.Key.Contains(CompaniesName));

                            if (!matchedCompany.Equals(default(KeyValuePair<string, long>)))
                            {
                                GetCompanyId = matchedCompany.Value;
                            }


                            EmploymentDetailInputParams employmentDetailInputParams = new EmploymentDetailInputParams()
                            {
                                CompanyID = GetCompanyId ??0,
                                EmployeeID = 0
                            };
                            var EmploymentDetailsDictionaries = _businessLayer.SendPostAPIRequest(employmentDetailInputParams,_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetEmploymentDetailsDictionaries), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                            var employmentDetailsDictionaries = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long>>>(EmploymentDetailsDictionaries);

                            EmployeeInputParams employmentSubDepartmentInputParams = new EmployeeInputParams()
                            {
                                CompanyID = GetCompanyId ??0,
                            };
                            // Normalize sub-department dictionary
                            var EmploymentSubDepartment = _businessLayer.SendPostAPIRequest(
                                employmentSubDepartmentInputParams,
                                _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetSubDepartmentDictionary),
                                HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();

                            var SubDepartmentDictionaries = JsonConvert.DeserializeObject<Dictionary<string, long>>(EmploymentSubDepartment);

                            // Extract and match
                            string SubDepartmentName = GetCellValue(worksheet, row, columnIndexes, "SubDepartmentName")?.Trim();
                            long? SubDepartmentNameId = string.IsNullOrWhiteSpace(SubDepartmentName) ? 0 : GetIdFromDictionary(SubDepartmentDictionaries, SubDepartmentName);

                            // Initialize all IDs
                            long DepartmentId = 0, DesignationsId = 0, EmploymentTypesId = 0, ShiftTypeId = 0;
                            long JobLocationId = 0, ReportingToIDL1Id = 0, ReportingToIDL2Id = 0, RoleId = 0;
                            long PayrollTypeId = 0, LeavePolicyId = 0, GenderId = 0;

                            // Matching from employmentDetailsDictionaries
                            string departmentName = GetCellValue(worksheet, row, columnIndexes, "DepartmentName")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("Departments", out var departmentDict))
                                DepartmentId = GetIdFromDictionary(departmentDict, departmentName);

                            string DesignationName = GetCellValue(worksheet, row, columnIndexes, "DesignationName")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("Designations", out var DesignationsDict))
                                DesignationsId = GetIdFromDictionary(DesignationsDict, DesignationName);

                            string EmployeeType = GetCellValue(worksheet, row, columnIndexes, "EmployeeType")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("EmploymentTypes", out var EmploymentTypesDict))
                                EmploymentTypesId = GetIdFromDictionary(EmploymentTypesDict, EmployeeType);

                            string ShiftTypeName = GetCellValue(worksheet, row, columnIndexes, "ShiftTypeName")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("ShiftTypes", out var ShiftTypeNameDict))
                                ShiftTypeId = GetIdFromDictionary(ShiftTypeNameDict, ShiftTypeName);

                            string JobLocationName = GetCellValue(worksheet, row, columnIndexes, "JobLocationName")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("JobLocations", out var JobLocationNameDict))
                                JobLocationId = GetIdFromDictionary(JobLocationNameDict, JobLocationName);

                            string ReportingToIDL1Name = GetCellValue(worksheet, row, columnIndexes, "ReportingToIDL1Name")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("Employees", out var ReportingToIDL1NameDict))
                                ReportingToIDL1Id = GetIdFromDictionary(ReportingToIDL1NameDict, ReportingToIDL1Name);

                            string ReportingToIDL2Name = GetCellValue(worksheet, row, columnIndexes, "ReportingToIDL2Name")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("Employees", out var ReportingToIDL2NameDict))
                                ReportingToIDL2Id = GetIdFromDictionary(ReportingToIDL2NameDict, ReportingToIDL2Name);

                            string RoleName = GetCellValue(worksheet, row, columnIndexes, "RoleName")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("Roles", out var RoleNameDict))
                                RoleId = GetIdFromDictionary(RoleNameDict, RoleName);

                            string PayrollTypeName = GetCellValue(worksheet, row, columnIndexes, "PayrollTypeName")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("PayrollTypes", out var PayrollTypeNameDict))
                                PayrollTypeId = GetIdFromDictionary(PayrollTypeNameDict, PayrollTypeName);

                            string LeavePolicyName = GetCellValue(worksheet, row, columnIndexes, "LeavePolicyName")?.Trim();
                            if (employmentDetailsDictionaries.TryGetValue("LeavePolicies", out var LeavePolicyNameDict))
                                LeavePolicyId = GetIdFromDictionary(LeavePolicyNameDict, LeavePolicyName);

                            string GenderName = GetCellValue(worksheet, row, columnIndexes, "Gender")?.Trim();
                            GenderId = (GenderName?.ToLower() == "female") ? 2 : 1;


                            string GetTrimmedValue(string columnName) => GetCellValue(worksheet, row, columnIndexes, columnName)?.Trim();

                            var employee = new ImportEmployeeDetail
                            {
                                // Basic Details
                                EmployeeNumber = GetTrimmedValue("EmployeeNumber"),
                                CompanyName = GetCompanyId.ToString(),
                                FirstName = GetTrimmedValue("FirstName"),
                                MiddleName = GetTrimmedValue("MiddleName"),
                                Surname = GetTrimmedValue("Surname"),
                                EmailAddress = email,
                                PersonalEmailAddress = GetTrimmedValue("PersonalEmailAddress"),
                                DateOfBirth = GetCellValue(worksheet, row, columnIndexes, "DateOfBirth"),
                                PlaceOfBirth = GetTrimmedValue("PlaceOfBirth"),
                                Gender = GenderId.ToString(),
                                BloodGroup = GetTrimmedValue("BloodGroup"),

                                // Contact Details
                                Landline = GetTrimmedValue("Landline"),
                                Mobile = GetTrimmedValue("Mobile"),
                                Telephone = GetTrimmedValue("Telephone"),
                                OfficialEmailID = GetTrimmedValue("OfficialEmailID"),
                                OfficialContactNo = GetTrimmedValue("OfficialContactNo"),

                                // Address
                                CorrespondenceAddress = GetTrimmedValue("CorrespondenceAddress"),
                                CorrespondenceCity = GetTrimmedValue("CorrespondenceCity"),
                                CorrespondencePinCode = GetTrimmedValue("CorrespondencePinCode"),
                                CorrespondenceState = GetTrimmedValue("CorrespondenceState"),
                                CorrespondenceCountryName = countryId.ToString(),

                                PermanentAddress = GetTrimmedValue("PermanentAddress"),
                                PermanentCity = GetTrimmedValue("PermanentCity"),
                                PermanentPinCode = GetTrimmedValue("PermanentPinCode"),
                                PermanentState = GetTrimmedValue("PermanentState"),
                                PermanentCountryName = GetTrimmedValue("PermanentCountryName"),
                                PeriodOfStay = GetTrimmedValue("PeriodOfStay"),

                                // Emergency Contact
                                ContactPersonName = GetTrimmedValue("ContactPersonName"),
                                ContactPersonMobile = GetTrimmedValue("ContactPersonMobile"),
                                ContactPersonTelephone = GetTrimmedValue("ContactPersonTelephone"),
                                ContactPersonRelationship = GetTrimmedValue("ContactPersonRelationship"),

                                // Referral & Relatives
                                IsReferredByExistingEmployee = GetBooleanValue(worksheet, row, columnIndexes, "IsReferredByExistingEmployee"),
                                ReferredByEmployeeName = GetTrimmedValue("ReferredByEmployeeName"),
                                IsRelativesWorkingWithCompany = GetBooleanValue(worksheet, row, columnIndexes, "IsRelativesWorkingWithCompany"),
                                RelativesDetails = GetTrimmedValue("RelativesDetails"),

                                // Medical / Background
                                AadharCardNo = GetTrimmedValue("AadharCardNo"),
                                PANNo = GetTrimmedValue("PANNo"),
                                Allergies = GetTrimmedValue("Allergies"),
                                MajorIllnessOrDisability = GetTrimmedValue("MajorIllnessOrDisability"),
                                AwardsAchievements = GetTrimmedValue("AwardsAchievements"),
                                EducationGap = GetTrimmedValue("EducationGap"),
                                ExtraCurricularActivities = GetTrimmedValue("ExtraCurricularActivities"),
                                ForeignCountryVisits = GetTrimmedValue("ForeignCountryVisits"),
                                ExtraCuricuarActivities = GetTrimmedValue("ExtraCuricuarActivities"),
                                ITSkillsKnowledge = GetTrimmedValue("ITSkillsKnowledge"),
                                PreviousExperience = GetTrimmedValue("Previous Experience"),

                                // Employment Details
                                JoiningDate = GetCellValue(worksheet, row, columnIndexes, "JoiningDate"),
                                DesignationName = DesignationsId.ToString(),
                                EmployeeType = EmploymentTypesId.ToString(),
                                DepartmentName = DepartmentId.ToString(),
                                SubDepartmentName = SubDepartmentNameId.ToString(),
                                ShiftTypeName = ShiftTypeId.ToString(),
                                JobLocationName = JobLocationId.ToString(),
                                ReportingToIDL1Name = ReportingToIDL1Id.ToString(),
                                ReportingToIDL2Name = ReportingToIDL2Id.ToString(),
                                PayrollTypeName = PayrollTypeId.ToString(),
                                LeavePolicyName = LeavePolicyId.ToString(),
                                ClientName = GetTrimmedValue("ClientName"),
                                RoleName = RoleId.ToString(),

                                // Exit Details
                                DOJInTraining = GetCellValue(worksheet, row, columnIndexes, "D.O.J in Training"),
                                DOJInOJTOnroll = GetCellValue(worksheet, row, columnIndexes, "DOJ IN OJT/Onroll"),
                                DateOfResignation = GetCellValue(worksheet, row, columnIndexes, "DATE OF RESIGNATION/intimation"),
                                DateOfLeaving = GetCellValue(worksheet, row, columnIndexes, "DOL"),
                                BackOnFloor = GetTrimmedValue("Back On Floor"),
                                LeavingType = GetTrimmedValue("Leaving Type"),
                                LeavingRemarks = GetTrimmedValue("Leaving Remarks"),
                                NoticeServed = GetTrimmedValue("Notice Served"),
                                MailReceivedFromAndDate = GetTrimmedValue("Mail received from and Date"),
                                DateOfEmailSentToITForIDDeletion = GetCellValue(worksheet, row, columnIndexes, "DateOfEmailSentToITForIDDeletion"),

                                // Banking & ESI
                                ESINumber = GetTrimmedValue("ESI Number"),
                                BankAccountNumber = GetTrimmedValue("Bank Account Number"),
                                IFSCCode = GetTrimmedValue("IFSC CODE"),
                                BankName = GetTrimmedValue("BANK NAME"),
                                RegistrationDateInESIC = GetCellValue(worksheet, row, columnIndexes, "Registration Date in ESIC"),

                                // Misc
                                EmployeeID = 0
                            };

                            var UserID = Convert.ToInt64(_context.HttpContext.Session.GetString(Constants.UserID));
                            employee.InsertedBy =  UserID.ToString();
                            // list.Add(employee);
                            //uniqueEmails.Add(email);
                            var EmaployeeData =await _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.AddUpdateEmployeeFromExecel), HttpContext.Session.GetString(Constants.SessionBearerToken), true);
                            var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(data);
                        }
                     

                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error processing Excel file: {ex.Message}" });
            }

            return Json(new { success = "Excel data imported successfully.", totalRecords = list.Count });
        }

        // Helper Method: Get Cell Value Safely
        private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string columnName)
        {
            return columnIndexes.ContainsKey(columnName)
                ? worksheet.Cells[row, columnIndexes[columnName]]?.Value?.ToString()?.Trim()
                : null;
        }

        // Helper Method: Get Date Value Safely
        private string GetDateValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string columnName)
        {
            if (columnIndexes.ContainsKey(columnName) && worksheet.Cells[row, columnIndexes[columnName]]?.Value != null)
            {
                try
                {
                    return DateTime.FromOADate(Convert.ToDouble(worksheet.Cells[row, columnIndexes[columnName]].Value)).ToString("yyyy-MM-dd");
                }
                catch
                {
                    return null; // Return null if conversion fails
                }
            }
            return null;
        }

        // Helper Method: Get Boolean Value Safely
        private bool GetBooleanValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string columnName)
        {
            if (columnIndexes.ContainsKey(columnName) && worksheet.Cells[row, columnIndexes[columnName]]?.Value != null)
            {
                string value = worksheet.Cells[row, columnIndexes[columnName]].Value.ToString().Trim();
                return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("1");
            }
            return false;
        }
        private long GetIdFromDictionary(Dictionary<string, long> dictionary, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Trim();

            // 1. Try exact match (case-insensitive)
            var exactMatch = dictionary.FirstOrDefault(kvp =>
                kvp.Key.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (!exactMatch.Equals(default(KeyValuePair<string, long>)))
                return exactMatch.Value;

            // 2. Try ends-with match (e.g., match "Operations" over "Field Operations")
            var endsWithMatch = dictionary.FirstOrDefault(kvp =>
                kvp.Key.EndsWith(value, StringComparison.OrdinalIgnoreCase));
            if (!endsWithMatch.Equals(default(KeyValuePair<string, long>)))
                return endsWithMatch.Value;

            // 3. Fallback: contains
            var containsMatch = dictionary.FirstOrDefault(kvp =>
                kvp.Key.Contains(value, StringComparison.OrdinalIgnoreCase));
            if (!containsMatch.Equals(default(KeyValuePair<string, long>)))
                return containsMatch.Value;

            return 0;
        }



    }
}
