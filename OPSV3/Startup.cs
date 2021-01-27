using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(OPSV3.Startup))]

namespace OPSV3
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
