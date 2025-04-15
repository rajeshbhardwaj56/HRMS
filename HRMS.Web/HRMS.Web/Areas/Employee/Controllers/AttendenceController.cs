using DocumentFormat.OpenXml.Vml.Office;
using eSSLWebserviceReference;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Areas.Employee.Controllers
{
    public class AttendenceController : Controller
    {
        public IActionResult Index()
        {

            WebServiceSoapClient.EndpointConfiguration endpoint = new WebServiceSoapClient.EndpointConfiguration();
            WebServiceSoapClient myService = new WebServiceSoapClient(endpoint, "");
            //AuthorizationSoapHeader MyAuthHeader = new AuthorizationSoapHeader();

            //MyAuthHeader.AppName = FDSServiceAppName;
            //MyAuthHeader.AppID = Guid.Parse(MyAppID);

            var entries = myService.GetDeviceListAsync("", "", "").Result;
            //var entries1 = myService.("", "", "").Result;
            //WebServiceSoapClient webServiceSoapClient = new WebServiceSoapClient();

            //using (var client = new WebServiceSoapClient("PhoneNotifySoap"))
            //{
            //    var result = client.GetVersion();
            //}


            return View();
        }
    }
}
