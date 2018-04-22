using System.Web;
using System.Web.Optimization;

namespace TMShops
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/jquery.unobtrusive-ajax.min.js",
                      "~/Scripts/jquery.validate.unobtrusive.min.js",
                      "~/Scripts/validator.unobtrusive.parseDynamicContent.js",
                      "~/Scripts/moment-with-locales.min.js",
                      "~/Scripts/moment.min.js",
                      "~/Scripts/tinymce/tinymce.min.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/TMLibrary.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/site.css"));

            //CMS
            //"~/Areas/cms/Content/bootstrap.css",
            bundles.Add(new StyleBundle("~/cms/css").Include(
                      "~/Content/font-awesome.min.css",
                      "~/Areas/cms/Content/simple-line-icons.css",
                      "~/Areas/cms/Content/style.css",
                      "~/Areas/cms/Content/Site.css"));
            bundles.Add(new ScriptBundle("~/cms/js").Include(
                      "~/Areas/cms/Scripts/jquery.min.js",
                      "~/Areas/cms/Scripts/tether.min.js",
                      "~/Areas/cms/Scripts/bootstrap.min.js",
                      "~/Areas/cms/Scripts/pace.min.js",
                      "~/Areas/cms/Scripts/Chart.min.js",
                      "~/Areas/cms/Scripts/app.js",
                      "~/Areas/cms/Scripts/main.js"));
        }
    }
}
