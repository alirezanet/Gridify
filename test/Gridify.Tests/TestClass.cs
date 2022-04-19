using System;

namespace Gridify.Tests;

public class TestClass : ICloneable
{
   public TestClass()
   {
   }

   public TestClass(int id, string name, TestClass? classProp, Guid myGuid = default, DateTime? date = default, string? tag = "",
      bool isActive = false)
   {
      Id = id;
      Name = name;
      ChildClass = classProp;
      MyGuid = myGuid;
      MyDateTime = date;
      Tag = tag;
      IsActive = isActive;
   }

   public int Id { get; set; }
   public string? Name { get; set; } = string.Empty;
   public TestClass? ChildClass { get; set; }
   public DateTime? MyDateTime { get; set; }
   public Guid MyGuid { get; set; }
   public string? Tag { get; set; }

   public bool IsActive { get; set; }

   public object Clone()
   {
      return new TestClass
      {
         Id = Id,
         Name = Name,
         ChildClass = (TestClass)ChildClass?.Clone()!,
         MyGuid = MyGuid,
         Tag = Tag,
         MyDateTime = MyDateTime,
         IsActive = IsActive
      };
   }
}
