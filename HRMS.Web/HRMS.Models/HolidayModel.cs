
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Models
{
    public class HolidayInputParams
    {
        public long HolidayID { get; set; }
        public long CompanyID { get; set; }
        public long? LocationID { get; set; }
        public long? EmployeeID { get; set; }


    }
    public class HolidayModel
    {
        public long HolidayID { get; set; }
        public string? EncodedId { get; set; }
        public long CompanyID { get; set; }
        public string? HolidayName { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public long JobLocationTypeID { get; set; }
        public string? Location { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long CreatedBy { get; set; }
        public long UpdatedBy { get; set; }
        public List<SelectListItem> JobLocationList { get; set; } = new List<SelectListItem>();
    }
}
