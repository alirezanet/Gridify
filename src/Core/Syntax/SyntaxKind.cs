namespace Gridify.Syntax
{
   public enum SyntaxKind
   {
      // specials
      End,
      BadToken,
      WhiteSpace,

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

      // expressions
      FieldExpression,
      BinaryExpression,
      ValueExpression,
      ValueToken,
      ParenthesizedExpression,
      NotStartsWith,
      NotEndsWith
   }
}