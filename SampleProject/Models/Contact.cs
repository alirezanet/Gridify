using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SampleProject.Models
{
   public class Contact
   {
      public int Id { get; set; }
      public string Address { get; set; }
      public int PhoneNumber { get; set; }
      public virtual Person Person { get; set; }

      [ForeignKey("Person")]
      public int PersonId { get; set; }
   }
}
