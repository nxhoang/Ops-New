using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MES.Startup))]

namespace MES
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        { 
            // Any connection or hub wire up and configuration should go here
            //this line is move from ./Hub/Startup.cs. We should move this file to this root folder
            app.MapSignalR(); 
        }
    }
}
