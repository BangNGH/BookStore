using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Buoi6_TrenLop.Startup))]
namespace Buoi6_TrenLop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
