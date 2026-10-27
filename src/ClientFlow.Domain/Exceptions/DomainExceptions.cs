namespace ClientFlow.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}

public class BusinessRuleValidationException : DomainException
{
    public string PropertyName { get; }

    public BusinessRuleValidationException(string propertyName, string message)
        : base(message)
    {
        PropertyName = propertyName;
    }
}
