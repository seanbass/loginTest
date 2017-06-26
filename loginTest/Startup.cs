using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(loginTest.Startup))]
namespace loginTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
