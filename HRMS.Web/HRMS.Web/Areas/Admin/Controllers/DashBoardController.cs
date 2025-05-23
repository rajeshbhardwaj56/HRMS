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

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = (RoleConstants.Admin + "," + RoleConstants.SuperAdmin + "," + RoleConstants.HR))]
    public class DashBoardController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        IHttpContextAccessor _context;
        private readonly IS3Service _s3Service;
        public DashBoardController(IConfiguration configuration, IBusinessLayer businessLayer, Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment, IHttpContextAccessor context, IS3Service s3Service)
        {
            Environment = _environment;
            _configuration = configuration;
            _context = context;
            _businessLayer = businessLayer;
            _s3Service = s3Service;

        }
        public async Task<IActionResult> Index()
        {
            var session = _context.HttpContext.Session;

            var companyId = Convert.ToInt64(session.GetString(Constants.CompanyID));
            var employeeId = Convert.ToInt64(session.GetString(Constants.EmployeeID));
            var roleId = Convert.ToInt64(session.GetString(Constants.RoleID));
            var token = session.GetString(Constants.SessionBearerToken);

            var inputParams = new DashBoardModelInputParams
            {
                EmployeeID = employeeId,
                RoleID = roleId
            };

            var apiUrl = _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.DashBoard,
                APIApiActionConstants.GetDashBoardModel
            );

            var apiResponse = await _businessLayer.SendPostAPIRequest(inputParams, apiUrl, token, true);
            var model = JsonConvert.DeserializeObject<DashBoardModel>(apiResponse?.ToString());

            if (model == null)
                return View();

            // Update employee photos if profile photo exists
            if (!string.IsNullOrEmpty(model.ProfilePhoto) && model.EmployeeDetails != null)
            {
                foreach (var employee in model.EmployeeDetails.Where(e => !string.IsNullOrEmpty(e.EmployeePhoto)))
                {
                    employee.EmployeePhoto = _s3Service.GetFileUrl(employee.EmployeePhoto);
                }
            }

            // Update WhatsHappening icons
            if (model.WhatsHappening != null)
            {
                foreach (var item in model.WhatsHappening.Where(w => !string.IsNullOrEmpty(w.IconImage)))
                {
                    item.IconImage = _s3Service.GetFileUrl(item.IconImage);
                }
            }

            // Leave calculation
            var leavePolicy = GetLeavePolicyData(companyId, model.LeavePolicyId ?? 0);
            if (leavePolicy != null)
            {
                var joiningDate = model.JoiningDate.GetValueOrDefault();
                var maxLeaves = leavePolicy.Annual_MaximumLeaveAllocationAllowed;

                var accruedLeaves = CalculateAccruedLeaveForCurrentFiscalYear(joiningDate, maxLeaves);
                var usedLeaves = Convert.ToDouble(model.TotalLeave);
                var carryForward = leavePolicy.Annual_IsCarryForward ? Convert.ToDouble(model.CarryForword) : 0.0;

                var totalAvailableLeaves = accruedLeaves - usedLeaves + carryForward;
                model.NoOfLeaves = Convert.ToInt64(totalAvailableLeaves);

            }
            // Optionally store profile photo in session
            // session.SetString(Constants.ProfilePhoto, model.ProfilePhoto);

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

        [HttpGet]
        public async Task<IActionResult> ImportExcel()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> ImportExcel(IFormFile file)
        {

            var data = _businessLayer.SendGetAPIRequest(_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetCountryDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var countryDictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(data);

            var GetCompaniesDictionary = _businessLayer.SendGetAPIRequest(_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetCompaniesDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var CompaniesDictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(GetCompaniesDictionary);

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

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

                            string CompaniesName = GetCellValue(worksheet, row, columnIndexes, "CompanyName");

                            // Get Country ID from the dictionary (if exists)
                            long? GetCompanyId = CompaniesDictionary.TryGetValue(CompaniesName.ToLower().Trim(), out long CompanyId) ? CompanyId : (long?)0;

                            EmploymentDetailInputParams employmentDetailInputParams = new EmploymentDetailInputParams()
                            {
                                CompanyID = GetCompanyId ?? 0,
                                EmployeeID = 0
                            };
                            var EmploymentDetailsDictionaries = _businessLayer.SendPostAPIRequest(employmentDetailInputParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetEmploymentDetailsDictionaries), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                            var employmentDetailsDictionaries = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, long>>>(EmploymentDetailsDictionaries);

                            EmployeeInputParams employmentSubDepartmentInputParams = new EmployeeInputParams()
                            {
                                CompanyID = GetCompanyId ?? 0,
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
                                    .Where(kvp => kvp.Key.Contains(ReportingToIDL1Name, StringComparison.OrdinalIgnoreCase))
                                    .Select(kvp => kvp.Value)
                                    .FirstOrDefault(id => id != 0); // only take the first non-zero id

                                if (ReportingToIDL1Id == 0)
                                {
                                    // Handle case where no match found
                                }
                            }

                            string ReportingToIDL2Name = GetCellValue(worksheet, row, columnIndexes, "ReportingToIDL2Name")?.Trim();

                            if (!string.IsNullOrEmpty(ReportingToIDL2Name) &&
                                employmentDetailsDictionaries.TryGetValue("Employees", out var ReportingToIDL2NameDict))
                            {
                                ReportingToIDL2Id = ReportingToIDL2NameDict
                                    .Where(kvp => kvp.Key.Contains(ReportingToIDL2Name, StringComparison.OrdinalIgnoreCase))
                                    .Select(kvp => kvp.Value)
                                    .FirstOrDefault(id => id != 0);
                            }

                            string RoleName = GetCellValue(worksheet, row, columnIndexes, "RoleName")?.Trim();
                            if (!string.IsNullOrEmpty(RoleName) && employmentDetailsDictionaries.TryGetValue("Roles", out var RoleNameDict))
                            {
                                RoleId = RoleNameDict
                                    .FirstOrDefault(kvp => kvp.Key.Contains(RoleName, StringComparison.OrdinalIgnoreCase)).Value;
                                if (RoleId == 0) RoleId = 0;
                            }
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
                            string GenderName = GetCellValue(worksheet, row, columnIndexes, "Gender")?.Trim();
                            if (GenderName == "FeMale")
                            {
                                GenderId = 2;
                            }
                            else
                            {
                                GenderId = 1;
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
                                DateOfBirth = GetCellValue(worksheet, row, columnIndexes, "DateOfBirth"),
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
            return Json(new { success = "Excel data imported successfully.", });
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
                long RoleId = 0;
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
                                case "EmployeeNumber":
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
                                case "CorrespondenceCountryName":
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
                                case "JobLocationName":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: JobLocationName is required.");
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
                                            AddError(errorDataTable, columnName, $"Row {row}: JobLocationName  not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: JobLocationName dictionary is missing or empty.");
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
                                case "RoleName":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: RoleName is required.");
                                        hasError = true;
                                    }
                                    else if (employmentDetailsDictionaries.TryGetValue("Roles", out var RoleNameDict))
                                    {
                                        var matchedPolicy = RoleNameDict
                                            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                        RoleId = matchedPolicy.Value;
                                        prop.SetValue(item, RoleId.ToString());

                                        if (RoleId == 0)
                                        {
                                            AddError(errorDataTable, columnName, $"Row {row}: RoleName not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: RoleName dictionary is missing or empty.");
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
                                        string combinedKey = $"{departmentNameValue}_{cellValue}";

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
                                case "EmploymentType":
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: EmploymentType is required.");
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
                                            AddError(errorDataTable, columnName, $"Row {row}: EmploymentType not found in master data.");
                                            hasError = true;
                                        }
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: EmploymentType dictionary is missing or empty.");
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
                    CorrespondenceAddress = item.CorrespondenceAddress,
                    CorrespondenceCity = item.CorrespondenceCity,
                    CorrespondencePinCode = item.CorrespondencePinCode,
                    CorrespondenceState = item.CorrespondenceState,
                    CorrespondenceCountryName = item.CorrespondenceCountryName,
                    EmailAddress = item.EmailAddress,
                    Landline = item.Landline,
                    Mobile = item.Mobile,
                    Telephone = item.Telephone,
                    PersonalEmailAddress = item.PersonalEmailAddress,
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
                    RoleName = item.RoleName,
                    EmployeeNumber = item.EmployeeNumber,
                    DesignationName = item.DesignationName,
                    EmploymentType = item.EmploymentType,
                    DepartmentName = item.DepartmentName,
                    JobLocationName = item.JobLocationName,
                    OfficialEmail = item.OfficialEmail,
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
                    UANNumber=item.UANNumber,
                    IFSCCode = item.IFSCCode,
                    BankName = item.BankName,
                    AgeOnNetwork = item.AgeOnNetwork,
                    NoticeServed = item.NoticeServed,
                    LeavingType = item.LeavingType,
                    PreviousExperience = item.PreviousExperience,
                    DOJInTraining = item.DOJInTraining,
                    DOJOnFloor = item.DOJOnFloor,
                    DOJInOJT = item.DOJInOJT,
                    DOJInOnroll=item.DOJInOnroll,
                    DateOfLeaving = item.DateOfLeaving,
                    BackOnFloor = item.BackOnFloor,
                    LeavingRemarks = item.LeavingRemarks,
                    MailReceivedFromAndDate = item.MailReceivedFromAndDate,
                    DateOfEmailSentToITForDeletion = item.DateOfEmailSentToITForDeletion,
                    ReportingToManagerEmployeeNumber = item.ReportingToManagerEmployeeNumber,
                    ReportingToIDL2Name = HttpContext.Session.GetString(Constants.EmployeeID),
                    InsertedByUserID = HttpContext.Session.GetString(Constants.UserID),
                    Status = item.Status,
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


    }
}
