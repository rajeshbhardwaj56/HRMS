using Microsoft.AspNetCore.Mvc;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using HRMS.Models.Common;
namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;


        public PayrollController(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
        }
    }
}
