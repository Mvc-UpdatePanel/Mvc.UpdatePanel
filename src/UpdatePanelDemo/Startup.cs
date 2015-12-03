using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(UpdatePanelDemo.Startup))]
namespace UpdatePanelDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
