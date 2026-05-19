using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace HRMS.Web.BusinessLayer.Hubs
{
    public class NotificationHub : Hub
    {
        //public override async Task OnConnectedAsync()
        //{
        //    var employeeId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    Console.WriteLine("Connected Employee: " + employeeId);

        //    await base.OnConnectedAsync();
        //}
    }
}
