using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gridify.Builder;

/// <summary>
/// Resolved tree of select paths. The root holds the source type T;
/// each node represents one property on its parent type.
/// </summary>
internal sealed class SelectShape
{
   /// <summary>Type whose properties are described by <see cref="Children"/>.</summary>
   public System.Type SourceType { get; set; } = null!;

   public List<SelectNode> Children { get; } = new();
}

internal sealed class SelectNode
{
   /// <summary>Name as written by the user (used as the emitted property name).</summary>
   public string Name { get; set; } = null!;

   /// <summary>
   /// Expression that, given a parameter of the parent's source type, produces this node's value.
   /// For leaf nodes this is the resolved property/path expression.
   /// For nested-object nodes this is the parent property access (e.g. p => p.Address);
   /// the <see cref="ChildShape"/> describes how to project from there.
   /// For collection nodes this is the collection access (e.g. p => p.Orders);
   /// the <see cref="ChildShape"/> describes how to project each element.
   /// </summary>
   public LambdaExpression Accessor { get; set; } = null!;

   /// <summary>The CLR type of the value this node ultimately produces (after projection).</summary>
   public System.Type ResultType { get; set; } = null!;

   /// <summary>Null for leaves; populated for nested-object and collection nodes.</summary>
   public SelectShape? ChildShape { get; set; }

   /// <summary>True when the parent's property is a collection and we project each element.</summary>
   public bool IsCollection { get; set; }

   /// <summary>For collection nodes, the element type (T in IEnumerable&lt;T&gt;).</summary>
   public System.Type? ElementType { get; set; }
}
