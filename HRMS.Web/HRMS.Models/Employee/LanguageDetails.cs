using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Models.Employee
{
    public class LanguageDetailParams
    {
        public long EmployeeID { get; set; }
        public long EmployeeLanguageDetailID { get; set; }
    }
    public class LanguageDetail
    {
        public long? LanguageDetailID { get; set; }
        public long EmployeeID { get; set; }
        public long LanguageID { get; set; }
        public string LanguageName { get; set; } = string.Empty;
        public bool IsSpeak { get; set; }
        public bool IsRead { get; set; }
        public bool IsWrite { get; set; }
        public long UserID { get; set; }

        public List<SelectListItem> Languages = new List<SelectListItem>();
    }
}
