namespace UltraReader.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public NotFoundException(string entityType, object? entityId = null)
        : base($"{entityType} not found" + (entityId != null ? $" (Id: {entityId})" : ""))
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public NotFoundException(string entityType, object entityId, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    public string Field { get; }
    public object? Value { get; }

    public ValidationException(string message) : base(message)
    {
        Field = string.Empty;
    }

    public ValidationException(string field, string message) : base(message)
    {
        Field = field;
    }

    public ValidationException(string field, object? value, string message) : base(message)
    {
        Field = field;
        Value = value;
    }
}
