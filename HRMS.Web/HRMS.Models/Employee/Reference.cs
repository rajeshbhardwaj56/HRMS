using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class Reference
    {
        public long ReferenceDetailID { get; set; }
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Phone number is not valid.")]
        public string Contact { get; set; } = string.Empty;
        public string OrgnizationName { get; set; } = string.Empty;
        public string RelationWithCandidate { get; set; } = string.Empty;
    }
}
