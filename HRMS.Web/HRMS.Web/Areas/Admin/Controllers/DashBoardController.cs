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
                                    .FirstOrDefault(kvp => kvp.Key.Contains(ReportingToIDL1Name.ToLower(), StringComparison.OrdinalIgnoreCase)).Value;
                                if (ReportingToIDL1Id == 0) ReportingToIDL1Id = 0;
                            }                   
                            string ReportingToIDL2Name = GetCellValue(worksheet, row, columnIndexes, "ReportingToIDL2Name")?.Trim();
                            if (!string.IsNullOrEmpty(ReportingToIDL2Name) && employmentDetailsDictionaries.TryGetValue("Employees", out var ReportingToIDL2NameDict))
                            {
                                ReportingToIDL2Id = ReportingToIDL2NameDict
                                    .FirstOrDefault(kvp => kvp.Key.Contains(ReportingToIDL2Name.ToLower(), StringComparison.OrdinalIgnoreCase)).Value;
                                if (ReportingToIDL2Id == 0) ReportingToIDL2Id = 0;
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

                            // Get Gender (Male or Female)
                            string GenderName = GetCellValue(worksheet, row, columnIndexes, "Gender")?.Trim();
                            if (GenderName == "FeMale")
                            {
                                GenderId = 2;  // Female
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

            return Json(new { success = "Excel data imported successfully.",  });
        }

        [HttpPost]
        public async Task<JsonResult> ImportExcels(IFormFile file)
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

                    if (!string.IsNullOrEmpty(htmlTable) && htmlTable.Length > 5)
                    {
                        return Json(new
                        {
                            success = false,
                            hasErrors = true,
                            message = "Some rows contain errors.",
                            errorTable = htmlTable
                        });
                    }
                    return Json(new
                    {
                        success = true,
                        message = $"File uploaded and processed successfully. Processed {htmlTable} records."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"An error occurred while processing the file: {ex.Message}"
                });
            }
        }
        public string ProcessExcelFile(Stream stream, string fileName)
        {          
            var data = _businessLayer.SendGetAPIRequest(_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetCountryDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var countryDictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(data);
            var GetCompaniesDictionary = _businessLayer.SendGetAPIRequest(_businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetCompaniesDictionary), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var CompaniesDictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(GetCompaniesDictionary);
            DataTable mainDataTable = new DataTable();
            foreach (var prop in typeof(ImportExcelDataTable).GetProperties())
            {
                mainDataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            //DataTable importDataTable = new DataTable();
            //foreach (var prop in typeof(ImportExcelDataTableType).GetProperties())
            //{
            //    importDataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            //}
            DataTable errorDataTable = new DataTable();
            errorDataTable = mainDataTable.Clone();
            errorDataTable.Columns.Add("ErrorColumn", typeof(string));
            errorDataTable.Columns.Add("ErrorMessage", typeof(string));
            using (var package = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    var errorDataRow = errorDataTable.NewRow();
                    errorDataRow["ErrorColumn"] = "Excel file";
                    errorDataRow["ErrorMessage"] = $"The Excel sheet is empty.";
                    errorDataTable.Rows.Add(errorDataRow);
                    return ConvertDataTableToHTML(errorDataTable);
                }
                int totalColumns = worksheet.Dimension?.Columns ?? 0;
                int totalRows = worksheet.Dimension?.Rows ?? 0;
                if (totalColumns == 0 || totalRows < 2)
                {
                    var errorDataRow = errorDataTable.NewRow();
                    errorDataRow["ErrorColumn"] = "Excel file";
                    errorDataRow["ErrorMessage"] = $"The Excel sheet is empty.";
                    errorDataTable.Rows.Add(errorDataRow);
                    return ConvertDataTableToHTML(errorDataTable);
                }
                Dictionary<string, int> columnIndexes = new Dictionary<string, int>();
                var (isHeaderValid, mismatchedColumn) = ValidateHeaderRow(worksheet, mainDataTable);
                if (!isHeaderValid)
                {
                    var errorDataRow = errorDataTable.NewRow();
                    errorDataRow["ErrorColumn"] = "Excel file";
                    errorDataRow["ErrorMessage"] = $"The Excel file's header does not match the expected format. {mismatchedColumn}";
                    errorDataTable.Rows.Add(errorDataRow);
                    return ConvertDataTableToHTML(errorDataTable);
                }             
                long LeavePolicyId = 0;           
                long? countryId = 0;
                long DepartmentId = 0;
                long DesignationsId = 0;
                long EmploymentTypesId = 0;
                long SubDepartmentNameId = 0;
                long ShiftTypeId = 0;
                long JobLocationId = 0;
                long ReportingToIDL1Id = 0;
                long ReportingToIDL2Id = 0;
                long RoleId = 0;
                long PayrollTypeId = 0;              
                long GenderId = 0;
                var columnIndexMap = mainDataTable.Columns.Cast<DataColumn>()
    .Select((col, idx) => new { col.ColumnName, Index = idx + 1 })
    .ToDictionary(x => x.ColumnName, x => x.Index);
                for (int row = 2; row <= totalRows; row++)
                {
                    if (IsRowEmpty(worksheet, row))
                        continue;
                    bool hasError = false;
                    bool isEmptyRow = true;
                    string CompanyName = worksheet.Cells[row, mainDataTable.Columns.IndexOf("CompanyName") + 1].Text;
                    long? GetCompanyId = CompaniesDictionary.TryGetValue(CompanyName.ToLower().Trim(), out long CompanyId) ? CompanyId : (long?)0;
                    if (string.IsNullOrEmpty(CompanyName) || GetCompanyId == 0)
                    {
                        hasError = true;
                        var errorRow = errorDataTable.NewRow();
                        errorRow["ErrorColumn"] = "CompanyName";
                        errorRow["ErrorMessage"] = $"Row {row}: Company '{CompanyName}' not found in the system.";
                        errorDataTable.Rows.Add(errorRow);
                        continue;
                    }
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
                    DataRow dataRow = mainDataTable.NewRow();                
                    foreach (DataColumn column in mainDataTable.Columns)
                    {

                        string columnName = column.ColumnName;
                        var cellValue = worksheet.Cells[row, columnIndexMap[columnName]].Text?.Trim();
                        switch (columnName)
                        {
                            case "CompanyName":
                                cellValue = GetCompanyId.ToString();
                                break;
                            case "CorrespondenceCountryName":
                                if (!string.IsNullOrEmpty(cellValue))
                                {
                                    countryId = countryDictionary.TryGetValue(cellValue.ToLower().Trim(), out long id) ? id : (long?)0;
                                    if (countryId != 0)
                                    {
                                        cellValue = countryId.ToString();
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Country '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: Country name is mandatory.");
                                    hasError = true;
                                }
                                break;
                            case "PermanentCountryName":
                                if (!string.IsNullOrEmpty(cellValue))
                                {
                                    countryId = countryDictionary.TryGetValue(cellValue.ToLower().Trim(), out long id) ? id : (long?)0;
                                    if (countryId != 0)
                                    {
                                        cellValue = countryId.ToString();
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Country '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: Country name is mandatory.");
                                    hasError = true;
                                }
                                break;
                            case "JobLocationName":
                                if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("JobLocations", out var JobLocationNameDict))
                                {
                                    var matchedPolicy = JobLocationNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                    JobLocationId = matchedPolicy.Value;
                                    if (JobLocationId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: JobLocationName '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
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
                                    if (LeavePolicyId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: LeavePolicy '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: LeavePolicies dictionary is missing or empty.");
                                    hasError = true;
                                }
                                break;
                            case "RoleName":
                                if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("Roles", out var RoleNameDict))
                                {
                                    var matchedPolicy = RoleNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                    RoleId = matchedPolicy.Value;
                                    if (LeavePolicyId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: RoleName '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: RoleName dictionary is missing or empty.");
                                    hasError = true;
                                }
                                break;
                            case "PayrollTypeName":
                                if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("PayrollTypes", out var PayrollTypeNameDict))
                                {
                                    var matchedPolicy = PayrollTypeNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                    PayrollTypeId = matchedPolicy.Value;
                                    if (PayrollTypeId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: PayrollTypeName '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: PayrollTypeName dictionary is missing or empty.");
                                    hasError = true;
                                }
                                break;
                            case "DepartmentName":
                                if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("Departments", out var departmentDict))
                                {
                                    var DepartmentName = departmentDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                    DepartmentId = DepartmentName.Value;
                                    if (DepartmentId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: DepartmentName '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: DepartmentName dictionary is missing or empty.");
                                    hasError = true;
                                }
                                break;
                            case "SubDepartmentName":
                                if (!string.IsNullOrEmpty(cellValue) && SubDepartmentDictionaries != null)
                                {
                                    var matchedSubDept = SubDepartmentDictionaries
                                        .FirstOrDefault(kvp => kvp.Key.Equals(cellValue.Trim(), StringComparison.OrdinalIgnoreCase));

                                    SubDepartmentNameId = matchedSubDept.Value;

                                    if (SubDepartmentNameId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: SubDepartmentName '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: SubDepartment dictionary is missing or empty.");
                                    hasError = true;
                                }
                                break;
                            case "DesignationName":
                                if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("Designations", out var DesignationsDict))
                                {
                                    var DesignationName = DesignationsDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                    DesignationsId = DesignationName.Value;
                                    if (DesignationsId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: DesignationName '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: LeavePolicies dictionary is missing or empty.");
                                    hasError = true;
                                }
                                break;
                            case "EmployeeType":
                                if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("EmploymentTypes", out var EmploymentTypesDict))
                                {
                                    var matchedPolicy = EmploymentTypesDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                    EmploymentTypesId = matchedPolicy.Value;
                                    if (EmploymentTypesId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: EmployeeType '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: EmployeeType dictionary is missing or empty.");
                                    hasError = true;
                                }
                                break;
                            case "ShiftTypeName":
                                if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("ShiftTypes", out var ShiftTypeNameDict))
                                {
                                    var matchedPolicy = ShiftTypeNameDict
                                        .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                                    ShiftTypeId = matchedPolicy.Value;
                                    if (ShiftTypeId == 0)
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: ShiftTypeName '{cellValue}' not found in master data.");
                                        hasError = true;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cellValue))
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: ShiftTypeName dictionary is missing or empty.");
                                    hasError = true;
                                }
                                break;
                            //case "ReportingToIDL1Name":
                            //    if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("Employees", out var ReportingToIDL1NameDict))
                            //    {
                            //        var matchedPolicy = ReportingToIDL1NameDict
                            //            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                            //        ReportingToIDL1Id = matchedPolicy.Value;
                            //        if (ReportingToIDL1Id == 0)
                            //        {
                            //            AddError(errorDataTable, columnName, $"Row {row}: ReportingToIDL1Name '{cellValue}' not found in master data.");
                            //            hasError = true;
                            //        }
                            //    }
                            //    else if (!string.IsNullOrEmpty(cellValue))
                            //    {
                            //        AddError(errorDataTable, columnName, $"Row {row}: LeavePolicies dictionary is missing or empty.");
                            //        hasError = true;
                            //    }
                            //    break;
                            //case "ReportingToIDL2Name":
                            //    if (!string.IsNullOrEmpty(cellValue) && employmentDetailsDictionaries.TryGetValue("Employees", out var ReportingToIDL2NameDict))
                            //    {
                            //        var matchedPolicy = ReportingToIDL2NameDict
                            //            .FirstOrDefault(kvp => kvp.Key.Contains(cellValue, StringComparison.OrdinalIgnoreCase));
                            //        ReportingToIDL2Id = matchedPolicy.Value;
                            //        if (ReportingToIDL2Id == 0)
                            //        {
                            //            AddError(errorDataTable, columnName, $"Row {row}: ReportingToIDL2Name '{cellValue}' not found in master data.");
                            //            hasError = true;
                            //        }
                            //    }
                            //    else if (!string.IsNullOrEmpty(cellValue))
                            //    {
                            //        AddError(errorDataTable, columnName, $"Row {row}: ReportingToIDL2Name dictionary is missing or empty.");
                            //        hasError = true;
                            //    }
                            //    break;
                            case "Gender":
                                if (!string.IsNullOrEmpty(cellValue))
                                {
                                    if (cellValue.Equals("Female", StringComparison.OrdinalIgnoreCase))
                                    {
                                        GenderId = 2;
                                    }
                                    else if (cellValue.Equals("Male", StringComparison.OrdinalIgnoreCase))
                                    {
                                        GenderId = 1;
                                    }
                                    else
                                    {
                                        AddError(errorDataTable, columnName, $"Row {row}: Gender '{cellValue}' is invalid. Allowed values are 'Male' or 'Female'.");
                                        hasError = true;
                                    }
                                }
                                else
                                {
                                    AddError(errorDataTable, columnName, $"Row {row}: Gender is required.");
                                    hasError = true;
                                }
                                break;
                        }





                        if (!string.IsNullOrEmpty(cellValue))
                        {
                            isEmptyRow = false;
                        }
                        dataRow[columnName] = cellValue;
                    }
                    if (!isEmptyRow && !hasError)
                    {
                        mainDataTable.Rows.Add(dataRow);                      
                    }
                    
                }
            }
            if (mainDataTable.Rows.Count > 0 && errorDataTable.Rows.Count == 0)
            {
                var importDataTable = ConvertToImportExcelDataTableType(mainDataTable);
                var EmployeeData = _businessLayer.SendPostAPIRequest(importDataTable, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.AddUpdateEmployeeFromExecel1), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<HRMS.Models.Common.Result>(EmployeeData);
            }
            return ConvertDataTableToHTML(errorDataTable);
        }
        private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string columnName)
        {
            return columnIndexes.ContainsKey(columnName)
                ? worksheet.Cells[row, columnIndexes[columnName]]?.Value?.ToString()?.Trim()
                : null;
        }
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
       
        private bool GetBooleanValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string columnName)
        {
            if (columnIndexes.ContainsKey(columnName) && worksheet.Cells[row, columnIndexes[columnName]]?.Value != null)
            {
                string value = worksheet.Cells[row, columnIndexes[columnName]].Value.ToString().Trim();
                return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("1");
            }
            return false;
        }

        private bool IsValidData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return false;

            return true;
        }

        private bool IsNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
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

                    // Add only the ErrorColumn and ErrorMessage values
                    html.Append("<td>").Append(row["ErrorColumn"]).Append("</td>");
                    html.Append("<td>").Append(row["ErrorMessage"]).Append("</td>");

                    html.Append("</tr>");
                }

                html.Append("</tbody></table>");
                return html.ToString();
            }
            return string.Empty;
        }
        private (bool isValid, string mismatchedColumn)  ValidateHeaderRow(ExcelWorksheet worksheet, DataTable mainDataTable)
        {
            for (int col = 1; col < mainDataTable.Columns.Count; col++)
            {
                if (!string.Equals(mainDataTable.Columns[col - 1].ColumnName, worksheet.Cells[1, col].Text.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return (
                 false,
                 $"Expected '{mainDataTable.Columns[col - 1].ColumnName}' but found '{worksheet.Cells[1, col].Text.Trim()}' at column {col}"
             );
                }
            }
            return (true, null);
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

        public static DataTable ConvertToImportExcelDataTableType(DataTable mainDataTable)
        {
            var dt = new DataTable();           
            var manualMapping = new Dictionary<string, string>
    {
     { "CompanyName", "CompanyID" },
    { "CorrespondenceCountryName", "CorrespondenceCountryID" },
    { "PermanentCountryName", "PermanentCountryID" },
    { "ReferredByEmployeeName", "ReferredByEmployeeID" },
    { "ExtraCurricularActivities", "ExtraCuricuarActivities" },
    { "ForeignCountryVisits", "ForiegnCountryVisits " },
    { "DesignationName", "DesignationID" },
    { "EmployeeType", "EmployeeTypeID" },
    { "DepartmentName", "DepartmentID" },
    { "SubDepartmentName", "SubDepartmentID" },
    { "ShiftTypeName", "ShiftTypeID" },
    { "JobLocationName", "JobLocationID" },
    { "ReportingToIDL1Name", "ReportingToIDL1" },
    { "ReportingToIDL2Name", "ReportingToIDL2" },
    { "PayrollTypeName", "PayrollTypeID" },
    { "LeavePolicyName", "LeavePolicyID" },
    { "RoleName", "RoleID" },
    { "DOJInTraining", "DateOfJoiningTraining" },
    { "DOJOnFloor", "DateOfJoiningFloor" },
    { "DOJInOJTOnroll", "DateOfJoiningOJT" },
    { "BackOnFloor", "BackOnFloorDate" },
    { "DateOfEmailSentToITForIDDeletion", "EmailSentToITDate" },        
    };

            
   var destinationColumns = new Dictionary<string, Type>
{
    { "EmployeeID", typeof(long?) },
    { "CompanyID", typeof(long?) },
    { "FirstName", typeof(string) },
    { "MiddleName", typeof(string) },
    { "Surname", typeof(string) },
    { "ProfilePhoto", typeof(string) },
    { "CorrespondenceAddress", typeof(string) },
    { "CorrespondenceCity", typeof(string) },
    { "CorrespondencePinCode", typeof(string) },
    { "CorrespondenceState", typeof(string) },
    { "CorrespondenceCountryID", typeof(long?) },
    { "EmailAddress", typeof(string) },
    { "Landline", typeof(string) },
    { "Mobile", typeof(string) },
    { "Telephone", typeof(string) },
    { "PersonalEmailAddress", typeof(string) },
    { "PermanentAddress", typeof(string) },
    { "PermanentCity", typeof(string) },
    { "PermanentPinCode", typeof(string) },
    { "PermanentState", typeof(string) },
    { "PermanentCountryID", typeof(long?) },
    { "PeriodOfStay", typeof(string) },
    { "VerificationContactPersonName", typeof(string) },
    { "VerificationContactPersonContactNo", typeof(string) },
    { "DateOfBirth", typeof(DateTime?) },
    { "PlaceOfBirth", typeof(string) },
    { "IsReferredByExistingEmployee", typeof(bool?) },
    { "ReferredByEmployeeID", typeof(string) },
    { "BloodGroup", typeof(string) },
    { "PANNo", typeof(string) },
    { "AadharCardNo", typeof(string) },
    { "Allergies", typeof(string) },
    { "IsRelativesWorkingWithCompany", typeof(bool?) },
    { "RelativesDetails", typeof(string) },
    { "MajorIllnessOrDisability", typeof(string) },
    { "AwardsAchievements", typeof(string) },
    { "EducationGap", typeof(string) },
    { "ExtraCuricuarActivities", typeof(string) },
    { "ForiegnCountryVisits", typeof(string) },
    { "ContactPersonName", typeof(string) },
    { "ContactPersonMobile", typeof(string) },
    { "ContactPersonTelephone", typeof(string) },
    { "ContactPersonRelationship", typeof(string) },
    { "ITSkillsKnowledge", typeof(string) },
    { "InsertedByUserID", typeof(long?) },
    { "UpdatedByUserID", typeof(long?) },
    { "InsertedDate", typeof(DateTime?) },
    { "UpdatedDate", typeof(DateTime?) },
    { "IsActive", typeof(bool?) },
    { "IsDeleted", typeof(bool?) },
    { "LeavePolicyID", typeof(long?) },
    { "CarryForword", typeof(long?) },
    { "Gender", typeof(int?) },
    { "PanCardImage", typeof(string) },
    { "AadhaarCardImage", typeof(string) },
    { "UserName", typeof(string) },
    { "PasswordHash", typeof(string) },
    { "Email", typeof(string) },
    { "ModifiedDate", typeof(DateTime?) },
    { "IsResetPasswordRequired", typeof(bool) },
    { "RoleID", typeof(int) },
    { "EmployeNumber", typeof(string) },
    { "DesignationID", typeof(long?) },
    { "EmployeeTypeID", typeof(long?) },
    { "DepartmentID", typeof(long?) },
    { "JobLocationID", typeof(long?) },
    { "OfficialEmailID", typeof(string) },
    { "OfficialContactNo", typeof(string) },
    { "JoiningDate", typeof(DateTime?) },
    { "JobSeprationDate", typeof(DateTime?) },
    { "ReportingToIDL1", typeof(long?) },
    { "PayrollTypeID", typeof(long?) },
    { "ReportingToIDL2", typeof(long?) },
    { "ClientName", typeof(string) },
    { "SubDepartmentID", typeof(long?) },
    { "ShiftTypeID", typeof(long?) },
    { "ESINumber", typeof(string) },
    { "ESIRegistrationDate", typeof(DateTime?) },
    { "BankAccountNumber", typeof(string) },
    { "IFSCCode", typeof(string) },
    { "BankName", typeof(string) },
    { "AgeOnNetwork", typeof(int?) },
    { "NoticeServed", typeof(int?) },
    { "LeavingType", typeof(string) },
    { "PreviousExperience", typeof(int?) },
    { "DateOfJoiningTraining", typeof(DateTime?) },
    { "DateOfJoiningFloor", typeof(DateTime?) },
    { "DateOfJoiningOJT", typeof(DateTime?) },
    { "DateOfResignation", typeof(DateTime?) },
    { "DateOfLeaving", typeof(DateTime?) },
    { "BackOnFloorDate", typeof(DateTime?) },
    { "LeavingRemarks", typeof(string) },
    { "MailReceivedFromAndDate", typeof(string) },
    { "EmailSentToITDate", typeof(DateTime?) },
};

            foreach (var destColumn in destinationColumns)
            {
                var columnType = Nullable.GetUnderlyingType(destColumn.Value) ?? destColumn.Value;
                dt.Columns.Add(destColumn.Key, columnType);
            }
            foreach (DataRow sourceRow in mainDataTable.Rows)
            {
                var newRow = dt.NewRow();

                foreach (var destCol in destinationColumns)
                {
                    string sourceCol = manualMapping.FirstOrDefault(x => x.Value == destCol.Key).Key ?? destCol.Key;

                    if (mainDataTable.Columns.Contains(sourceCol) && !string.IsNullOrWhiteSpace(sourceRow[sourceCol]?.ToString()))
                    {
                        try
                        {
                            newRow[destCol.Key] = Convert.ChangeType(sourceRow[sourceCol], destCol.Value);
                        }
                        catch
                        {
                            newRow[destCol.Key] = DBNull.Value;
                        }
                    }
                    else
                    {
                        newRow[destCol.Key] = DBNull.Value;
                    }
                }

                dt.Rows.Add(newRow);
            }

            return dt;
        }
    }
}
