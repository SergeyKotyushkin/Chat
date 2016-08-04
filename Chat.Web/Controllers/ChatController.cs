using System.Web.Mvc;

namespace Chat.Web.Controllers
{
    public class ChatController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

    }
}
