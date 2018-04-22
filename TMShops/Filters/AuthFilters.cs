﻿using System.Web.Mvc;
using System.Web.Routing;

namespace TMShops.Filters
{
    public class AuthFilters : ActionFilterAttribute
    {
        public string Role { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var result = new RedirectToRouteResult(
                new RouteValueDictionary {
                    { "controller", "Auth" },
                    { "action", "Index" },
                    { "continue", TM.Url.ContinueUrl() } });
            if (Common.Auth.isAuth())
            {
                //var roles = Role.Split(',');
                if (Role != null && !Role.Split(',').Contains(Common.Auth.getUser().roles))
                {
                    filterContext.Controller.TempData.Add("MsgDanger", "Bạn không có quyền truy cập. Vui lòng liên hệ với admin!");
                    filterContext.Result = result;
                }
            }
            else
            {
                filterContext.Result = result;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}