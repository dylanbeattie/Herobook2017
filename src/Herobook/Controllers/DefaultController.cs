using System.Web.Mvc;

namespace Herobook.Controllers {
    public class DefaultController : Controller {
        // GET: Default
        public ActionResult Index() {
            return View();
        }
    }
}
