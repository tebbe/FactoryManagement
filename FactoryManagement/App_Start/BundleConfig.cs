using System.Web;
using System.Web.Optimization;

namespace FactoryManagement
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));


            //for telerik

            bundles.Add(new ScriptBundle("~/bundles/kendo/2015.3.1111").Include(
                    "~/Scripts/kendo/2015.3.1111/kendo.all.min.js",
                    "~/Scripts/kendo/2015.3.1111/kendo.timezones.min.js", // uncomment if using the Scheduler
                    "~/Scripts/kendo/2015.3.1111/kendo.aspnetmvc.min.js"));

            bundles.Add(new StyleBundle("~/Content/kendo/2015.3.1111/css").Include(
                    "~/Content/kendo/2015.3.1111/kendo.common-bootstrap.min.css",
                    "~/Content/kendo/2015.3.1111/kendo.bootstrap.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                                "~/Content/bootstrap.css",
                                "~/Content/bootstrap-responsive.css"));

            bundles.IgnoreList.Clear();
        }
    }
}
