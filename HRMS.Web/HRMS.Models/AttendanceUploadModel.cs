using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class AttendanceUploadModel
    {
        public string EmployeeNumber { get; set; }

        public DateTime? WorkDate { get; set; }

        public string? AttendanceStatus { get; set; }

        public string? Remarks { get; set; }
    }
    public class AttendanceUploadModelList
    {
        public List<AttendanceUploadModel> AttendanceList { get; set; } = new();

        public long UserID { get; set; }
    }
}
