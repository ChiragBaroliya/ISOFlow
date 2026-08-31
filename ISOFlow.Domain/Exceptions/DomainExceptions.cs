namespace ISOFlow.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entityName, object key) : base($"Entity '{entityName}' with identifier ({key}) was not found.") { }
}

public class ValidationException : Exception
{
    public List<string> Errors { get; } = new();

    public ValidationException(string message) : base(message)
    {
        Errors.Add(message);
    }

    public ValidationException(IEnumerable<string> errors) : base("One or more validation failures have occurred.")
    {
        Errors.AddRange(errors);
    }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
