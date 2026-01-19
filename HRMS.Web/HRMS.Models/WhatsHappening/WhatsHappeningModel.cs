using HRMS.Models.Leave;
using HRMS.Models.MyInfo;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.WhatsHappeningModel
{

    public class WhatsHappeningModelParans
    {
        public long CompanyID { get; set; }
        public long WhatsHappeningID { get; set; }
    }


    public class WhatsHappeningModels
    {
        public string EncodedWhatsHappeningID { get; set; } = string.Empty;

        public long WhatsHappeningID { get; set; }  
        public long CompanyID { get; set; }  
        public string? Title { get; set; } 
        public string? Description { get; set; }  
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } 
        public DateTime CreatedDate { get; set; } 
        public DateTime UpdatedDate { get; set; }  
        public long CreatedBy { get; set; }  
        public long UpdatedBy { get; set; }  
        public string IconImage { get; set; }


    }
}
