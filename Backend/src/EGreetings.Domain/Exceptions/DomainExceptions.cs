namespace EGreetings.Domain.Exceptions;

public class BusinessRuleViolationException : Exception
{
    public string RuleCode { get; }

    public BusinessRuleViolationException(string ruleCode, string message)
        : base(message)
    {
        RuleCode = ruleCode;
    }
}

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized") : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Forbidden") : base(message) { }
}
