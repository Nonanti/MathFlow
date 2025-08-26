# MathFlow

C# math expression library with symbolic computation support.

## Install

```
dotnet add package MathFlow
```

## Basic usage

```csharp
using MathFlow.Core;

var engine = new MathEngine();

// basic stuff
var result = engine.Calculate("2 + 3 * 4"); // returns 14
Console.WriteLine(engine.Calculate("sin(pi/2)")); // 1

// with variables
var vars = new Dictionary<string, double> { ["x"] = 3, ["y"] = 4 };
var answer = engine.Calculate("x^2 + y^2", vars); // 25
```

## Features

### Expressions
- Parse mathematical expressions from strings
- Evaluate with variables
- Works with standard math notation

### Operations

**Basic**: +, -, *, /, ^, %

**Trig**: sin, cos, tan, asin, acos, atan (also sinh, cosh, tanh)

**Other**: exp, ln, log10, sqrt, abs, floor, ceil, round, sign, min, max, factorial

**Constants**: pi, e, tau (2*pi), phi (golden ratio)

## Advanced stuff

### Derivatives

```csharp
var derivative = engine.Differentiate("x^3 + 2*x^2 - 5*x + 3", "x");
// gives you: 3*x^2 + 4*x - 5
```

### Simplification

```csharp
var simplified = engine.Simplify("x + 2*x + 3*x");
Console.WriteLine(simplified); // 6*x
```

### Equation solving

```csharp
// find root near initial guess
double root = engine.FindRoot("x^2 - 4", 3); // returns 2

// find all roots in range
double[] roots = engine.FindRoots("x^3 - 6*x^2 + 11*x - 6", 0, 4);
// gives [1, 2, 3]
```

### Integration (numerical)

```csharp
double integral = engine.Integrate("x^2", "x", 0, 1); 
// returns ~0.333333
```

## Working with expressions

You can also build expressions programmatically:

```csharp
var expr1 = engine.Parse("x + 2");
var expr2 = engine.Parse("y - 1");
var combined = expr1 * expr2; // creates (x + 2) * (y - 1)
```

Substitute variables:
```csharp
var substituted = engine.Substitute("x^2 + y", "x", "sin(t)");
// gives: sin(t)^2 + y
```

## Output formats

### LaTeX
```csharp
string latex = engine.ToLatex("sqrt(x^2 + y^2)");
// \sqrt{x^{2} + y^{2}}
```

### MathML
```csharp
string mathml = engine.ToMathML("x/2");
// outputs MathML format
```

## Custom functions

You can register your own:

```csharp
engine.RegisterFunction("double", args => args[0] * 2);
var result = engine.Calculate("double(5)"); // 10
```

## More examples

Check the Examples folder for more usage examples including:
- Complex numbers
- Vector operations  
- Statistical functions
- Linear regression

## API

Main methods:

- `Calculate(expression, variables?)` - evaluate expression
- `Parse(expression)` - parse to AST
- `Simplify(expression)` - simplify expression
- `Differentiate(expression, variable)` - take derivative
- `Integrate(expression, variable, a, b)` - numerical integration
- `FindRoot(expression, guess)` - find root using Newton's method
- `FindRoots(expression, start, end)` - find all roots in range
- `Expand(expression)` - expand expression
- `Factor(expression)` - factor expression (limited support)
- `Substitute(expression, var, replacement)` - replace variable
- `ToLatex(expression)` - convert to LaTeX
- `ToMathML(expression)` - convert to MathML

## Requirements

.NET 8.0 or later

## Tests

Run tests with:
```
dotnet test
```

## Known issues

- Factor() method is not fully implemented
- Some edge cases in complex expressions might not simplify optimally
- Performance could be better for very large expressions

## Contributing

PRs welcome. Please add tests for new features.

## License

MIT

## Credits

Inspired by various math expression parsers and CAS systems.