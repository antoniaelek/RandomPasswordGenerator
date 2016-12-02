using Microsoft.AspNetCore.Mvc;

namespace RandomPasswordGenerator 
{
    
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return new RedirectResult("home");
        }
    }
}