using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using HRMS.Models.Common;
using HRMS.Models.Employee;


namespace HRMS.Web.BusinessLayer.Hubs
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IBusinessLayer _businessLayer;
        IHttpContextAccessor _context;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            IBusinessLayer businessLayer, IHttpContextAccessor context)
        {
            _hubContext = hubContext;
            _context = context;
            _businessLayer = businessLayer;
        }

        public async Task SendHierarchyNotification(
            long employeeId,
            NotificationType type)
        {

            var body = new EmployeeManagerInputParams
            {
                EmployeeID = employeeId,
                Type = 1
                //Type = (int)type
            };

            var apiUrl = _businessLayer.GetFormattedAPIUrl(
                "Employee",
                "GetEmployeeHierarchyManagers");


            var managersJson = await _businessLayer.SendPostAPIRequest(
                body,
                apiUrl,
                _context.HttpContext?.Session.GetString(Constants.SessionBearerToken),
                true
            );
            var managers = JsonConvert.DeserializeObject<List<long>>(managersJson.ToString());
            foreach (var managerId in managers)
            {
                if ((int)type == 5)
                {

                    await _hubContext.Clients.User(managerId.ToString())
                        .SendAsync("ReceiveUpdatedCounts");
                }
                else
                {

                    await _hubContext.Clients.User(managerId.ToString())
                            .SendAsync("ReceiveNotification");
                }
            }
        }
    }

}