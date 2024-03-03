using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.Common
{
    public class LoginUser
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Result { get; set; } = "";
        public long UserID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Alias { get; set; } = "";
        public string Role { get; set; } = "";
        public int RoleId { get; set; }
        public string token { get; set; } = "";

    }

}