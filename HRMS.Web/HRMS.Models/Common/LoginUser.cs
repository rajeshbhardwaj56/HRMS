using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.Common
{
    public class LoginUser
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Result { get; set; } = "";
        public long UserID { get; set; }
        public long CompanyID { get; set; }
        public long EmployeeID { get; set; }
        public int GenderId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Alias { get; set; } = "";
        public string Role { get; set; } = "";
        public int RoleId { get; set; }
        public string token { get; set; } = "";
        public string Manager1Name { get; set; } = "";
        public string Manager1Email { get; set; } = "";
        public string Manager2Name { get; set; } = "";
        public string Manager2Email { get; set; } = "";
        public bool IsResetPasswordRequired { get; set; }

    }

}