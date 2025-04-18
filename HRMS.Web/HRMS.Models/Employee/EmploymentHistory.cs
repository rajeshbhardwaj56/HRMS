﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class EmploymentHistory
    {
        public long EmploymentHistoryID { get; set; }
        public long EmployeeID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string EmploymentID { get; set; } = string.Empty;//Identity card Number
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public long CountryID { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string ReasionFoLeaving { get; set; } = string.Empty;
        public string Designition { get; set; } = string.Empty;
        public string GrossSalary { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public string SupervisorDesignition { get; set; } = string.Empty;
        public string SupervisorContactNo { get; set; } = string.Empty;
        public string HRName { get; set; } = string.Empty;
        public string HREmail { get; set; } = string.Empty;
        public string HRContactNo { get; set; } = string.Empty;
    }
}
