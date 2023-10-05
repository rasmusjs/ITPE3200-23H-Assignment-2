using Microsoft.AspNetCore.Mvc;

namespace forum.Controllers;

// Home controller sending the user to index
public class HomeController : Controller
{
    // GET: /<controller>/
    public IActionResult Index()
    {
        return View();
    }
}