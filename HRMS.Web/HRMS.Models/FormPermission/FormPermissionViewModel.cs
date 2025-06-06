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
        [Required(ErrorMessage = "Department is required")]
        public int SelectedDepartment { get; set; }
        public int FormID { get; set; }
        [Required(ErrorMessage = "Form is required")]
        public List<string> SelectedFormIds { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public long CreatedByID { get; set; }
    }
}
