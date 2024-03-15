using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class Results
    {
        public string Result { get; set; }
        public List<SelectListItem> Countries { get; set; }
        public List<SelectListItem> Langueges { get; set; }
    }
}
