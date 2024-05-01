using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Company
{
    public class CompanyModel
    {
        public string EncryptedIdentity { get; set; } = string.Empty;
        public long CompanyID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DefaultLetterHead { get; set; } = string.Empty;
        public string Abbr { get; set; } = string.Empty;
        public string TaxID { get; set; } = string.Empty;
        public long? DefaultCurrencyID { get; set; }
        public string Domain { get; set; } = string.Empty;
        public long? CountryID { get; set; }
        public DateTime? DateOfEstablished { get; set; }
        public bool IsGroup { get; set; }
        public string ParentCompany { get; set; } = string.Empty;
        public string CompanyLogo { get; set; } = string.Empty;
        public List<SelectListItem> Countries = new List<SelectListItem>();
        public List<SelectListItem> Currencies = new List<SelectListItem>();
    }
}
