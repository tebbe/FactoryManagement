using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FactoryManagement
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            JobScheduler.Start();

            
        }
        public void Application_BeginRequest()
        {
            int localTime = 0;
            var cookieLT = Request.Cookies["CookieCostLT"];
            if (cookieLT != null)
            {
                localTime = Convert.ToInt32(cookieLT.Value);
            }
            ConfigurationManager.AppSettings["localTime"] = localTime.ToString();
            //if (User.Identity.IsAuthenticated)
            //{
            //    FactoryManagement.Models.FactoryManagementEntities db = new Models.FactoryManagementEntities();
            //    var userid = Convert.ToInt64(User.Identity.GetUserId());
            //    var user = db.UserLogins.Where(x => x.UserId == userid).FirstOrDefault();
            //    if (user == null && user.HasResetPassword && !user.IsFirstLoginAfterReset)
            //    {
            //        HttpContext.Current.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //        Response.Redirect("/Account/Login", true);
            //    }
            //}
        }
    }
}
