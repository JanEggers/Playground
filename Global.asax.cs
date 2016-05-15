using System.Web;
using System.Web.Http;

namespace Playground
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(IoCConfig.Register);
            GlobalConfiguration.Configure(ODataConfig.Register);
        }
    }
}
