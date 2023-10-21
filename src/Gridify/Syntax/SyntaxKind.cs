namespace Gridify.Syntax;

public enum SyntaxKind
{
   // specials
   End,
   BadToken,
   WhiteSpace,
   Operator,

   FieldToken,
   OpenParenthesisToken,
   CloseParenthesis,
   And,
   Or,
   Equal,
   Like,
   NotEqual,
   NotLike,
   LessThan,
   GreaterThan,
   LessOrEqualThan,
   GreaterOrEqualThan,
   StartsWith,
   EndsWith,
   CustomOperator,

   // expressions
   FieldExpression,
   BinaryExpression,
   ValueExpression,
   ValueToken,
   ParenthesizedExpression,
   NotStartsWith,
   NotEndsWith,
   CaseInsensitive,
   FieldIndexToken,
}
