using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Course3_2.Startup))]
namespace Course3_2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
