using HRMS.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HRMS.Models.WhatsHappening
{
    public class WhatsHappeningModel
    {
        public WhatsHappening _WhatsHappening { get; set; } = new WhatsHappening();
        public List<WhatsHappening> _WhatsHappenings { get; set; } = new List<WhatsHappening>();
        public Result result { get; set; } = new Result();
    }


    public class WhatsHappening
    {

        public string EncryptedIdentity { get; set; } = string.Empty;
        public long WhatsHappeningID { get; set; }
        public long CompanyID { get; set; }
        public long UserID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string StringFromDate { get; set; } = string.Empty;
        public string StringToDate { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public string? IconImage { get; set; } = string.Empty;
    }
}
