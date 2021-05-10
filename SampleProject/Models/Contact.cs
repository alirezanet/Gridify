using System.ComponentModel.DataAnnotations.Schema;

namespace SampleProject.Models
{
   public class Contact
   {
      public int Id { get; set; }
      public string Address { get; set; }
      public long PhoneNumber { get; set; }
      public virtual Person Person { get; set; }

      [ForeignKey("Person")]
      public int PersonId { get; set; }
   }
}
