using System.Collections.Generic;

namespace Gridify.Syntax;

internal sealed class FieldExpressionSyntax : ExpressionSyntax
{
   internal FieldExpressionSyntax(SyntaxToken fieldToken, FieldExpressionSyntaxType syntaxType, string? subKey = default)
   {
      FieldToken = fieldToken;
      SyntaxType = syntaxType;
      SubKey = subKey;
   }

   public override SyntaxKind Kind => SyntaxKind.FieldExpression;

   public override IEnumerable<SyntaxNode> GetChildren()
   {
      yield return FieldToken;
   }

   public FieldExpressionSyntaxType SyntaxType { get; }
   public string? SubKey { get; }
   public SyntaxToken FieldToken { get; }
}

public enum FieldExpressionSyntaxType
{
   Field,
   Collection,
   Dictionary
}
