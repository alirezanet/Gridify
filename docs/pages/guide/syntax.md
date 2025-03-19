# Parsing and Syntax Tree

Gridify internally uses a syntax tree to generate the expression tree that is used to filter or order object on their members. If you need more control or insights over the `SyntaxTree`, you can use the following extension methods inside the `Gridify.Syntax` namespace.

## SyntaxTree

The `SyntaxTree` always starts root of the tree and can be accessed by the `Root` property.

``` csharp
var root = SyntaxTree.Parse(filterings).Root // ISyntaxNode
```

### Descendants

To get all syntax nodes (self and descendants) of the syntax tree, you can use the `Descendants` method.

``` csharp
  var filterings = "name = Jack, arrayProp[8] > 10, dictProp[name] = John";

  var syntaxNodes = SyntaxTree.Parse(filterings).Root.Descendants();

  var expressions = syntaxNodes.OfType<ExpressionSyntax>();
  var valueExpressions = syntaxNodes.OfType<ValueExpressionSyntax>();
  var fieldExpressions = syntaxNodes.OfType<FieldExpressionSyntax>();
  var binaryExpressions = syntaxNodes.OfType<BinaryExpressionSyntax>();
  var parenthesizedExpressions = syntaxNodes.OfType<ParenthesizedExpressionSyntax>();
```

### DistinctFieldExpressions

To get all distinct field expressions syntax nodes (self and descendants) of the syntax tree, you can use the `DistinctFieldExpressions` method.

``` csharp
  var filterings = "name = Jack, arrayProp[8] > 10, dictProp[name] = John";

  var fieldExpressions = SyntaxTree.Parse(filterings).Root.DistinctFieldExpressions()
```

## Direct Parsing

If you like to parse directly a filtering or an ordering string, you can use the following extension methods `ParseFilterings` and `ParseOrderings`.

::: note
Make sure that you have included the `Gridify.Syntax` namespace.
:::

### ParseFilterings

``` csharp
var filterings = "name = Jack, arrayProp[8] > 10, dictProp[name] = John".ParseFilterings();

foreach (var filtering in filterings)
{
    Console.WriteLine(filtering.MemberName);
}

// name
// arrayProp
// dictProp

```

### ParseOrderings

``` csharp
var orderings = "Id desc, FirstName asc, LastName".ParseOrderings();
```
