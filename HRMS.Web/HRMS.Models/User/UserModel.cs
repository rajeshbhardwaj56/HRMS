using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.User
{
    public class UserModel
    {
        public long UserID { get; set; }
        public Guid guid { get; set; }
        public long EmployeeID { get; set; }
        public long CompanyID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public bool IsResetPasswordRequired { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int RoleID { get; set; }
        public long EmployeeTypeID { get; set; }
    }
}
