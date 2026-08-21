using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.PayRoll
{
    public class SalaryInputParams
    {
        public long CompanyID { get; set; }
        public long? EmployeeID { get; set; }
        public long? MonthlySalaryID { get; set; }
        public long? SalaryID { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? DisplayStart { get; set; } = 0;
        public int? DisplayLength { get; set; } = 10;
        public string? Searching { get; set; }
        public string? SortCol { get; set; }
        public string? SortDir { get; set; }
    }
    public class AutoCalculateSalaryRequestModel
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public long? UserID { get; set; }
    }
    public class SalaryDetails
    {
        public string? EncryptedSalaryID { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }

        public long EmployeeID { get; set; }
        public long MonthlySalaryID { get; set; }
        public long SalaryID { get; set; }

        public long PayrollTypeID { get; set; }
        public string? PayrollTypeName { get; set; }

        public int SalaryMonth { get; set; }
        public int SalaryYear { get; set; }

        public decimal RevisedGross { get; set; }

        public decimal MonthDays { get; set; }
        public decimal PayableDays { get; set; }

        public decimal BasicFixed { get; set; }
        public decimal HRAFixed { get; set; }
        public decimal ConveyanceFixed { get; set; }
        public decimal SpecialAllowanceFixed { get; set; }
        public decimal GrossSalaryFixed { get; set; }

        public decimal BasicPayable { get; set; }
        public decimal HRAPayable { get; set; }
        public decimal ConveyancePayable { get; set; }
        public decimal SpecialAllowancePayable { get; set; }
        public decimal GrossSalaryPayable { get; set; }

        public decimal ClientIncentive { get; set; }
        public decimal PLI { get; set; }
        public decimal FloorIncentive { get; set; }
        public decimal EmpReferal { get; set; }
        public decimal TrainingFee { get; set; }
        public decimal GWR { get; set; }
        public decimal OtherAdditonArrear { get; set; }

        public decimal EMPLWF { get; set; }
        public decimal TDS { get; set; }
        public decimal DbtDeduction { get; set; }
        public decimal Advanceded { get; set; }
        public decimal InsuranceDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        public decimal EMPPF { get; set; }
        public decimal EMPESI { get; set; }
        public decimal PTAX { get; set; }

        public decimal TotalDeduction { get; set; }
        public decimal NetPayable { get; set; }

        public decimal EmployerPF { get; set; }
        public decimal EmployerESI { get; set; }
        public decimal EmployerLWF { get; set; }
        public decimal TotalEmployerContribution { get; set; }

        public decimal EPFWages { get; set; }
        public decimal EPSWages { get; set; }
        public decimal EDLIWages { get; set; }

        public decimal EPFAdminCharges { get; set; }
        public decimal EDLIContribution { get; set; }
        public decimal EDLIAdminCharges { get; set; }

        public decimal CTC { get; set; }

        public string? OfficialEmail { get; set; }

        // ============================================================
        // EXPORT / SALARY SLIP DETAILS
        // ============================================================

        public DateTime? PayrollStartDate { get; set; }
        public DateTime? PayrollEndDate { get; set; }

        public string? Designation { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }

        public DateTime? DateOfJoining { get; set; }

        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? IFSCCode { get; set; }
        public string? UANNumber { get; set; }

        public string? MonthName { get; set; }

        public decimal EmployeeGrossSalaryFixed { get; set; }

        public int? TotalRecords { get; set; } = 0;
        public int? FilteredRecords { get; set; } = 0;
        public bool? IsVerified { get; set; }
    }


    public class EmployeeMonthlySalaryModel
    {
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public long? MonthlySalaryID { get; set; }
        public decimal? GrossSalary { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? HRA { get; set; }
        public decimal? ConveyanceAllowance { get; set; }
        public decimal? SpecialAllowance { get; set; }
        public string? PayrollTypeName { get; set; }

        public decimal? PF { get; set; }
        public decimal? ESI { get; set; }
        public decimal? LWF { get; set; }
        public decimal? PTax { get; set; }
        public decimal? TDS { get; set; }
        public decimal? EmployerPF { get; set; }
        public decimal? EmployerESI { get; set; }
        public decimal? EmployerLWF { get; set; }
        public decimal? Gratuity { get; set; }
        public decimal? TotalEarnings { get; set; }
        public decimal? TotalDeductions { get; set; }
        public decimal? InHandSalary { get; set; }
        public decimal? CostToCompany { get; set; }

        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public long UpdatedByUserID { get; set; }
    }





    public class EmployeeSalaryRequestModel
    {
        public long EmployeeID { get; set; }
        public long SalaryID { get; set; }
        public long PayrollTypeID { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal MonthDays { get; set; }
        public decimal PayableDays { get; set; }

        public decimal RevisedGross { get; set; }

        public decimal ClientIncentive { get; set; }
        public decimal PLI { get; set; }
        public decimal FloorIncentive { get; set; }
        public decimal EmpReferal { get; set; }
        public decimal TrainingFee { get; set; }
        public decimal GWR { get; set; }
        public decimal OtherAdditonArrear { get; set; }

        public decimal EMPLWF { get; set; }
        public decimal TDS { get; set; }
        public decimal DbtDeduction { get; set; }
        public decimal Advanceded { get; set; }
        public decimal InsuranceDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        public long InsertedByUserID { get; set; }

        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }

        public decimal BasicFixed { get; set; }
        public decimal HRAFixed { get; set; }
        public decimal ConveyanceFixed { get; set; }
        public decimal SpecialAllowanceFixed { get; set; }
        public decimal GrossSalaryFixed { get; set; }

        public decimal BasicPayable { get; set; }
        public decimal HRAPayable { get; set; }
        public decimal ConveyancePayable { get; set; }
        public decimal SpecialAllowancePayable { get; set; }

        public decimal GrossSalaryPayable { get; set; }

        

        public decimal EMPPF { get; set; }
        public decimal EMPESI { get; set; }
        public decimal PTAX { get; set; }

        public decimal TotalDeduction { get; set; }
        public decimal NetPayable { get; set; }
        public decimal EmployerPF { get; set; }
        public decimal EmployerESI { get; set; }
        public decimal EmployerLWF { get; set; }
        public decimal TotalEmployerContribution { get; set; }

        public decimal EPFWages { get; set; }
        public decimal EPSWages { get; set; }
        public decimal EDLIWages { get; set; }

        public decimal EPFAdminCharges { get; set; }
        public decimal EDLIContribution { get; set; }
        public decimal EDLIAdminCharges { get; set; }

        public decimal CTC { get; set; }
        public string? BankAccountNumber { get; set; }

        public string? BankName { get; set; }

        public string? Designation { get; set; }

        public string? Department { get; set; }

        public string? Location { get; set; }

        public DateTime? DateOfJoining { get; set; }

        public string? CompanyLogo { get; set; }

        public string? MonthName { get; set; }

        public string? NetPayableInWords { get; set; }
        public string? IFSCCode { get; set; }
        public string? UANNumber { get; set; }
        public string? OfficialEmail { get; set; }
    }


    public class EmployeeSalaryGetRequestModel
    {
        public long EmployeeID { get; set; }
        public int SalaryMonth { get; set; }
        public int SalaryYear { get; set; }
    }
   


public class EmployeeSalaryCalculationModel
    {
        // ------------------------------------------------------------
        // BASIC INFORMATION
        // ------------------------------------------------------------

        public long EmployeeID { get; set; }
        public long PayrollTypeID { get; set; }
        public int SalaryMonth { get; set; }
        public int SalaryYear { get; set; }

        // ------------------------------------------------------------
        // SALARY
        // ------------------------------------------------------------

        public decimal RevisedGross { get; set; }
        public decimal MonthDays { get; set; }
        public decimal PayableDays { get; set; }

        // ------------------------------------------------------------
        // FIXED SALARY
        // ------------------------------------------------------------

        public decimal BasicFixed { get; set; }
        public decimal HRAFixed { get; set; }
        public decimal ConveyanceFixed { get; set; }
        public decimal SpecialAllowanceFixed { get; set; }
        public decimal GrossSalaryFixed { get; set; }

        // ------------------------------------------------------------
        // PAYABLE SALARY
        // ------------------------------------------------------------

        public decimal BasicPayable { get; set; }
        public decimal HRAPayable { get; set; }
        public decimal ConveyancePayable { get; set; }
        public decimal SpecialAllowancePayable { get; set; }

        public decimal GrossSalaryPayable { get; set; }

        // ------------------------------------------------------------
        // EARNINGS
        // ------------------------------------------------------------

        public decimal ClientIncentive { get; set; }
        public decimal PLI { get; set; }
        public decimal FloorIncentive { get; set; }
        public decimal EmpReferal { get; set; }
        public decimal TrainingFee { get; set; }
        public decimal GWR { get; set; }
        public decimal OtherAdditonArrear { get; set; }

        // ------------------------------------------------------------
        // EMPLOYEE DEDUCTIONS
        // ------------------------------------------------------------

        public decimal EMPLWF { get; set; }
        public decimal TDS { get; set; }
        public decimal DbtDeduction { get; set; }
        public decimal Advanceded { get; set; }
        public decimal InsuranceDeduction { get; set; }
        public decimal OtherDeduction { get; set; }

        // ------------------------------------------------------------
        // EMPLOYEE STATUTORY DEDUCTIONS
        // ------------------------------------------------------------

        public decimal EMPPF { get; set; }
        public decimal EMPESI { get; set; }
        public decimal PTAX { get; set; }

        // ------------------------------------------------------------
        // TOTAL EMPLOYEE DEDUCTION
        // ------------------------------------------------------------

        public decimal TotalDeduction { get; set; }
        public decimal NetPayable { get; set; }

        // ------------------------------------------------------------
        // EMPLOYER CONTRIBUTION
        // ------------------------------------------------------------

        public decimal EmployerPF { get; set; }
        public decimal EmployerESI { get; set; }
        public decimal EmployerLWF { get; set; }

        public decimal TotalEmployerContribution { get; set; }

        // ------------------------------------------------------------
        // EPF / EPS / EDLI
        // ------------------------------------------------------------

        public decimal EPFWages { get; set; }
        public decimal EPSWages { get; set; }
        public decimal EDLIWages { get; set; }

        public decimal EPFAdminCharges { get; set; }
        public decimal EDLIContribution { get; set; }
        public decimal EDLIAdminCharges { get; set; }

        // ------------------------------------------------------------
        // FINAL CTC
        // ------------------------------------------------------------

        public decimal CTC { get; set; }

        // ------------------------------------------------------------
        // USER
        // ------------------------------------------------------------

        public long InsertedByUserID { get; set; }
        // =====================================================
        // SALARY SLIP DISPLAY FIELDS
        // =====================================================

        public string? BankAccountNumber { get; set; }

        public string? BankName { get; set; }

        public string? Designation { get; set; }

        public string? Department { get; set; }

        public string? Location { get; set; }

        public DateTime? DateOfJoining { get; set; }

        public string? CompanyLogo { get; set; }

        public string? MonthName { get; set; }

        public string? NetPayableInWords { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public string? IFSCCode { get; set; }
        public string? UANNumber { get; set; }
        public string? OfficialEmail { get; set; }
    }

    public class EmailSalarySlipsRequest
    {
        public long EmployeeID { get; set; }

        public List<SalaryMonthRequest> Months { get; set; }
            = new List<SalaryMonthRequest>();
    }

    public class SalaryMonthRequest
    {
        public int Month { get; set; }

        public int Year { get; set; }
    }
    public class EmployeeSalaryMonth
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public string? MonthName { get; set; }
    }
    public class EmployeeSalaryMonthRequestModel
    {
        public long EmployeeID { get; set; }
    }
    public class BulkEmployeeSalaryRequestModel
    {
        public int RowNumber { get; set; }

        public string? EmployeeNumber { get; set; }

        public string? PayrollType { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public decimal GrossSalary { get; set; }

        public decimal? MonthDays { get; set; }

        public decimal? PayableDays { get; set; }

        // Earnings

        public decimal ClientIncentive { get; set; }

        public decimal PLI { get; set; }

        public decimal FloorIncentive { get; set; }

        public decimal EmployeeReferral { get; set; }

        public decimal TrainingFee { get; set; }

        public decimal GWR { get; set; }

        public decimal OtherAdditionArrear { get; set; }

        // Deductions

        public decimal EMPLWF { get; set; }

        public decimal TDS { get; set; }

        public decimal DBTDeduction { get; set; }

        public decimal AdvanceDeduction { get; set; }

        public decimal InsuranceDeduction { get; set; }

        public decimal OtherDeduction { get; set; }

    }
    public class BulkSalaryImportResultModel
    {
        public Guid BatchID { get; set; }

        public int TotalRecords { get; set; }

        public int ValidRecords { get; set; }

        public int FailedRecords { get; set; }

        public List<BulkSalaryImportErrorModel> Errors { get; set; }
            = new List<BulkSalaryImportErrorModel>();
        public int InsertedRecords { get; set; }

        public int UpdatedRecords { get; set; }

        public int SkippedRecords { get; set; }
    }
    public class BulkSalaryImportErrorModel
    {
        public int RowNumber { get; set; }

        public string? EmployeeNumber { get; set; }

        public string? PayrollType { get; set; }

        public int? SalaryYear { get; set; }

        public int? SalaryMonth { get; set; }

        public string? ErrorMessage { get; set; }
    }
    public class BulkSalaryImportRequestModel
    {
        public long UserID { get; set; }

        public string? FileName { get; set; }

        public List<BulkEmployeeSalaryRequestModel> SalaryList { get; set; } = new();
    }
    public class AutoSalaryCalculationModel
    {
        public int SalaryYear { get; set; }
        public int SalaryMonth { get; set; }
        public DateTime? PayrollStartDate { get; set; }
        public DateTime? PayrollEndDate { get; set; }
        public int InsertedRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public int FailedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public int TotalProcessed { get; set; }
    }

    public class AutoSalaryCalculationErrorModel
    {
        public long? EmployeeID { get; set; }
        public long? PayrollTypeID { get; set; }
        public decimal? GrossSalary { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ErrorDate { get; set; }
    }
    public class VerifyEmployeeSalaryRequestModel
    {
        public List<long> EmployeeIDs { get; set; } = new List<long>();

        public int Month { get; set; }

        public int Year { get; set; }

        public long UserID { get; set; }

    }
}
