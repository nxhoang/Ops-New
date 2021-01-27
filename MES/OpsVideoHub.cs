using Microsoft.AspNet.SignalR;

namespace MES
{
    public class OpsVideoHub : Hub
    {
        public string GetConnectionId() => Context.ConnectionId;

        public static void SendMessage(string msg, int count, string connectionId)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<OpsVideoHub>();
            hubContext.Clients.Client(connectionId).sendMessage(msg, count);
        }
    }
}