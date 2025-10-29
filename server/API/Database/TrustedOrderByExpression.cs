namespace API.Database;

// I built this class because I want to allow the use of custom "order by" expressions,
// particularly in pagination, but I also want to prevent the potential for SQL injection
// This requires anyone working on this project to specifically "Trust" an order by statement
// Example: new TrustedOrderByExpression("Relevance ASC, p.isDemoProduct DESC, p.productId")
// Also, sealing the class prevents inheritance
public sealed class TrustedOrderByExpression
{
    private readonly string _expression;

    public TrustedOrderByExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("ORDER BY expression cannot be empty", nameof(expression));
        
        _expression = expression;
    }
    
    public string ToSql() => _expression;
    
    public override string ToString() => _expression;
}