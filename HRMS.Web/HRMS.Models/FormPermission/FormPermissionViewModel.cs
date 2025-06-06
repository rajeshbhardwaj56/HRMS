using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Models.FormPermission
{
    public class FormPermissionViewModel
    {
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> FormsPermission { get; set; } = new List<SelectListItem>();
        public long? GroupFormID  { get; set; }
        public long SelectedDepartment { get; set; }
        public string? FormName { get; set; }
        public long? FormID { get; set; }
        [Required(ErrorMessage = "Form is required")]
        public List<string> SelectedFormIds { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public long CreatedByID { get; set; }
        public long? FormLevel { get; set; }
        public long? ParentId { get; set; }
        public bool? Status { get; set; }
        public bool IsFormPermissions { get; set; }
        public int? IsSelected { get; set; }
        
    }
    public class FormPermissionVM
    {
        public long DepartmentId { get; set; }
        public long EmployeeID { get; set; }
        public long FormId { get; set; }
        public List<string>? SelectedFormIds { get; set; }

    }
    public class FormPermissionListViewModel
    {
        public List<FormPermissionViewModel> FormPermissionList { get; set; }

    }
    public class EmployeePermissionVM 
    {
        public int? HasPermission { get; set; }

    }

}
