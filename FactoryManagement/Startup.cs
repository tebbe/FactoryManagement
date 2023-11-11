using Microsoft.Owin;
using Owin;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;

[assembly: OwinStartupAttribute(typeof(FactoryManagement.Startup))]
namespace FactoryManagement
{

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR(); 
        }
    }
}
