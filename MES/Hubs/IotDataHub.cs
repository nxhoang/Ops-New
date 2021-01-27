using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MES.Hubs
{
    public class IotDataHub: Hub
    {
        public async Task UpdateIoTDataChangeAsync()
        {
            await Clients.All.SendAsync("ReceiveMessage");
        }
    }
}