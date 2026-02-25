using System.Web.Mvc;
using System.Web.Routing;

namespace DayKast_VFeb2026.Filters
{
    /// <summary>
    /// Admin paneli sayfalarına erişimi kontrol eden filtre.
    /// Session'da AdminID veya Admin rolünde UserRole yoksa Admin/Login'e yönlendirir.
    /// </summary>
    public class AdminAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            bool isAuthenticated = false;

            // AdminID ile giriş yapmışsa
            if (session["AdminID"] != null)
            {
                isAuthenticated = true;
            }
            // User tarafından admin rolüyle giriş yapmışsa
            else if (session["UserRole"] != null && session["UserRole"].ToString() == "1")
            {
                session["AdminID"] = session["UserID"];
                session["AdminName"] = session["UserName"];
                isAuthenticated = true;
            }

            if (!isAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Admin" },
                        { "action", "Login" }
                    });
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
