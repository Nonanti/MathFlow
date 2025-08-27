namespace MathFlow.Core.Interfaces;
public interface IExpression
{
    double Evaluate(Dictionary<string, double>? variables = null);
    IExpression Simplify();
    IExpression Differentiate(string variable);
    string ToString();
    IExpression Clone();
    HashSet<string> GetVariables();
    bool IsConstant();
    IExpression Substitute(string variable, IExpression value);
}