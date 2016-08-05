using Chat.Logic.Elastic;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Chat.Web.Startup))]

namespace Chat.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            GlobalHost.DependencyResolver.Register(typeof(ChatHub),
                () => new ChatHub(
                    new UserRepository())
                );

            app.MapSignalR();
        }
    }
}
