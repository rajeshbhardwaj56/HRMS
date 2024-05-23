using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class ResetPasswordModel
    {
        public string UserID { get; set; }
        public string EmployeeID { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(15, MinimumLength = 6)]
        [Display(Name = "Password: ")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password mismatch")]
        [StringLength(15, MinimumLength = 6)]
        [Display(Name = "Confirm Password: ")]
        public string ConfirmPassword { get; set; }
        public bool IsResetPasswordRequired { get; set; }
        public bool IsResetPasswordExpired { get; set; }
        public DateTime dt { get; set; }

    }
}
