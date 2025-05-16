using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{


    public class EmploymentBankDetailInputParams
    {
        public long EmployeeID { get; set; }
        public long UserID { get; set; }
    }
    public class EmploymentBankDetail
    {
        public string EncryptedIdentity { get; set; } = string.Empty;
        public long BankDetailID { get; set; }
        public long EmployeeID { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? UANNumber { get; set; }
        public string? IFSCCode { get; set; }
        public string? BankName { get; set; }       
       public long UserID { get; set; } 
    }
}
