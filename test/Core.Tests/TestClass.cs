using System;

namespace Gridify.Tests
{

   public class TestClass : ICloneable
   {
      public TestClass () { }
      public TestClass (int id, string name, TestClass classProp,Guid guid = default)
      {
         this.Id = id;
         this.Name = name;
         this.ChildClass = classProp;
         this.guid = guid;
      }

      public int Id { get; set; }
      public string Name { get; set; }
      public TestClass ChildClass { get; set; }
      public Guid guid { get; set;}
      

      public object Clone ()
      {
         return new TestClass ()
         {
            Id = this.Id,
            Name = this.Name,
            ChildClass = this.ChildClass != null ? (TestClass) this.ChildClass.Clone () : null,
            guid =this.guid
         };
      }
   }
}