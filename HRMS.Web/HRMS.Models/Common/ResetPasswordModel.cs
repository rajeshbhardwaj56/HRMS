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
<<<<<<< HEAD
        public string CompanyID { get; set; }

    }
=======

	}
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39


	public class ChangePasswordModel
	{

		//[Required(ErrorMessage = "Username is required")]
<<<<<<< HEAD
		//[Required(ErrorMessage = "Email is required")]
		//[StringLength(200)]
		//[EmailAddress(ErrorMessage = "Invalid Email Address")]
		//[Display(Name = "Email ID: ")]
=======
		[Required(ErrorMessage = "Email is required")]
		[StringLength(200)]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		[Display(Name = "Email ID: ")]
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
		public string EmailId { get; set; } = string.Empty;

		//[Required(ErrorMessage = "Old Password is required")]
		//[DataType(DataType.Password)]
		//[StringLength(15, MinimumLength = 6)]
		//[Display(Name = "Old Password: ")]
		//public string OldPassword { get; set; } = string.Empty;


		//[Required(ErrorMessage = "Password is required")]
		//[DataType(DataType.Password)]
		//[StringLength(15, MinimumLength = 6)]
		//[Display(Name = "Password: ")]
		//public string Password { get; set; } = string.Empty;

		//[Required(ErrorMessage = "Confirm Password is required")]
		//[DataType(DataType.Password)]
		//[Compare("Password", ErrorMessage = "Password mismatch")]
		//[StringLength(15, MinimumLength = 6)]
		//[Display(Name = "Confirm Password: ")]
		//public string ConfirmPassword { get; set; } = string.Empty;
		//public Int64 UserID { get; set; }

		//[Required(ErrorMessage = "Phone is required")]
		//[Display(Name = "Phone")]
		//[StringLength(15, MinimumLength = 6)]
		//[DataType(DataType.PhoneNumber)]
		//[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Invalid phone number. Phone number should be of 10 digits")]
		//public string Phone { get; set; } = string.Empty;

	}
}
