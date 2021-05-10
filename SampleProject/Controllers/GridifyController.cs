using AutoMapper;
using AutoMapper.QueryableExtensions;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleProject.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleProject.Controllers
{
   [ApiController]
   [Route("/api/[controller]")]
   public class GridifyController : ControllerBase
   {
      private readonly AppDbContext _context;
      private readonly IMapper _mapper;

      public GridifyController(AppDbContext context, IMapper mapper)
      {
         this._context = context;
         this._mapper = mapper;
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      [HttpGet]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> Get([FromQuery] GridifyQuery filter)
      {
         // Simple usage of gridify with AutoMapper
         return Ok(await _context.People.AsNoTracking()
            .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
            .GridifyAsync(filter));
      }

      /// <summary>
      /// Returns a List of PersonDto with only Filtering applied (Simple filter)
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      [HttpGet("SimpleFilter")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetSimpleFilteredList([FromQuery] GridifyQuery filter)
      {
         if (filter == null)
         {
            // Simple filter
            // Usually we don't need create this object manually, We get it from Query
            filter = new GridifyQuery()
            {
               // FirstName equals to Alireza
               Filter = "FirstName==Alireza"
            };
         }
         var result = _context.People.ApplyFiltering(filter);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns a List of PersonDto with only Filtering applied (Complex filter)
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      [HttpGet("ComplexFilter")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetComplexFilteredList([FromQuery] GridifyQuery filter)
      {
         if (filter == null)
         {
            // Complex filter
            filter = new GridifyQuery()
            {
               // FirstName contains Ali AND FirstName doesn't contain reza
               // OR
               // FirstName contains Ali AND Age is greater than 30
               Filter = "(FirstName=*Ali,FirstName!*reza)|(FirstName=*Ali,Age>>30)"
            };
         }
         var result = _context.People.ApplyFiltering(filter);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns a List of PersonDto with only Ordering applied
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      [HttpGet("Ordering")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetOrderedList([FromQuery] GridifyQuery filter)
      {
         if (filter == null)
         {
            filter = new GridifyQuery()
            {
               // Decending order
               IsSortAsc = false,
               SortBy = "Age"
            };
         }
         var result = _context.People.ApplyOrdering(filter);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with only Paging applied
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      [HttpGet("Paging")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetPaginatedList([FromQuery] GridifyQuery filter)
      {
         if (filter == null)
         {
            filter = new GridifyQuery()
            {
               // Page is defined by client, or send in URL query.
               Page = 1,
               PageSize = 20
            };
         }
         var result = _context.People.ApplyPaging(filter);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with Ordering and Paging applied 
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      [HttpGet("OrderingAndPaging")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetOrderedAndPaginatedList([FromQuery] GridifyQuery filter)
      {
         if (filter == null)
         {
            filter = new GridifyQuery()
            {
               // Ascending order
               IsSortAsc = true,
               SortBy = "Age",
               PageSize = 2
               // Page is defined by client, or send in URL query.
            };
         }
         var result = _context.People.ApplyOrderingAndPaging(filter);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with Everything applied
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      [HttpGet("Everything")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetEverythingList([FromQuery] GridifyQuery filter)
      {
         if (filter == null)
         {
            filter = new GridifyQuery()
            {
               // Exclude a specific LastName from result
               Filter = "LastName!*D",
               // Ascending order
               IsSortAsc = true,
               SortBy = "Address",
               PageSize = 10,
               // Page is defined by client, or send in URL query.
               Page = 1
            };
         }
         var result = _context.People.ApplyEverything(filter);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with CustomMapping
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      [HttpGet("CustomMapping")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetCustomMappedList([FromQuery] GridifyQuery filter)
      {
         // Case sensetive is false by default. But we can enable it here.
         var customMappings = new GridifyMapper<Person>(false)
           // Because properties with same name exists in both DTO and Entity classes, we can Generate them.
           .GenerateMappings()
           // Add custom mappings
           .AddMap("livingAddress", q => q.Contact.Address)
           .AddMap("phone", q => q.Contact.PhoneNumber);

         if (filter == null)
         {
            filter = new GridifyQuery()
            {
               // Notice we dont use Address as we used on filters before,
               // instead, we use new map that we defined in GridifyMapper.
               Filter = "FirstName==Alireza,livingAddress=*Calvin",
               IsSortAsc = true,
               SortBy = "phone"
            };
         }

         // GridifyQueryable return a QueryablePaging<T>
         var result = await _context.People.GridifyQueryableAsync(filter, customMappings);

         // We then apply AutoMapper to the query result and return a Paging.
         return Ok(new Paging<PersonDto>()
         {
            Items = result.Query.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToList(),
            TotalItems = result.TotalItems
         });
      }
   }
}
