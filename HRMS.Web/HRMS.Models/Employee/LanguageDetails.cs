using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class LanguageDetail
    {
        public long LanguageDetailID { get; set; }
        public long LanguageID { get; set; }
        public bool IsSpeak { get; set; }
        public bool IsRead { get; set; }
        public bool IsWrite { get; set; }
    }
}
