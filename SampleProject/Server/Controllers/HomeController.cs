using Microsoft.AspNetCore.Mvc;

namespace SampleProject.Controllers
{
   public class HomeController : Controller
   {
      public IActionResult Index()
      {
         return View();
      }

      public IActionResult CharacterFilter()
      {
         return View();
      }

      public IActionResult ComplexFilter()
      {
         return View();
      }

      public IActionResult CustomMapping()
      {
         return View();
      }

      public IActionResult FilterByPage()
      {
         return View();
      }

      public IActionResult NumericFilter()
      {
         return View();
      }

      public IActionResult Ordering()
      {
         return View();
      }
   }
}
