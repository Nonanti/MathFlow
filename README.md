# MathFlow

<div align="center">

[![NuGet](https://img.shields.io/nuget/v/MathFlow.svg?style=flat-square)](https://www.nuget.org/packages/MathFlow/)
[![License](https://img.shields.io/github/license/Nonanti/MathFlow?style=flat-square)](LICENSE)
[![Build Status](https://img.shields.io/github/actions/workflow/status/Nonanti/MathFlow/ci-cd.yml?branch=master&style=flat-square)](https://github.com/Nonanti/MathFlow/actions)
[![.NET](https://img.shields.io/badge/.NET-9.0%2B-512BD4?style=flat-square)](https://dotnet.microsoft.com/download)

**C# math expression library with symbolic computation support**

Parse • Evaluate • Differentiate • Simplify • Solve

</div>

## Installation

### Package Manager
```bash
dotnet add package MathFlow
```

### Package Reference
```xml
<PackageReference Include="MathFlow" Version="2.1.0" />
```

### Package Manager Console
```powershell
Install-Package MathFlow
```

## Quick Start

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

## What's New in v2.1.0

### New Features
- **Combinatorics Functions**: Added binomial coefficients and permutation calculations
- **Reorganized Math Functions**: GCD, LCM, and factorial moved to dedicated CombinatoricsFunctions class

### Breaking Changes
- `GCD`, `LCM`, and `Factorial` functions have been moved from their previous location to `CombinatoricsFunctions` class
- Update your imports if you were using these functions directly

## What's New in v2.0.0

### Major Features Added
- **Complex Number Support**: Full integration with expression parser
- **Enhanced Polynomial Factoring**: Quadratic, cubic, and special forms
- **New ODE Solvers**: RungeKutta2 and Adams-Bashforth methods
- **Rational Function Integration**: Partial fractions and special forms
- **Symbolic Integration**: Extended support for trigonometric and exponential functions

## Features

### Core Capabilities
- Parse mathematical expressions from strings
- Evaluate with variables and custom functions
- Works with standard math notation
- Symbolic differentiation and integration
- Expression simplification and factoring
- Numerical integration & equation solving
- ASCII plotting for function visualization
- Matrix operations & linear algebra
- Arbitrary precision arithmetic
- Complex number arithmetic

### Supported Operations

| Category | Functions |
|----------|-----------|
| **Basic** | `+` `-` `*` `/` `^` `%` |
| **Trigonometric** | `sin` `cos` `tan` `asin` `acos` `atan` |
| **Hyperbolic** | `sinh` `cosh` `tanh` |
| **Logarithmic** | `ln` `log10` `exp` |
| **Other** | `sqrt` `abs` `floor` `ceil` `round` `sign` `min` `max` `factorial` |
| **Constants** | `pi` `e` `tau` `phi` |

## Advanced Features

### Derivatives

```csharp
// First derivative
var derivative = engine.Differentiate("x^3 + 2*x^2 - 5*x + 3", "x");
// gives you: 3*x^2 + 4*x - 5

// Higher order derivatives
var secondDerivative = engine.Differentiate("x^4", "x", order: 2);
// gives you: 12*x^2

var thirdDerivative = engine.Differentiate("sin(x)", "x", order: 2);
// gives you: -sin(x)
```

### Simplification

```csharp
var simplified = engine.Simplify("x + 2*x + 3*x");
Console.WriteLine(simplified); // 6*x
```

### Equation Solving

```csharp
// find root near initial guess
double root = engine.FindRoot("x^2 - 4", 3); // returns 2

// find all roots in range
double[] roots = engine.FindRoots("x^3 - 6*x^2 + 11*x - 6", 0, 4);
// gives [1, 2, 3]
```

### Integration (Numerical)

```csharp
double integral = engine.Integrate("x^2", "x", 0, 1); 
// returns ~0.333333
```

### Function Plotting (ASCII)

Display mathematical functions directly in terminal:

```csharp
// Simple plot
var plot = engine.Plot("sin(x)", -Math.PI, Math.PI);
Console.WriteLine(plot.ToAsciiChart(60, 20));

// Multiple functions
var multiPlot = engine.CreatePlotter()
    .AddFunction("sin(x)", -Math.PI, Math.PI, label: "sin")
    .AddFunction("cos(x)", -Math.PI, Math.PI, label: "cos");
Console.WriteLine(multiPlot.ToAsciiChart(60, 20));
```

Output:
```
    1.0 ┤      ●●●●●                
        │    ●●    ●●              
        │  ●●        ●●            
        │ ●●          ●●           
    0.0 ┤●●────────────●●──────────
        │              ●●         ●
        │               ●●      ●● 
        │                ●●●●●●●   
   -1.0 ┤                          
        └──────────────────────────
         -3.14      0.00      3.14
```

## Working with Expressions

Build expressions programmatically:

```csharp
var expr1 = engine.Parse("x + 2");
var expr2 = engine.Parse("y - 1");
var combined = expr1 * expr2; // creates (x + 2) * (y - 1)
```

Variable substitution:
```csharp
var substituted = engine.Substitute("x^2 + y", "x", "sin(t)");
// gives: sin(t)^2 + y
```

## Output Formats

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

## Custom Functions

Register your own functions:

```csharp
engine.RegisterFunction("double", args => args[0] * 2);
var result = engine.Calculate("double(5)"); // 10
```

## More Examples

Check the `Examples` folder for advanced usage:
- Complex numbers
- Vector operations  
- Statistical functions
- Linear regression

## API Reference

### Core Methods

| Method | Description |
|--------|-------------|
| `Calculate(expression, variables?)` | Evaluate expression |
| `Parse(expression)` | Parse to AST |
| `Simplify(expression)` | Simplify expression |
| `Differentiate(expression, variable, order?)` | Take derivative (supports higher orders) |
| `Integrate(expression, variable, a, b)` | Numerical integration |
| `FindRoot(expression, guess)` | Find root using Newton's method |
| `FindRoots(expression, start, end)` | Find all roots in range |
| `Expand(expression)` | Expand expression |
| `Factor(expression)` | Factor expression (limited) |
| `Substitute(expression, var, replacement)` | Replace variable |
| `ToLatex(expression)` | Convert to LaTeX |
| `ToMathML(expression)` | Convert to MathML |

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Add tests for new features
4. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by various mathematical expression parsers
- Built with passion for mathematics and clean code

---

<div align="center">
Made by <a href="https://github.com/Nonanti">Nonanti</a>
</div>
