using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleProject.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SampleProject.Controllers
{
   [ApiController]
   [Route("/api/[controller]")]
   public class HomeController : ControllerBase
   {
      private readonly ILogger<HomeController> _logger;
      private readonly AppDbContext _context;

      public HomeController(ILogger<HomeController> logger, AppDbContext context)
      {
         _logger = logger;
         this._context = context;
      }

      [HttpGet]
      [Produces(typeof(Paging<Person>))]
      public async Task<IActionResult> Get([FromQuery] GridifyQuery filter)
      {
         return Ok(await _context.People.GridifyAsync(filter));
      }
      //public IActionResult Index()
      //{
      //   return View();
      //}

      //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
      //public IActionResult Error()
      //{
      //   return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
      //}
   }
}
