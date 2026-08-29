using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMSConsoleApp.Model
{
    public class AttendanceInputParams
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public long UserId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
