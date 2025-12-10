namespace API.Database;

// I built this class because I want to allow the use of custom SQL expressions
// (e.g., ORDER BY clauses) that can't be parameterized, while preventing SQL injection.
// This requires anyone working on this project to specifically "Trust" a SQL expression.
// Example: new TrustedSqlExpression("Relevance ASC, p.isDemoProduct DESC, p.productId")
// Also, sealing the class prevents inheritance
public sealed class TrustedSqlExpression
{
    private readonly string _expression;

    public TrustedSqlExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("SQL expression cannot be empty", nameof(expression));

        _expression = expression;
    }

    public string ToSql() => _expression;

    public override string ToString() => _expression;
}