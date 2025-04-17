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
                            Dictionary<string, int> columnIndexes = new Dictionary<string, int>(); // Store column index

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

                            // Ensure required headers exist
                            HashSet<string> requiredHeaders = new HashSet<string> { "FirstName", "MiddleName", "Surname", "EmailAddress" };
                            if (!requiredHeaders.IsSubsetOf(headerCheck))
                            {
                                return Json(new { error = "Excel file is missing required header titles." });
                            }

                            HashSet<string> uniqueEmails = new HashSet<string>(); // Store unique emails

                            for (int row = 2; row <= totalRows; row++)
                            {
                                string email = GetCellValue(worksheet, row, columnIndexes, "EmailAddress");
                                if (string.IsNullOrEmpty(email) || uniqueEmails.Contains(email))
                                {
                                    continue;
                                }

                                string countryName = GetCellValue(worksheet, row, columnIndexes, "CorrespondenceCountryName");

                                // Get Country ID from the dictionary (if exists)
                                long? countryId = countryDictionary.TryGetValue(countryName.ToLower().Trim(), out long id) ? id : (long?)0;
                            
                                string CompaniesName= GetCellValue(worksheet, row, columnIndexes, "CompanyName");

                                // Get Country ID from the dictionary (if exists)
                                long? GetCompanyId = CompaniesDictionary.TryGetValue(CompaniesName.ToLower().Trim(), out long CompanyId) ? CompanyId : (long?)0;
                             
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
                                var EmploymentSubDepartment = _businessLayer.SendPostAPIRequest(employmentSubDepartmentInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetSubDepartmentDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                                var SubDepartmentDictionaries = JsonConvert.DeserializeObject<Dictionary<string, long>>(EmploymentSubDepartment);


                                // Get SubDepartment Name with LIKE
                                string SubDepartmentName = GetCellValue(worksheet, row, columnIndexes, "SubDepartmentName")?.Trim();
                                long? SubDepartmentNameId = SubDepartmentDictionaries.TryGetValue(SubDepartmentName.ToLower().Trim(), out long DID) ? DID : (long?)null;

                                // Initialize all variables with 0
                                long DepartmentId = 0;
                                long DesignationsId = 0;
                                long EmploymentTypesId = 0;
                          
                                long ShiftTypeId = 0;
                                long JobLocationId = 0;
                                long ReportingToIDL1Id = 0;
                                long ReportingToIDL2Id = 0;
                                long RoleId = 0;
                                long PayrollTypeId = 0;
                                long LeavePolicyId = 0;
                                long GenderId = 0;

                                // Get Department Name with LIKE
                                string departmentName = GetCellValue(worksheet, row, columnIndexes, "DepartmentName")?.Trim();
                          
                                if (!string.IsNullOrEmpty(departmentName) && employmentDetailsDictionaries.TryGetValue("Departments", out var departmentDict))
                                {
                                    DepartmentId = departmentDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(departmentName, StringComparison.OrdinalIgnoreCase)).Value;
                                    if (DepartmentId == 0) DepartmentId = 0; // Default to 0 if no match is found
                                }

                                // Get Designation Name with LIKE
                                string DesignationName = GetCellValue(worksheet, row, columnIndexes, "DesignationName")?.Trim();
                                if (!string.IsNullOrEmpty(DesignationName) && employmentDetailsDictionaries.TryGetValue("Designations", out var DesignationsDict))
                                {
                                    DesignationsId = DesignationsDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(DesignationName, StringComparison.OrdinalIgnoreCase)).Value;
                                    if (DesignationsId == 0) DesignationsId = 0;
                                }

                                // Get Employee Type with LIKE
                                string EmployeeType = GetCellValue(worksheet, row, columnIndexes, "EmployeeType")?.Trim();
                                if (!string.IsNullOrEmpty(EmployeeType) && employmentDetailsDictionaries.TryGetValue("EmploymentTypes", out var EmploymentTypesDict))
                                {
                                    EmploymentTypesId = EmploymentTypesDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(EmployeeType, StringComparison.OrdinalIgnoreCase)).Value;
                                    if (EmploymentTypesId == 0) EmploymentTypesId = 0;
                                }

                          

                                // Get ShiftType Name with LIKE
                                string ShiftTypeName = GetCellValue(worksheet, row, columnIndexes, "ShiftTypeName")?.Trim();
                                if (!string.IsNullOrEmpty(ShiftTypeName) && employmentDetailsDictionaries.TryGetValue("ShiftTypes", out var ShiftTypeNameDict))
                                {
                                    ShiftTypeId = ShiftTypeNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(ShiftTypeName, StringComparison.OrdinalIgnoreCase)).Value;
                                    if (ShiftTypeId == 0) ShiftTypeId = 0;
                                }

                                // Get JobLocation Name with LIKE
                                string JobLocationName = GetCellValue(worksheet, row, columnIndexes, "JobLocationName")?.Trim();
                                if (!string.IsNullOrEmpty(JobLocationName) && employmentDetailsDictionaries.TryGetValue("JobLocations", out var JobLocationNameDict))
                                {
                                    JobLocationId = JobLocationNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(JobLocationName, StringComparison.OrdinalIgnoreCase)).Value;
                                    if (JobLocationId == 0) JobLocationId = 0;
                                }

                                // Get ReportingToIDL1 Name with LIKE
                                string ReportingToIDL1Name = GetCellValue(worksheet, row, columnIndexes, "ReportingToIDL1Name")?.Trim();
                                if (!string.IsNullOrEmpty(ReportingToIDL1Name) && employmentDetailsDictionaries.TryGetValue("Employees", out var ReportingToIDL1NameDict))
                                {
                                    ReportingToIDL1Id = ReportingToIDL1NameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(ReportingToIDL1Name.ToLower(), StringComparison.OrdinalIgnoreCase)).Value;
                                    if (ReportingToIDL1Id == 0) ReportingToIDL1Id = 0;
                                }

                                // Get ReportingToIDL2 Name with LIKE
                                string ReportingToIDL2Name = GetCellValue(worksheet, row, columnIndexes, "ReportingToIDL2Name")?.Trim();
                                if (!string.IsNullOrEmpty(ReportingToIDL2Name) && employmentDetailsDictionaries.TryGetValue("Employees", out var ReportingToIDL2NameDict))
                                {
                                    ReportingToIDL2Id = ReportingToIDL2NameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(ReportingToIDL2Name.ToLower(), StringComparison.OrdinalIgnoreCase)).Value;
                                    if (ReportingToIDL2Id == 0) ReportingToIDL2Id = 0;
                                }

                                // Get Role Name with LIKE
                                string RoleName = GetCellValue(worksheet, row, columnIndexes, "RoleName")?.Trim();
                                if (!string.IsNullOrEmpty(RoleName) && employmentDetailsDictionaries.TryGetValue("Roles", out var RoleNameDict))
                                {
                                    RoleId = RoleNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(RoleName, StringComparison.OrdinalIgnoreCase)).Value;
                                    if (RoleId == 0) RoleId = 0;
                                }

                                // Get PayrollType Name with LIKE
                                string PayrollTypeName = GetCellValue(worksheet, row, columnIndexes, "PayrollTypeName")?.Trim();
                                if (!string.IsNullOrEmpty(PayrollTypeName) && employmentDetailsDictionaries.TryGetValue("PayrollTypes", out var PayrollTypeNameDict))
                                {
                                    PayrollTypeId = PayrollTypeNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(PayrollTypeName, StringComparison.OrdinalIgnoreCase)).Value;
                                    if (PayrollTypeId == 0) PayrollTypeId = 0;
                                }

                                // Get LeavePolicy Name with LIKE
                                string LeavePolicyName = GetCellValue(worksheet, row, columnIndexes, "LeavePolicyName")?.Trim();
                                if (!string.IsNullOrEmpty(LeavePolicyName) && employmentDetailsDictionaries.TryGetValue("LeavePolicies", out var LeavePolicyNameDict))
                                {
                                    LeavePolicyId = LeavePolicyNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(LeavePolicyName, StringComparison.OrdinalIgnoreCase)).Value;
                                    if (LeavePolicyId == 0) LeavePolicyId = 0;
                                }

                                // Get Gender (Male or Female)
                                string GenderName = GetCellValue(worksheet, row, columnIndexes, "Gender")?.Trim();
                                if (GenderName == "FeMale")
                                {
                                    GenderId = 2;  // Female
                                }
                                else
                                {
                                    GenderId = 1;  // Male or Default
                                }

                                var employee = new ImportEmployeeDetail
                                {
                                    EmployeeNumber = GetCellValue(worksheet, row, columnIndexes, "EmployeeNumber"),
                                    CompanyName = GetCompanyId.ToString(),
                                    FirstName = GetCellValue(worksheet, row, columnIndexes, "FirstName"),
                                    MiddleName = GetCellValue(worksheet, row, columnIndexes, "MiddleName"),
                                    Surname = GetCellValue(worksheet, row, columnIndexes, "Surname"),
                                    CorrespondenceAddress = GetCellValue(worksheet, row, columnIndexes, "CorrespondenceAddress"),
                                    CorrespondenceCity = GetCellValue(worksheet, row, columnIndexes, "CorrespondenceCity"),
                                    CorrespondencePinCode = GetCellValue(worksheet, row, columnIndexes, "CorrespondencePinCode"),
                                    CorrespondenceState = GetCellValue(worksheet, row, columnIndexes, "CorrespondenceState"),
                                    CorrespondenceCountryName = countryId.ToString(),
                                    EmailAddress = email,
                                    Landline = GetCellValue(worksheet, row, columnIndexes, "Landline"),
                                    Mobile = GetCellValue(worksheet, row, columnIndexes, "Mobile"),
                                    Telephone = GetCellValue(worksheet, row, columnIndexes, "Telephone"),
                                    PersonalEmailAddress = GetCellValue(worksheet, row, columnIndexes, "PersonalEmailAddress"),
                                    PermanentAddress = GetCellValue(worksheet, row, columnIndexes, "PermanentAddress"),
                                    PermanentCity = GetCellValue(worksheet, row, columnIndexes, "PermanentCity"),
                                    PermanentPinCode = GetCellValue(worksheet, row, columnIndexes, "PermanentPinCode"),
                                    PermanentState = GetCellValue(worksheet, row, columnIndexes, "PermanentState"),
                                    PermanentCountryName = GetCellValue(worksheet, row, columnIndexes, "PermanentCountryName"),
                                    PeriodOfStay = GetCellValue(worksheet, row, columnIndexes, "PeriodOfStay"),
                                    VerificationContactPersonName = GetCellValue(worksheet, row, columnIndexes, "VerificationContactPersonName"),
                                    VerificationContactPersonContactNo = GetCellValue(worksheet, row, columnIndexes, "VerificationContactPersonContactNo"),
                                    DateOfBirth = "1990 - 01 - 01",
                                    PlaceOfBirth = GetCellValue(worksheet, row, columnIndexes, "PlaceOfBirth"),
                                    IsReferredByExistingEmployee = GetBooleanValue(worksheet, row, columnIndexes, "IsReferredByExistingEmployee"),
                                    ReferredByEmployeeName = GetCellValue(worksheet, row, columnIndexes, "ReferredByEmployeeName"),
                                    BloodGroup = GetCellValue(worksheet, row, columnIndexes, "BloodGroup"),
                                    AadharCardNo = GetCellValue(worksheet, row, columnIndexes, "AadharCardNo"),
                                    PANNo = GetCellValue(worksheet, row, columnIndexes, "PANNo"),
                                    Allergies = GetCellValue(worksheet, row, columnIndexes, "Allergies"),
                                    IsRelativesWorkingWithCompany = GetBooleanValue(worksheet, row, columnIndexes, "IsRelativesWorkingWithCompany"),
                                    RelativesDetails = GetCellValue(worksheet, row, columnIndexes, "RelativesDetails"),
                                    MajorIllnessOrDisability = GetCellValue(worksheet, row, columnIndexes, "MajorIllnessOrDisability"),
                                    AwardsAchievements = GetCellValue(worksheet, row, columnIndexes, "AwardsAchievements"),
                                    EducationGap = GetCellValue(worksheet, row, columnIndexes, "EducationGap"),
                                    ExtraCurricularActivities = GetCellValue(worksheet, row, columnIndexes, "ExtraCurricularActivities"),
                                    ForeignCountryVisits = GetCellValue(worksheet, row, columnIndexes, "ForeignCountryVisits"),
                                    ContactPersonName = GetCellValue(worksheet, row, columnIndexes, "ContactPersonName"),
                                    ContactPersonMobile = GetCellValue(worksheet, row, columnIndexes, "ContactPersonMobile"),
                                    ContactPersonTelephone = GetCellValue(worksheet, row, columnIndexes, "ContactPersonTelephone"),
                                    ContactPersonRelationship = GetCellValue(worksheet, row, columnIndexes, "ContactPersonRelationship"),
                                    ITSkillsKnowledge = GetCellValue(worksheet, row, columnIndexes, "ITSkillsKnowledge"),
                                    JoiningDate = GetCellValue(worksheet, row, columnIndexes, "JoiningDate"),
                                    DesignationName = DesignationsId.ToString(),
                                    EmployeeType = EmploymentTypesId.ToString(),
                                    DepartmentName = DepartmentId.ToString(),
                                    SubDepartmentName = SubDepartmentNameId.ToString(),
                                    ShiftTypeName = ShiftTypeId.ToString(),
                                    JobLocationName = JobLocationId.ToString(),
                                    ReportingToIDL1Name = ReportingToIDL1Id.ToString(),
                                    ReportingToIDL2Name = ReportingToIDL2Id.ToString(),
                                    OfficialEmailID = GetCellValue(worksheet, row, columnIndexes, "OfficialEmailID"),
                                    OfficialContactNo = GetCellValue(worksheet, row, columnIndexes, "OfficialContactNo"),
                                    PayrollTypeName = PayrollTypeId.ToString(),
                                    LeavePolicyName = LeavePolicyId.ToString(),
                                    ClientName = GetCellValue(worksheet, row, columnIndexes, "ClientName"),
                                    RoleName = RoleId.ToString(),
                                    ForiegnCountryVisits = GetCellValue(worksheet, row, columnIndexes, "ForiegnCountryVisits"),
                                    ExtraCuricuarActivities = GetCellValue(worksheet, row, columnIndexes, "ExtraCuricuarActivities"),
                                    ESINumber = GetCellValue(worksheet, row, columnIndexes, "ESI Number"),
                                    BankAccountNumber = GetCellValue(worksheet, row, columnIndexes, "Bank Account Number"),
                                    RegistrationDateInESIC = GetCellValue(worksheet, row, columnIndexes, "Registration Date in ESIC"),
                                    IFSCCode = GetCellValue(worksheet, row, columnIndexes, "IFSC CODE"),
                                    BankName = GetCellValue(worksheet, row, columnIndexes, "BANK NAME"),
                                    DOJInTraining = GetCellValue(worksheet, row, columnIndexes, "D.O.J in Training"),
                                    DOJInOJTOnroll = GetCellValue(worksheet, row, columnIndexes, "DOJ IN OJT/Onroll"),
                                    DateOfResignation = GetCellValue(worksheet, row, columnIndexes, "DATE OF RESIGNATION/intimation"),
                                    DateOfLeaving = GetCellValue(worksheet, row, columnIndexes, "DOL"),
                                    BackOnFloor = GetCellValue(worksheet, row, columnIndexes, "Back On Floor"),
                                    LeavingType = GetCellValue(worksheet, row, columnIndexes, "Leaving Type"),
                                    LeavingRemarks = GetCellValue(worksheet, row, columnIndexes, "Leaving Remarks"),
                                    NoticeServed = GetCellValue(worksheet, row, columnIndexes, "Notice Served"),
                                    MailReceivedFromAndDate = GetCellValue(worksheet, row, columnIndexes, "Mail received from and Date"),
                                    DateOfEmailSentToITForIDDeletion = GetCellValue(worksheet, row, columnIndexes, "DateOfEmailSentToITForIDDeletion"),
                                    PreviousExperience = GetCellValue(worksheet, row, columnIndexes, "Previous Experience"),
                                    AON = GetCellValue(worksheet, row, columnIndexes, "AON"),

                                    Gender = GenderId.ToString(),
                                    EmployeeID = 0,

                                };                            
                            // list.Add(employee);
                            uniqueEmails.Add(email);
                                var EmaployeeData = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.AddUpdateEmployeeFromExecel), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
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


    }
}
