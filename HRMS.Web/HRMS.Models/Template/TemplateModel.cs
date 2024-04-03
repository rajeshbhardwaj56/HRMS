namespace HRMS.Models.Template
{
    public class TemplateInputParans
    {
        public long CompanyID { get; set; }
        public long TemplateID { get; set; }
    }


    public class TemplateModel
    {
        public long TemplateID { get; set; }
        public long CompanyID { get; set; }
        public string LetterHeadName { get; set; } = string.Empty;
        public string HeaderImage { get; set; } = string.Empty;
        public string FooterImage { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
