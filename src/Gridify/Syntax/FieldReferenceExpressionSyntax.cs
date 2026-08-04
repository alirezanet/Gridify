using System.Collections.Generic;

namespace Gridify.Syntax;

/// <summary>
/// Represents a field reference on the right-hand side of a comparison operator.
/// Syntax: <c>(fieldName)</c> — the parentheses differentiate it from a literal value.
/// </summary>
public sealed class FieldReferenceExpressionSyntax(FieldExpressionSyntax fieldExpression) : ExpressionSyntax
{
   public override SyntaxKind Kind => SyntaxKind.FieldReferenceExpression;

   public FieldExpressionSyntax FieldExpression { get; } = fieldExpression;

   public override IEnumerable<ISyntaxNode> GetChildren()
   {
      yield return FieldExpression;
   }
}
