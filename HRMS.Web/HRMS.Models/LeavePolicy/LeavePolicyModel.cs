using HRMS.Models.Leave;
using HRMS.Models.MyInfo;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.LeavePolicy
{

    public class LeavePolicyInputParans
    {
        public long CompanyID { get; set; }
        public long LeavePolicyID { get; set; }
    }


    public class LeavePolicyModel
    {
        public long LeavePolicyID { get; set; }
        public long CompanyID { get; set; }
        public string LeavePolicyName { get; set; } = string.Empty;
        //Annual
        public int Annual_MaximumLeaveAllocationAllowed { get; set; }
        public int Annual_ApplicableAfterWorkingDays { get; set; }
        public int Annual_MaximumConsecutiveLeavesAllowed { get; set; }
        public int Annual_MaximumEarnedLeaveAllowed { get; set; }
        public int Annual_MaximumMedicalLeaveAllocationAllowed { get; set; }
        public bool Annual_IsCarryForward { get; set; }
        public bool Annual_MedicalDocument { get; set; }

        //Maternity
        public int Maternity_MaximumLeaveAllocationAllowed { get; set; }
        public int Maternity_ApplicableAfterWorkingDays { get; set; }
        public int Maternity_ApplyBeforeHowManyDays { get; set; }
        public bool Maternity_MedicalDocument { get; set; }
        //Adoption
        public int Adoption_MaximumLeaveAllocationAllowed { get; set; }
        public int Adoption_ApplicableAfterWorkingDays { get; set; }
        public bool Adoption_MedicalDocument { get; set; }
        //Miscarriage
        public int Miscarriage_MaximumLeaveAllocationAllowed { get; set; }
        public bool Miscarriage_MedicalDocument { get; set; }
        //Comp Off
        public int CampOff_HoursOfWork { get; set; }
        public int CampOff_ExpiryDate { get; set; }
        public bool CampOff_MedicalDocument { get; set; }


        //Paternity
        public int Paternity_maximumLeaveAllocationAllowed { get; set; }
        public int Paternity_applicableAfterWorkingMonth { get; set; }
        public bool Paternity_active { get; set; }
        public bool Paternity_medicalDocument { get; set; }


    }
}
