using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TimeSheet.Startup))]
namespace TimeSheet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
