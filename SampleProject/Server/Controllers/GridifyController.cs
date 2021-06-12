using AutoMapper;
using AutoMapper.QueryableExtensions;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleProject.Entites;
using SampleProject.DataTransferObjects;
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
      [HttpGet]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> Get([FromQuery] GridifyQuery gridifyQuery)
      {
         // General usage of Gridify with AutoMapper
         return Ok(await _context.People.AsNoTracking()
            .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
            .GridifyAsync(gridifyQuery));
      }

      /// <summary>
      /// Returns a List of PersonDto with only Filtering applied
      /// </summary>
      [HttpGet("Filtering")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetSimpleFilteredList([FromQuery] GridifyQuery gridifyQuery)
      {
         var result = _context.People.ApplyFiltering(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns a List of PersonDto with only Ordering applied
      /// </summary>
      [HttpGet("Ordering")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetOrderedList([FromQuery] GridifyQuery gridifyQuery)
      {
         var result = _context.People.ApplyOrdering(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with only Paging applied
      /// </summary>
      [HttpGet("Paging")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetPaginatedList([FromQuery] GridifyQuery gridifyQuery)
      {
         var result = _context.People.ApplyPaging(gridifyQuery);
         return Ok(new Paging<PersonDto>()
         {
            Items = await result.AsQueryable().ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync(),
            TotalItems = await _context.People.CountAsync()
         });
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with Ordering and Paging applied 
      /// </summary>
      [HttpGet("OrderingAndPaging")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetOrderedAndPaginatedList([FromQuery] GridifyQuery gridifyQuery)
      {
         var result = _context.People.ApplyOrderingAndPaging(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with Everything applied
      /// </summary>
      [HttpGet("Everything")]
      [Produces(typeof(List<PersonDto>))]
      public async Task<IActionResult> GetEverythingList([FromQuery] GridifyQuery gridifyQuery)
      {
         var result = _context.People.ApplyEverything(gridifyQuery);
         return Ok(await result.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToListAsync());
      }

      /// <summary>
      /// Returns Gridify Paging List of PersonDto with CustomMapping
      /// </summary>
      [HttpGet("CustomMapping")]
      [Produces(typeof(Paging<PersonDto>))]
      public async Task<IActionResult> GetCustomMappedList([FromQuery] GridifyQuery gridifyQuery)
      {
         // Case sensetive is false by default. But we can enable it here.
         var customMappings = new GridifyMapper<Person>(false)
           // Because properties with same name exists in both DTO and Entity classes, we can generate them.
           .GenerateMappings()
           // Add custom mappings
           .AddMap("livingAddress", q => q.Contact.Address)
           .AddMap("phone", q => q.Contact.PhoneNumber.ToString());

         // GridifyQueryable return a QueryablePaging<T>
         var result = await _context.People.GridifyQueryableAsync(gridifyQuery, customMappings);

         // We then apply AutoMapper to the query result and return a Paging.
         return Ok(new Paging<PersonDto>()
         {
            Items = result.Query.ProjectTo<PersonDto>(_mapper.ConfigurationProvider).ToList(),
            TotalItems = result.TotalItems
         });
      }
   }
}
