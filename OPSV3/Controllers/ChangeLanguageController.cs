using System.Web.Mvc;

namespace OPS.Controllers
{
    public class ChangeLanguageController : Controller
    {
        // GET: ChangeLanguage
        public ActionResult ChangeLanguage(string culture, string returnUrl)
        {            
            if (!string.IsNullOrEmpty(culture))
            {
                var httpCookie = Request.Cookies["language"];
                if (httpCookie != null)
                {
                    var cookie = Response.Cookies["language"];
                    if (cookie != null) cookie.Value = culture;
                    Response.Cookies.Add(cookie);
                }
            }
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Ops", "Ops");
        }
    }
}