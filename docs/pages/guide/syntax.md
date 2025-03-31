# Parsing and Syntax Tree

Gridify internally uses a syntax tree to generate the expression tree that is used to filter or order objects based on their members. If you need more control or insights over the `SyntaxTree`, you can use the following extension methods inside the `Gridify.Syntax` namespace.

## SyntaxTree

The `SyntaxTree` always starts at the root of the tree and can be accessed by the `Root` property.

``` csharp
var root = SyntaxTree.Parse(filterings).Root; // ISyntaxNode
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

To get all distinct field expression syntax nodes (self and descendants) of the syntax tree, you can use the `DistinctFieldExpressions` method.

``` csharp
var filterings = "name = Jack, arrayProp[8] > 10, dictProp[name] = John";

var fieldExpressions = SyntaxTree.Parse(filterings).Root.DistinctFieldExpressions();
```

## Direct Parsing

If you would like to parse a filtering or ordering string directly, you can use the following methods: `ParseFilterings` and `ParseOrderings`.

### ParseFilterings

``` csharp
var filterings = SyntaxTree.ParseFilterings("name = Jack, arrayProp[8] > 10, dictProp[name] = John");

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
var orderings = SyntaxTree.ParseOrderings("Id desc, FirstName asc, LastName");

foreach (var ordering in orderings)
{
    Console.WriteLine(ordering.MemberName);
}

// Id
// FirstName
// LastName
```
