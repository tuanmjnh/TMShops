using System.Web.Mvc;

namespace TMShops.Areas.cms
{
    public class cmsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "cms";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "cms_default",
                "cms/{controller}/{action}/{id}",
                new { Controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "TMShops.Areas.cms.Controllers" }
            );
        }
    }
}