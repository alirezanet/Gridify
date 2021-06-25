using System;

namespace Gridify.Tests
{
   public class TestClass : ICloneable
   {
      public TestClass()
      {
      }

      public TestClass(int id, string name, TestClass classProp, Guid myGuid = default, DateTime? date = default)
      {
         Id = id;
         Name = name;
         ChildClass = classProp;
         MyGuid = myGuid;
         MyDateTime = date;
      }

      public int Id { get; set; }
      public string Name { get; set; }
      public TestClass ChildClass { get; set; }
      public DateTime? MyDateTime { get; set; }
      public Guid MyGuid { get; set; }


      public object Clone()
      {
         return new TestClass
         {
            Id = Id,
            Name = Name,
            ChildClass = ChildClass != null ? (TestClass) ChildClass.Clone() : null,
            MyGuid = MyGuid
         };
      }
   }
}