using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class EducationalDetail
    {
        public long EducationDetailID { get; set; }
        public long EducationID { get; set; }
        public string School_University { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string YearOfPassing { get; set; } = string.Empty;
        public string Percentage { get; set; } = string.Empty;
        public string Major_OptionalSubjects { get; set; } = string.Empty;

    }
}
