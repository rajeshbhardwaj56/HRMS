using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class EducationDetailParams
    {
        public long EmployeeID { get; set; }
        public long EducationDetailID { get; set; }
    }
    public class EducationalDetail
    {
        //public string EncodedEducationDetailID { get; set; } = string.Empty;
        public long EducationDetailID { get; set; }
        public long EmployeeID { get; set; }
        public long EducationID { get; set; }
        public string School_University { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string YearOfPassing { get; set; } = string.Empty;
        public string Percentage { get; set; } = string.Empty;
        public string Major_OptionalSubjects { get; set; } = string.Empty;
        public string CertificateImage { get; set; } = string.Empty;
        public long UserID { get; set; }

    }
    public class EducationDetailList
    {
        public List<EducationalDetail> EducationalDetails { get; set; } = new List<EducationalDetail>();
    }

}
