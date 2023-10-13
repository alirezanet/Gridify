using System;

namespace Gridify.Elasticsearch.Tests;

public class TestClass
{
   public int Id { get; set; }
   public string? Name { get; set; }
   public TestClass? ChildClass { get; set; }
   public DateTime? MyDateTime { get; set; }
   public DateOnly MyDateOnly { get; set; }
   public Guid MyGuid { get; set; }
   public string? Tag { get; set; }
   public bool IsActive { get; set; }
   public byte MyByte { get; set; }
   public sbyte MySByte { get; set; }
   public short MyShort { get; set; }
   public ushort MyUShort { get; set; }
   public int MyInt { get; set; }
   public uint MyUInt { get; set; }
   public long MyLong { get; set; }
   public ulong MyULong { get; set; }
   public float MyFloat { get; set; }
   public double MyDouble { get; set; }
   public decimal MyDecimal { get; set; }
}
