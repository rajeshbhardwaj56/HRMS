using HRMS.Models.Employee;

namespace HRMS.Models.Template
{
    public class TemplateInputParams
    {
        public long CompanyID { get; set; }
        public long TemplateID { get; set; }
    }


    public class TemplateModel
    {
        public long CompanyID { get; set; }
        public string? EncodedId { get; set; }  
        public string Description { get; set; } = string.Empty;
        public string HeaderImage { get; set; } = string.Empty;
        public string FooterImage { get; set; } = string.Empty;
        public string LetterHeadName { get; set; } = string.Empty;
        public long TemplateID { get; set; }
        public string TemplateName { get; set; } = string.Empty;
    }
}
