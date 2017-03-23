using System.Web;
using System.Web.Optimization;

namespace MessageSender
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

            // Use KnockoutJS
            bundles.Add(new ScriptBundle("~/bundles/knockoutjs").Include(
                       "~/Scripts/knockout-{version}.js"));

            // Use SignalR
            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                       "~/Scripts/jquery.signalR-{version}.js"));

            // Custom scripts
            bundles.Add(new ScriptBundle("~/bundles/jobs").Include(
                       "~/Scripts/jobs.js"));
            bundles.Add(new ScriptBundle("~/bundles/toggleCellContent")
                    .Include("~/Scripts/toggleCellContent.js"));

            // custom stylesheet
            bundles.Add(new StyleBundle("~/Content/customcss").Include("~/Content/custom.css"));

            // Unobtrusive AJAX
            bundles.Add(new ScriptBundle("~/bundles/uajax").
                Include("~/Scripts/jquery.unobtrusive-ajax*"));

        }
    }
}
